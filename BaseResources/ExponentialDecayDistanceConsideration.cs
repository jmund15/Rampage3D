using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class ExponentialDecayDistanceConsideration : DistanceConsideration, IAIDistanceConsideration
{
    [Export]
    private float _distThresh;
    public ExponentialDecayDistanceConsideration()
    {

    }
    public override float GetDistanceConsideration(RayCast3D raycast) 
    {
        var castLength = raycast.TargetPosition.Length();
        var collDist = (raycast.GetCollisionPoint() - raycast.GlobalPosition).Length();
        // the closer the collision is to the raycast, the higher the "danger" weight
        //var distWeight = 1f - (collisionDist / castLength); 
        var minWeight = 0.1f;
        var k = 2.5f;
        float distWeight;
        if (collDist <= _distThresh)
        {
            distWeight = 1.0f;  // Ensure max weight
        }
        else
        {
            distWeight = minWeight + (1.0f - minWeight) *
                (float)Math.Exp(-k * (collDist - _distThresh) / (castLength - _distThresh));
        }

        return distWeight;
    }
}
