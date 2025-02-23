using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public partial class VelocityBody3DAIConsideration : BaseAIConsideration, IAIConsideration<Dir16>
{
    [Export]
    private int _collLayer;
    [Export]
    private float _distThresh;
    [Export]
    private float _velocityConsideration = 1f;
    public VelocityBody3DAIConsideration()
    {

    }
    public Dictionary<Dir16, float> GetConsiderationVector(IBlackboard bb)
    {
        var aiNav = bb.GetVar<AINav3DComponent>(BBDataSig.AINavComp);
        var considerVec = new Dictionary<Dir16, float>();
        foreach (var pair in aiNav.Rays.Raycasts)
        {
            var dir = pair.Key;
            var raycast = pair.Value;
            var castLength = raycast.TargetPosition.Length();
            if (raycast.IsColliding())
            {
                var rayCollision = raycast.GetCollider() as CollisionObject3D;
                if (rayCollision == bb.GetVar<Node>(BBDataSig.Agent)) { continue; } 
                if (rayCollision is not IVelocity3DComponent velBody) { continue; }

                if (rayCollision.GetCollisionLayerValue(_collLayer))
                {
                    var collDist = (raycast.GetCollisionPoint() - raycast.GlobalPosition).Length();
                    // the closer the collision is to the raycast, the higher the "danger" weight
                    // at max dist, weight is 0. TODO: change s.t. it's not zero, just lower (i.e. 0.1)
                    // at dist 0, weight is 1.0. TODO: change s.t. weight being 0 occurs not only at dist 0, since that is impossible and too late
                    //var distWeight = 1f - (collisionDist / castLength); 
                    var minWeight = 0.1f;
                    var k = 2.5f;
                    var distDropThresh = 1.0f;//NavDistThresh[navLayer.Key];//1.5f; 
                    float distWeight;
                    if (collDist <= distDropThresh)
                    {
                        distWeight = 1.0f;  // Ensure max weight
                    }
                    else
                    {
                        distWeight = minWeight + (1.0f - minWeight) * (float)Math.Exp(-k * (collDist - distDropThresh) / (castLength - distDropThresh));
                    }
                    //GD.Print($"Raycast found danger {navLayer.Key} @ dir {dir}!");
                    //var spatialAwarenessMod = SpatialAwarenessWeights[dir.GetAIFacing(_moveComp.GetFaceDirection())];
                    var dangerAmt = 1.0f;//NavWeights[navLayer.Key] * distWeight;
                    //* spatialAwarenessMod;

                    considerVec[dir] += dangerAmt;

                    //PROPOGATE DANGER OUT
                    var propogateNum = 3;
                    var propLDir = dir;
                    var propRDir = dir;
                    var weightDrop = 0.75f;
                    var propWeight = 0.5f;
                    while (propogateNum > 0)
                    {
                        propLDir = propLDir.GetLeftDir();
                        propRDir = propRDir.GetRightDir();
                        considerVec[propLDir] += dangerAmt * propWeight;
                        considerVec[propRDir] += dangerAmt * propWeight;

                        propWeight *= weightDrop;
                        propogateNum--;
                    }


                    // add danger to any danger dir in a 45 degree sweep
                    //TODO: to increase performance, static these comparisons for quicker calcs
                    //foreach (var dir8 in dangerVector.Keys)
                    //{
                    //var angle = Mathf.Abs(dir8.GetVector2().GetAngleToVector(dir.GetVector2()));
                    //var dangerAngle = 30f;
                    //if (angle <= dangerAngle)
                    //{
                    //    // base is 0.5, max is 1
                    //    var angleDangerMod = 1.0f - ((angle / 2) / dangerAngle);
                    //    //GD.Print($"{dir8} danger amt for cast {dir}: {NavWeights[navLayer.Key]} * {distWeight} " +
                    //    //    $"* {spatialAwarenessMod} * {angleDangerMod}");
                    //    dangerVector[dir8] += dangerAmt * angleDangerMod;
                    //}
                    //}

                }
                
            }
        }
        return considerVec;
    }
}
