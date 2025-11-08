using System;
using Bonsai.Expressions;
using C_Sglx_namespace;
using OpenCV.Net;

namespace Bonsai.SpikeGLX
{    
    /// <summary>
    /// Represents an instance of SpikeGLX communicating with Bonsai.
    /// </summary>
    internal class SpikeGLX : IDisposable
    {
        private bool disposed;  // Whether unmanaged resources have been disposed
        private readonly IntPtr hSglx;  // SpikeGLX connection handle

        /// <summary>
        /// Initialise an instance of SpikeGLX communicating with Bonsai.
        /// </summary>
        /// <param name="host">IP address of the SpikeGLX command server.</param>
        /// <param name="port">Port of the SpikeGLX command server.</param>
        /// <exception cref="SpikeGLXException"></exception>
        public SpikeGLX(string host, int port)
        {
            hSglx = C_Sglx.c_sglx_createHandle();
            int ok = C_Sglx.c_sglx_connect(hSglx, host, port);
            if (ok == 0) throw new SpikeGLXException(hSglx);
        }

        /// <summary>
        /// Internal function used to get strings from SpikeGLX after calling
        /// a relevant function.
        /// </summary>
        /// <param name="nval">The number of strings to retrieve.</param>
        /// <returns>Array of the returned strings.</returns>
        private string[] GetStrings(int nval)
        {
            string[] output = new string[nval];
            for (int i = 0; i < nval; i++)
            {
                output[i] = C_Sglx.cs_sglx_getstr(hSglx, i);
            }
            return output;
        }

        /// <summary>
        /// Gets SpikeGLX probe geometry map for the given probe number.
        /// </summary>
        /// <param name="ip">Probe number.</param>
        /// <returns>String array of geometry parameters.</returns>
        /// <exception cref="SpikeGLXException"></exception>
        public string[] GetGeomMap(int ip)
        {
            // Request parameters
            int ok = C_Sglx.c_sglx_getGeomMap(out int nval, hSglx, ip);
            if (ok == 0) throw new SpikeGLXException(hSglx);

            // Parse parameters into a string array
            return GetStrings(nval);
        }

        /// <summary>
        /// Gets SpikeGLX run parameters.
        /// </summary>
        /// <returns>Array of "key:value" parameters.</returns>
        /// <exception cref="SpikeGLXException"></exception>
        public string[] GetParams()
        {
            // Request parameters
            int ok = C_Sglx.c_sglx_getParams(out int nval, hSglx);
            if (ok == 0) throw new SpikeGLXException(hSglx);

            // Parse parameters into a string array
            return GetStrings(nval);
        }

        /// <summary>
        /// Gets SpikeGLX parameters common to all IMEC probes.
        /// </summary>
        /// <returns>Array of "key:value" parameters.</returns>
        /// <exception cref="SpikeGLXException"></exception>
        public string[] GetParamsImecCommon()
        {
            // Request parameters
            int ok = C_Sglx.c_sglx_getParamsImecCommon(out int nval, hSglx);
            if (ok == 0) throw new SpikeGLXException(hSglx);

            // Parse parameters into a string array
            return GetStrings(nval);
        }

        /// <summary>
        /// Gets SpikeGLX parameters for a given IMEC probe.
        /// </summary>
        /// <param name="ip">Probe number.</param>
        /// <returns>Array of "key:value" parameters.</returns>
        /// <exception cref="SpikeGLXException"></exception>
        public string[] GetParamsImecProbe(int ip)
        {
            // Request parameters
            int ok = C_Sglx.c_sglx_getParamsImecProbe(out int nval, hSglx, ip);
            if (ok == 0) throw new SpikeGLXException(hSglx);

            // Parse parameters into a string array
            return GetStrings(nval);
        }
            
        /// <summary>
        /// Get the sample rate of a given data stream.
        /// </summary>
        /// <param name="js">Stream type (0: NIDAQ, 1: Onebox, 2: IMEC Probe)</param>
        /// <param name="ip">Substream (0 for NIDAQ, probe number for IMEC Probe)</param>
        /// <returns>The stream sample rate</returns>
        /// <exception cref="SpikeGLXException"></exception>
        protected double GetStreamSampleRate(int js, int ip)
        {
            double sampleRate = C_Sglx.c_sglx_getStreamSampleRate(hSglx, js, ip);
            if (sampleRate == 0) throw new SpikeGLXException(hSglx);
            return sampleRate;
        }

        /// <summary>
        /// Get the constant multiple for converting a channels output between a unitless
        /// integer and a voltage, in volts.
        /// </summary>
        /// <param name="js">Stream type (0: NIDAQ, 1: Onebox, 2: IMEC Probe)</param>
        /// <param name="ip">Substream (0 for NIDAQ, probe number for IMEC Probe)</param>
        /// <param name="chan">Channel</param>
        /// <returns>The multiple to convert the channel output to a voltage, in volts. </returns>
        /// <exception cref="SpikeGLXException"></exception>
        protected double GetStreamI16ToVolts(int js, int ip, int chan)
        {
            int ok = C_Sglx.c_sglx_getStreamI16ToVolts(out double mult, hSglx, js, ip, chan);
            if (ok == 0) throw new SpikeGLXException(hSglx);
            return mult;
        }

        /// <summary>
        /// Get the number of samples on a given channel
        /// </summary>
        /// <param name="js">Stream type (0: NIDAQ, 1: Onebox, 2: IMEC Probe)</param>
        /// <param name="ip">Substream (0 for NIDAQ, probe number for IMEC Probe)</param>
        /// <returns>The number of samples on the channel.</returns>
        /// <exception cref="SpikeGLXException"></exception>
        protected ulong GetStreamSampleCount(int js, int ip)
        {
            ulong count = C_Sglx.c_sglx_getStreamSampleCount(hSglx, js, ip);
            if (count == 0) throw new SpikeGLXException(hSglx);
            return count;
        }

        /// <summary>
        /// Fetch data from SpikeGLX.
        /// </summary>
        /// <param name="data">Array of fetched data.</param>
        /// <param name="js">Stream type (0: NIDAQ, 1: Onebox, 2: IMEC Probe).</param>
        /// <param name="ip">Substream (0 for NIDAQ, probe number for IMEC Probe).</param>
        /// <param name="startSample">Sample count to fetch from.</param>
        /// <param name="maxSamples">Maximum number of samples to fetch.</param>
        /// <param name="channels">Channels to fetch data from.</param>
        /// <param name="downsample">Factor by which to downsample fetched data.</param>
        /// <returns>Sample count at the beginning of the fetched data.</returns>
        /// <exception cref="SpikeGLXException"></exception>
        protected ulong Fetch(out short[] data, int js, int ip, ulong startSample, 
            int maxSamples, int[] channels, int downsample)
        {
            ulong headCount = C_Sglx.cs_sglx_fetch(out data, hSglx, js, ip, startSample, 
                maxSamples, channels, downsample);
            if (headCount == 0) throw new SpikeGLXException(hSglx);
            return headCount;
        }

        /// <summary>
        /// Fetch data from SpikeGLX.
        /// </summary>
        /// <param name="data">
        /// Matrix of fetched data. Each row is a channel, each column is a sample.
        /// </param>
        /// <param name="js">Stream type (0: NIDAQ, 1: Onebox, 2: IMEC Probe).</param>
        /// <param name="ip">Substream (0 for NIDAQ, probe number for IMEC Probe).</param>
        /// <param name="startSample">Sample count to fetch from.</param>
        /// <param name="maxSamples">Maximum number of samples to fetch.</param>
        /// <param name="channels">Channels to fetch data from.</param>
        /// <param name="downsample">Factor by which to downsample fetched data.</param>
        /// <returns>Sample count at the beginning of the fetched data.</returns>
        /// <exception cref="SpikeGLXException"></exception>
        protected ulong Fetch(out Mat data, int js, int ip, ulong startSample, int maxSamples, 
            int[] channels, int downsample)
        {
            // Get the data as an array of shorts.
            ulong headCount = Fetch(out short[] rawData, js, ip, startSample, maxSamples, 
                channels, downsample);

            // Convert the data into a matrix.
            int rows = channels.Length;
            int cols = rawData.Length / rows;
            var dataTransposed = Mat.FromArray(rawData, cols, rows, Depth.S16, 1);
            data = new Mat(rows, cols, Depth.S16, 1);
            CV.Transpose(dataTransposed, data);

            return headCount;
        }

        /// <summary>
        /// Fetch the latest data from SpikeGLX.
        /// </summary>
        /// <param name="data">Array of fetched data.</param>
        /// <param name="js">Stream type (0: NIDAQ, 1: Onebox, 2: IMEC Probe).</param>
        /// <param name="ip">Substream (0 for NIDAQ, probe number for IMEC Probe).</param>
        /// <param name="maxSamples">Maximum number of samples to fetch.</param>
        /// <param name="channels">Channels to fetch data from.</param>
        /// <param name="downsample">Factor by which to downsample fetched data.</param>
        /// <returns>Sample count at the beginning of the fetched data.</returns>
        /// <exception cref="SpikeGLXException"></exception>
        protected ulong FetchLatest(out short[] data, int js, int ip, int maxSamples, int[] channels, int downsample)
        {
            ulong headCount = C_Sglx.cs_sglx_fetchLatest(out data, hSglx, js, ip, maxSamples, channels, downsample);
            if (headCount == 0) throw new SpikeGLXException(hSglx);
            return headCount;
        }

        /// <summary>
        /// Fetch the latest data from SpikeGLX.
        /// </summary>
        /// <param name="data">
        /// Matrix of fetched data. Each row is a channel, each column is a sample.
        /// </param>
        /// <param name="js">Stream type (0: NIDAQ, 1: Onebox, 2: IMEC Probe).</param>
        /// <param name="ip">Substream (0 for NIDAQ, probe number for IMEC Probe).</param>
        /// <param name="maxSamples">Maximum number of samples to fetch.</param>
        /// <param name="channels">Channels to fetch data from.</param>
        /// <param name="downsample">Factor by which to downsample fetched data.</param>
        /// <returns>Sample count at the beginning of the fetched data.</returns>
        /// <exception cref="SpikeGLXException"></exception>
        protected ulong FetchLatest(out Mat data, int js, int ip, int maxSamples, int[] channels, int downsample)
        {         
            // Get the data as an array of shorts.
            ulong headCount = FetchLatest(out short[] rawData, js, ip, maxSamples, channels, downsample);

            // Convert the data into a matrix.
            int rows = channels.Length;
            int cols = rawData.Length / rows;
            var dataTransposed = Mat.FromArray(rawData, cols, rows, Depth.S16, 1);
            data = new Mat(rows, cols, Depth.S16, 1);
            CV.Transpose(dataTransposed, data);

            return headCount;
        }

        /// <summary>
        /// Set a digital output through SpikeGLX.
        /// </summary>
        /// <param name="output">The digital output value to set</param>
        /// <param name="channels">
        /// The output channels to set the value of. Channel strings have form:
        /// "Dev6/port0/line2,Dev6/port0/line5".
        /// </param>
        /// <exception cref="SpikeGLXException"></exception>
        public void SetDigitalOut(int output, string channels)
        {
            int ok = C_Sglx.c_sglx_setDigitalOut(hSglx, output, channels);
            if (ok == 0) throw new SpikeGLXException(hSglx);
        }

        /// <summary>
        /// Control SpikeGLX recording by setting the gate and trigger values.
        /// </summary>
        /// <param name="gate">
        /// The gate set value:
        ///     -1: no change
        ///      0: set low
        ///      1: set high
        /// </param>
        /// <param name="trigger">
        /// The trigger value:
        ///     -1: no change
        ///      0: set low
        ///      1: set high
        /// </param>
        /// <exception cref="SpikeGLXException"></exception>
        public void TriggerGT(int gate, int trigger)
        {
            int ok = C_Sglx.c_sglx_triggerGT(hSglx, gate, trigger);
            if (ok == 0) throw new SpikeGLXException(hSglx);
        }

        // Implement IDisposable.
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                
                C_Sglx.c_sglx_close(hSglx);
                C_Sglx.c_sglx_destroyHandle(hSglx);

                disposed = true;
            }
        }

        // Use C# finalizer syntax for finalization code.
        // This finalizer will run only if the Dispose method
        // does not get called.
        ~SpikeGLX()
        {
            Dispose(disposing: false);
        }
    }
}
