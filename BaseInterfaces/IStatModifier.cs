using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeRobbers.BaseInterfaces
{
    public interface IStatModifier
    {
        public int GetModifierPriority();
        //public abstract HitboxAttack ApplyItemModifier();
    }
}
