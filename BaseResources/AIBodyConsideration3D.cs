using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[GlobalClass]
public partial class AIBodyConsideration3D : AIEntityConsideration3D
{
    [Export]
    private int _collLayer;
    [Export]
    public Godot.Collections.Array<BaseAIConsideration3D> _considerations;
    public AIBodyConsideration3D()
    {

    }
    public override void InitializeResources(IBlackboard bb)
    {
        BB = bb;
        AINav = BB.GetVar<AINav3DComponent>(BBDataSig.AINavComp);
    }
    public override Dictionary<Vector3, float> GetConsiderationVector(IAIDetector3D detector)
    {
        var rays = AINav.AIRayDetector;
        Dictionary<Vector3, float> considVec = new Dictionary<Vector3, float>();
        foreach (var dir in rays.Raycasts.Keys)
        {
            considVec[dir] = 0f;
        }

        // Calculate each consideration
        foreach (var consideration in _considerations)
        {
            var values = consideration.GetConsiderationVector(detector);

            // Accumulate the values
            foreach (var dir in rays.Raycasts.Keys)
            {
                considVec[dir] += values[dir];
            }
        }
        return considVec;
    }
}
