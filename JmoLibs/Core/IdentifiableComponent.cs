using Godot;
using Jmo.AI.Perception.Strategies;

namespace Jmo.Core
{
    /// <summary>
    /// A crucial component that should be attached to any Node that needs to be semantically identifiable
    /// within the game world. It acts as the bridge between a physical object and the game engine's
    /// abstract understanding of that object, providing essential semantic data for any system to query.
    /// </summary>
    [GlobalClass]
    public partial class IdentifiableComponent : Node
    {
        /// <summary>
        /// The primary Identity of this object (e.g., "HealthPotion.tres"). This is the core piece of
        /// information systems like AI, UI, or Inventory will use to make decisions.
        /// </summary>
        [Export] public Identity Identity { get; private set; }

        /// <summary>
        /// Optional. When exported as a Resource that implements IMemoryDecayStrategy, this will override a sensor's
        /// default decay logic for any AI memory of this object. Useful for creating important objects whose memory
        /// should not fade, or fleeting ones that should be forgotten quickly.
        /// </summary>
        [Export] public Resource OverrideDecayStrategy { get; private set; }
    }
}