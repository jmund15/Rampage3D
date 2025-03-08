using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[GlobalClass]
public partial class AIBodyConsideration : AIEntityConsideration
{
    [Export]
    private int _collLayer;
    [Export]
    public Godot.Collections.Array<BaseAIConsideration> _considerations;

    public AIBodyConsideration()
    {

    }

    public override Dictionary<Dir16, float> GetConsiderationVector(IBlackboard bb)
    {
        Dictionary<Dir16, float> considVec = new Dictionary<Dir16, float>();
        foreach (Dir16 dir in Enum.GetValues(typeof(Dir16)))
        {
            considVec[dir] = 0f;
        }

        // Calculate each consideration
        foreach (var consideration in _considerations)
        {
            var values = consideration.GetConsiderationVector(bb);

            // Accumulate the values
            foreach (Dir16 dir in Enum.GetValues(typeof(Dir16)))
            {
                considVec[dir] += values[dir];
            }
        }
        return considVec;
    }
}
