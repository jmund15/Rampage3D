using Godot;

namespace Jmo.Gameplay.Stats
{
    /// <summary>
    /// Defines a contract for a swappable, stateless calculation strategy. Its sole responsibility
    /// is to interpret the data from a VelocityProfile to calculate a new velocity based on input.
    /// It is a pure mathematical function, completely independent of character state.
    /// </summary>
    /// <remarks>
    /// Exmaples of different strategies could include instantaneous velocity changes, or using the terminal velocity formula
    /// </remarks>
    public interface IMovementStrategy
    {
        Vector3 CalculateVelocity(Vector3 currentVelocity, Vector3 desiredDirection, StatController statContoller, float delta);
    }
}