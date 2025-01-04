using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeRobbers.JmoAI.UtilityAI
{
    [GlobalClass, Tool]
    public partial class CurrentTargetDistConsideration : UtilityConsideration
    {
        public override void Init(UtilityContext context)
        {
        }
        public override float Evaluate(UtilityContext context)
        {
            if (!context.CurrTarget.IsValid()) { return 0f; }
            return 1 - 
                (context.BB.GetVar<Node2D>(BBDataSig.Agent).GlobalPosition.DistanceTo(context.CurrTarget.GlobalPosition) / 1000f);//TODO: _detectComp.;
        }
    }
}
