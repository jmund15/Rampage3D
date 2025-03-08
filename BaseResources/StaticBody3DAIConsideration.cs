using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
[GlobalClass]
public partial class StaticBody3DAIConsideration : AIEntityConsideration
{
    [Export]
    private int _collLayer;
    [Export]
    private float _distThresh;

    private AINav3DComponent _aiNav;
    public StaticBody3DAIConsideration()
    {

    }
    public override Dictionary<Dir16, float> GetConsiderationVector(IBlackboard bb)
    {
        BB = bb;
        _aiNav = BB.GetVar<AINav3DComponent>(BBDataSig.AINavComp);
        var considerVec = new Dictionary<Dir16, float>();
        foreach (var pair in _aiNav.Rays.Raycasts)
        {
            var dir = pair.Key;
            var raycast = pair.Value;

            if (!raycast.IsColliding())
            {
                continue;
            }
            var rayCollision = raycast.GetCollider() as CollisionObject3D;
            if (rayCollision == bb.GetVar<Node>(BBDataSig.Agent)) { continue; }
            if (!rayCollision.GetCollisionLayerValue(_collLayer)) { continue; }
            
            var distWeight = GetDistanceConsideration(raycast);
            //GD.Print($"Raycast found danger {navLayer.Key} @ dir {dir}!");
            //var spatialAwarenessMod = SpatialAwarenessWeights[dir.GetAIFacing(_moveComp.GetFaceDirection())];
            var dangerAmt = Consideration * distWeight;
            //* spatialAwarenessMod;

            considerVec[dir] += dangerAmt;
        }
        considerVec = PropogateConsiderations(considerVec);
        return considerVec;
    }

    public float GetDistanceConsideration(RayCast3D raycast)
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

    public Dictionary<Dir16, float> PropogateConsiderations(Dictionary<Dir16, float> considerations)
    {
        foreach (var consid in considerations)
        {
            var dir = consid.Key;
            var dangerAmt = consid.Value;
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
                considerations[propLDir] += dangerAmt * propWeight;
                considerations[propRDir] += dangerAmt * propWeight;

                propWeight *= weightDrop;
                propogateNum--;
            }
        }
        return considerations;
    }
}
