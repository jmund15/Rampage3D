
using Godot;

/// <summary>
/// A resource that defines a steering behavior (attraction/repulsion) for a specific DetectableType.
/// Used by the AISteeringComponent to build a context-aware steering map.
/// </summary>
[GlobalClass]
public partial class AISteeringBehaviorResource_gemini : Resource
{
    [Export]
    public DetectableType TargetType { get; set; }

    [Export(PropertyHint.Range, "-10.0, 10.0, 0.1")]
    public float Weight { get; set; } = -1.0f; // Negative for repulsion (default), positive for attraction

    [Export(PropertyHint.Range, "0.0, 50.0, 0.5")]
    public float EffectiveRadius { get; set; } = 15.0f;

    [Export]
    public Curve WeightFalloff { get; set; } // A curve to define how weight changes with distance (0 = at radius, 1 = at agent)
}
