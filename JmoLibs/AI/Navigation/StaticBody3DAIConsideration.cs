using Godot;
using System;
using System.Collections.Generic;
using DotnetUtils;

using Jmo.Core.World;

namespace Jmo.AI.Navigation;

[GlobalClass]
public partial class StaticBody3DAIConsideration : BaseAIConsideration3D
{
    [Export(PropertyHint.Range, "-2.5,2.5,0.1,or_greater,or_less")]
    protected float Consideration; // negative values are danger, positive are interest

    // TODO: use Category instead of collision layer?
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
    protected override Dictionary<Vector3, float> CalculateBaseScores(DirectionSet3D directions, DecisionContext context, IBlackboard blackboard)
    {
        var Agent = blackboard.GetVar<Node3D>(BBDataSig.Agent);
        var AINav = blackboard.GetVar<AINavigator3D>(BBDataSig.AINavComp);
        var percept = context.Memory;

        var considerVec = new Dictionary<Vector3, float>();
        foreach (var dir in directions.Directions) { considerVec[dir] = 0f; }

        var sensedPercepts = percept.GetSensedByCollLayer(_collLayer);
        foreach (var perceptInfo in sensedPercepts)
        {
            var sensed = (CollisionObject3D)perceptInfo.Target!;
            if (sensed == Agent) continue;


            Vector3 collVec = (sensed.GlobalPosition - Agent.GlobalPosition).Normalized();
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
