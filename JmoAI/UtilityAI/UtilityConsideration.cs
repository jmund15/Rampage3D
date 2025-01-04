using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeRobbers.JmoAI.UtilityAI
{
    [GlobalClass, Tool]
    public abstract partial class UtilityConsideration : Resource
    {
        //protected UtilityContext Context;
        public float Desire { get; protected set; }
        public float Logic { get; protected set; }
        public abstract void Init(UtilityContext context);
        public abstract float Evaluate(UtilityContext context);
        //public void SetContext(UtilityContext context) { Context =  context; }
    }
}
