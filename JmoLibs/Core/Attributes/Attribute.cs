using Godot;

namespace Jmo.Core.Attributes
{
    /// <summary>
    /// A data-driven Resource that provides a unique, type-safe identifier for any
    /// modifiable stat or property in the game(e.g., "MaxSpeed", "MaxHealth").
    /// This is the definitive replacement for brittle "magic string" or rigid enum-based
    /// stat systems.It allows designers to define new stats without code changes.
    /// </summary>
    [GlobalClass]
    public partial class Attribute : Resource
    {
        [Export] public string AttributeName { get; private set; } = "Unnamed Attribute";
    }
}