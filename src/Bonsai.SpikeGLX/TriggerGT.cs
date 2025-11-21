using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;

namespace Bonsai.SpikeGLX
{
    /// <summary>
    /// Represents an operator that controls SpikeGLX file writing by setting 
    /// the gate and trigger levels whenever the source sequence emits a notification.
    /// </summary>
    /// <remarks>
    /// Gate and/or trigger must be set to "Remote Controlled Start and Stop" in SpikeGLX.
    /// </remarks>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Combinator)]
    [Description("Controls SpikeGLX file writing by setting the gate and trigger levels whenever the source sequence emits a notification.")]
    public class TriggerGT
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
        /// Gets or sets the level at which to set the gate.
        /// </summary>
        [Description("The level at which to set the gate.")]
        public TriggerGTLevel Gate { get; set; }

        /// <summary>
        /// Gets or sets the level at which to set the trigger.
        /// </summary>
        [Description("The level at which to set the trigger.")]
        public TriggerGTLevel Trigger { get; set; }

        /// <summary>
        /// Sets the gate and trigger values in SpikeGLX and returns an observable
        /// sequence with a single notification once the levels have been set.
        /// </summary>
        /// <returns>
        /// A sequence with a single <see cref="Unit"/> value emitted once the
        /// gate and trigger levels have been set in SpikeGLX.
        /// </returns>
        public IObservable<Unit> Process()
        {
            return Process(Observable.Return(Unit.Default));
        }

        /// <summary>
        /// Sets the gate and trigger values in SpikeGLX, whenever an observable sequence
        /// emits a notification.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of setting the gate and trigger values
        /// in SpikeGLX.
        /// </returns>
        public IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Using(() => new SpikeGLX(Host, Port),
                connection => source.Do(input => connection.TriggerGT((int)Gate, (int)Trigger)));
        }
    }

    /// <summary>
    /// Specifies possible values for the gate and trigger levels.
    /// </summary>
    public enum TriggerGTLevel
    {
        /// <summary>
        /// Do not change the current level.
        /// </summary>
        NoChange = -1,

        /// <summary>
        /// Set the level LOW.
        /// </summary>
        SetLow = 0,

        /// <summary>
        /// Increment and set the level HIGH.
        /// </summary>
        SetHigh = 1
    }
}
