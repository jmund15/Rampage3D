// OverrideCalculationStrategy.cs
using Jmo.Core.Modifiers.CalculationStrategy;
using System.Collections.Generic;
using System.Linq;

namespace Jmo.Core.Modifiers.CalculationStrategy;

public class OverrideCalculationStrategy<T> : ICalculationStrategy<T>
{
    public T Calculate(T baseValue, List<IModifier<T>> modifiers)
    {
        // For non-numeric types, we assume the highest priority modifier simply overrides the value.
        // The list is already sorted by priority.
        if (modifiers.Count > 0)
        {
            // The 'Modify' method for an override modifier should just return its own value, ignoring input.
            return modifiers[0].Modify(baseValue);
        }
        return baseValue;
    }
}