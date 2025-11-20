using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.SpikeGLX
{
    /// <summary>
    /// Represents an operator that remotely controls SpikeGLX saving data by setting 
    /// the gate and trigger.
    /// </summary>
    /// <remarks>
    /// Gate and/or trigger must be set to "Remote Controlled Start and Stop" in SpikeGLX.
    /// </remarks>
    [Combinator]
    [Description("Remotely controls SpikeGLX recording by setting the gate and trigger.")]
    [WorkflowElementCategory(ElementCategory.Sink)]
    public class Trigger
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
        /// Sets the gate and trigger values in SpikeGLX.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs of 32-bit signed integers used to set the gate and trigger (in that order).
        ///     -1: no change
        ///      0: set low
        ///      1: set high
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of setting the gate and trigger values
        /// in SpikeGLX.
        /// </returns>
        public IObservable<Tuple<int,int>> Process(IObservable<Tuple<int, int>> source)
        {
            return Observable.Using(() => new SpikeGLX(Host, Port),
                connection => source.Do(input => connection.TriggerGT(input.Item1, input.Item2)));
        }
    }
}
