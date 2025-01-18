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

        BB.GetVar<AnimationPlayer>(BBDataSig.Anim).Play(_animName +
            IMovementComponent.GetFaceDirectionString(_moveComp.GetAnimDirection()));

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
        
        if (_inputDirection.Y != 0)
        {
            if (_moveComp.GetAnimDirection() == AnimDirection.Up)
            {
                if (_inputDirection.Y < 0) // in 2d input, negative is up
                {
                    EmitSignal(SignalName.TransitionState, this, _climbState);
                }
                else
                {
                    EmitSignal(SignalName.TransitionState, this, _descendState);
                }
            }
            else
            {
                if (_inputDirection.Y < 0) // when facing down, actions are reversed
                {
                    EmitSignal(SignalName.TransitionState, this, _descendState);
                }
                else
                {
                    EmitSignal(SignalName.TransitionState, this, _climbState);
                }
            }
        }
        else if (_moveComp.WantsJump())
        {
            _climberComp.EjectRequested = true;
        }
    }
    public override void ProcessPhysics(float delta)
    {
        base.ProcessPhysics(delta);
        
    }
    #endregion
    #region STATE_HELPER
    #endregion
}
