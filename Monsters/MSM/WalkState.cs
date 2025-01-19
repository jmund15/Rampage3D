using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class WalkState : State
{
    #region STATE_VARIABLES
    [Export]
    public string AnimName { get; protected set; }
    private Monster _body;
    private IMovementComponent _moveComp;
    private ClimberComponent _climberComp;

    [Export(PropertyHint.NodeType, "State")]
    private State _idleState;
    [Export(PropertyHint.NodeType, "State")]
    private State _jumpState;
    [Export(PropertyHint.NodeType, "State")]
    private State _fallState;
    [Export(PropertyHint.NodeType, "State")]
    private State _startClimbState;


    private bool _bufferingClimbingTransition = false;
    private float _climbBufferTime = 0.1f;

    private Vector2 _inputDir;
    private AnimDirection _currAnimDir;
    #endregion
    #region STATE_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        base.Init(agent, bb);
        _body = Agent as Monster;
        _moveComp = BB.GetVar<IMovementComponent>(BBDataSig.MoveComp);
        _climberComp = BB.GetVar<ClimberComponent>(BBDataSig.ClimberComp);
    }
    public override void Enter(Dictionary<State, bool> parallelStates)
    {
        base.Enter(parallelStates);
        _climberComp.FoundClimbable += OnFoundClimbable;
        _inputDir = _moveComp.GetDesiredDirection();
        if (_inputDir.Y > 0)
        {
            _currAnimDir = IMovementComponent.GetAnimDirectionFromVector(_moveComp.GetDesiredDirection());
        }
        else
        {
            _currAnimDir = _moveComp.GetAnimDirection();
        }

        BB.GetVar<AnimationPlayer>(BBDataSig.Anim).Play(AnimName + 
            IMovementComponent.GetFaceDirectionString(_currAnimDir));
        _bufferingClimbingTransition = false;
    }
    public override void Exit()
    {
        base.Exit();
        _bufferingClimbingTransition = false;
        _climberComp.FoundClimbable -= OnFoundClimbable;
    }
    public override void ProcessFrame(float delta)
    {
        base.ProcessFrame(delta);
        _inputDir = _moveComp.GetDesiredDirection();

        

        if (_inputDir.IsZeroApprox())
        { //BUFFER AFTER MOVEMENT CHANGES
            //GetTree().CreateTimer(Global.MovementTransitionBufferTime).Timeout += ChangeMovementState;
            //_bufferingMovementTransition = true;
            EmitSignal(SignalName.TransitionState, this, _idleState);
            return;
        }
        else if (_moveComp.WantsJump() && _body.IsOnFloor())
        {
            EmitSignal(SignalName.TransitionState, this, _jumpState);
            return;
        }

        var animDir = IMovementComponent.GetAnimDirectionFromVector(_inputDir);
        if (_currAnimDir != animDir && _inputDir.Y != 0)
        {
            _currAnimDir = animDir;
            BB.GetVar<AnimationPlayer>(BBDataSig.Anim).Play(AnimName +
                IMovementComponent.GetFaceDirectionString(_currAnimDir));
        }
        BB.GetVar<Sprite3D>(BBDataSig.Sprite).FlipH = IMovementComponent.GetDesiredFlipH(_inputDir);
    }
    public override void ProcessPhysics(float delta)
    {   
        base.ProcessPhysics(delta);
        Vector3 velocity = _body.Velocity;

        Vector3 direction = (_body.Transform.Basis * new Vector3(_inputDir.X, 0, _inputDir.Y)).Normalized();

        // Add the gravity.
        if (!_body.IsOnFloor())
        {
            EmitSignal(SignalName.TransitionState, this, _fallState);
        }

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Monster.Speed;
            velocity.Z = direction.Z * Monster.Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(_body.Velocity.X, 0, Monster.Speed);
            velocity.Z = Mathf.MoveToward(_body.Velocity.Z, 0, Monster.Speed);
        }

        _body.Velocity = velocity;
        _body.MoveAndSlide();
    }
    public override void HandleInput(InputEvent @event)
    {
        //if (@event.IsActionPressed(_body.AttackInput))
        //{
        //    EmitSignal(SignalName.TransitionState, this, _attackState);
        //}
        if (_inputDir.Length() < 0.1f)
        {
            //EmitSignal(SignalName.TransitionState, this, _idleState);
        }
    }
    #endregion
    #region STATE_HELPER
    private void OnFoundClimbable(object sender, ClimbableComponent e)
    {
        GetTree().CreateTimer(_climbBufferTime).Timeout += ChangeMovementState;
        _bufferingClimbingTransition = true;
    }
    private void ChangeMovementState()
    {
        if (!_bufferingClimbingTransition) { return; }
        if (_climberComp.AvailableClimbable)
        {
            EmitSignal(SignalName.TransitionState, this, _startClimbState);
        }
        _bufferingClimbingTransition = false;
    }
    

    #endregion
}
