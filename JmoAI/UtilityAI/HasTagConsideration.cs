// --- HasTagConsideration.cs ---
using Godot;
using JmoAI.UtilityAI;

[GlobalClass]
public partial class HasTagConsideration : UtilityConsideration
{
    // A designer drags a StringName resource here, or just types the string.
    [Export]
    private StringName _requiredTag;
    
    protected override float CalculateBaseScore(IBlackboard blackboard)
    {
        // Because of our "bubble-up" GetVar logic, this will seamlessly check
        // the local blackboard first, and then the squad's blackboard (its parent)
        // without this consideration needing to know the difference.
        //StringName activeTag = blackboard.GetVar<StringName>(BBDataSig.ActiveSquadTag);

        //if (activeTag == _requiredTag)
        //{
        //    return 1.0f; // Perfect match!
        //}

        return 0.0f; // No match.
    }
}