using Godot;

namespace Jmo.Core.Movement.Strategies
{
    /// <summary>
    /// A concrete movement strategy that produces smooth, accelerated movement based on the
    /// standard properties of a VelocityProfile. This is a robust default for most ground or air characters.
    /// </summary>
    public class AcceleratedMovementStrategy : IMovementStrategy
    {
        public Vector3 CalculateVelocity(Vector3 currentVelocity, Vector3 desiredDirection, VelocityProfile profile, float delta)
        {
            // The strategy's recipe for calculating the new velocity.
            Vector3 targetVelocity = desiredDirection * profile.MaxSpeed;
            Vector3 newVelocity = currentVelocity;

            // If there's input, accelerate towards the target.
            if (!desiredDirection.IsZeroApprox())
            {
                newVelocity = newVelocity.MoveToward(targetVelocity, profile.Acceleration * delta);
            }
            else // If no input, apply friction.
            {
                // Note: a more complex strategy could use the BrakingMultiplier here.
                newVelocity = newVelocity.MoveToward(Vector3.Zero, profile.Friction * delta);
            }

            return newVelocity;
        }
    }
}