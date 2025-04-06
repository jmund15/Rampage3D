using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
[GlobalClass]
public partial class StaticBody3DAIConsideration : AIEntityConsideration3D
{
    [Export]
    private int _collLayer;
    [Export]
    private Vector2 _distDiminishRange;
    [Export]
    private int _dirsToPropogate = 2;
    [Export]
    private float _initPropWeight = 0.75f;
    [Export]
    private float _propDiminishWeight = 0.5f;

    public StaticBody3DAIConsideration()
    {

    }
    public override Dictionary<Vector3, float> GetConsiderationVector(IBlackboard bb)
    {
        BB = bb;
        AINav = BB.GetVar<AINav3DComponent>(BBDataSig.AINavComp);
        var rays = AINav.AIRays;
        var considerVec = new Dictionary<Vector3, float>();
        foreach (var dir in rays.Raycasts.Keys)
        {
            considerVec[dir] = 0f;
        }

        foreach (var pair in AINav.AIRays.Raycasts)
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
            //GD.Print($"{dir} dist weight: {distWeight}");
            //GD.Print($"Raycast found danger {navLayer.Key} @ dir {dir}!");
            //var spatialAwarenessMod = SpatialAwarenessWeights[dir.GetAIFacing(_moveComp.GetFaceDirection())];
            var dangerAmt = Consideration * distWeight;
            //* spatialAwarenessMod;

            considerVec[dir] += dangerAmt;
        }
        //considerVec = PropogateConsiderations(considerVec);
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
        if (collDist <= _distDiminishRange.X)
        {
            distWeight = 1.0f;  // Ensure max weight
        }
        else
        {
            distWeight = minWeight + (1.0f - minWeight) *
                (float)Math.Exp(-k * (collDist - _distDiminishRange.X) / (_distDiminishRange.Y/*castLength*/ - _distDiminishRange.X));
        }
        return distWeight;
    }

    public Dictionary<Dir16, float> PropogateConsiderations(Dictionary<Dir16, float> considerations)
    {
        var preConsiderations = new Dictionary<Dir16, float>(considerations);
        foreach (var preConsid in preConsiderations)
        {
            var dir = preConsid.Key;
            var dangerAmt = preConsid.Value;
            if (dangerAmt == 0.0f)
            {
                continue;
            }
            //PROPOGATE DANGER OUT
            var propogateNum = _dirsToPropogate;
            var propLDir = dir;
            var propRDir = dir;
            var propWeight = _initPropWeight;
            while (propogateNum > 0)
            {
                propLDir = propLDir.GetLeftDir();
                propRDir = propRDir.GetRightDir();
                considerations[propLDir] += dangerAmt * propWeight;
                considerations[propRDir] += dangerAmt * propWeight;
                //GD.Print($"orig dir: {dir}; left dir: {propLDir}; right dir: {propRDir}; tbmb: {propWeight}" +
                //    $"\norig left: {preConsiderations[propLDir]}; new left: {considerations[propLDir]}");

                propWeight *= _propDiminishWeight;
                propogateNum--;
            }
        }
        return considerations;
    }
}
