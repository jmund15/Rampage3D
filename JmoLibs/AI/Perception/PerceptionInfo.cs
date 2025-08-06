using Godot;
using Jmo.AI.Perception.Strategies;
using Jmo.Core;
using System;

namespace Jmo.AI.Perception
{
    /// <summary>
    /// A stateful class representing an AI's "living memory" of a single target. It is created
    /// and managed by the AIPerceptionManager and applies a decay strategy to its confidence,
    /// providing a dynamic and realistic memory model.
    /// </summary>
    public class PerceptionInfo
    {
        /// <summary>The target Node this memory pertains to.</summary>
        public Node3D Target { get; }
        public Vector3 LastKnownPosition { get; private set; }
        public Vector3 LastKnownVelocity { get; private set; }
        public Identity Identity { get; private set; }
        public ulong LastUpdateTime { get; private set; }

        private float _baseConfidence;
        private MemoryDecayStrategy _decayStrategy;

        /// <summary>
        /// Calculates and returns the current, time-decayed confidence for this memory. This is an
        /// efficient, on-demand property that performs the calculation only when asked.
        /// </summary>
        public float CurrentConfidence => _decayStrategy?.CalculateConfidence(_baseConfidence, (Time.GetTicksMsec() - LastUpdateTime) / 1000.0f) ?? _baseConfidence;

        /// <summary>Returns true if the memory is still considered active (confidence is above zero).</summary>
        public bool IsActive => CurrentConfidence > 0.001f; // Use a small epsilon to avoid floating point issues.

        public PerceptionInfo(Percept initialPercept)
        {
            Target = initialPercept.Target;
            Update(initialPercept);
        }

        /// <summary>Updates the memory record with information from a new, incoming percept.</summary>
        public void Update(Percept latestPercept)
        {
            LastKnownPosition = latestPercept.LastKnownPosition;
            LastKnownVelocity = latestPercept.LastKnownVelocity;
            Identity = latestPercept.Identity;
            _baseConfidence = latestPercept.Confidence;
            _decayStrategy = latestPercept.DecayStrategy;
            LastUpdateTime = latestPercept.Timestamp;
        }
}
}