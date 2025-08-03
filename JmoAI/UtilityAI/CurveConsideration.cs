using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JmoAI.UtilityAI
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
        protected override float CalculateBaseScore(IBlackboard context)
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
