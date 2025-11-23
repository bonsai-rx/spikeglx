using System;
using System.ComponentModel;
using System.Reactive.Linq;
using OpenCV.Net;
using System.Threading;
using System.Threading.Tasks;
using PrecisionTiming;

namespace Bonsai.SpikeGLX
{
    /// <summary>
    /// Represents an operator that generates a sequence of data buffers from a SpikeGLX data stream.
    /// </summary>
    [Combinator(MethodName = nameof(Generate))]
    [WorkflowElementCategory(ElementCategory.Source)]
    [Description("Streams buffers of data from a SpikeGLX data stream.")]
    public class Fetch : Source<Mat>
    {
        /// <summary>
        /// Gets or sets the IP address of the SpikeGLX command server
        /// </summary>
        [Category("Command Server")]
        [Description("IP address of the SpikeGLX command server." +
            "\"localhost\" evaluates to 127.0.0.1.")]
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets the port of the SpikeGLX command server.
        /// </summary>
        [Category("Command Server")]
        [Description("Port of the SpikeGLX command server.")]
        public int Port { get; set; } = 4142;

        /// <summary>
        /// Gets or sets the duration of fetched data buffers, in ms. 
        /// </summary>
        [Description("Duration of streamed data buffers, in ms.")]
        public int BufferLength { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value specifying the type of stream to fetch from.
        /// </summary>
        [Description("The type of stream to fetch from.")]
        public StreamType StreamType { get; set; } = StreamType.Daq;

        /// <summary>
        /// Gets or sets the substream (0 for NIDAQ, probe number for IMEC Probe).
        /// </summary>
        [Description("Substream (0 for NIDAQ, probe number for IMEC Probe).")]
        public int Substream { get; set; } = 0;

        /// <summary>
        /// Gets or sets the array of channels to fetch data from. 
        /// </summary>
        /// <remarks>
        /// Channels may be provided as an array of integers, or as comma separated ranges of 
        /// channels with an optional step size, e.g. "0:10,20:5:100". These ranges include both
        /// end points. 
        /// </remarks>
        [Description("Array of channels to fetch data from.")]
        [TypeConverter(typeof(ChannelRangeTypeConverter))]
        public int[] Channels { get; set; } = { 0 };

        /// <summary>
        /// Gets or sets the factor by which streamed data is downsampled.
        /// </summary>
        [Description("Factor by which streamed data is downsampled.")]
        public int Downsample { get; set; } = 1;

        /// <summary>
        /// Gets or sets the flag to convert the streamed data from a unitless quantity
        /// to a voltage, in volts.
        /// </summary>
        [Description("Flag to convert the streamed data from a unitless quantity to a voltage, in volts.")]
        public bool ConvertToVoltage { get; set; } = false;

        /// <summary>
        /// Gets or sets the flag to use a high resolution timer (resolution ~1ms vs. ~15ms).
        /// </summary>
        /// <remarks>
        /// Using the high resolution timer allows streaming data at a higher rate, at the 
        /// cost of more computational load due to increased polling of the SpikeGLX command server. 
        /// </remarks>
        [Description("Flag to use a high resolution timer (resolution ~1ms vs. ~15ms).")]
        public bool HighResolutionTimer { get; set; } = false;

        /// <summary>
        /// Generates an observable sequence of buffers of data streamed from a SpikeGLX data stream.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects representing buffers of streamed data from a
        /// SpikeGLX data stream. 
        /// </returns>
        public override IObservable<Mat> Generate()
        {
            return Observable.Create<Mat>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    // Establish connection to SpikeGLX command server.
                    using SpikeGLXDataStream connection = new(Host, Port, (int)StreamType, Substream, Channels);

                    // Get the sample rate of the stream and use it to convert the buffer length,
                    // in ms, to a buffer size, in number of elements.
                    double sampleRate = connection.GetStreamSampleRate();
                    ulong bufferSize = (ulong)Math.Ceiling(sampleRate * BufferLength / 1000);

                    // Create a timer that triggers this task to poll SpikeGLX for new data
                    // at regular intervals. The interval is set to be half the length of the
                    // buffer. If a high resolution timer has been requested, the resolution of
                    // the timer is set to its minimum possible value. Otherwise it is set to
                    // 15ms. 
                    using AutoResetEvent pollSignal = new(false);
                    using PrecisionTimer pollTimer = new();
                    int pollPeriod = (int)Math.Floor((double)BufferLength / 2);
                    pollTimer.SetInterval(() => pollSignal.Set(),
                        pollPeriod,
                        false);
                    pollTimer.Start(HighResolutionTimer ? 0 : 15);

                    // Get the current sample count from the data stream
                    ulong tailCount = connection.GetStreamSampleCount();

                    // While cancellation has not been requested, see if there are enough
                    // available samples to fill a buffer. If so, fetch and emit them. Then
                    // update the most recently fetched sample number. If not, wait for the
                    // timer to signal it is time to poll again. 
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        while ((connection.GetStreamSampleCount() - tailCount) >= bufferSize)
                        {
                            ulong headCount = connection.Fetch(out Mat data, tailCount,
                                (int)bufferSize, Downsample, ConvertToVoltage);
                            tailCount = headCount + bufferSize;
                            observer.OnNext(data);
                        }
                        pollSignal.WaitOne();
                    }

                    // If cancellation has been requested, stop the timer
                    pollTimer.Stop();
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            })
            .PublishReconnectable()
            .RefCount();
        }

        /// <summary>
        /// Generates an observable sequence of buffers of data fetched from a SpikeGLX data stream.
        /// </summary>
        /// <typeparam name="TSource">The type of the input sequence.</typeparam>
        /// <param name="source">Input sequence used to trigger fetching. A new buffer is fetched every time the input sequence emits a notification.</param>
        /// <returns>A sequence of <see cref="Mat"/> objects representing buffers of fetched data from a SpikeGLX data stream.</returns>
        public IObservable<Mat> Generate<TSource>(IObservable<TSource> source)
        {
            // Create a disposable data stream connection using the provided host, port, stream type, substream, and channels.
            return Observable
                .Using<Mat, SpikeGLXDataStream>(() => new SpikeGLXDataStream(Host, Port, (int)StreamType, Substream, Channels),
                    // Use the data stream connection to fetch the latest data for each input notification.
                    connection => source
                        .Select(input =>
                        {
                            // Calculate the maximum number of samples to fetch based on the buffer length and stream sample rate.
                            int maxSamples = (int)(BufferLength * connection.GetStreamSampleRate() / 1000);
                            // Fetch the latest data from the data stream, downsampling and converting to voltage as needed.
                            connection.FetchLatest(out Mat data, maxSamples, Downsample, ConvertToVoltage);
                            return data;
                        }))
                // Publish the observable sequence and reconnect if it is disconnected.
                .PublishReconnectable()
                // Reference count the observable sequence to manage its lifetime.
                .RefCount();
        }
    }

    /// <summary>
    /// Specifies possible values for the SpikeGLX stream.
    /// </summary>
    public enum StreamType
    {
        /// <summary>
        /// NIDAQ
        /// </summary>
        Daq = 0,

        /// <summary>
        /// Onebox
        /// </summary>
        OneBox = 1,

        /// <summary>
        /// IMEC probe
        /// </summary>
        Probe = 2
    }
}
