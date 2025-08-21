using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmo.Core.Movement
{
    // IRigidController3D.cs (A new, separate interface)
    using Godot;

    /// <summary>
    /// The definitive, low-level interface for applying physics forces to a RigidBody3D.
    /// Commands correspond to applying forces and impulses within Godot's physics simulation.
    /// </summary>
    public interface IRigidController3D
    {
        Vector3 GlobalPosition { get; }
        Vector3 LinearVelocity { get; } // Read-only for rigid bodies

        /// <summary>Applies a continuous force, affecting acceleration.</summary>
        void ApplyForce(Vector3 force);

        /// <summary>Applies an instantaneous change in velocity.</summary>
        void ApplyImpulse(Vector3 impulse);

        void Teleport(Vector3 newGlobalPosition);
    }
}
