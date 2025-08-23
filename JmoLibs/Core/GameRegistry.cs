using Godot;
using Godot.Collections;
using Jmo.Core.IntentInput;

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
        /// <summary>
        /// All categories in the game, indexed by their CategoryName for fast lookup.
        /// This is automatically populated from the Categories array.
        /// </summary>
        private Dictionary<StringName, Category> _categoryLookup = new();

        [ExportGroup("New Collectors")]
        [Export] public Array<Category> Categories { get; private set; } = new();
        [Export] public Array<Identity> Identities { get; private set; } = new();
        [Export] public Array<InputAction> InputActions { get; private set; } = new();
        [ExportGroup("Core Semantic Categories")]
        [Export] public Category EnemyCategory { get; private set; }
        [Export] public Category FriendlyCategory { get; private set; }
        [Export] public Category ItemCategory { get; private set; }
        [Export] public Category ObjectiveCategory { get; private set; }
        [Export] public Category PlayerFactionCategory { get; private set; }

        [ExportGroup("Unique Core Identities")]
        [Export] public Identity PlayerIdentity { get; private set; }

        [ExportGroup("Core Input Actions")]
        [Export] public InputAction MoveAction { get; private set; }
        [Export] public InputAction JumpAction { get; private set; }

        /// <summary>
        /// Gets a category by its key. The lookup dictionary is built on first access.
        /// </summary>
        public Category? GetCategory(StringName categoryKey)
        {
            // This is the lazy-loading pattern. The dictionary is only built once,
            // the very first time a category is requested.
            if (_categoryLookup == null)
            {
                BuildLookup();
            }

            _categoryLookup!.TryGetValue(categoryKey, out var category);
            return category; // Returns the category or null if not found.
        }

        private void BuildLookup()
        {
            _categoryLookup = new Dictionary<StringName, Category>();
            if (Categories == null) return;

            foreach (var category in Categories)
            {
                if (category != null && !string.IsNullOrEmpty(category.CategoryName))
                {
                    // This prevents crashes if a designer makes a duplicate.
                    if (_categoryLookup.ContainsKey(category.CategoryName))
                    {
                        GD.PrintErr($"GameRegistry Error: Duplicate category name '{category.CategoryName}'. The first one found was kept.");
                        continue;
                    }
                    _categoryLookup[new StringName(category.CategoryName)] = category;
                }
            }
        }
    }
}
