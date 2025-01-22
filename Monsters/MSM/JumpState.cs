using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class JumpState : State
{
    #region STATE_VARIABLES
    private const float _jumpUninteruptibleTime = 0.25f;
    private bool _jumpCanInterupt = false;

    [Export]
    public string AnimName { get; protected set; }
    private Monster _body;
    private IMovementComponent _moveComp;

    [Export(PropertyHint.NodeType, "State")]
    private State _jumpFallState;
    [Export(PropertyHint.NodeType, "State")]
    private State _landFloorState;
    [Export(PropertyHint.NodeType, "State")]
    private State _landWallState;

    private AnimationPlayer _animPlayer;
    private ClimberComponent _climberComp;
    private Vector2 _inputDir = new Vector2();
    private AnimDirection _currAnimDir;

    private bool _startedDescent;
    private float _jumpFullHeight;
    #endregion
    #region STATE_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        base.Init(agent, bb);
        _body = Agent as Monster;
        _moveComp = BB.GetVar<IMovementComponent>(BBDataSig.MoveComp);
        _animPlayer = BB.GetVar<AnimationPlayer>(BBDataSig.Anim);
        _climberComp = BB.GetVar<ClimberComponent>(BBDataSig.ClimberComp);
    }
    public override void Enter(Dictionary<State, bool> parallelStates)
    {
        base.Enter(parallelStates);
        _climberComp.FoundClimbable += OnFoundClimbable;

        _inputDir = _moveComp.GetDesiredDirection();
        var velocity = _body.Velocity;

        if (_climberComp.EjectRequested)
        {
            var orthogDir = _climberComp.EjectDir;
            _currAnimDir = IMovementComponent.GetAnimDirFromOrthog(orthogDir);
            BB.GetVar<Sprite3D>(BBDataSig.Sprite).FlipH = IMovementComponent.GetFlipHFromOrthog(orthogDir);

            velocity.X += Monster.ClimbJumpPushOff * IMovementComponent.GetVectorFromDirection(orthogDir).X;
            velocity.Z += Monster.ClimbJumpPushOff * IMovementComponent.GetVectorFromDirection(orthogDir).Y;

            _climberComp.EjectRequested = false;
        }
        else if (_inputDir.Y > 0)
        {
            _currAnimDir = IMovementComponent.GetAnimDirectionFromVector(_moveComp.GetDesiredDirection());
        }
        else
        {
            _currAnimDir = _moveComp.GetAnimDirection();
        }
        _animPlayer.Play(AnimName +
            IMovementComponent.GetFaceDirectionString(_currAnimDir));
        _animPlayer.Seek(0f, true);
        _animPlayer.Pause();

        velocity.Y = Monster.JumpVelocity;
        _body.Velocity = velocity;
        GD.Print("body velocity: ", _body.Velocity);
        _body.MoveAndSlide();
        GD.Print("body velocity: ", _body.Velocity);
        _startedDescent = false;

        _jumpCanInterupt = false;
        GetTree().CreateTimer(_jumpUninteruptibleTime).Timeout += () =>
        {
            _jumpCanInterupt = true;
        };
    }
    public override void Exit()
    {
        base.Exit();
        _climberComp.FoundClimbable -= OnFoundClimbable;
    }
    public override void ProcessFrame(float delta)
    {
        base.ProcessFrame(delta);
        _inputDir = BB.GetVar<IMovementComponent>(BBDataSig.MoveComp).GetDesiredDirection();   
    }
    public override void ProcessPhysics(float delta)
    {   
        base.ProcessPhysics(delta);

        if (_jumpCanInterupt && _climberComp.AvailableClimbable)
        {
            EmitSignal(SignalName.TransitionState, this, _landWallState);
            return;
        }

        //GD.Print("curr jump vel: ", _body.Velocity);
        // Add the gravity.
        //if (_body.IsOnFloor())
        //{
        //    EmitSignal(SignalName.TransitionState, this, _landFloorState);
        //    return;
        //}
        Vector3 velocity = _body.Velocity;
        Vector3 direction = (_body.Transform.Basis * new Vector3(_inputDir.X, 0, _inputDir.Y)).Normalized();

        GD.Print("body velocity before gravity: ", velocity);
        velocity += _body.GetGravity() * delta;
        GD.Print("body velocity after gravity: ", velocity);

        if (!_startedDescent && velocity.Y < 0)
        {
            EmitSignal(SignalName.TransitionState, this, _jumpFallState);
            return;
        }

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Monster.AirSpeed;
            velocity.Z = direction.Z * Monster.AirSpeed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(_body.Velocity.X, 0, Monster.AirHorizontalFriction);
            velocity.Z = Mathf.MoveToward(_body.Velocity.Z, 0, Monster.AirHorizontalFriction);
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

    }
    #endregion
    #region STATE_HELPER
    private void OnFoundClimbable(object sender, ClimbableComponent e)
    {
        if (_jumpCanInterupt)
        {
            EmitSignal(SignalName.TransitionState, this, _landWallState);
        }
    }
    #endregion
}
