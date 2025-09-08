using Godot;
using Godot.Collections;
using Jmo.Core.Modifiers.CalculationStrategy;
using Jmo.Gameplay.Stats;

namespace Jmo.Gameplay.Stats
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
        // The dictionary holds the raw base value.
        [Export] public Dictionary<Attribute, Variant> UniversalAttributes { get; private set; } = new();

        // a separate dictionary to explicitly assign strategies.
        // This is more verbose but architecturally 100% correct and flexible.
        [Export] public Dictionary<Attribute, VariantDefaultCalculationStrategy> UniversalAttributeStrategies { get; private set; } = new();

        /// <summary>
        /// A dictionary mapping the character's available movement modes to their physics profiles.
        /// </summary>
        [ExportGroup("Contextual Movement")]
        [Export] public Dictionary<MovementMode, VelocityProfile> MovementProfiles { get; private set; } = new();

        
        /// <summary>
        /// A list of all mechanics (like jumps or dashes) this character is capable of performing.
        /// A State Machine will query this list to find the data for a specific mechanic.
        /// </summary>
        [ExportGroup("Mechanics")]
        [Export] public Dictionary<MechanicType, MechanicData> MechanicLibrary { get; private set; } = new();
    }
}