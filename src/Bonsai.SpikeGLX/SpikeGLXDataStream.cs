using OpenCV.Net;

namespace Bonsai.SpikeGLX
{
    /// <summary>
    /// Represents an instance of a single SpikeGLX data stream being accessed from Bonsai.
    /// </summary>
    internal class SpikeGLXDataStream : SpikeGLX
    {
        protected readonly int JS;              // Stream type (0: NIDAQ, 1: Onebox, 2: IMEC Probe).  
        protected readonly int IP;              // Substream (0 for NIDAQ, probe number for IMEC Probe).
        protected readonly int[] Channels;      // Channels to fetch data from.
        protected readonly double[] Mults;      // Constants to convert raw data to voltages for each channel.
        protected readonly double SampleRate;   // Channel sample rate.

        /// <summary>
        /// Initialise an instance of a single SpikeGLX data stream communicating with Bonsai.
        /// </summary>
        /// <param name="host">IP address of the SpikeGLX command server.</param>
        /// <param name="port">Port of the SpikeGLX command server.</param>
        /// <param name="js">Stream type (0: NIDAQ, 1: Onebox, 2: IMEC Probe).</param>
        /// <param name="ip">Substream (0 for NIDAQ, probe number for IMEC Probe).</param>
        /// <param name="channels">Channels to fetch data from.</param>
        /// <exception cref="SpikeGLXException"></exception>
        public SpikeGLXDataStream(string host, int port, int js, int ip, int[] channels) : base(host, port)
        {
            // Set stream parameters
            JS = js;
            IP = ip;
            Channels = channels;

            // Get stream sample rate.
            SampleRate = GetStreamSampleRate(JS, IP);

            // Get multiples for converting raw data to voltages.
            Mults = new double[channels.Length];
            for (int i = 0; i < channels.Length; i++)
            {
                Mults[i] = GetStreamI16ToVolts(JS, IP, channels[i]);
            }

        }

        /// <summary>
        /// Get the number of samples in the data stream.
        /// </summary>
        /// <returns>The number of samples in the data stream.</returns>
        public ulong GetStreamSampleCount()
        {
            return GetStreamSampleCount(JS, IP);
        }

        /// <summary>
        /// Get the sample rate of the given data stream.
        /// </summary>
        /// <returns>The stream sample rate.</returns>
        public double GetStreamSampleRate()
        {
            return SampleRate;
        }

        /// <summary>
        /// Convert a matrix of raw data from the data stream into a matrix
        /// of voltages, in volts.
        /// </summary>
        /// <param name="data">The data matrix to convert.</param>
        /// <returns>The converted data matrix.</returns>
        private Mat ConvertToVoltage(Mat data)
        {
            // Initialise the new data matrix
            Mat dataVolts = Mat.Zeros(data.Rows, data.Cols, Depth.F64, 1);

            // Mulitply each channel (row) by its associated multiple
            for (int row = 0; row < data.Rows; row++)
                for (int col = 0; col < data.Cols; col++)
                {
                    dataVolts.SetReal(row, col, (double)data.GetReal(row, col) * Mults[row]);
                }

            // Dispose of the original data matrix
            data.Close();

            return dataVolts;
        }

        /// <summary>
        /// Fetch data from the SpikeGLX data stream.
        /// </summary>
        /// <param name="data">
        /// Matrix of fetched data. Each row is a channel, each column is a sample.
        /// </param>
        /// <param name="startSample">Sample count to fetch from.</param>
        /// <param name="maxSamples">Maximum number of samples to fetch.</param>
        /// <param name="downsample">Factor by which to downsample fetched data.</param>
        /// <param name="volts">Convert the data to a voltage, in volts.</param>
        /// <returns>Sample count at the beginning of the fetched data.</returns>
        /// <exception cref="SpikeGLXException"></exception>
        public ulong Fetch(out Mat data, ulong startSample, int maxSamples, int downsample, bool volts)
        {
            ulong headCount = Fetch(out data, JS, IP, startSample, maxSamples, Channels, downsample);
            if (volts) data = ConvertToVoltage(data);
            return headCount;
        }

        /// <summary>
        /// Fetch the latest data from the SpikeGLX data stream.
        /// </summary>
        /// <param name="data">
        /// Matrix of fetched data. Each row is a channel, each column is a sample.
        /// </param>
        /// <param name="maxSamples">Maximum number of samples to fetch.</param>
        /// <param name="downsample">Factor by which to downsample fetched data.</param>
        /// <param name="volts">Convert the data to a voltage, in volts.</param>
        /// <returns>Sample count at the beginning of the fetched data.</returns>
        /// <exception cref="SpikeGLXException"></exception>
        public ulong FetchLatest(out Mat data, int maxSamples, int downsample, bool volts)
        {
            ulong headCount = FetchLatest(out data, JS, IP, maxSamples, Channels, downsample);
            if (volts) data = ConvertToVoltage(data);
            return headCount;
        }
    }
}
