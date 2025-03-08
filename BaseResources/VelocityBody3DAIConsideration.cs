using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[GlobalClass]
public partial class VelocityBody3DAIConsideration : AIEntityConsideration
{
    [Export]
    private int _collLayer;

    [Export(PropertyHint.Range, "-1.0,1.0,0.1")]
    // Positive values prioritize position-based avoidance (moving away from target)
    // Negative values prioritize velocity-based avoidance (moving opposite to target's movement)
    // Range: -1.0 (100% velocity) to 1.0 (100% position)
    private float _avoidancePositionVelocityBalance = 0.4f;

    [Export(PropertyHint.Range, "-1.0,1.0,0.1")]
    // Positive values prioritize direct approach when chasing
    // Negative values prioritize interception (perpendicular approach)
    // Range: -1.0 (100% interception) to 1.0 (100% direct)
    private float _chasePositionVelocityBalance = -0.4f;

    //[Export]
    //// Overall weighting for velocity consideration (positive = chase, negative = avoid)
    //private float _velocityConsideration = -0.5f;

    private AINav3DComponent _aiNav;
    public VelocityBody3DAIConsideration()
    {

    }
    public override Dictionary<Dir16, float> GetConsiderationVector(IBlackboard bb)
    {
        BB = bb;
        _aiNav = BB.GetVar<AINav3DComponent>(BBDataSig.AINavComp);
        var considerVec = new Dictionary<Dir16, float>();
        foreach (var dir in Global.GetEnumValues<Dir16>())
        {
            considerVec.Add(dir, 0f);
        }
        var velBodiesDetected = new List<CharacterBody3D>();
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
            if (rayCollision is not CharacterBody3D charBody) { continue; }
            if (velBodiesDetected.Contains(charBody)) { continue; }

            velBodiesDetected.Add(charBody);
        }

        foreach (var velBody in velBodiesDetected)
        {
            var velBodyConsid = GetVelocityConsideration(velBody);
            foreach (var dir in Global.GetEnumValues<Dir16>())
            {
                considerVec[dir] += velBodyConsid[dir];
            }
        }
        return considerVec;
    }
    public Dictionary<Dir16, float> GetVelocityConsideration(CharacterBody3D considBody)
    {
        // Get the velocities of both the target body and our navigation agent
        var charVel = considBody.Velocity;
        var navVel = BB.GetVar<CharacterBody3D>(BBDataSig.Agent).Velocity;

        // Get the positions of both entities
        Vector3 targetPosition = considBody.GlobalPosition;
        Vector3 agentPosition = BB.GetVar<CharacterBody3D>(BBDataSig.Agent).GlobalPosition;

        // Calculate the vector from agent to target
        Vector3 toTargetVector = targetPosition - agentPosition;

        // Project this vector onto the horizontal plane
        Vector2 distToTarget2D = new Vector2(toTargetVector.X, toTargetVector.Z).Normalized();

        // Initialize the result dictionary with zero values for all 16 directions
        Dictionary<Dir16, float> directionValues = new Dictionary<Dir16, float>();
        foreach (Dir16 dir in Enum.GetValues(typeof(Dir16)))
        {
            directionValues[dir] = 0f;
        }

        // If the consideration value is near zero, velocity doesn't matter much
        if (Mathf.Abs(Consideration) < 0.01f)
        {
            return directionValues; // Return all zeros (neutral)
        }

        // Calculate the relative velocity (how the target is moving relative to the agent)
        Vector3 relativeVelocity = charVel - navVel;

        // If the target isn't moving much relative to us, return neutral values
        if (relativeVelocity.LengthSquared() < 0.1f)
        {
            return directionValues;
        }

        // Project the velocity onto the horizontal plane
        Vector2 velDir2D = new Vector2(relativeVelocity.X, relativeVelocity.Z).Normalized();

        // Determine if the target is moving toward or away from the agent
        float approachFactor = velDir2D.Dot(distToTarget2D);

        // Calculate the ideal direction vector based on the situation
        Vector2 idealDirection;
        float weightFactor = Mathf.Abs(Consideration) * relativeVelocity.Length();
        bool shouldChase = Consideration > 0;

        if (shouldChase)
        {
            if (approachFactor < 0) // Target is coming toward us
            {
                // Convert balance parameter to weights for interception vs. direct approach
                float directWeight = (_chasePositionVelocityBalance + 1f) / 2f;
                float interceptWeight = 1f - directWeight;

                // Interception direction (perpendicular to velocity)
                Vector2 interceptDir = new Vector2(-velDir2D.Y, velDir2D.X);

                // Blend between interception and direct approach
                idealDirection = (interceptDir * interceptWeight + distToTarget2D * directWeight).Normalized();
            }
            else // Target is moving away
            {
                // Blend between prediction and direct approach based on how directly they're moving away
                float predictionInfluence = Mathf.Abs(approachFactor); // 0 to 1

                // Convert our chase balance parameter based on how directly they're moving away
                float directWeight = ((_chasePositionVelocityBalance + 1f) / 2f) * (1f - predictionInfluence);
                float predictionWeight = 1f - directWeight;

                idealDirection = (velDir2D * predictionWeight + distToTarget2D * directWeight).Normalized();
            }
        }
        else // Avoid
        {
            if (approachFactor < 0) // Target is coming toward us
            {
                // For direct approach, evasion is more urgent and we favor moving perpendicular
                Vector2 evadeDir = new Vector2(-velDir2D.Y, velDir2D.X);
                idealDirection = evadeDir.Normalized();

                // Scale by urgency
                weightFactor *= Mathf.Abs(approachFactor);
            }
            else // Target is moving away
            {
                // Convert avoidance balance parameter to actual weights
                float positionWeight = (_avoidancePositionVelocityBalance + 1f) / 2f;
                float velocityWeight = 1f - positionWeight;

                // Blend between moving opposite to their direction and away from their position
                idealDirection = (-velDir2D * velocityWeight - distToTarget2D * positionWeight).Normalized();
            }
        }

        // Now project this ideal direction onto our 16 discrete directions
        Dir16 closestDir = idealDirection.GetDir16();

        // Set the value for the closest direction
        directionValues[closestDir] = weightFactor;

        directionValues = PropogateConsiderations(directionValues);

        return directionValues;
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

    //public static void AddDictionary<T, U>(this Dictionary<T, U> sumToDict, Dictionary<T, U> sumDict)
    //where U : IAdditionOperators<U, U, U>
    //{
    //    foreach (var key in sumDict.Keys)
    //    {
    //        if (sumToDict.ContainsKey(key))
    //        {
    //            sumToDict[key] += sumDict[key];
    //        }
    //        else
    //        {
    //            sumToDict[key] = sumDict[key];
    //        }
    //    }
    //}
}
