using Clonepage.JmoLibs.Shared;
using Jmo.Shared;
using System;

namespace Jmo.AI.Perception
{
    /// <summary>
    /// Represents the arguments for a perception event, containing the generated Percept.
    /// </summary>
    public class PerceptEventArgs : EventArgs
    {
        public Percept Percept { get; }
        public IAISensor Sensor { get; }
        public PerceptEventArgs(Percept percept, IAISensor sensor) { Percept = percept; Sensor = sensor; }
    }

    /// <summary>
    /// The core interface for any sensor in the AI system. A sensor's only job is to
    /// observe the world and fire an event when it perceives something. It is responsible
    /// for generating a `Percept` struct containing all relevant contextual information about the sensation.
    /// </summary>
    public interface IAISensor : IGodotNodeInterface
    {
        /// <summary>
        /// Fired when a new percept is created or an existing one is updated with new information.
        /// The AIPerceptionManager subscribes to this event to populate its memory bank.
        /// </summary>
        event EventHandler<PerceptEventArgs> PerceptUpdated;
    }
}