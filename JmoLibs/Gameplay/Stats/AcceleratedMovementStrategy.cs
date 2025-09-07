using Godot;

using Jmo.Core;

namespace Jmo.Gameplay.Stats
{
    /// <summary>
    /// A concrete movement strategy that produces smooth, accelerated movement based on the
    /// standard properties of a VelocityProfile. This is a robust default for most ground or air characters.
    /// </summary>
    [GlobalClass]
    public partial class AcceleratedMovementStrategy : Resource, IMovementStrategy
    {
        public Vector3 CalculateVelocity(Vector3 currentVelocity, Vector3 desiredDirection, StatController statContoller, float delta)
        {
            var registry = Registry.Instance;
            // The strategy's recipe for calculating the new velocity.
            Vector3 targetVelocity = desiredDirection * statContoller.GetStatValue<float>(Registry.DB.MaxSpeedAttr);
            Vector3 newVelocity = currentVelocity;

            // If there's input, accelerate towards the target.
            if (!desiredDirection.IsZeroApprox())
            {
                newVelocity = newVelocity.MoveToward(targetVelocity, statContoller.GetStatValue<float>(Registry.DB.AccelerationAttr) * delta);
            }
            else // If no input, apply friction.
            {
                // Note: a more complex strategy could use the BrakingMultiplier here.
                newVelocity = newVelocity.MoveToward(Vector3.Zero, statContoller.GetStatValue<float>(Registry.DB.FrictionAttr) * delta);
            }

            return newVelocity;
        }
    }
}