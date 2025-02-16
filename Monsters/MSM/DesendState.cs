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
    private AnimDirection _descendAnimDir;
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
        _descendAnimDir = _climberComp.ClimbingDir.GetAnimDir();
        //GD.Print("DESCEND ANIM DIRECTION: ",  _descendDir);
        BB.GetVar<IAnimComponent>(BBDataSig.Anim).StartAnim(_animName + _descendAnimDir.GetAnimationString());

        _body.Velocity = Vector3.Zero;
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void ProcessFrame(float delta)
    {
        base.ProcessFrame(delta);
        
    }
    public override void ProcessPhysics(float delta)
    {
        base.ProcessPhysics(delta);
        _inputDir = MoveComp.GetDesiredDirection();

        if (_inputDir.IsZeroApprox())
        {
            EmitSignal(SignalName.TransitionState, this, _climbIdleState);
            return;
        }
        else if (MoveComp.WantsJump())
        {
            _climberComp.EjectRequested = true;
        }

        var desiredOrthogDirection = _inputDir.GetOrthogDirection();
        //GD.Print($"desiredAnimDir: {desiredAnimDirection}; descend dir: {_descendDir}");
        if (_climberComp.ClimbingDir == desiredOrthogDirection && _inputDir.Y != 0)
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

        if (_climberComp.ClimbingDir.GetOppositeDir() != desiredOrthogDirection)
        {
            return;
        }

        Vector3 velocity = _body.Velocity;
        velocity.X = 0; velocity.Z = 0;
        var descendInput = -_inputDir.Length();//Mathf.Abs(_inputDir.Y);

        velocity.Y = descendInput * Monster.ClimbSpeed;

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
