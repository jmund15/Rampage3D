using Godot;

namespace Jmo.Core.Movement
{
    /// <summary>
    /// A data-driven Resource that defines a mechanic (fromally one-time physics impulse). It contains the raw
    /// physics data for an action like a jump or a dash. The link to what *triggers* this
    /// impulse is defined contextually by the character's State Machine.
    /// </summary>
    [GlobalClass]
    public partial class MechanicData : Resource
    {
        /// <summary>
        /// Name for UI and debugging purposes.
        /// </summary>
        [Export] public string ImpulseName { get; private set; } = "Unnamed Impulse";
        [Export] public float Strength { get; private set; } = 15.0f;
    }
}