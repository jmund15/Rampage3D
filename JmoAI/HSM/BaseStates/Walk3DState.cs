using Godot;
using Godot.Collections;

[Tool]
public partial class Walk3DState : Base3DState
{
	#region STATE_VARIABLES
	[Export]
	public string AnimName { get; protected set; } = "walk";
    
	[Export(PropertyHint.NodeType, "State")]
	private State _onNoInputState;
    [Export(PropertyHint.NodeType, "State")]
    private State _onJumpInputState;
    [Export(PropertyHint.NodeType, "State")]
    private State _onNotOnFloorState;

    private Vector2 _inputDirection = new Vector2();
    private OrthogDirection _orthogDir;
    private AnimDirection _currAnimDir;
    #endregion
    #region STATE_UPDATES
    public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
	}
	public override void Enter(Dictionary<State, bool> parallelStates)
	{
		base.Enter(parallelStates);

        AnimPlayer.Play(AnimName +
            MoveComp.GetAnimDirection().GetAnimationString());
    }
	public override void Exit()
	{
		base.Exit();
	}
	public override void ProcessFrame(float delta)
	{
		base.ProcessFrame(delta);
        _inputDirection = MoveComp.GetDesiredDirection();
        
        if (MoveComp.WantsJump() && Body.IsOnFloor())
        {
            EmitSignal(SignalName.TransitionState, this, _onJumpInputState);
        }

        _orthogDir = _inputDirection.GetOrthogDirection();
        var animDir = _orthogDir.GetAnimDir();
        if (_currAnimDir != animDir)
        {
            _currAnimDir = animDir;
            BB.GetVar<AnimationPlayer>(BBDataSig.Anim).Play(AnimName + _currAnimDir.GetAnimationString());
        }
        BB.GetVar<Sprite3D>(BBDataSig.Sprite).FlipH = _orthogDir.GetFlipH();
    }
	public override void ProcessPhysics(float delta)
	{
		base.ProcessPhysics(delta);
        Vector3 velocity = Body.Velocity;

        //if (!Body.IsOnFloor())
        //{
        //    EmitSignal(SignalName.TransitionState, this, _onNotOnFloorState);
        //}

        if (_inputDirection.IsZeroApprox())
        {
            EmitSignal(SignalName.TransitionState, this, _onNoInputState);
            return;
        }
        var orthogDir = _inputDirection.GetOrthogDirection();
        Vector3 direction = orthogDir.GetVector3();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Monster.Speed;
            velocity.Z = direction.Z * Monster.Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Body.Velocity.X, 0, Monster.Speed);
            velocity.Z = Mathf.MoveToward(Body.Velocity.Z, 0, Monster.Speed);
        }

        Body.Velocity = velocity;
        Body.MoveAndSlide();
    }
	public override void HandleInput(InputEvent @event)
	{
		base.HandleInput(@event);
	}
	#endregion
	#region STATE_HELPER
	#endregion
}
