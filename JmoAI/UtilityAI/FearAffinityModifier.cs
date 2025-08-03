// --- FearAffinityModifier.cs ---
using Godot;
using JmoAI.UtilityAI;

[GlobalClass]
public partial class FearAffinityModifier : ConsiderationModifier
{
    [Export(PropertyHint.Range, "0.1, 2.0, 0.05")]
    private float _fearMultiplier = 1.5f; // How much fear affects the score.

    public override float Modify(float baseScore, IBlackboard blackboard)
    {
        var affinities = blackboard.GetVar<AIAffinitiesComponent>(BBDataSig.Affinities);
        if (affinities == null)
        {
            // If the AI has no affinities component, don't modify the score.
            return baseScore;
        }

        // The logic: High fear amplifies the base score.
        // A base threat score of 0.5 with Fear of 1.0 becomes 0.5 * (1 + 1.0 * 1.5) = 1.25
        float modifiedScore = baseScore * (1 + (affinities.Fear * _fearMultiplier));

        return modifiedScore;
    }
}