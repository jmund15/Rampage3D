using Godot;
using Godot.Collections;
using Jmo.AI.Affinities;
using Jmo.Core.World;
using SysCol = System.Collections.Generic;

namespace Jmo.AI.Navigation;

/// <summary>
/// The path that defines any weight or modifiers for the base navigation path to the target.
/// This is typically optional, but if you want certain entities to regard the path strictly or less of a priority based on certain conditions, use this.
/// </summary>
[GlobalClass]
public  partial class NavigationPathConsideration : BaseAIConsideration3D
{
    [Export(PropertyHint.Range, "0,10,0.1,or_greater")] 
    private float _baseWeight = 1f;
    public NavigationPathConsideration() { }

    /// <summary>
    /// Child classes must implement this to provide the raw directional scores
    /// before any personality-driven modifications are applied.
    /// </summary>
    protected override SysCol.Dictionary<Vector3, float> CalculateBaseScores(DirectionSet3D directions, DecisionContext context, IBlackboard blackboard)
    {
        throw new System.NotImplementedException();
    }
}