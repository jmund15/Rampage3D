using Godot;
using Godot.Collections;

using Jmo.Gameplay.Stats;

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
        [ExportGroup("Universal Attributes")]
        [Export] public Dictionary<Attribute, Variant> UniversalAttributes { get; private set; } = new();

        /// <summary>
        /// A dictionary mapping the character's available movement modes to their physics profiles.
        /// </summary>
        [ExportGroup("Contextual Movement")]
        [Export] public Dictionary<MovementMode, VelocityProfile> MovementProfiles { get; private set; } = new();

        /// <summary>
        /// A list of all impulses (like jumps or dashes) this character is capable of performing.
        /// A State Machine will query this list to find the data for a specific mechanic.
        /// </summary>
        [ExportGroup("Mechanics")]
        [Export] public Dictionary<MechanicType, MechanicData> ImpulseLibrary { get; private set; } = new();
    }
}