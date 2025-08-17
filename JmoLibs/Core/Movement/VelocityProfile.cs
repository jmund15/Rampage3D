using Godot;

namespace Jmo.Core.Movement
{
    /// <summary>
    /// A data-driven Resource that defines a set of independent, raw physics properties.
    /// Its sole purpose is to act as a "dumb" data container. The actual movement
    /// behavior is determined by a "Movement Strategy" that interprets this data.
    /// </summary>
    [GlobalClass]
    public sealed partial class VelocityProfile : Resource
    {
        /// <summary>
        /// Name for UI and debugging purposes.
        /// </summary>
        [Export] public string VelocityProfileName { get; private set; } = "Unnamed Velocity Profile";

        /// <summary>The maximum speed in units per second.</summary>
        [Export(PropertyHint.Range, "0,100,0.1")]
        public float MaxSpeed { get; set; } = 10.0f;

        /// <summary>The rate at which the character's speed builds toward MaxSpeed.</summary>
        [Export(PropertyHint.Range, "0,200,0.1")]
        public float Acceleration { get; set; } = 80.0f;

        /// <summary>The rate at which the character slows down when there is no input.</summary>
        [Export(PropertyHint.Range, "0,200,0.1")]
        public float Friction { get; set; } = 50.0f;

        /// <summary>
        /// A multiplier applied to friction when braking (e.g., inputting the opposite direction).
        /// A value > 1 provides more responsive, "tighter" stops.
        /// </summary>
        [Export(PropertyHint.Range, "1.0,10.0,0.1")]
        public float BrakingMultiplier { get; set; } = 2.0f;

        //NOTE: could consider adding more in the future, such as: turn speed curve, etc.
    }
}