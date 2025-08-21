using Godot;
using Jmo.Core;
using Godot;
using System.Collections.Generic;

using Jmo.Core.Movement;
using Jmo.Core.Attributes;
using Jmo.Core.Modifiers;

namespace Jmo.Gameplay.Actors
{
    [GlobalClass]
    public partial class StatController : Node
    {
        private readonly Dictionary<Attribute, ModifiableProperty<Variant>> _universalStats = new();
        private readonly Dictionary<MovementMode, Dictionary<Attribute, ModifiableProperty<Variant>>> _contextualStats = new();

        public event Action<Attribute, Variant> OnStatChanged;

        public void InitializeFromArchetype(CharacterArchetype archetype)
        {
            // Initialize Universal Stats
            foreach (var entry in archetype.UniversalAttributes)
            {
                _universalStats[entry.Key] = new ModifiableProperty(entry.Value);
            }

            // Initialize Contextual Movement Stats
            foreach (var modeEntry in archetype.MovementProfiles)
            {
                var mode = modeEntry.Key;
                var profile = modeEntry.Value;
                _contextualStats[mode] = new Dictionary<Attribute, ModifiableVariantProperty>();

                foreach (var attrEntry in profile.Attributes)
                {
                    _contextualStats[mode][attrEntry.Key] = new ModifiableVariantProperty(attrEntry.Value);
                }
            }

            // A good practice to notify listeners that initial values are set
            foreach (var stat in _universalStats)
            {
                OnStatChanged?.Invoke(stat.Key, stat.Value.Value);
            }
        }

        // TODO: review and then use as template!
        public ModifiableProperty<Variant>? GetStat(Attribute attribute) // TODO: where t : variant is impossible, so some other contraint?
        {
            _universalStats.TryGetValue(attribute, out var prop);
            return prop ?? null;
        }

        public ModifiableProperty GetStat(MovementMode mode, Attribute attribute)
        {
            if (_contextualStats.TryGetValue(mode, out var stats) && stats.TryGetValue(attribute, out var prop))
            {
                return prop;
            }
            return null;
        }

        public T GetStatValue<T>(Attribute attribute, T defaultValue = default)
        {
            var prop = GetStat(attribute);
            if (prop != null)
            {
                return prop.Value.As<T>();
            }
            return defaultValue;
        }

        public T GetStatValue<T>(MovementMode mode, Attribute attribute, T defaultValue = default)
        {
            var prop = GetStat(mode, attribute);
            if (prop != null)
            {
                return prop.Value.As<T>();
            }
            return defaultValue;
        }
    }
}
