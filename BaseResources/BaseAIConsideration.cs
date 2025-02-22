using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class BaseAIConsideration : Resource, IAIConsideration<Dir16>
{
    protected IBlackboard BB;
    public BaseAIConsideration()
    {

    }
    public Dictionary<Dir16, float> GetConsiderationVector(IBlackboard bb)
    {
        throw new NotImplementedException();
    }
}
