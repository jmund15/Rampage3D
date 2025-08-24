using Godot;
using Jmo.AI.Navigation;
using System.Collections.Generic;

namespace Jmo.AI.Affinities;

/// <summary>
/// An abstract resource that modifies the directional scores produced by a steering consideration.
/// This allows an AI's personality (Affinities) to influence its low-level movement behavior.
/// </summary>
[GlobalClass]
public abstract partial class SteeringConsiderationModifier : Resource
{
    /// <summary>
    /// Modifies the dictionary of steering scores.
    /// </summary>
    /// <param name="scores">The current dictionary of directional scores to be modified.</param>
    /// <param name="context">The per-frame snapshot of the AI's state and world view.</param>
    /// <param name="blackboard">The AI's blackboard for accessing core components.</param>
    /// <param name="owner">The Node using this resource, for logging context.</param>
    public abstract void Modify(ref Dictionary<Vector3, float> scores, DecisionContext context, IBlackboard blackboard, Node owner);
}