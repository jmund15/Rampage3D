using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeRobbers.JmoAI.UtilityAI
{
    [GlobalClass, Tool]
    public partial class CurveConsideration : UtilityConsideration
    {
        [Export]
        public bool CallResetCurve { get; set; } = false;
        [Export]
        protected Curve SampleCurve = new Curve();
        [Export]
        protected UtilityConsideration BaseConsideration;
        public override void Init(UtilityContext context)
        {
        }
        public override float Evaluate(UtilityContext context)
        {
            if (!SampleCurve.IsValid() || !BaseConsideration.IsValid())
            {
                // TODO: WARNING HERE
                return 0f;
            }
            return SampleCurve.Sample(BaseConsideration.Evaluate(context));
        }
        public virtual void ResetCurve()
        {
            //idk reset curve here
            SampleCurve = new Curve();
            SampleCurve.SetPointValue(0, 0f);
            SampleCurve.SetPointValue(1, 1f);
        }
    }
}
