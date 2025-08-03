// --- HealthConsideration.cs (MODIFIED EXAMPLE) ---
// A concrete example of a self-sourcing consideration.

using Godot;

namespace JmoAI.UtilityAI
{
    [GlobalClass, Tool]
    public partial class HealthConsideration : UtilityConsideration
    {
        // Now evaluates using the Blackboard, not a pre-built context.
        protected override float CalculateBaseScore(IBlackboard blackboard)
        {
            var healthComp = blackboard.GetVar<HealthComponent>(BBDataSig.HealthComp);

            // Always check if the component exists!
            if (healthComp == null)
            {
                GD.PrintErr("HealthConsideration: Could not find HealthComponent on Blackboard.");
                return 0f;
            }

            if (healthComp.MaxHealth <= 0) return 0f;

            // Return a score from 0.0 to 1.0 based on current health.
            return healthComp.Health / healthComp.MaxHealth;
        }
    }
}