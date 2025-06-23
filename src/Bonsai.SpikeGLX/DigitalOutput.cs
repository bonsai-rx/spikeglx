using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.SpikeGLX
{
    // <summary>
    /// Represents an operator that controls a digital output.
    /// </summary>
    [Combinator]
    [Description("Controls a digital output.")]
    [WorkflowElementCategory(ElementCategory.Sink)]
    public class DigitalOutput
    {
        /// <summary>
        /// Gets or sets the IP address of the SpikeGLX command server
        /// </summary>
        [Description("IP address of the SpikeGLX command server.")]
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets the port of the SpikeGLX command server.
        /// </summary>
        [Description("Port of the SpikeGLX command server.")]
        public int Port { get; set; } = 4142;

        /// <summary>
        /// Gets or sets the channels to output the digital signal to.
        /// Strings have format \"Dev6/port0/line2,Dev6/port0/line5\".
        /// </summary>
        [Description("Channels to output the digital signal to. " +
            "Strings have format \"Dev6/port0/line2,Dev6/port0/line5\"")]
        public string Channels { get; set; }

        /// <summary>
        /// Set a digital output.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="int"/>s, each representing a high (1) or low (0) digital state.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of setting a digital output. 
        /// </returns>
        public IObservable<int> Process(IObservable<int> source)
        {
            return Observable.Using<int, SpikeGLX>(() => new SpikeGLX(Host, Port),
                connection => source.Do(input => connection.SetDigitalOut(input, Channels)));
        }

        /// <summary>
        /// Set a digital output.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="float"/>s, each representing a high (1.0) or low (0.0) digital state.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of setting a digital output. 
        /// </returns>
        public IObservable<int> Process(IObservable<float> source)
        {
            return Process(source.Select(input => (int)input));
        }

        /// <summary>
        /// Set a digital output.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="float"/>s, each representing a high (1.0) or low (0.0) digital state.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of setting a digital output. 
        /// </returns>
        public IObservable<int> Process(IObservable<double> source)
        {
            return Process(source.Select(input => (int)input));
        }

        /// <summary>
        /// Set a digital output.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="bool"/>s, each representing a high (true) or low (false) digital state.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of setting a digital output. 
        /// </returns>
        public IObservable<int> Process(IObservable<bool> source)
        {
            return Process(source.Select(input => input ? 1 : 0));
        }
    }
}
