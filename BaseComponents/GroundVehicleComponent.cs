using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

/* VEHICLE TODO: 
 * 1. Braking sfx volume directly corresponding with negative acceleration
 * 
 */
[GlobalClass, Tool]
public partial class GroundVehicleComponent : RigidBody3D, IMovementComponent, IVelocity3DComponent
{
	#region COMPONENT_VARIABLES
	private CollisionShape3D _collShape;
	private List<Node3D> _wheels = new List<Node3D>();

    public VehicleOccupantsComponent? OccupantComp { get; private set; }
	private IBlackboard _bb;
	private AINav3DComponent _aiNav;

    [Export]
    private DriverAptitude _debugDriverApt;


    //[Export]
    //private float _maxSpeed = 1000f;
    [Export]
	private bool _allWheelDrive = true;
	private float _frontWheelXPos;

    [Export]
    private Vector2 LateralFriction = new Vector2(0.9f, 0.975f);





    public Vector2 XRange { get; private set; } = new Vector2();
    public Vector2 YRange { get; private set; } = new Vector2();
    public Vector2 ZRange { get; private set; } = new Vector2();
    public Vector3 Dimensions { get; private set; } = new Vector3(); // X width, z length, and y height

    public bool Parked { get; protected set; } = false;
    //public bool ParkBrakeEngaged { get; private set; } = false; // Start with park brake off

    private VelocityIDResource _baseVelProp;

    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
	{
		base._Ready();
        _bb = this.GetFirstChildOfInterface<IBlackboard>();
        _aiNav = this.GetFirstChildOfType<AINav3DComponent>();
        OccupantComp = this.GetFirstChildOfType<VehicleOccupantsComponent>();
        if (OccupantComp != null)
        {
            OccupantComp.OccupantsChanged += OnOccupantsChanged;
        }
        else
        {
            Parked = false;
        }


        _bb.SetVar(BBDataSig.AINavComp, _aiNav);
        _bb.SetVar(BBDataSig.Agent, this);

        if (Engine.IsEditorHint()) { return; }
		_wheels = this.GetChildrenOfType<Node3D>().ToList();
        _collShape = this.GetFirstChildOfType<CollisionShape3D>();
        //_collShape.MakeConvexFromSiblings();

        _frontWheelXPos = float.MinValue;
		foreach (var wheel in _wheels)
		{
			if (wheel.Position.X > _frontWheelXPos)
			{
                _frontWheelXPos = wheel.Position.X;
			}
		}

        LinearDamp = VelocityProperties.VelocityIds[0].Friction;

        _baseVelProp = VelocityProperties.VelocityIds[0].Duplicate() as VelocityIDResource;


        GD.Print("VEHICLE determined center of mass: ", CenterOfMass);
		GD.Print("Front Wheel drive determined loc: ", new Vector3(_frontWheelXPos, CenterOfMass.Y, CenterOfMass.Z));
    }
    public override void _Process(double delta)
	{
		base._Process(delta);
	}
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
        if (Engine.IsEditorHint()) { return; }

        if (_aiNav == null || VelocityProperties == null || VelocityProperties.VelocityIds.Count == 0)
        {
            GD.PrintErr($"{Name}: AI Nav or VelocityProperties not set up.");
            return;
        }
        if (!_aiNav.NavigationEnabled)
        {
            return;
        }

        var velProps = VelocityProperties.VelocityIds[0]; // Assuming this is always valid
        velProps.Acceleration = _baseVelProp.Acceleration * Global.Remap(_debugDriverApt.DriverAggression, 0.1f, 2.0f, 0.9f, 1.2f);
        //GD.Print($"Vehicle Acceleration: {velProps.Acceleration}");
        Vector3 desiredDir = _aiNav.WeightedNextPathDirection.Normalized(); 

        //GD.Print("desired vehicle direction: ", direction);
        // Determine actual driving inputs based on _throttleInput, _steeringInput, _brakeInput
        // and desiredDirFromNav if _isNavigationActive.
        // This section will be simplified: if navigation is active, it tries to follow.
        // Manual inputs (_throttleInput, _steeringInput) can override or blend.


        // --- Corrected Force Application Point for Front-Wheel Drive ---
        // _frontWheelXPos should be the local X-coordinate of the front axle.
        // If vehicle origin is center and -X is forward, _frontWheelXPos is negative (e.g., -1.5f).
        Vector3 localForcePoint = new Vector3(_frontWheelXPos, CenterOfMass.Y, CenterOfMass.Z); // Or a more precise Y/Z if needed
        Vector3 forceApplicationPointGlobal = ToGlobal(localForcePoint);

        //Find angle vehicle is facing
        var facingAngle = Rotation.Y;
        var facingForce = new Vector3(Mathf.Cos(facingAngle), 0f, Mathf.Sin(facingAngle));


        Vector3 forceLoc; 
        if (_allWheelDrive)
		{
            //forceLoc = CenterOfMass;

            // basis.z is forward facing vector, but for car it seems -basis.x is front of car
            forceLoc = ToGlobal(CenterOfMass);//-Transform.Basis.X + (-Transform.Basis.X * CenterOfMass);
        }
		else
		{
            //forceLoc =
            //    new Vector3(_frontWheelXPos, CenterOfMass.Y, CenterOfMass.Z);

            // basis.z is forward facing vector, but for car it seems -basis.x is front of car
            //forceLoc = -Transform.Basis.X + (-Transform.Basis.X * _frontWheelXPos);
            forceLoc = forceApplicationPointGlobal;
        }
        forceLoc = -Transform.Basis.X + (-Transform.Basis.X * _frontWheelXPos);



        if (Parked)
        {
            // Apply strong braking if park brake is on and vehicle is moving
            if (LinearVelocity.LengthSquared() > 0.01f)
            {
                // Use CenterOfMassOffset or a specific point for brake force
                ApplyForce(-LinearVelocity.Normalized() * velProps.Acceleration * 0.5f /*strong brake multiplier*/, forceLoc);
            }
            return;
        }


        // VEHICLE DRIVING LOGIC
        Vector3 forwardDir = -Transform.Basis.X.Normalized(); // Assuming -X is forward
        var forwardFacingDir = -Transform.Basis.X.Normalized();
        //GD.Print($"\nForward Dir: {forwardFacingDir}" +
        //    $"\nDesired Dir: {desiredDir}");



        // compare front of vehicle to desired direction
        var desiredDirAngle = desiredDir.AngleTo(forwardFacingDir);
        var signedAngle = forwardFacingDir.SignedAngleTo(desiredDir, Vector3.Up);
        //GD.Print($"Desired dir angle: {Mathf.RadToDeg(desiredDirAngle)}" +
        //    $"\nSigned angle: {Mathf.RadToDeg(signedAngle)}");

        // get dot product of facing and desired direction
        // higher the dot product, the more aligned they are, thus more force is applied
        var forwardDesDot = forwardFacingDir.Dot(desiredDir);

        var baseBrakeSpeed = 3f;
        var needBrakeSpeed = baseBrakeSpeed / _debugDriverApt.DriverAggression; 
        if (desiredDir == Vector3.Zero || 
            (forwardDesDot < 0f && LinearVelocity.Length() > needBrakeSpeed))
        {
            //var brakingForce = VelocityProperties.VelocityIds[0].BrakingFrictionMod;
            // ADD BRAKING FORCE
            var brakingForcePercentage = 0.15f;
            ApplyForce(/*-forwardFacingDir*/-LinearVelocity.Normalized() * velProps.Acceleration * brakingForcePercentage, forceLoc);
            return;
        }

        // Threshold for "extremely low" speed
        float lowSpeedThreshold = 0.5f;

        // REVERSE LOGIC CONDITIONAL
        // If we need to turn more than 180 and are nearly stopped, back up and turn
        if (forwardDesDot < 0f && LinearVelocity.Length() < lowSpeedThreshold)
        {
            Vector3 reverseDir = -forwardFacingDir;

            // Calculate the angle between reverseDir and desiredDir
            float angle = reverseDir.AngleTo(desiredDir); // in radians

            // Define the maximum angle you want to allow for reverse turning (e.g., 90 degrees)
            float maxReverseTurnAngle = Mathf.DegToRad(90f);

            // Map angle to [0, 1] for slerp: 0 = straight back, 1 = max allowed turn
            float slerpFactor = Mathf.Clamp(angle / maxReverseTurnAngle, 0f, 1f);

            // Slerp between reverseDir and desiredDir by the computed factor
            Vector3 desiredReverseDir = reverseDir.Slerp(desiredDir, slerpFactor).Normalized();

            // Clamp the result to your actual allowed per-frame turn angle
            float maxPerFrameReverseTurn = Mathf.DegToRad(15f);
            Vector3 clampedReverseDir = GetClampedDrivingDirection(reverseDir, desiredReverseDir, maxPerFrameReverseTurn);

            // Apply a small reverse force to back up and turn
            var accel = velProps.Acceleration;
            //float reverseAccel = accel * 0.85f; // lower acceleration for reverse

            // accel percentage calc
            // slerp factor = 0 when straight back, 1 when max turn
            //var accelPercent = Global.Remap(slerpFactor, 0, 1f, 0.8f, 0.95f);
            var accelPercent = Global.Remap(slerpFactor, 0, 1f, 1.5f, 0.9f);
            //accelPercent = accelPercent * _debugDriverApt.DriverAggression;

            ApplyForce(clampedReverseDir * accelPercent * accel, forceLoc);

            //GD.Print($"\nreverse accel percent {accelPercent}" +
            //    $"\ndesired reverse dir: {desiredReverseDir}" +
            //    $"\nactual reverse dir: {clampedReverseDir}");
            return;
        }



        var minDot = 0.65f;
        var clampedDot = Mathf.Clamp(forwardDesDot, minDot, 1f);
        //var speedMult = Mathf.Clamp(forwardDesDot, 0.7f, 1f);
        var minSpeedMult = 0.65f * _debugDriverApt.DriverAggression;
        minSpeedMult = Mathf.Clamp(minSpeedMult, 0.5f, 1f);

        var maxSpeedMult = 1f;// * _debugDriverApt.DriverAggression;

        //0.5f is 90 degree turn, so clamp speed min at that
        var speedMult = Global.Remap(clampedDot, minDot, 1f, minSpeedMult, maxSpeedMult); 

        //GD.Print($"Desired dir dot prod: {forwardDesDot}" +
        //    $"\nspeed Mult: {speedMult}");

        // only allow driving in slight arc
        var maxDrivingDegAng = 30f * _debugDriverApt.DriverAggression;
        maxDrivingDegAng = Mathf.Clamp(maxDrivingDegAng, 15f, 90f);
        var maxDrivingTurnAngle = Mathf.DegToRad(maxDrivingDegAng);

        var drivingDir = GetClampedDrivingDirection(forwardFacingDir, desiredDir, maxDrivingTurnAngle);
        //Vector3 drivingDir;
        //if (desiredDirAngle > maxDrivingAngle)
        //{
        //    GD.Print("Desired dir angle is greater than max driving angle");
        //    //adjust desired direction to clamp at max angle

        //    // find if angle is pos or neg relative to force loc
        //    if (signedAngle > 0)
        //    {
        //        drivingDir = forwardFacingDir.Rotated(Vector3.Up, maxDrivingAngle);
        //    }
        //    else
        //    {
        //        drivingDir = forwardFacingDir.Rotated(Vector3.Up, -maxDrivingAngle);
        //    }
        //}
        //else
        //{
        //    GD.Print("Desired dir angle is less than max driving angle");
        //    drivingDir = desiredDir;
        //}

        // get dot product of facing and driving direction
        // higher the dot product, the more aligned they are, thus more force is applied
        var driveDot = forwardFacingDir.Dot(drivingDir);
        var minDriveDot = Mathf.Cos(maxDrivingTurnAngle);
        speedMult = Global.Remap(driveDot, minDriveDot, 1f, minSpeedMult, maxDrivingTurnAngle);

        //GD.Print($"final driving dir: {drivingDir}" +
        //    $"\ndriving dot prod: {driveDot}");

       

        if (LinearVelocity.Length() >= velProps.MaxSpeed)
        {
            //ApplyCentralForce(direction * velProps.MaxSpeed);
            ApplyForce(drivingDir * velProps.MaxSpeed * speedMult, forceLoc);
            return;
        }
        else
        {
            //ApplyCentralForce(direction * velProps.Acceleration);
            ApplyForce(drivingDir * velProps.Acceleration * speedMult, forceLoc);
        }
        //ApplyForce(direction * _maxSpeed, ToGlobal(forceLoc));
        //GD.Print($"Lin velocity: {LinearVelocity}" +
        //	$"\nAng Vel: {AngularVelocity}");
    }
    public static Vector3 GetClampedDrivingDirection(Vector3 forwardDir, Vector3 desiredDir, float maxTurnAngleRad)
    {
        // Ensure both vectors are normalized
        forwardDir = forwardDir.Normalized();
        desiredDir = desiredDir.Normalized();

        // Calculate the signed angle between forward and desired, around the Y axis (up)
        float signedAngle = forwardDir.SignedAngleTo(desiredDir, Vector3.Up);

        // Clamp the angle to the max turn angle
        float clampedAngle = Mathf.Clamp(signedAngle, -maxTurnAngleRad, maxTurnAngleRad);

        // Rotate the forward direction by the clamped angle
        Vector3 newDir = forwardDir.Rotated(Vector3.Up, clampedAngle).Normalized();

        return newDir;
    }
    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        // Convert global velocity to local
        Vector3 localVelocity = Transform.Basis.Inverse() * state.LinearVelocity;
        var lateralVelocity = localVelocity.Z;
        //GD.Print("\ncurr lateral movement: ", lateralVelocity);

        var maxSpeed = VelocityProperties.VelocityIds[0].MaxSpeed;
        var lateralDamping = Global.Remap(localVelocity.Length(), 0f, maxSpeed, LateralFriction.X, LateralFriction.Y);
        //GD.Print($"lateral damping: {lateralDamping}" +
        //    $"curr vel: {localVelocity.Length()}");

        if (_debugDriverApt.DriverAggression > 1f)
        {
            return;
            //lateralDamping = 1f;
        }
        float damping = Mathf.Pow(lateralDamping, state.Step / (1f / 60f));
        localVelocity.Z *= damping;
        //localVelocity.Z *= lateralDamping;

        // Convert back to global and set
        state.LinearVelocity = Transform.Basis * localVelocity;
        //GD.Print($"new lateral movement: {localVelocity.Z}");


        //// Example: More grip at low speed, less at high speed
        //float speed = state.LinearVelocity.Length();
        //float minFriction = 6.0f;
        //float maxFriction = 12.0f;
        //float minFrictionSpeed = 15f;//VelocityProperties.VelocityIds[0].MaxSpeed;
        ////40.0f; // Tune as needed

        //// Lerp friction: high at low speed, low at high speed
        //float lateralFrictionCoef = Mathf.Lerp(maxFriction, minFriction, Mathf.Clamp(speed / minFrictionSpeed, 0.1f, 1f));

        //float lateralFrictionForce = -lateralVelocity * lateralFrictionCoef;

        //if (Mathf.Abs(lateralVelocity) > 0.01f)
        //{
        //    Vector3 localFriction = new Vector3(0, 0, lateralFrictionForce);
        //    Vector3 globalFriction = Transform.Basis * localFriction;
        //    state.ApplyCentralImpulse(globalFriction * state.Step);

        //    GD.Print($"friction coeff: {lateralFrictionCoef} " +
        //    $"\nLateral velocity: {lateralVelocity}" +
        //    $"\nFriction force: {lateralFrictionForce}" +
        //    $"\nglobal friction: {globalFriction}");
        //}

    }
    // Pseudocode for terrain detection
    float GetTerrainFrictionMultiplier()
    {
        // Assume you have a RayCast3D node named "GroundRay"
        var ray = GetNode<RayCast3D>("GroundRay");
        if (ray.IsColliding())
        {
            var collider = ray.GetCollider() as CollisionObject3D;
            // You can check collider's name, group, or a custom property
            if (collider.IsInGroup("Mud"))
                return 0.5f;
            if (collider.IsInGroup("Ice"))
                return 0.2f;
            if (collider.IsInGroup("Road"))
                return 1.0f;
        }
        return 1.0f; // Default
    }
    public AnimDirection GetAnimDirection()
    {
        throw new System.NotImplementedException();
    }

    public Dir4 GetFaceDirection()
    {
        throw new System.NotImplementedException();
    }

    public Dir4 GetDesiredFaceDirection()
    {
        throw new System.NotImplementedException();
    }

    public Vector2 GetDesiredDirection()
    {
        var dir = _aiNav.WeightedNextPathDirection;
        return new Vector2(dir.X, dir.Z);
    }

    public Vector2 GetDesiredDirectionNormalized()
    {
        return GetDesiredDirection().Normalized();
    }

    public bool WantsJump()
    {
        throw new System.NotImplementedException();
    }

    public bool WantsAttack()
    {
        throw new System.NotImplementedException();
    }

    public float TimeSinceAttackRequest()
    {
        throw new System.NotImplementedException();
    }

    public bool WantsStrafe()
    {
        throw new System.NotImplementedException();
    }

    public float GetRunSpeedMult()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetVelocity()
    {
        return LinearVelocity;
    }

    public void AppendAddMod(float mod)
    {
        throw new System.NotImplementedException();
    }

    public void AppendMultMod(float mod)
    {
        throw new System.NotImplementedException();
    }

    public void SetAddMod(float mod)
    {
        throw new System.NotImplementedException();
    }

    public void SetMultMod(float mod)
    {
        throw new System.NotImplementedException();
    }

    public void ApplyGravity(float delta)
    {
        throw new System.NotImplementedException();
    }

    public void ApplyCustomGravity(float delta, Vector3 customGravity, float weightPercentage = 0)
    {
        throw new System.NotImplementedException();
    }

    public void ApplyCustomVelocity(Vector3 velocity)
    {
        throw new System.NotImplementedException();
    }

    public void Move()
    {
        throw new System.NotImplementedException();
    }

    public void CustomMove(Vector3 velocity)
    {
        throw new System.NotImplementedException();
    }

    public void ResetVelocity()
    {
        throw new System.NotImplementedException();
    }

    public Node GetInterfaceNode()
    {
        throw new System.NotImplementedException();
    }
    #endregion
    #region PUBLIC_API
    // --- Public API for Controllers ---
    public void SetParkStatus(bool engage)
    {
        Parked = engage;
        if (engage)
        {
            _aiNav.DisableNavigation();
        }
        else
        {
            _aiNav.EnableNavigation(); // Re-enable navigation if park brake is disengaged
        }
    }

    public void SetNavigationTarget(Vector3 globalTargetPosition)
    {

        if (_aiNav != null)
        {
            _aiNav.EnableNavigation();
            _aiNav.SetTargetPosition(globalTargetPosition);
        }
        SetParkStatus(false);
    }

    public void ClearNavigationTarget()
    {
        if (_aiNav != null)
        {
            _aiNav.DisableNavigation(); // Assumes AINavComponent has this method
        }
        // Vehicle will now brake/coast based on _PhysicsProcess logic when _isNavigationActive is false
    }

    #endregion
    #region COMPONENT_HELPER
    #endregion
    #region SIGNAL_LISTENERS
    private void OnOccupantsChanged(object sender, List<VehicleSeat> e)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region VELOCITY_PROPERTIES
    [Export]
    public Char3DVelocityProperties VelocityProperties { get; private set; }
    #endregion
}
