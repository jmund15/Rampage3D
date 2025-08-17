using Godot;
using Jmo.Core;
using Jmo.Core.Movement;
using Jmo.Core.Modifiers;
using System.Collections.Generic;
using Jmo.Gameplay.Actors;

namespace Jmo.Gameplay.Actors
{
    /// <summary>
    /// The definitive high-level orchestrator for character movement. Its sole responsibility
    /// is to be a pure calculation engine. It takes the active strategy, the character's final
    /// modified stats, and a pre-calculated desired direction, and uses them to calculate the
    /// final velocity command for the IMovementController. It is a reusable, stateless service
    /// called by the character's State Machine.
    /// </summary>
    public class MovementProcessor
    {
        private readonly IMovementController3D _controller;

        // --- Live Data References ---
        private readonly Dictionary<MovementMode, ModifiableProperty<VelocityProfile>> _movementProfiles;
        private readonly Dictionary<MechanicType, ModifiableProperty<ImpulseData>> _impulses;

        // --- External Systems ---
        private readonly ExternalForceReceiver _forceReceiver;
        private readonly Node3D _owner;

        private readonly Vector3 _gravity = Vector3.Down * 9.8f;

        public MovementProcessor(
            IMovementController3D controller,
            Dictionary<MovementMode, ModifiableProperty<VelocityProfile>> movementProfiles,
            Dictionary<MechanicType, ModifiableProperty<ImpulseData>> impulses,
            ExternalForceReceiver forceReceiver,
            Node3D owner)
        {
            _controller = controller;
            _movementProfiles = movementProfiles;
            _impulses = impulses;
            _forceReceiver = forceReceiver;
            _owner = owner;
        }

        /// <summary>
        /// The main update loop for continuous movement. It is called by the active State,
        /// which provides all necessary contextual information.
        /// </summary>
        public void ProcessMovement(IMovementStrategy strategy, MovementMode activeMode, Vector3 desiredDirection, float delta)
        {
            // --- 1. Get Final, Modified Velocity Profile ---
            if (!_movementProfiles.TryGetValue(activeMode, out var modifiableProfile))
            {
                // This character has no defined physics for this mode. Apply external forces only.
                ApplyExternalForces(delta);
                _controller.Move();
                return;
            }
            VelocityProfile finalProfile = modifiableProfile.Value;

            // --- 2. Calculate Character-Driven Velocity via the Strategy ---
            Vector3 characterVelocity = strategy.CalculateVelocity(_controller.Velocity, desiredDirection, finalProfile, _controller.IsOnFloor, delta);
            _controller.SetVelocity(new Vector3(characterVelocity.x, _controller.Velocity.y, characterVelocity.z));

            // --- 3. Apply External Forces (Gravity, Environment) ---
            ApplyExternalForces(delta);

            // --- 4. Execute the Final Move ---
            _controller.Move();
        }

        /// <summary>
        /// Executes a discrete, one-time impulse mechanic. This is called by the State in response to an intent.
        /// </summary>
        public void ProcessImpulse(MechanicType mechanic, Vector3 impulseDirection)
        {
            if (_impulses.TryGetValue(mechanic, out var modifiableImpulse))
            {
                ImpulseData finalImpulse = modifiableImpulse.Value;
                _controller.ApplyImpulse(impulseDirection * finalImpulse.Strength);
            }
        }

        private void ApplyExternalForces(float delta)
        {
            if (!_controller.IsOnFloor)
            {
                _controller.AddVelocity(_gravity * delta);
            }

            var externalForce = _forceReceiver.GetTotalForce(_owner);
            _controller.AddVelocity(externalForce * delta);
        }
    }
}