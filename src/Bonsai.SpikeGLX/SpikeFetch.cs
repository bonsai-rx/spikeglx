using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using PrecisionTiming;

namespace Bonsai.SpikeGLX
{
    /// <summary>
    /// Represents an operator that generates a sequence of the latest buffers of data
    /// from a SpikeGLX data stream.
    /// </summary>
    [Obsolete("Replaced by Fetch.")]
    [Description("Outputs the most recent buffer of data from SpikeGLX.")]
    [Combinator(MethodName = nameof(Generate))]
    [WorkflowElementCategory(ElementCategory.Source)]
    public class SpikeFetch : Source<Mat>
    {
        /// <summary>
        /// Gets or sets the duration of fetched data, in ms.
        /// </summary>
        [Description("Duration of fetched data, in ms.")]
        public int FetchLength { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the period at which to fetch data, in ms. Only relevant
        /// if no external input is provided.
        /// </summary>
        [Description("How often to fetch data (ms). Only relevant if no external input is provided.")]
        public int FetchPeriod { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the IP address of the SpikeGLX command server
        /// </summary>
        [Description("IP address of the SpikeGLX command server." +
            "\"localhost\" evaluates to 127.0.0.1.")]
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets the port of the SpikeGLX command server.
        /// </summary>
        [Description("Port of the SpikeGLX command server.")]
        public int Port { get; set; } = 4142;

        /// <summary>
        /// Gets or sets the stream type (0: NIDAQ, 1: Onebox, 2: IMEC Probe).
        /// </summary>
        [Description("Stream type (0: NIDAQ, 1: Onebox, 2: IMEC Probe).")]
        public int StreamType { get; set; } = 0;

        /// <summary>
        /// Gets or sets the substream (0 for NIDAQ, probe number for IMEC Probe).
        /// </summary>
        [Description("Substream (0 for NIDAQ, probe number for IMEC Probe).")]
        public int Substream { get; set; } = 0;

        /// <summary>
        /// Gets or sets the array of channels to fetch data from.
        /// </summary>
        [Description("Array of channels to fetch data from.")]
        [TypeConverter(typeof(ChannelRangeTypeConverter))]
        public int[] Channels { get; set; } = null;

        /// <summary>
        /// Gets or sets the factor by which fetched data is downsampled.
        /// </summary>
        [Description("Factor by which fetched data is downsampled.")]
        public int Downsample { get; set; } = 1;

        /// <summary>
        /// Gets or sets the flag to convert the fetched data from a unitless quantity
        /// to a voltage, in volts.
        /// </summary>
        [Description("Flag to convert the fetched data from a unitless quantity to a voltage, in volts.")]
        public bool ConvertToVoltage { get; set; } = false;

        /// <summary>
        /// Gets or sets the flag to use a high resolution timer (~1ms vs. ~15ms). Only relevant
        /// if no external input is provided. 
        /// </summary>
        [Description("Flag to use a high resolution timer (~1ms vs. ~15ms). Only relevant if no external input is provided.")]
        public bool HighResolutionTimer { get; set; } = false;

        /// <summary>
        /// Generates an observable sequence of buffers of data fetched from a SpikeGLX data stream.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects representing buffers of fetched data from a
        /// SpikeGLX data stream. 
        /// </returns>
        public override IObservable<Mat> Generate()
        {
             IObservable<long> fetchTimer;

            // If a high resolution timer is not requested,
            if (!HighResolutionTimer)
            {
                // use Reactive-X's default interval observable.
                fetchTimer = Observable.Interval(TimeSpan.FromMilliseconds(FetchPeriod));
            } 

            // Otherwise,
            else
            {
                // create a custom observable sequence using a PrecisionTimer. 
                fetchTimer = Observable.Create<long>(
                    observer =>
                    {
                        var timer = new PrecisionTimer();
                        timer.SetInterval(() => observer.OnNext(0), FetchPeriod);
                        return timer; // Return the timer so that it is disposed correctly
                    });
            }

            // Return an observable sequence that fetches a buffer of data whenever the
            // fetchTimer emits a notification.
            return Generate(fetchTimer);
        }

        /// <summary>
        /// Generates an observable sequence of buffers of data fetched from a SpikeGLX data stream.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source">
        /// Input sequence used to trigger fetching. A new buffer is fetched every time the input sequence
        /// emits a notification.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects representing buffers of fetched data from a
        /// SpikeGLX data stream. 
        /// </returns>
        public IObservable<Mat> Generate<TSource>(IObservable<TSource> source)
        {
            return Observable
                .Using<Mat, SpikeGLXDataStream>(() => new SpikeGLXDataStream(Host, Port, StreamType, Substream, Channels),
                    connection => source
                        .Select(input =>
                        {
                            connection.FetchLatest(out Mat data, 
                                (int)(FetchLength * connection.GetStreamSampleRate() / 1000),
                                Downsample,
                                ConvertToVoltage);
                            return data;
                        }))
                .PublishReconnectable()
                .RefCount();            
        }
    }
}
