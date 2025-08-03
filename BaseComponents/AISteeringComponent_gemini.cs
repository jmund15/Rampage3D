
using Godot;
using System.Collections.Generic;

/// <summary>
/// The "driver" component for an AI agent.
/// It takes the ideal path direction from the AINav3DComponent (the "GPS"),
/// combines it with environmental data from the AISensorsComponent,
/// and produces a final, context-aware desired velocity.
/// </summary>
[GlobalClass]
public partial class AISteeringComponent_gemini : Node
{
    [Export]
    public Node3D ParentAgent { get; private set; }

    [Export]
    public AINav3DComponent_gemini Navigation { get; private set; }

    [Export]
    public AISensors_gemini Sensors { get; private set; }

    [Export(PropertyHint.Range, "0.0, 1.0, 0.05")]
    public float PathFollowingStrength { get; set; } = 0.8f;

    [Export(PropertyHint.Range, "0.0, 1.0, 0.05")]
    public float ContextMapStrength { get; set; } = 0.5f;

    [Export]
    public Godot.Collections.Array<AISteeringBehaviorResource_gemini> SteeringBehaviors { get; private set; }

    public Vector3 DesiredVelocity { get; private set; } = Vector3.Zero;

    private Dictionary<DetectableType, AISteeringBehaviorResource_gemini> _steeringBehaviorMap;

    public override void _Ready()
    {
        if (ParentAgent == null) ParentAgent = GetParent<Node3D>();
        if (Navigation == null || Sensors == null)
        {
            GD.PrintErr($"{ParentAgent.Name} AISteeringComponent_gemini ERROR: Navigation or Sensors component is not assigned!");
            SetPhysicsProcess(false);
            return;
        }

        // Build a dictionary for quick lookup of steering behaviors by DetectableType
        _steeringBehaviorMap = new Dictionary<DetectableType, AISteeringBehaviorResource_gemini>();
        if (SteeringBehaviors != null)
        {
            foreach (var behaviorResource in SteeringBehaviors)
            {
                if (!_steeringBehaviorMap.ContainsKey(behaviorResource.TargetType))
                {
                    _steeringBehaviorMap.Add(behaviorResource.TargetType, behaviorResource);
                }
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 idealDirection = Navigation.GetIdealDirection().Normalized();
        Vector3 contextDirection = BuildContextMap().Normalized();

        // Blend the two directions based on their respective strengths
        Vector3 blendedDirection = idealDirection.Lerp(contextDirection, ContextMapStrength).Normalized();

        // Final desired velocity is a combination of the path direction and the blended context-aware direction
        DesiredVelocity = idealDirection.Lerp(blendedDirection, PathFollowingStrength).Normalized();

        // Optional: Add debug visualization
        // DebugDraw3D.DrawArrow(ParentAgent.GlobalPosition, ParentAgent.GlobalPosition + DesiredVelocity * 2, Colors.Cyan);
    }

    /// <summary>
    /// Builds a "context map" vector by evaluating all detected objects.
    /// This vector represents the combined influence of all environmental factors (dangers, interests).
    /// </summary>
    /// <returns>A vector representing the desired direction based on context.</returns>
    private Vector3 BuildContextMap()
    {
        Vector3 contextVector = Vector3.Zero;

        foreach (var ray in Sensors.GetCollidingRays())
        {
            if (ray.GetCollider() is Node3D colliderNode)
            {
                contextVector += EvaluateCollider(colliderNode, ray.GetCollisionPoint());
            }
        }

        // You could also iterate through Area3D detections here if needed

        return contextVector;
    }

    /// <summary>
    /// Evaluates a single detected collider and returns an influence vector.
    /// </summary>
    private Vector3 EvaluateCollider(Node3D colliderNode, Vector3 collisionPoint)
    {
        Vector3 influenceVector = Vector3.Zero;

        var detectable = colliderNode.GetNode<DetectableComponent_gemini>("DetectableComponent_gemini");
        if (detectable == null) return influenceVector; // Not a detectable entity

        if (_steeringBehaviorMap.TryGetValue(detectable.Type, out AISteeringBehaviorResource_gemini behavior))
        {
            float distance = ParentAgent.GlobalPosition.DistanceTo(collisionPoint);
            if (distance < behavior.EffectiveRadius)
            {
                float weight = behavior.Weight;

                // Apply distance falloff if a curve is defined
                if (behavior.WeightFalloff != null)
                {
                    float distanceRatio = Mathf.Clamp(distance / behavior.EffectiveRadius, 0.0f, 1.0f);
                    weight *= behavior.WeightFalloff.Sample(distanceRatio);
                }

                // Vector from agent to the collision point, weighted
                // The sign of the weight determines attraction (positive) or repulsion (negative)
                Vector3 directionToCollider = ParentAgent.GlobalPosition.DirectionTo(collisionPoint);
                influenceVector += -directionToCollider * weight;
            }
        }

        return influenceVector;
    }
}
