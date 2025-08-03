using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JmoAI.UtilityAI
{
    [GlobalClass, Tool]
    public partial class ConstConsideration : UtilityConsideration
    {
        [Export]
        protected float ConstEvalution;

        protected override float CalculateBaseScore(IBlackboard context)
        {
            return ConstEvalution;
        }

    }
}
