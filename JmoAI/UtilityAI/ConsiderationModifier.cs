// --- ConsiderationModifier.cs ---
using Godot;

namespace JmoAI.UtilityAI
{
    [GlobalClass]
    public abstract partial class ConsiderationModifier : Resource
    {
        public abstract float Modify(float baseScore, IBlackboard blackboard);
    }
}