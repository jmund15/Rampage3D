using Godot;

namespace Jmo.Core
{
    /// <summary>
    /// A data-driven Resource representing a high-level, abstract category.
    /// This is a cornerstone of the world's semantic system, allowing for broad-level grouping.
    /// For example, this allows an AI to ask "is there an Enemy nearby?" and get a match for any
    /// object whose Identity belongs to the "Enemy" category.
    /// </summary>
    [GlobalClass]
    public partial class Category : Resource
    {
        /// <summary>
        /// The user-friendly name of the category for debugging and editor purposes (e.g., "Enemy", "Item", "Consumable").
        /// </summary>
        [Export] public string CategoryName { get; private set; } = "Unnamed Category";
    }
}