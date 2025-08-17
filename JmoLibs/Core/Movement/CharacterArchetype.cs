using Godot;
using Godot.Collections;

namespace Jmo.Core.Movement
{
    /// <summary>
    /// A comprehensive, data-driven "character sheet" that acts as a library of all available
    /// movement capabilities for an actor. It allows a State Machine/Behavior Tree to dynamically select the
    /// correct physics data for its current state and actions.
    /// </summary>
    [GlobalClass]
    public partial class CharacterArchetype : Resource
    {
        /// <summary>
        /// A dictionary mapping the character's available movement modes to their physics profiles.
        /// </summary>
        [Export] public Dictionary<MovementMode, VelocityProfile> MovementProfiles { get; private set; } = new();

        /// <summary>
        /// A list of all impulses (like jumps or dashes) this character is capable of performing.
        /// A State Machine will query this list to find the data for a specific mechanic.
        /// </summary>
        [Export] public Dictionary<MechanicType, ImpulseData> ImpulseLibrary { get; private set; } = new();
    }
}