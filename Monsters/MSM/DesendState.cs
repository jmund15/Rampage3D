using Godot;
using Godot.Collections;

[Tool]
public partial class DesendState : AnimState
{
    #region STATE_VARIABLES
    private CharacterBody3D _body;

    [Export(PropertyHint.NodeType, "State")]
    private State _climbIdleState;
    [Export(PropertyHint.NodeType, "State")]
    private State _landState;
    [Export(PropertyHint.NodeType, "State")]
    private State _climbState;
    [Export(PropertyHint.NodeType, "State")]
    private State _jumpState;

    private ClimbableComponent _climbComp;
    private Vector2 _inputDir;
    private AnimDirection _descendDir;
    #endregion
    #region STATE_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        base.Init(agent, bb);
        _body = Agent as CharacterBody3D;
    }
    public override void Enter(Dictionary<State, bool> parallelStates)
    {
        base.Enter(parallelStates);
        _climbComp = BB.GetVar<ClimbableComponent>(BBDataSig.CurrClimbComp);
        //GD.Print("on descend enter curr anim: ", AnimPlayer.CurrentAnimation);
        _descendDir = MoveComp.GetAnimDirection();
        //GD.Print("DESCEND ANIM DIRECTION: ",  _descendDir);
        BB.GetVar<AnimationPlayer>(BBDataSig.Anim).Play(AnimName +
            IMovementComponent.GetFaceDirectionString(_descendDir));

        _body.Velocity = Vector3.Zero;
    }
    public override void Exit()
    {
        //base.Exit();
    }
    public override void ProcessFrame(float delta)
    {
        base.ProcessFrame(delta);
        _inputDir = MoveComp.GetDesiredDirection();

        if (_inputDir.Y == 0)
        {
            EmitSignal(SignalName.TransitionState, this, _climbIdleState);
        }
        else if (MoveComp.WantsJump())
        {
            EmitSignal(SignalName.TransitionState, this, _jumpState);
        }
    }
    public override void ProcessPhysics(float delta)
    {
        base.ProcessPhysics(delta);
        var desiredAnimDirection = IMovementComponent.GetAnimDirectionFromVector(_inputDir);
        if (_descendDir == desiredAnimDirection && _inputDir.Y != 0)
        {
            EmitSignal(SignalName.TransitionState, this, _climbState);
            return;
        }
        if (_body.IsOnFloor())
        {
            EmitSignal(SignalName.TransitionState, this, _landState);
            return;
        }
        //BB.GetVar<Sprite3D>(BBDataSig.Sprite).FlipH = IMovementComponent.GetDesiredFlipH(_inputDir);

        Vector3 velocity = _body.Velocity;
        velocity.X = 0; velocity.Z = 0;
        var descendInput = -Mathf.Abs(_inputDir.Y);

        velocity.Y = descendInput * Monster.ClimbSpeed;
        AnimPlayer.Play();

        _body.Velocity = velocity;
        _body.MoveAndSlide();
    }
    public override void HandleInput(InputEvent @event)
    {
        base.HandleInput(@event);
    }
    #endregion
    #region STATE_HELPER
    protected override void AnimStateStart()
    {
        
    }
    #endregion
}
