
using Godot;

/// <summary>
/// Defines the specific, high-level categories of entities that AI systems can detect and identify.
/// This provides a more abstract and modular way to handle AI perception.
/// </summary>
public enum DetectableType
{
    Monster,
    Critter,
    Vehicle,
    Military,
    Civilian,
    Building,
    SceneryObject, // e.g., a large rock or tree
    FoodSource
}
