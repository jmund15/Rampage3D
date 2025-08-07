using Godot;

namespace Jmo.Core.Input
{
    /// <summary>
    /// A data-driven Resource that represents a unique, abstract input action.
    /// This allows systems to query for intent (e.g., "Jump", "Move", "Fire")
    /// in a standardized way, without being coupled to physical buttons or AI logic.
    /// </summary>
    [GlobalClass]
    public partial class InputAction : Resource
    {
        /// <summary>The name of the action for debugging and editor purposes.</summary>
        [Export] public string ActionName { get; private set; } = "Unnamed Action";
    }
}