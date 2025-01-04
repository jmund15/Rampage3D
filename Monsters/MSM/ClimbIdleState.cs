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
    [Export(PropertyHint.NodeType, "State")]
    private State _jumpState;

    private Monster _body;
    private IMovementComponent _moveComp;
    private ClimbableComponent _climbableComp;


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

        var climbComp = BB.GetVar<ClimbableComponent>(BBDataSig.CurrClimbComp);
        if (climbComp == null)
        {
            EmitSignal(SignalName.TransitionState, this, _jumpState);
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
                if (_inputDirection.Y > 0)
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
                if (_inputDirection.Y > 0)
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
            EmitSignal(SignalName.TransitionState, this, _jumpState);
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
