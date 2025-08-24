using Godot;
using Jmo.AI.Affinities;
using Jmo.AI.Navigation;
using Jmo.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Jmo.AI.Affinities;

/// <summary>
/// A powerful steering modifier that scales directional scores based on an AI's affinity.
/// It uses a Curve resource to translate an affinity value into a multiplier, allowing designers
/// to visually sculpt how personality traits affect movement choices.
/// </summary>
[GlobalClass]
public partial class AffinitySteeringModifier : SteeringConsiderationModifier
{
    [Export] private Affinity _affinityToMeasure;
    [Export] private Curve _responseCurve;

    public override void Modify(ref Dictionary<Vector3, float> scores, DecisionContext context, IBlackboard blackboard, Node owner)
    {
        // --- Configuration Validation ---
        if (_affinityToMeasure == null || _responseCurve == null)
        {
            Logger.Error(this, owner, "Modifier is misconfigured. Either 'Affinity To Measure' or 'Response Curve' is not set. It will be skipped.");
            return;
        }

        var affinities = blackboard.GetVar<AIAffinitiesComponent>(BBDataSig.Affinities);
        if (affinities == null) return; // Agent will have logged this critical error already.

        // --- Core Logic ---
        if (!affinities.TryGetAffinity(_affinityToMeasure, out float affinityValue))
        {
            // This is a recoverable state; the AI just doesn't have this personality trait.
            Logger.Warning(
                            this,
                            blackboard.GetVar<Node>(BBDataSig.Agent),
                            "AffinitySteeringModifier could not find the affinity '{0}' in the AIAffinitiesComponent. It will be skipped.",
                            _affinityToMeasure.AffinityName);
            return;
        }

        float multiplier = _responseCurve.SampleBaked(affinityValue);

        foreach (var key in scores.Keys.ToList())
        {
            scores[key] *= multiplier;
        }
    }
}