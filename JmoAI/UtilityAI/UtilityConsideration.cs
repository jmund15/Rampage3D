// --- UtilityConsideration.cs ---
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace JmoAI.UtilityAI
{
    [GlobalClass, Tool]
    public abstract partial class UtilityConsideration : Resource
    {
        [Export]
        private Godot.Collections.Array<ConsiderationModifier> _modifiers;

        //private List<IConsiderationModifier> _typedModifiers;

        /// <summary>
        /// Evaluates the final score of this consideration after applying all modifiers.
        /// This is the primary method called by the UtilitySelector.
        /// </summary>
        /// <param name="blackboard">The agent's blackboard.</param>
        /// <returns>The final, modified utility score, clamped between 0 and 1.</returns>
        public float Evaluate(IBlackboard blackboard)
        {
            // // Lazy load the typed modifiers once.
            // if (_typedModifiers == null)
            // {
            //     _typedModifiers = _modifiers?.OfType<IConsiderationModifier>().ToList() ?? new List<IConsiderationModifier>();
            // }

            // 1. Calculate the objective, raw score.
            float baseScore = CalculateBaseScore(blackboard);

            // 2. Apply each subjective modifier in the stack.
            foreach(var modifier in _modifiers)
            {
                baseScore = modifier.Modify(baseScore, blackboard);
            }

            // 3. Return the final, clamped score.
            return Mathf.Clamp(baseScore, 0f, 1f);
        }

        /// <summary>
        /// Child classes must implement this method to provide the raw, objective
        /// utility score before any modifiers are applied.
        /// </summary>
        /// <param name="blackboard">The agent's blackboard.</param>
        /// <returns>An unclamped base utility score.</returns>
        protected abstract float CalculateBaseScore(IBlackboard blackboard);
    }
}