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
        protected bool _isDirty = true;
        protected T _cachedValue;

        public ModifiableProperty(T baseValue) { BaseValue = baseValue; _cachedValue = baseValue; }

        public virtual void AddModifier(IModifier<T> modifier) { _modifiers.Add(modifier); _isDirty = true; }
        public virtual void RemoveModifier(IModifier<T> modifier) { _modifiers.Remove(modifier); _isDirty = true; }

        protected virtual T GetValue()
        {
            if (!_isDirty) return _cachedValue;

            var finalModifiers = GetFinalModifiers(); // Use the powerful filtering helper

            T currentValue = BaseValue;
            foreach (var modifier in finalModifiers)
            {
                currentValue = modifier.Modify(currentValue);
            }

            _cachedValue = currentValue;
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
    /// <summary>
    /// A wrapper class for a float value that needs to be dynamically modified. This class is the
    /// heart of the modding system. It manages a list of modifiers, resolves conflicts via tags,
    /// and executes the calculation pipeline in the correct, guaranteed order of operations.
    /// </summary>
    public class ModifiableFloatProperty: ModifiableProperty<float>
    {
        private readonly List<IFloatModifier> _floatModifiers = new();

        public ModifiableFloatProperty(float baseValue) : base(baseValue) { }

        // Override Add/Remove to work with the specific list
        public override void AddModifier(IFloatModifier modifier)
        {
            _floatModifiers.Add(modifier);
            _isDirty = true;
        }

        public override void RemoveModifier(IFloatModifier modifier)
        {
            _floatModifiers.Remove(modifier);
            _isDirty = true;
        }
        protected override float GetValue()
        {
            if (!_isDirty) return _cachedValue;

            // --- Step 0: Conflict Resolution (Tags and Cancellation) ---
            if (_modifiers.Count == 0)
            {
                _cachedValue = BaseValue;
                _isDirty = false;
                return _cachedValue;
            }

            // Sort all modifiers by priority once to handle cancellations correctly.
            // Higher numeric value means higher priority.
            var sortedModifiers = _floatModifiers.OrderByDescending(m => m.Priority).ToList();

            var tagsToCancel = new HashSet<string>();
            foreach (var mod in sortedModifiers)
            {
                if (mod.CancelsTags != null)
                {
                    foreach (var tag in mod.CancelsTags) tagsToCancel.Add(tag);
                }
            }

            // Filter out any modifiers that possess a cancelled tag.
            var finalModifiers = sortedModifiers.Where(mod =>
                !(mod.Tags?.Any(tagsToCancel.Contains) ?? false)
            ).ToList();

            // --- Step 1: The Calculation Pipeline ---
            float currentFloatValue = BaseValue;

            // --- Stage 1: BaseAdd ---
            var baseAddMods = finalModifiers.Where(m => m.Stage == CalculationStage.BaseAdd);
            foreach (var mod in baseAddMods)
            {
                currentFloatValue = mod.Modify(currentFloatValue);
            }

            // --- Stage 2: PercentAdd ---
            var percentAddMods = finalModifiers.Where(m => m.Stage == CalculationStage.PercentAdd);
            if (percentAddMods.Any())
            {
                float totalPercentBonus = 0f;
                // Sum all percentage bonuses together first.
                foreach (var mod in percentAddMods)
                {
                    // Pass a dummy value, as the Modify method for this stage just returns its own value.
                    totalPercentBonus += mod.Modify(0);
                }
                currentFloatValue *= (1.0f + totalPercentBonus);
            }

            // --- Stage 3: FinalMultiply ---
            // This sub-list is already sorted by priority from our initial sort.
            var finalMultMods = finalModifiers.Where(m => m.Stage == CalculationStage.FinalMultiply);
            foreach (var mod in finalMultMods)
            {
                currentFloatValue = mod.Modify(currentFloatValue);
            }

            _cachedValue = currentFloatValue;
            _isDirty = false;
            return _cachedValue;
        }
    }
}