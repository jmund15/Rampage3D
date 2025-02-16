using Godot;
using Godot.Collections;

[Tool]
public partial class ClimbIdleState : State
{
	#region STATE_VARIABLES
	[Export]
	private string _animName;

	[Export(PropertyHint.NodeType, "State")]
	private State _climbState;
    [Export(PropertyHint.NodeType, "State")]
    private State _descendState;

    private Monster _body;
    private IMovementComponent _moveComp;
    private ClimberComponent _climberComp;


    private Vector2 _inputDirection = new Vector2();
    #endregion
    #region STATE_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        base.Init(agent, bb);
        _body = Agent as Monster;
        _moveComp = BB.GetVar<IMovementComponent>(BBDataSig.MoveComp);
    }
    public override void Enter(Dictionary<State, bool> parallelStates)
    {
        base.Enter(parallelStates);

        _climberComp = BB.GetVar<ClimberComponent>(BBDataSig.ClimberComp);
        if (!_climberComp.IsClimbing) {
            _climberComp.EjectRequested = true;
        }

        BB.GetVar<IAnimComponent>(BBDataSig.Anim).StartAnim(_animName +
            _moveComp.GetAnimDirection().GetAnimationString());

        _body.Velocity = Vector3.Zero;
        _body.MoveAndSlide();
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void ProcessFrame(float delta)
    {
        base.ProcessFrame(delta);
        _inputDirection = _moveComp.GetDesiredDirection();
        
        if (_inputDirection.IsZeroApprox()) { return; }

        if (_inputDirection.GetOrthogDirection() == _climberComp.ClimbingDir)
        {
            EmitSignal(SignalName.TransitionState, this, _climbState);
        }
        else if (_inputDirection.GetOrthogDirection() == _climberComp.ClimbingDir.GetOppositeDir())
        {
            EmitSignal(SignalName.TransitionState, this, _descendState);
        }
        else if (_moveComp.WantsJump())
        {
            _climberComp.EjectRequested = true;
        }
    }
    public override void ProcessPhysics(float delta)
    {
        base.ProcessPhysics(delta);
        if (_moveComp.WantsJump())
        {
            _climberComp.EjectRequested = true;
        }
    }
    #endregion
    #region STATE_HELPER
    #endregion
}
