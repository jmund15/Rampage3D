using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class IdleState : State
{
    #region STATE_VARIABLES
    [Export]
    public string AnimName { get; protected set; }
    private Monster _body;
    private IMovementComponent _moveComp;

    [Export(PropertyHint.NodeType, "State")]
    private State _walkState;
    [Export(PropertyHint.NodeType, "State")]
    private State _jumpState;
    [Export(PropertyHint.NodeType, "State")]
    private State _fallState;

    private Vector2 _inputDirection = new Vector2();
    private bool _bufferingMovementTransition = false;
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

        BB.GetVar<IAnimComponent>(BBDataSig.Anim).StartAnim(AnimName + 
            _moveComp.GetAnimDirection().GetAnimationString());
        _bufferingMovementTransition = false;
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void ProcessFrame(float delta)
    {
        base.ProcessFrame(delta);
        _inputDirection = _moveComp.GetDesiredDirection();
        if (!_inputDirection.IsZeroApprox())
        {
            EmitSignal(SignalName.TransitionState, this, _walkState);
        }
        else if (_moveComp.WantsJump() && _body.IsOnFloor())
        {
            EmitSignal(SignalName.TransitionState, this, _jumpState);
        }
        //     !_bufferingMovementTransition)
        //{ //BUFFER AFTER MOVEMENT CHANGES
        //    GetTree().CreateTimer(Global.MovementTransitionBufferTime).Timeout += ChangeMovementState;
        //    _bufferingMovementTransition = true;
        //}
    }
    public override void ProcessPhysics(float delta)
    {   
        base.ProcessPhysics(delta);
        Vector3 velocity = _body.Velocity;

        if (!_body.IsOnFloor())
        {
            EmitSignal(SignalName.TransitionState, this, _fallState);
        }

        velocity.X = 0; // TODO: slow down?
        velocity.Z = 0; // slow down

        _body.Velocity = velocity;
        _body.MoveAndSlide();
    }
    public override void HandleInput(InputEvent @event)
    {
        //if (@event.IsActionPressed(_body.AttackInput))
        //{
        //    EmitSignal(SignalName.TransitionState, this, _attackState);
        //}
        if (_inputDirection.Length() > 0.1f)
        {
            //EmitSignal(SignalName.TransitionState, this, _walkState);
        }
    }
    #endregion
    #region STATE_HELPER
    private void ChangeMovementState()
    {
        
        _bufferingMovementTransition = false;
    }
    

    #endregion
}
