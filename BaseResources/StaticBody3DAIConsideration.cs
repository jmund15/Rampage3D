using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotnetUtils;

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
    // bidirectional dictionary (using '.Forward' & '.Reverse')
    private Map<int, Vector3> _dirIds = new Map<int, Vector3>();
    public StaticBody3DAIConsideration()
    {

    }
    public override void InitializeResources(IBlackboard bb)
    {
        BB = bb;
        AINav = BB.GetVar<AINav3DComponent>(BBDataSig.AINavComp);
        Agent = BB.GetVar<Node3D>(BBDataSig.Agent);
        //int i = 0;
        //foreach (var dir in AINav.AIRays.Directions)
        //{
        //    _dirIds.Add(i, dir);
        //    i++;
        //}
    }
    public override Dictionary<Vector3, float> GetConsiderationVector(IAISensor3D detector)
    {
        
        var rays = AINav.AIRayDetector;
        var considerVec = new Dictionary<Vector3, float>();
        foreach (var dir in rays.Directions) { considerVec[dir] = 0f; }


        foreach (var detected in detector.GetSensedBodies())
        {
            if (detected == Agent) continue;
            if (detected is not CollisionObject3D collisionObj) continue;
            if (!collisionObj.GetCollisionLayerValue(_collLayer)) continue;

            Vector3 collVec = (detected.GlobalPosition - Agent.GlobalPosition).Normalized();
            var dist = collVec.Length();
            Vector3 dir = collVec.Normalized();
            float distWeight = GetDistanceConsideration(dist);
            float dangerAmt = Consideration * distWeight;

            considerVec[dir] = dangerAmt;
        }
        considerVec = PropogateConsiderations(considerVec);
        return considerVec;
    }

    public float GetDistanceConsideration(float detectDist)
    {
        if (detectDist > _distDiminishRange.Y)
        {
            return 0f;
        }
        // the closer the collision is to the raycast, the higher the "danger" weight
        var minWeight = 0.1f;
        var k = 2.5f;
        float distWeight;

        if (detectDist <= _distDiminishRange.X)
        {
            distWeight = 1.0f;  // Ensure max weight
        }
        else
        {
            distWeight = 1f - ( (detectDist - _distDiminishRange.X) / (_distDiminishRange.Y - _distDiminishRange.X) );

            //distWeight = minWeight + (1.0f - minWeight) *
            //    (float)Math.Exp(-k * (collDist - _distDiminishRange.X) / (_distDiminishRange.Y/*castLength*/ - _distDiminishRange.X));
        }
        distWeight = Mathf.Clamp(distWeight, 0f, 1f);
        //GD.Print($"{raycast.TargetPosition.Normalized().GetDir16()}'s wall dist: {collDist}\ndistWeight: {distWeight}");
        return distWeight;
    }

    public Dictionary<Vector3, float> PropogateConsiderations(Dictionary<Vector3, float> considerations)
    {
        var preConsiderations = new Dictionary<Vector3, float>(considerations);
        

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
            int propLDir = _dirIds.Reverse[dir];
            int propRDir = _dirIds.Reverse[dir];
            var dirId = _dirIds.Reverse[dir];
            var propWeight = _initPropWeight;
            while (propogateNum > 0)
            {
                if (propLDir == 0)
                {
                    propLDir = considerations.Count;
                }
                else { propLDir--; }

                if (propRDir == considerations.Count)
                {
                    propRDir = 0;
                }
                else { propRDir++; }
                //propLDir = propLDir.GetLeftDir();
                //propRDir = propRDir.GetRightDir();
                considerations[_dirIds.Forward[propLDir]] += dangerAmt * propWeight;
                considerations[_dirIds.Forward[propRDir]] += dangerAmt * propWeight;
                //GD.Print($"orig dir: {dir}; left dir: {propLDir}; right dir: {propRDir}; tbmb: {propWeight}" +
                //    $"\norig left: {preConsiderations[propLDir]}; new left: {considerations[propLDir]}");

                propWeight *= _propDiminishWeight;
                propogateNum--;
            }
        }
        return considerations;
    }
}
