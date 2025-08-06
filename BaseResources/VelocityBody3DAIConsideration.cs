using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

[GlobalClass]
public partial class VelocityBody3DAIConsideration : AIEntityConsideration3D
{
    [Export]
    private int _collLayer;

    [Export(PropertyHint.Range, "-1.0,1.0,0.1")]
    // Positive values prioritize position-based avoidance (moving away from target)
    // Negative values prioritize velocity-based avoidance (moving opposite to target's movement)
    // Range: -1.0 (100% velocity) to 1.0 (100% position)
    private float _positionVelocityBalance = 0.4f;

    //[Export(PropertyHint.Range, "-1.0,1.0,0.1")]
    // Positive values prioritize direct approach when chasing
    // Negative values prioritize interception (perpendicular approach)
    // EDIT: Negative values ACTUALLY prioritize parallel velocity (move the same velocity as the target)
    // Range: -1.0 (100% interception) to 1.0 (100% direct)
    //private float _positionVelocityBalance = -0.4f;

    [Export]
    private bool _hasVerticalMovement = false;
    //[Export]
    //// Overall weighting for velocity consideration (positive = chase, negative = avoid)
    //private float _velocityConsideration = -0.5f;

    private Vector3 _agentPosition;
    private Vector3 _agentVelocity;
    private IEnumerable<Vector3> _considDirections;

    private float Epsilon = 0.001f;
    public VelocityBody3DAIConsideration()
    {

    }
    public override void InitializeResources(IBlackboard bb)
    {
        GD.Print("INITIALIZING VELOCITY BODY CONSIDERATION");
        BB = bb;
        Agent = BB.GetVar<Node3D>(BBDataSig.Agent);
        AINav = BB.GetVar<AINav3DComponent>(BBDataSig.AINavComp);
        // Early exit if essential components are missing
        if (Agent == null || AINav == null || AINav.AIRayDetector == null)
        {
            GD.PrintErr($"VelocityBody3DAIConsideration ERROR || Missing Agent Body or AINavComponent/AIRays on Blackboard.");
        }
    }
    public override Dictionary<Vector3, float> GetConsiderationVector(IAISensor3D detector)
    {
        //GD.Print("GETTING VELOCITY BODY CONSIDERATION");
        _agentPosition = Agent.GlobalPosition;
        _agentVelocity = (Agent as IVelocity3DComponent).GetVelocity();
        _considDirections = AINav.AIRayDetector.Directions; // Use the definitive list

        var rays = AINav.AIRayDetector;
        var considerVec = _considDirections.ToDictionary(dir => dir, dir => 0f);

        // Get all relevant bodies detected!
        var velBodiesDetected = DetectRelevantBodies(detector);

        foreach (var velBody in velBodiesDetected)
        {
            var velBodyConsid = GetConsiderationsForSingleTarget(velBody);
            foreach (var dir in rays.Directions)
            {
                considerVec[dir] += velBodyConsid[dir];
            }
        }
        return considerVec;
    }
    // Helper to detect bodies using raycasts
    private HashSet<IVelocity3DComponent> DetectRelevantBodies(IAISensor3D detector)
    {
        var detectedBodies = new HashSet<IVelocity3DComponent>(); // Use HashSet for efficient uniqueness check

        foreach (var detectNode in detector.GetSensedBodies())
        {
            if (detectNode is not CollisionObject3D collObj) { continue; }
            if (detectNode == Agent) { continue; }
            if (!collObj.GetCollisionLayerValue(_collLayer)) { continue; }
            if (detectNode is not IVelocity3DComponent velBody) { continue; }

            // Add returns true if the item was added, false if it already existed
            detectedBodies.Add(velBody);
        }
        return detectedBodies;
    }
    private Dictionary<Vector3, float> GetConsiderationsForSingleTarget(IVelocity3DComponent targetBody)
    {
        // Initialize scores for this target using the cached directions
        var considerations = _considDirections.ToDictionary(dir => dir, dir => 0f);

        // --- Basic Checks ---
        if (Mathf.Abs(Consideration) < Epsilon) return considerations;

        // --- Gather Target Info ---
        Vector3 targetPosition = targetBody.GlobalPosition;
        Vector3 targetVelocity = targetBody.GetVelocity();

        // --- Calculate Relative Vectors (Full 3D, respecting HasVerticalMovement) ---
        Vector3 toTargetVector = targetPosition - _agentPosition;
        Vector3 relativeVelocity = targetVelocity - _agentVelocity;

        // Flatten vectors if agent cannot move vertically
        if (!_hasVerticalMovement)
        {
            toTargetVector.Y = 0;
            relativeVelocity.Y = 0;
        }

        // --- Check for negligible distance/velocity ---
        Vector3 toTargetDir = Vector3.Zero;
        float distanceSq = toTargetVector.LengthSquared();
        if (distanceSq > Epsilon * Epsilon)
        {
            toTargetDir = toTargetVector.Normalized(); // Normalize *after* potential flattening
        }

        Vector3 relativeVelDir = Vector3.Zero;
        float relativeSpeedSq = relativeVelocity.LengthSquared();
        bool useVelocity = relativeSpeedSq > Epsilon * Epsilon;

        // If both distance and relative velocity are negligible, nothing to consider
        if (!useVelocity && distanceSq < Epsilon * Epsilon)
        {
            return considerations;
        }

        if (useVelocity)
        {
            relativeVelDir = relativeVelocity.Normalized(); // Normalize *after* potential flattening
        }

        // --- Determine Ideal Movement Direction ---
        Vector3 idealDirection;
        bool shouldChase = Consideration > 0;

        if (shouldChase)
        {
            idealDirection = CalculateChaseDirection(targetPosition, targetVelocity, toTargetDir, relativeVelDir, useVelocity);
        }
        else // Avoid
        {
            idealDirection = CalculateAvoidDirection(toTargetDir, relativeVelDir, useVelocity);
        }

        // If calculation failed or resulted in zero vector, return current scores
        if (idealDirection.LengthSquared() < Epsilon * Epsilon)
        {
            return considerations;
        }
        // Note: idealDirection is already normalized by CalculateChase/Avoid functions

        // --- Calculate Weight/Score Magnitude ---
        float weightFactor = Mathf.Abs(Consideration);
        // Option 1: Scale by relative speed (if velocity is usable)
        if (useVelocity) weightFactor *= relativeVelocity.Length();
        // Option 2: Scale inversely by distance (closer = more important) - Combine carefully!
        // weightFactor /= (1f + Mathf.Sqrt(distanceSq)); // Example

        // --- Distribute Score to Available Directions ---
        foreach (var availableDirRaw in _considDirections)
        {
            Vector3 availableDir = availableDirRaw;
            if (!_hasVerticalMovement)
            {
                availableDir.Y = 0; // Flatten available dir if needed
            }

            if (availableDir.LengthSquared() < Epsilon * Epsilon) continue;

            Vector3 normAvailableDir = availableDir.Normalized();

            // Calculate alignment with the final ideal direction
            float alignment = normAvailableDir.Dot(idealDirection); // Both are normalized

            // Map alignment to score multiplier (e.g., 0 to 1)
            float scoreMultiplier = Mathf.Max(0f, alignment);

            // Assign score (Add to existing score if multiple considerations affect same direction)
            // NOTE: If this function is the *only* source for this dictionary, use '='
            // If aggregating like in the AINav example, use '+='
            // Assuming this function returns scores for ONE target, use '='
            considerations[availableDirRaw] = weightFactor * scoreMultiplier; // Store score using the ORIGINAL direction key
        }

        return considerations;
    }

    // --- Separated Chase/Avoid Logic ---

    private Vector3 CalculateChaseDirection(Vector3 targetPosition, Vector3 targetVelocity, Vector3 toTargetDir, Vector3 relativeVelDir, bool useVelocity)
    {
        // 1. Direct Position Vector: Always valid if target is not right on top of agent
        Vector3 positionBasedDir = (toTargetDir.LengthSquared() > Epsilon * Epsilon) ? toTargetDir : Vector3.Forward; // Fallback if dir is zero

        // --- Velocity-Based Component Modification ---
        Vector3 velocityBasedDir;
        if (useVelocity && targetVelocity.LengthSquared() > Epsilon * Epsilon)
        {
            // OPTION A (Recommended): Use the target's absolute velocity direction.
            // This aims parallel to the target's current movement.
            velocityBasedDir = targetVelocity.Normalized();

            // OPTION B (Alternative): Use the relative velocity direction.
            // This aims in the direction the target is moving *relative* to the agent.
            //velocityBasedDir = relativeVelDir; // Already normalized if useVelocity is true
        }
        else
        {
            // Fallback if velocity is unusable or target isn't moving:
            // Just use the position-based direction as the only component.
            if (!_hasVerticalMovement) positionBasedDir.Y = 0;
            return positionBasedDir.Normalized();
        }
        // --- End Modification ---

        // 3. Blend based on _chasePositionVelocityBalance
        float interceptWeight = (-_positionVelocityBalance + 1.0f) / 2.0f;
        float positionWeight = 1.0f - interceptWeight;
        Vector3 blendedDirection = (positionBasedDir * positionWeight + velocityBasedDir * interceptWeight);

        // Flatten final blend if needed
        if (!_hasVerticalMovement) blendedDirection.Y = 0;

        // Check if blending resulted in a near-zero vector
        if (blendedDirection.LengthSquared() < Epsilon * Epsilon)
        {
            // Fallback if blending cancels out, prioritize position?
            if (!_hasVerticalMovement) positionBasedDir.Y = 0;
            return positionBasedDir.Normalized();
        }

        return blendedDirection.Normalized();
    }

    private Vector3 CalculateAvoidDirection(Vector3 toTargetDir, Vector3 relativeVelDir, bool useVelocity)
    {
        // 1. Position Vector: Directly away from the target
        Vector3 positionBasedDir = (toTargetDir.LengthSquared() > Epsilon * Epsilon) ? -toTargetDir : Vector3.Back; // Fallback if dir is zero

        // If we can't use velocity, just return the position-based direction (flattened if needed)
        if (!useVelocity)
        {
            if (!_hasVerticalMovement) positionBasedDir.Y = 0;
            return positionBasedDir.Normalized(); // Ensure normalized
        }

        // 2. Velocity Vector: Directly opposite the target's relative velocity towards us
        Vector3 velocityBasedDir = -relativeVelDir; // Already calculated from potentially flattened relativeVelocity

        // 3. Blend based on _avoidancePositionVelocityBalance
        float velocityWeight = (-_positionVelocityBalance + 1.0f) / 2.0f;
        float positionWeight = 1.0f - velocityWeight;
        Vector3 blendedDirection = (positionBasedDir * positionWeight + velocityBasedDir * velocityWeight);

        // Flatten final blend if needed (redundant if inputs were flattened, but safe)
        if (!_hasVerticalMovement) blendedDirection.Y = 0;

        // Check if blending resulted in a near-zero vector
        if (blendedDirection.LengthSquared() < Epsilon * Epsilon)
        {
            // Fallback if blending cancels out, prioritize moving away from position?
            if (!_hasVerticalMovement) positionBasedDir.Y = 0;
            return positionBasedDir.Normalized();
        }

        return blendedDirection.Normalized();
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
