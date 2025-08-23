using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Jmo.AI.Core;
/// <summary>
/// The AI's low-level "brain" responsible for moment-to-moment steering. It synthesizes a
/// high-level goal (from a Behavior Tree) with a set of environmental considerations, weighted
/// by an AIPersonality, to produce a final, desired movement direction.
/// </summary>
[GlobalClass]
public partial class AISteeringProcessor : Node
{
    /// <summary>The list of all possible environmental factors this AI can think about.</summary>
    [Export] private Array<BaseAIConsideration3D> _considerations = new();

    /// <summary>A list of vectors representing the directions the AI can choose to move in.</summary>
    [Export] public Array<Vector3> MovementDirections { get; private set; } = new();

    private AIPersonality _personality;
    private Dictionary<Vector3, float> _scores;

    public void Initialize(AIPersonality personality)
    {
        _personality = personality;
        _scores = MovementDirections.ToDictionary(dir => dir, dir => 0f);
    }

    /// <summary>
    /// The main calculation method. It evaluates all considerations and combines them
    /// into a single, optimal steering vector.
    /// </summary>
    public Vector3 CalculateSteering(DecisionContext context)
    {
        // Reset scores for this frame
        foreach (var key in _scores.Keys.ToList()) _scores[key] = 0f;

        // --- 1. Score High-Level Goal ---
        // Add a base "desire" to move towards the BT's target.
        if (context.HighLevelTarget != context.AgentPosition)
        {
            Vector3 toTarget = (context.HighLevelTarget - context.AgentPosition).Normalized();
            foreach (var dir in MovementDirections)
            {
                float dot = dir.Dot(toTarget);
                if (dot > 0) _scores[dir] += dot; // Simple linear scoring
            }
        }

        // --- 2. Score Environmental Considerations ---
        foreach (var consideration in _considerations)
        {
            // Check if the personality has a weight for this consideration.
            if (_personality.ConsiderationWeights.TryGetValue(consideration, out float weight))
            {
                // A weight of zero means this personality ignores this consideration.
                if (Mathf.IsZeroApprox(weight)) continue;

                // Evaluate the consideration and apply the personality's weight to the scores.
                consideration.Evaluate(context, ref _scores, weight);
            }
        }

        // --- 3. Choose the Best Direction ---
        // (Could be highest score, a weighted random choice, or vector summation)
        Vector3 finalDirection = Vector3.Zero;
        foreach (var score in _scores)
        {
            // Clamp negative scores to zero so "danger" cancels out "interest" but doesn't create a desire to move away.
            // Avoidance is simply the absence of interest in a direction.
            finalDirection += score.Key * Mathf.Max(0, score.Value);
        }

        return finalDirection.IsNormalized() ? finalDirection : finalDirection.Normalized();
    }
}