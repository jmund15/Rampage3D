using Godot;
using Godot.Collections;
using Jmo.AI.Affinities;

/// <summary>
/// The abstract base class for any environmental consideration. Its sole purpose is to
/// evaluate the current world state (via the DecisionContext) and modify a dictionary of
/// directional scores to reflect the interest or danger it perceives.
/// </summary>
[GlobalClass]
public abstract partial class BaseAIConsideration3D : Resource
{
    // The base "Consideration" float is no longer needed here, as it's defined in the AIPersonality.

    /// <summary>
    /// Evaluates the current situation and modifies the directional scores.
    /// </summary>
    /// <param name="context">A snapshot of the current world and agent state.</param>
    /// <param name="scores">The dictionary of directional scores to be modified.</param>
    /// <param name="personalityWeight">The weight multiplier from the AI's Personality.</param>
    public abstract void Evaluate(DecisionContext context, ref Dictionary<Vector3, float> scores, float personalityWeight);


    [Export] private Array<SteeringConsiderationModifier> _modifiers = new();

    /// <summary>
    /// The primary evaluation method. It calculates the base scores and then allows
    /// all registered modifiers to alter them.
    /// </summary>
    public void Evaluate(DecisionContext context, IBlackboard blackboard, ref Dictionary<Vector3, float> finalScores)
    {
        // 1. Calculate the raw, objective scores for this consideration.
        var baseScores = CalculateBaseScores(context, blackboard);

        // 2. Apply all subjective modifiers to the base scores.
        if (_modifiers != null)
        {
            foreach (var modifier in _modifiers)
            {
                modifier.Modify(ref baseScores, context, blackboard);
            }
        }

        // 3. Add the final, modified scores to the processor's master score dictionary.
        foreach (var score in baseScores)
        {
            finalScores[score.Key] += score.Value;
        }
    }

    /// <summary>
    /// Child classes must implement this to provide the raw directional scores
    /// before any personality-driven modifications are applied.
    /// </summary>
    protected abstract Dictionary<Vector3, float> CalculateBaseScores(DecisionContext context, IBlackboard blackboard);
}