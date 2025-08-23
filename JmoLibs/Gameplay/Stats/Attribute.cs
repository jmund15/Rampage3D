using Godot;
using Jmo.Core.Modifiers.CalculationStrategy;

namespace Jmo.Gameplay.Stats
{
    /// <summary>
    /// A data-driven "tag" Resource that represents a specific character statistic
    /// (e.g., "MaxSpeed", "JumpHeight", "AirControl"). It is used as a type-safe key
    /// in dictionaries, replacing brittle enums or strings and allowing designers
    /// to define new stats without changing code.
    /// </summary>
    [GlobalClass]
    public partial class Attribute : Resource
    {
        [Export] public string AttributeName { get; private set; } = "Unnamed Attribute";
    }

    [GlobalClass]
    public partial class AttributeValue : Resource
    {
        [Export] public Variant Value { get; private set; } = 0f;
        [Export] public VariantDefaultCalculationStrategy CalculationStrategy { get; private set; } = new();
    }
}
