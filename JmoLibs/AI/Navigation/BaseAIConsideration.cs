using Godot;
using Godot.Collections;
using Jmo.AI.Affinities;
using System.Collections.Generic;

namespace Jmo.AI.Navigation;

/// <summary>
/// The abstract base class for any environmental consideration. Its purpose is to
/// evaluate the current world state (via the DecisionContext) and add its calculated
/// scores to the final directional score dictionary.
/// </summary>
[GlobalClass]
public abstract partial class BaseAIConsideration3D : Resource
{
    [Export] private Array<SteeringConsiderationModifier> _modifiers = new();

    /// <summary>
    /// The primary evaluation method. It calculates the base scores and then allows
    /// all registered modifiers to alter them before adding to the final scores.
    /// </summary>
    /// <param name="context">A snapshot of the current world and agent state.</param>
    /// <param name="blackboard">The AI's blackboard for accessing core components.</param>
    /// <param name="finalScores">The master score dictionary from the steering processor.</param>
    public void Evaluate(DecisionContext context, IBlackboard blackboard, ref Dictionary<Vector3, float> finalScores)
    {
        var agentNode = blackboard.GetVar<Node>(BBDataSig.Agent);

        // 1. Calculate the raw, objective scores for this consideration.
        var baseScores = CalculateBaseScores(context, blackboard);
        if (baseScores == null) return;

        // 2. Apply all subjective modifiers to the base scores.
        if (_modifiers != null)
        {
            foreach (var modifier in _modifiers)
            {
                if (modifier == null) continue;
                // Pass the agent node as the owner for better logging context.
                modifier.Modify(ref baseScores, context, blackboard, agentNode);
            }
        }

        // 3. Add the final, modified scores to the processor's master score dictionary.
        foreach (var score in baseScores)
        {
            if (finalScores.ContainsKey(score.Key))
            {
                finalScores[score.Key] += score.Value;
            }
        }
    }

    /// <summary>
    /// Child classes must implement this to provide the raw directional scores
    /// before any personality-driven modifications are applied.
    /// </summary>
    protected abstract Dictionary<Vector3, float> CalculateBaseScores(DecisionContext context, IBlackboard blackboard);
}