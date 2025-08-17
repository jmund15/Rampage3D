using Godot;

namespace Jmo.Core
{
    /// <summary>
    /// A central, project-wide Resource that acts as a manifest for all core game-defining
    /// semantic assets. It provides a "source of truth" for the framework, allowing programmers
    /// to get type-safe, non-brittle references to fundamental Categories and InputActions.
    /// </summary>
    [GlobalClass]
    public partial class GameRegistry : Resource
    {
        [ExportGroup("Core Semantic Categories")]
        [Export] public Category EnemyCategory { get; private set; }
        [Export] public Category FriendlyCategory { get; private set; }
        [Export] public Category ItemCategory { get; private set; }
        [Export] public Category ObjectiveCategory { get; private set; }
        [Export] public Category PlayerFactionCategory { get; private set; }

        [ExportGroup("Unique Core Identities")]
        [Export] public Identity PlayerIdentity { get; private set; }

        [ExportGroup("Core Input Actions")]
        [Export] public Input.InputAction MoveAction { get; private set; }
        [Export] public Input.InputAction JumpAction { get; private set; }
    }
}
