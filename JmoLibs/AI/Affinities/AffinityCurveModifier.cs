// --- This ONE class replaces all previous modifiers like FearAffinityModifier.cs ---
using Godot;
using JmoAI.UtilityAI;

namespace Jmo.AI.Affinities;

/// <summary>
/// A powerful, data-driven modifier that shapes a utility score based on an AI's
/// affinity. It uses a designer-friendly Curve resource to translate an affinity
/// value (e.g., Fear) into a score multiplier, allowing for complex, non-linear
/// behavioral tuning.
/// </summary>
[GlobalClass]
public partial class AffinityCurveModifier : ConsiderationModifier
{
    /// <summary>
    /// The specific affinity from the AIAffinitiesComponent to read from.
    /// </summary>
    [Export] private Affinity _affinityToMeasure;

    /// <summary>
    /// The Curve resource that defines the response to the affinity.
    /// X-axis: Affinity Value (0 to 1)
    /// Y-axis: Score Multiplier
    /// </summary>
    [Export] private Curve _responseCurve;

    public override float Modify(float baseScore, IBlackboard blackboard)
    {
        // A missing curve is a configuration error, so we fail gracefully and log it.
        if (_responseCurve == null)
        {
            GD.PrintErr($"AffinityCurveModifier is missing a ResponseCurve resource.");
            return baseScore;
        }

        // Get the affinities component from the blackboard.
        var affinities = blackboard.GetVar<AIAffinitiesComponent>(BBDataSig.Affinities);
        if (affinities == null)
        {
            // If this AI has no affinities, there's nothing to measure, so we don't modify.
            return baseScore;
        }

        // 1. Get the current value of the chosen affinity (e.g., Fear = 0.8).
        float affinityValue = affinities.GetAffinity(_affinityToMeasure)!.Value;

        // 2. Sample the curve at that value to get the multiplier.
        //    The curve's X-axis should be designed to be read from 0 to 1.
        float multiplier = _responseCurve.SampleBaked(affinityValue);

        // 3. Apply the multiplier to the base score.
        return baseScore * multiplier;
    }
}