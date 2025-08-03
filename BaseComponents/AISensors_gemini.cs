
using Godot;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A centralized component for managing an AI agent's sensors.
/// It holds references to ray and area detectors, provides methods to query
/// environmental data, and emits signals for detection events.
/// </summary>
[GlobalClass]
public partial class AISensors_gemini : Node
{
    // --- Signals ---
    [Signal]
    public delegate void BodyDetectedEventHandler(Node3D body);
    [Signal]
    public delegate void BodyExitedEventHandler(Node3D body);
    [Signal]
    public delegate void AreaDetectedEventHandler(Area3D area);
    [Signal]
    public delegate void AreaExitedEventHandler(Area3D area);

    [Signal]
    public delegate void DetectableEnteredEventHandler(DetectableComponent_gemini detectable);
    [Signal]
    public delegate void DetectableExitedEventHandler(DetectableComponent_gemini detectable);


    // --- Exports ---
    [Export]
    public AIRayDetector3D RayDetector { get; private set; }

    [Export]
    public AIAreaDetector3D AreaDetector { get; private set; }

    private List<DetectableComponent_gemini> _detectedComponents = new List<DetectableComponent_gemini>();


    // --- Godot Methods ---
    public override void _Ready()
    {
        if (RayDetector == null)
        {
            GD.PrintErr($"{GetParent().Name} AISensors_gemini ERROR: AIRayDetector3D is not assigned!");
        }

        if (AreaDetector == null)
        {
            GD.PrintErr($"{GetParent().Name} AISensors_gemini ERROR: AIAreaDetector3D is not assigned!");
        }
        else
        {
            // Connect to the internal detector's signals to forward them.
            AreaDetector.BodyEntered += OnBodyEntered;
            AreaDetector.BodyExited += OnBodyExited;
            AreaDetector.AreaEntered += OnAreaEntered;
            AreaDetector.AreaExited += OnAreaExited;
        }
    }


    // --- Public Methods ---

    /// <summary>
    /// Actively searches for the first detected entity of a specific type.
    /// </summary>
    /// <param name="type">The type of entity to find.</param>
    /// <returns>The DetectableComponent of the found entity, or null if none is found.</returns>
    public DetectableComponent_gemini FindDetectedEntity(DetectableType type)
    {
        return _detectedComponents.FirstOrDefault(c => c.Type == type);
    }

    /// <summary>
    /// Gets all currently colliding raycasts from the RayDetector. Best for polling.
    /// </summary>
    /// <returns>An enumerable of all active raycast collisions.</returns>
    public IEnumerable<RayCast3D> GetCollidingRays()
    {
        if (RayDetector == null) yield break;

        foreach (var ray in RayDetector.GetAllRaycasts())
        {
            if (ray.IsColliding())
            {
                yield return ray;
            }
        }
    }

    /// <summary>
    /// Gets all bodies currently inside the AreaDetector.
    /// </summary>
    /// <returns>A list of overlapping bodies.</returns>
    public Godot.Collections.Array<Node3D> GetDetectedBodies()
    {
        return AreaDetector?.GetOverlappingBodies() ?? new Godot.Collections.Array<Node3D>();
    }

    /// <summary>
    /// Gets all areas currently inside the AreaDetector.
    /// </summary>
    /// <returns>A list of overlapping areas.</returns>
    public Godot.Collections.Array<Area3D> GetDetectedAreas()
    {
        return AreaDetector?.GetOverlappingAreas() ?? new Godot.Collections.Array<Area3D>();
    }


    // --- Signal Handlers ---

    private void OnBodyEntered(Node3D body)
    {
        EmitSignal(SignalName.BodyDetected, body);

        var detectable = body.GetNode<DetectableComponent_gemini>("DetectableComponent_gemini");
        if (detectable != null)
        {
            if (!_detectedComponents.Contains(detectable))
            {
                _detectedComponents.Add(detectable);
                EmitSignal(SignalName.DetectableEntered, detectable);
            }
        }
    }

    private void OnBodyExited(Node3D body)
    {
        EmitSignal(SignalName.BodyExited, body);

        var detectable = body.GetNode<DetectableComponent_gemini>("DetectableComponent_gemini");
        if (detectable != null)
        {
            if (_detectedComponents.Contains(detectable))
            {
                _detectedComponents.Remove(detectable);
                EmitSignal(SignalName.DetectableExited, detectable);
            }
        }
    }

    private void OnAreaEntered(Area3D area)
    {
        EmitSignal(SignalName.AreaDetected, area);
    }

    private void OnAreaExited(Area3D area)
    {
        EmitSignal(SignalName.AreaExited, area);
    }
}
