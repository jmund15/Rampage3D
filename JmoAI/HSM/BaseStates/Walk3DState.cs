using Godot;
using Godot.Collections;
using static Godot.TextServer;

[Tool]
public partial class Walk3DState : Base3DState
{
	#region STATE_VARIABLES
	[Export]
	public string AnimName { get; protected set; } = "walk";

    [Export]
    private Timer _reactionTimer;
    [Export]
    private Timer _changeFaceDirTimer;
    [Export]
    private float _reactionTime = 0.2f;
    [Export(PropertyHint.Range, "0,180,0.1,radians_as_degrees")]
    private float _turnAngPerSec = 360f;//Mathf.Pi * 4;

    [Export(PropertyHint.NodeType, "State")]
	private State _onNoInputState;
    [Export(PropertyHint.NodeType, "State")]
    private State _onJumpInputState;
    [Export(PropertyHint.NodeType, "State")]
    private State _onNotOnFloorState;

    private Vector2 _direction;
    private Vector2 _inputDirection = new Vector2();
    private Dir4 _orthogDir;
    private AnimDirection _currAnimDir;

    private bool _useTurnSpeed = true;
    #endregion
    #region STATE_UPDATES
    public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
	}
	public override void Enter(Dictionary<State, bool> parallelStates)
	{
		base.Enter(parallelStates);
        _direction = MoveComp.GetDesiredDirectionNormalized();
        AnimPlayer.StartAnim(AnimName +
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

        var desiredDirection = MoveComp.GetDesiredDirectionNormalized();
        if (_useTurnSpeed)
        {
            var diff = desiredDirection - _direction;
            var diffAngle = _direction.AngleTo(diff);
            while (diffAngle > Mathf.Pi)
            {
                diffAngle -= 2 * Mathf.Pi;
            }
            while (diffAngle < -Mathf.Pi)
            {
                diffAngle += 2 * Mathf.Pi;
            }

            //GD.Print($"desired dir: {desiredDirection}; curr dir: {_direction}");
            var angPerPhysics = _turnAngPerSec * delta;
            if (diffAngle > angPerPhysics)
            {
                _direction = _direction.Rotated(angPerPhysics);
            }
            else if (diffAngle < -angPerPhysics)
            {
                _direction = _direction.Rotated(-angPerPhysics);
            }
            else
            {
                _direction = desiredDirection;
            }
            //GD.Print($"new dir: {_direction}");
        }
        else
        {
            _direction = desiredDirection;
        }
        

        _orthogDir = _direction.GetOrthogDirection();
        var animDir = _orthogDir.GetAnimDir();
        if (_currAnimDir != animDir)
        {
            _currAnimDir = animDir;
            BB.GetVar<IAnimComponent>(BBDataSig.Anim).StartAnim(AnimName + _currAnimDir.GetAnimationString());
        }
        BB.GetVar<ISpriteComponent>(BBDataSig.Sprite).FlipH = _orthogDir.GetFlipH();

        Vector3 direction = _orthogDir.GetVector3();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * (Monster.Speed / 2);
            velocity.Z = direction.Z * (Monster.Speed / 2);
        }
        else
        {
            velocity.X = Mathf.MoveToward(Body.Velocity.X, 0, (Monster.Speed / 2));
            velocity.Z = Mathf.MoveToward(Body.Velocity.Z, 0, (Monster.Speed / 2));
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
