using System;
using Godot;
using Jmo.Shared; // Assuming IGodotNodeInterface is here

namespace Jmo.Core.Movement
{

    public interface IMovementController : IGodotNodeInterface
    {
        Vector3 CurrentVelocity { get; set; }
        Vector3 GlobalPosition { get; }
        bool IsOnFloor { get; }

        /// <summary>
        /// The core movement method for CharacterBody3D and similar nodes.
        /// </summary>
        void Move();
    }
}
