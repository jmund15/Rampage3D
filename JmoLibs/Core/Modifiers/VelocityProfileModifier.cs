using Godot;
using Jmo.Core.Modifiers;

namespace Jmo.Core.Movement.Modifiers
{
    /// <summary>
    /// An abstract base class for modifiers that specifically target VelocityProfiles.
    /// It provides a clean foundation for creating data-driven buffs/debuffs to character physics.
    /// </summary>
    [GlobalClass]
    public abstract partial class VelocityProfileModifier : Resource, IModifier<VelocityProfile>
    {
        public abstract VelocityProfile Modify(VelocityProfile baseValue);
    }
}