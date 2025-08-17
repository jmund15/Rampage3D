using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmo.Core.Movement
{
    // --- The Modifier Interface ---
    public interface IMovementModifier
    {
        // A modifier takes the base VelocityID and returns a modified version.
        VelocityID Modify(VelocityID baseVelocity);
    }

    // --- An example concrete modifier ---
    public class SlowedModifier : IMovementModifier
    {
        private readonly float _slowFactor;
        public SlowedModifier(float slowFactor) { _slowFactor = slowFactor; }
        public VelocityID Modify(VelocityID baseVelocity)
        {
            // This modifier only affects max speed, leaving friction/acceleration alone.
            baseVelocity.MaxSpeed *= _slowFactor;
            return baseVelocity;
        }
    }
}
