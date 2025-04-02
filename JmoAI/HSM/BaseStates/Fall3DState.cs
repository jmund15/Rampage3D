using Godot;
using Godot.Collections;

[Tool]
public partial class Fall3DState : Base3DState
{
	#region STATE_VARIABLES
	[Export]
    public string AnimName = "fall";
    private IVelocityChar3DComponent _velComp;

    [Export(PropertyHint.NodeType, "State")]
    private State _landFloorState;
    [Export(PropertyHint.NodeType, "State")]
    private State _landWallState;
    [Export(PropertyHint.NodeType, "State")]
    private State _landFallState;


    private Vector2 _inputDir = new Vector2();
    private AnimDirection _currAnimDir;

    private float _fallHeight;
    #endregion
    #region STATE_UPDATES
    public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
        _velComp = BB.GetVar<IVelocityChar3DComponent>(BBDataSig.VelComp);
	}
	public override void Enter(Dictionary<State, bool> parallelStates)
	{
		base.Enter(parallelStates);
        AnimPlayer.StartAnim(AnimName +
            MoveComp.GetAnimDirection().GetAnimationString());

        _fallHeight = Body.Position.Y;
    }
	public override void Exit()
	{
		base.Exit();
	}
	public override void ProcessFrame(float delta)
	{
		base.ProcessFrame(delta);
        _inputDir = MoveComp.GetDesiredDirection();


        
    }
	public override void ProcessPhysics(float delta)
	{
		base.ProcessPhysics(delta);
        if (Body.IsOnFloor())
        {
            var landHeight = Body.Position.Y;
            var fallDist = _fallHeight - landHeight;
            //GD.Print("jump fall dist: ", fallDist);
            if (fallDist > _velComp.MaxLandVelocity)
            {
                EmitSignal(SignalName.TransitionState, this, _landFallState);
            }
            else
            {
                EmitSignal(SignalName.TransitionState, this, _landFloorState);
            }
        }

        if (Body.Velocity.X > Global.CHANGE_DIR_VEL_REQ || Body.Velocity.Z > Global.CHANGE_DIR_VEL_REQ)
        {
            var velDir = Body.Velocity.GetOrthogDirection();
            //GD.Print("velDir: ", velDir, "; curr dir: ", _moveComp.GetFaceDirection());
            if (velDir != MoveComp.GetFaceDirection())
            {
                //GD.Print("Velocity when changing dirs: ", Body.Velocity);
                //var prevPos = AnimPlayer.CurrentAnimationPosition;
                //PlayAnim.AnimWithOrthog(BB, AnimName, velDir);
                //AnimPlayer.Seek(prevPos, true);
                BB.GetVar<ISpriteComponent>(BBDataSig.Sprite).FlipH = velDir.GetFlipH();
                AnimPlayer.UpdateAnim(AnimName + velDir.GetAnimDir());
            }
        }

        Vector3 velocity = Body.Velocity;


        //velocity += Body.GetWeightedGravity() * delta;

        //GD.Print("body velocity after gravity: ", velocity);

        //if (_inputDirection.IsZeroApprox())
        //{
        //    velocity.X = Mathf.MoveToward(Body.Velocity.X, 0, Monster.AirHorizontalFriction);
        //    velocity.Z = Mathf.MoveToward(Body.Velocity.Z, 0, Monster.AirHorizontalFriction);
        //    Body.Velocity = velocity;
        //    Body.MoveAndSlide();
        //    return;
        //}
        //var orthogDir = _inputDir.GetOrthogDirection();
        //Vector3 direction = orthogDir.GetVector3();

        Vector3 direction;
        if (_inputDir.IsZeroApprox())
        {
            direction = Vector3.Zero;
        }
        else
        {
            var orthogDir = _inputDir.GetOrthogDirection();
            direction = orthogDir.GetVector3();
        }
        _velComp.SetMovement(delta, direction, VelocityType.Air);
        _velComp.ApplyGravity(delta);
        _velComp.Move();

        ////velocity.X = direction.X * Monster.AirSpeed;
        ////velocity.Z = direction.Z * Monster.AirSpeed;
        //velocity.X = Mathf.MoveToward(Body.Velocity.X, direction.X * Monster.AirMaxSpeed, Monster.AirAcceleration);
        //velocity.Z = Mathf.MoveToward(Body.Velocity.Z, direction.Z * Monster.AirMaxSpeed, Monster.AirAcceleration);


        //Body.Velocity = velocity;
        //Body.MoveAndSlide();
    }
	public override void HandleInput(InputEvent @event)
	{
		base.HandleInput(@event);
	}
	#endregion
	#region STATE_HELPER
	#endregion
}
