using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeRobbers.JmoAI.UtilityAI
{
    [GlobalClass, Tool]
    public partial class ConstConsideration : UtilityConsideration
    {
        [Export]
        protected float ConstEvalution;

        public override void Init(UtilityContext context)
        {
        }
        public override float Evaluate(UtilityContext context)
        {
            return ConstEvalution;
        }

    }
}
