﻿using Godot;
using Jmo.Core;
using Jmo.AI.Perception.Strategies;

namespace Jmo.AI.Perception
{
    /// <summary>
    /// An immutable struct representing a single, stateless sensory event or "snapshot" in time.
    /// It contains all context about a sensation at the moment it occurred. Sensors produce these
    /// to be processed by the AIPerceptionManager.
    /// </summary>
    public readonly struct Percept
    {
        /// <summary>The entity that was perceived. Can be null for location-only percepts like sounds.</summary>
        public readonly Node3D Target;

        /// <summary>The position where the sensation occurred or the last known position of the target.</summary>
        public readonly Vector3 LastKnownPosition;

        /// <summary>The velocity of the target at the moment of perception. For static objects, this will be Vector3.Zero.</summary>
        public readonly Vector3 LastKnownVelocity;

        /// <summary>The data-driven Identity of this percept (e.g., "Enemy.tres").</summary>
        public readonly Identity Identity;

        /// <summary>The strength of the sensation, from 0.0 (barely perceived) to 1.0 (clearly perceived).</summary>
        public readonly float Confidence;

        /// <summary>The strategy defining how this memory should fade over time.</summary>
        public readonly MemoryDecayStrategy DecayStrategy;

        /// <summary>The timestamp (in milliseconds via Time.GetTicksMsec()) when this percept was generated.</summary>
        public readonly ulong Timestamp;

        public Percept(Node3D target, Vector3 position, Vector3 velocity, Identity identity, float confidence, MemoryDecayStrategy decayStrategy)
        {
            Target = target;
            LastKnownPosition = position;
            LastKnownVelocity = velocity;
            Identity = identity;
            Confidence = Mathf.Clamp(confidence, 0.0f, 1.0f);
            DecayStrategy = decayStrategy;
            Timestamp = Time.GetTicksMsec();
        }
    }
}