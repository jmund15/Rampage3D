using Godot;

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
}