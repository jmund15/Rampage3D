using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public partial class GroundVehicleComponent : RigidBody3D, IMovementComponent
{
	#region COMPONENT_VARIABLES
	private CollisionShape3D _collShape;
	private List<Node3D> _wheels = new List<Node3D>();

	private IBlackboard _bb;
	private AINav3DComponent _aiNav;

	[Export]
	private float _maxSpeed = 1000f;
	[Export]
	private bool _allWheelDrive = true;
	private float _frontWheelXPos;

    public Vector2 XRange { get; private set; } = new Vector2();
    public Vector2 YRange { get; private set; } = new Vector2();
    public Vector2 ZRange { get; private set; } = new Vector2();
    public Vector3 Dimensions { get; private set; } = new Vector3(); // X width, z length, and y height

    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
	{
		base._Ready();
        _bb = this.GetFirstChildOfInterface<IBlackboard>();
        _aiNav = this.GetFirstChildOfType<AINav3DComponent>();

        _bb.SetVar(BBDataSig.AINavComp, _aiNav);
        GD.Print("Set ainav in bb!");

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
        Vector3 direction = (_aiNav.WeightedNextPathDirection); 
        direction = direction.Normalized();
        GD.Print("desired vehicle direction: ", direction);

        Vector3 forceLoc; 
        if (_allWheelDrive)
		{
			forceLoc = CenterOfMass;
        }
		else
		{
            forceLoc =
                new Vector3(_frontWheelXPos, CenterOfMass.Y, CenterOfMass.Z);
        }
        //ApplyForce(direction * _maxSpeed, ToGlobal(forceLoc));
        ApplyCentralForce(direction * _maxSpeed);
        //GD.Print($"Lin velocity: {LinearVelocity}" +
        //	$"\nAng Vel: {AngularVelocity}");
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
    #endregion
    #region COMPONENT_HELPER
    #endregion
    #region SIGNAL_LISTENERS
    #endregion
}
