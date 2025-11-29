using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.SpikeGLX
{
    /// <summary>
    /// Represents an operator that sets one or more NI digital output lines through SpikeGLX from
    /// a sequence of values representing the state of the line.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Sink)]
    [Description("Sets one or more NI digital output lines through SpikeGLX from a sequence of values representing the state of the line.")]
    public class DigitalOutput
    {
        /// <summary>
        /// Gets or sets the IP address of the SpikeGLX command server.
        /// </summary>
        [Category("Command Server")]
        [Description("The IP address of the SpikeGLX command server.")]
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets the port of the SpikeGLX command server.
        /// </summary>
        [Category("Command Server")]
        [Description("The port of the SpikeGLX command server.")]
        public int Port { get; set; } = 4142;

        /// <summary>
        /// Gets or sets the name of the channels to output the digital signal to.
        /// </summary>
        /// <remarks>
        /// Only lines from a single device should be listed. Valid formats include:
        /// <list type="bullet">
        /// <item><c>Dev6/port0/line2,Dev6/port0/line5</c></item>
        /// <item><c>Dev6/port1/line0:3</c></item>
        /// <item><c>Dev6/port1:2</c></item>
        /// </list>
        /// </remarks>
        [Description("The name of the channels to output the digital signal to.")]
        public string Channels { get; set; }

        /// <summary>
        /// Sets one or more NI digital output lines through SpikeGLX from an observable sequence
        /// of unsigned integer values representing a digital state.
        /// </summary>
        /// <param name="source">
        /// A sequence of 32-bit unsigned integers, where each value represents a bitmask of 
        /// states to which to set the digital output lines.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of setting digital output lines through
        /// SpikeGLX.
        /// </returns>
        /// <remarks>
        /// The lowest 8 bits of the bitmask map to port0, the next higher 8 bits to port1, etc.
        /// This mapping is fixed, irrespective of whether only a subset of lines have been
        /// selected in <see cref="Channels"/>. The effect of selecting a subset of lines
        /// in <see cref="Channels"/> is to ignore those bits in the bitmask corresponding
        /// to unlisted lines. 
        /// </remarks>
        public IObservable<uint> Process(IObservable<uint> source)
        {
            return Observable.Using(() => new SpikeGLX(Host, Port),
                connection =>
                {
                    var channels = Channels;
                    return source.Do(input => connection.SetNIDigitalOut(channels, input));
                });
        }

        /// <summary>
        /// Sets one or more NI digital output lines through SpikeGLX from an observable sequence
        /// of boolean values representing a digital state.
        /// </summary>
        /// <param name="source">
        /// A sequence of boolean values, where each value represents a high (<see langword="true"/>)
        /// or low (<see langword="false"/>) state to which to set the digital output lines.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of setting digital output lines through
        /// SpikeGLX.
        /// </returns>
        public IObservable<bool> Process(IObservable<bool> source)
        {
            return Observable.Using(() => new SpikeGLX(Host, Port),
                connection =>
                {
                    var channels = Channels;
                    return source.Do(input => connection.SetNIDigitalOut(channels, input ? uint.MaxValue : uint.MinValue));
                });
        }
    }
}
