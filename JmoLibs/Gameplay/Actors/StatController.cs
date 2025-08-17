using Godot;
using Jmo.Core;
using Jmo.Core.Attributes;
using Jmo.Core.Modifiers;
using Jmo.Core.Movement;
using System.Collections.Generic;

namespace Jmo.Gameplay.Actors
{
    [GlobalClass]
    public partial class StatController : Node
    {
        private readonly Dictionary<MovementMode, Dictionary<Attribute, ModifiableFloatProperty>> _contextualStats = new();

        public void InitializeFromArchetype(CharacterArchetype archetype)
        {
            foreach (var modeEntry in archetype.BaseMovementAttributes)
            {
                var mode = modeEntry.Key;
                var attributes = modeEntry.Value;

                _contextualStats[mode] = new Dictionary<Attribute, ModifiableFloatProperty>();
                foreach (var attrEntry in attributes)
                {
                    _contextualStats[mode][attrEntry.Key] = new ModifiableFloatProperty(attrEntry.Value);
                }
            }
        }

        public ModifiableFloatProperty GetStat(MovementMode mode, Attribute attribute)
        {
            if (_contextualStats.TryGetValue(mode, out var stats) && stats.TryGetValue(attribute, out var prop))
            {
                return prop;
            }
            return null;
        }

        public float GetStatValue(MovementMode mode, Attribute attribute)
        {
            return GetStat(mode, attribute)?.Value ?? 0f;
        }
    }
}
