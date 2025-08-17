using Godot;
using Jmo.AI.Perception.Strategies;

namespace Jmo.Core
{
    // In "Jmo/Core/IdentifiableComponent.cs"
    // This component is now much simpler.
    [GlobalClass]
    public partial class IdentifiableComponent : Node, IIdentifiable
    {
        // A designer drags "Rubble.tres" or "TriggerZone.tres" here.
        [Export] public Identity SimpleIdentity { get; private set; }

        public Array<Category> GetCategories() => SimpleIdentity?.Categories;
        public Resource GetIdentityResource() => SimpleIdentity;
    }

    // In "Jmo/Items/ItemComponent.cs"
    // This component now implements the interface directly.
    [GlobalClass]
    public partial class ItemComponent : Node, IIdentifiable
    {
        // A designer drags "HealthPotion_ItemData.tres" here.
        [Export] public ItemData Data { get; private set; }

        public Array<Category> GetCategories() => Data?.Categories;
        public Resource GetIdentityResource() => Data;
    }
}