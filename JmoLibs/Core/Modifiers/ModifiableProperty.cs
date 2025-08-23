using Godot;
using Jmo.Core.Modifiers.CalculationStrategy;
using System.Collections.Generic;
using System.Linq;

namespace Jmo.Core.Modifiers
{
    /// <summary>
    /// The generic wrapper class for any value that needs to be dynamically modified.
    /// It provides the core logic for managing modifiers, caching values, and resolving conflicts
    /// via tags and priority. The default calculation applies modifiers in a simple priority-sorted list.
    /// </summary>
    public class ModifiableProperty<T>
    {
        public T BaseValue { get; set; }
        public virtual T Value => GetValue();

        private readonly List<IModifier<T>> _modifiers = new();
        private readonly ICalculationStrategy<T> _calculationStrategy;
        protected bool _isDirty = true;
        protected T _cachedValue;

        public ModifiableProperty(T baseValue, ICalculationStrategy<T> calculationStrategy) 
        { 
            BaseValue = baseValue; 
            _cachedValue = baseValue;
            _calculationStrategy = calculationStrategy;
        }

        public virtual void AddModifier(IModifier<T> modifier) { _modifiers.Add(modifier); _isDirty = true; }
        public virtual void RemoveModifier(IModifier<T> modifier) { _modifiers.Remove(modifier); _isDirty = true; }

        protected virtual T GetValue()
        {
            if (!_isDirty) return _cachedValue;

            var finalModifiers = GetFinalModifiers(); // Use the powerful filtering helper

            // Delegate the calculation to the strategy
            _cachedValue = _calculationStrategy.Calculate(BaseValue, finalModifiers);

            _isDirty = false;
            return _cachedValue;
        }

        protected List<IModifier<T>> GetFinalModifiers()
        {
            if (_modifiers.Count == 0) return new List<IModifier<T>>();

            var sortedModifiers = _modifiers.OrderByDescending(m => m.Priority).ToList();
            var tagsToCancel = new HashSet<string>();
            foreach (var mod in sortedModifiers)
                if (mod.CancelsTags != null)
                    foreach (var tag in mod.CancelsTags) tagsToCancel.Add(tag);

            return sortedModifiers.Where(mod => !(mod.Tags?.Any(tagsToCancel.Contains) ?? false)).ToList();
        }
    }

    public class VariantModifiableProperty : ModifiableProperty<Variant>
    {
        public VariantModifiableProperty(Variant baseValue, ICalculationStrategy<Variant> calculationStrategy) 
            : base(baseValue, calculationStrategy) { }
    }
}