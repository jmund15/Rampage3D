using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeRobbers.JmoAI.UtilityAI
{
    [GlobalClass, Tool]
    public partial class HealthConsideration : UtilityConsideration
    {
        public override void Init(UtilityContext context)
        {
        }
        public override float Evaluate(UtilityContext context)
        {
            return context.HealthPercentage;
        }
    }
}
