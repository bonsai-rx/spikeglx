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
    /// Represents an operator that streams buffers of data from a SpikeGLX data stream.
    /// </summary>
    [Obsolete("Replaced by Fetch.")]
    [Description("Streams buffers of data from a SpikeGLX data stream.")]
    [Combinator(MethodName = nameof(Generate))]
    [WorkflowElementCategory(ElementCategory.Source)]
    public class SpikeStream : Source<Mat>
    {
        /// <summary>
        /// Gets or sets the duration of streamed data buffers, in ms.
        /// </summary>
        [Description("Duration of streamed data buffers, in ms.")]
        public int BufferLength { get; set; } = 1000;

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
        /// Gets or sets the flag to use a high resolution timer (~1ms vs. ~15ms).
        /// </summary>
        [Description("Flag to use a high resolution timer (~1ms vs. ~15ms).")]
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
                    using SpikeGLXDataStream connection = new(Host, Port, StreamType, Substream, Channels);

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
    }
}
