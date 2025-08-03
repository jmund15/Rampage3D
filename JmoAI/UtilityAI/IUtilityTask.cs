// --- IUtilityTask.cs ---
// This interface defines the contract for any BT action that can be used by the UtilitySelector.

namespace JmoAI.UtilityAI
{
    public interface IUtilityTask
    {
        // The consideration resource that calculates this action's score.
        public UtilityConsideration Consideration { get; }
        
        // Can this action be interrupted by a higher-scoring action?
        public bool Interruptible { get; }
        
        // Used by the tie-breaker. Higher values win in case of a score tie.
        public int Priority { get; }
    }
}