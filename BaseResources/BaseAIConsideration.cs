using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract partial class BaseAIConsideration : Resource, IAIConsideration<Dir16>
{
    [Export(PropertyHint.Range, "-2.5,2.5,0.1,or_greater,or_less")]
    protected float Consideration; // negative values are danger, positive are interest

    protected IBlackboard BB;
    public BaseAIConsideration()
    {
    }
    public abstract Dictionary<Dir16, float> GetConsiderationVector(IBlackboard bb);
}
