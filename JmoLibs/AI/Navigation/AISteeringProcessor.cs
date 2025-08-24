using Godot;
using Godot.Collections;
using Jmo.AI.Navigation;
using Jmo.Shared;
using SysCol =  System.Collections.Generic;
using System.Linq;

namespace Jmo.AI.Navigation;

/// <summary>
/// The AI's low-level "brain" responsible for moment-to-moment steering. It synthesizes a
/// high-level goal (from a Behavior Tree) with a set of environmental considerations,
/// to produce a final, desired movement direction.
/// </summary>
[Tool]
[GlobalClass]
public partial class AISteeringProcessor : Node
{
    [ExportGroup("Configuration")]
    [Export] private Array<BaseAIConsideration3D> _considerations = new();

    /// <summary>
    /// A list of normalized vectors representing the directions the AI can choose to move in (e.g., 8 or 16 directions).
    /// </summary>
    [Export] public Array<Vector3> MovementDirections { get; private set; } = new();

    private SysCol.Dictionary<Vector3, float> _scores;

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();
        if (_considerations.Count == 0) warnings.Add("No considerations are assigned. The AI will have no environmental awareness.");
        if (MovementDirections.Count == 0) warnings.Add("No movement directions are defined. The AI will not know how to score potential moves.");
        return warnings.ToArray();
    }

    /// <summary>
    /// Initializes the steering processor. Must be called by the parent AIAgent.
    /// </summary>
    public void Initialize()
    {
        if (MovementDirections == null || MovementDirections.Count == 0)
        {
            Logger.Error(this, "MovementDirections array is null or empty. The steering processor cannot function.");
            return;
        }
        _scores = MovementDirections.ToDictionary(dir => dir, dir => 0f);
    }

    /// <summary>
    /// The main calculation method. It evaluates all considerations and combines them
    /// into a single, optimal steering vector.
    /// </summary>
    public Vector3 CalculateSteering(DecisionContext context, IBlackboard blackboard)
    {
        if (_scores == null) return Vector3.Zero; // Not initialized.

        // --- 1. Reset scores for this frame's calculation ---
        foreach (var key in _scores.Keys.ToList()) _scores[key] = 0f;

        // --- 2. Score the High-Level Goal ---
        if (context.HighLevelTarget != context.AgentPosition)
        {
            Vector3 toTarget = (context.HighLevelTarget - context.AgentPosition).Normalized();
            foreach (var dir in MovementDirections)
            {
                float dot = dir.Dot(toTarget);
                // Score is higher for directions that align with the target direction.
                if (dot > 0) _scores[dir] += dot;
            }
        }

        // --- 3. Score Environmental Considerations ---
        foreach (var consideration in _considerations)
        {
            if (consideration == null) continue;
            consideration.Evaluate(context, blackboard, ref _scores);
        }

        // --- 4. Choose the Best Direction ---
        Vector3 finalDirection = Vector3.Zero;
        foreach (var score in _scores)
        {
            // A direction's final score is clamped at 0. Negative scores (danger) cancel out
            // interest, but do not create a "desire" to move in the opposite direction.
            // Avoidance is simply the absence of interest in a given direction.
            finalDirection += score.Key * Mathf.Max(0, score.Value);
        }

        return finalDirection.IsZeroApprox() ? Vector3.Zero : finalDirection.Normalized();
    }
}