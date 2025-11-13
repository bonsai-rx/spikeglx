using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.SpikeGLX
{
    /// <summary>
    /// Represents an operator that sets one or more digital output lines through SpikeGLX from
    /// a sequence of values representing the state of the line.
    /// </summary>
    [Combinator]
    [Description("Sets one or more digital output lines through SpikeGLX from a sequence of values representing the state of the line.")]
    [WorkflowElementCategory(ElementCategory.Sink)]
    public class DigitalOutput
    {
        /// <summary>
        /// Gets or sets the IP address of the SpikeGLX command server.
        /// </summary>
        [Description("The IP address of the SpikeGLX command server.")]
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets the port of the SpikeGLX command server.
        /// </summary>
        [Description("The port of the SpikeGLX command server.")]
        public int Port { get; set; } = 4142;

        /// <summary>
        /// Gets or sets the name of the channels to output the digital signal to.
        /// </summary>
        /// <remarks>
        /// Strings have format \"Dev6/port0/line2,Dev6/port0/line5\".
        /// </remarks>
        [Description("The name of the channels to output the digital signal to.")]
        public string Channels { get; set; }

        /// <summary>
        /// Sets one or more digital output lines through SpikeGLX from an observable sequence
        /// of integer values representing a digital state.
        /// </summary>
        /// <param name="source">
        /// A sequence of 32-bit signed integers, where each value represents a high (1) or low (0)
        /// state to which to set the digital output line.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of setting digital output lines through
        /// SpikeGLX.
        /// </returns>
        public IObservable<int> Process(IObservable<int> source)
        {
            return Observable.Using(() => new SpikeGLX(Host, Port),
                connection => source.Do(input => connection.SetDigitalOut(input, Channels)));
        }

        /// <summary>
        /// Sets one or more digital output lines through SpikeGLX from an observable sequence
        /// of boolean values representing a digital state.
        /// </summary>
        /// <param name="source">
        /// A sequence of boolean values, where each value represents a high (<see langword="true"/>)
        /// or low (<see langword="false"/>) state to which to set the digital output line.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of setting digital output lines through
        /// SpikeGLX.
        /// </returns>
        public IObservable<int> Process(IObservable<bool> source)
        {
            return Process(source.Select(input => input ? 1 : 0));
        }
    }
}
