using Godot;
using Godot.Collections;

[Tool]
public partial class DesendState : Base3DState
{
    #region STATE_VARIABLES
    [Export]
    private string _animName = "desend";
    private CharacterBody3D _body;

    [Export(PropertyHint.NodeType, "State")]
    private State _climbIdleState;
    [Export(PropertyHint.NodeType, "State")]
    private State _landState;
    [Export(PropertyHint.NodeType, "State")]
    private State _climbState;

    private ClimberComponent _climberComp;
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
        _climberComp = BB.GetVar<ClimberComponent>(BBDataSig.ClimberComp);
        //GD.Print("on descend enter curr anim: ", AnimPlayer.CurrentAnimation);
        _descendDir = _climberComp.ClimbingDir.GetAnimDir();
        //GD.Print("DESCEND ANIM DIRECTION: ",  _descendDir);
        BB.GetVar<AnimationPlayer>(BBDataSig.Anim).Play(_animName + _descendDir.GetAnimationString());

        _body.Velocity = Vector3.Zero;
    }
    public override void Exit()
    {
        base.Exit();
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
            _climberComp.EjectRequested = true;
        }
    }
    public override void ProcessPhysics(float delta)
    {
        base.ProcessPhysics(delta);
        var desiredAnimDirection = _inputDir.GetAnimDir();
        //GD.Print($"desiredAnimDir: {desiredAnimDirection}; descend dir: {_descendDir}");
        if (_descendDir == desiredAnimDirection && _inputDir.Y != 0)
        {
            EmitSignal(SignalName.TransitionState, this, _climbState);
            return;
        }
        if (_body.IsOnFloor())
        {
            _climberComp.StopClimb();
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
    #endregion
}
