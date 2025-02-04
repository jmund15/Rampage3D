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
    private bool _velocitySet;
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
            _currAnimDir = orthogDir.GetAnimDir();

        }
        else if (_inputDir.Y > 0)
        {
            _currAnimDir = _moveComp.GetDesiredDirection().GetAnimDir();
        }
        else
        {
            _currAnimDir = _moveComp.GetAnimDirection();
        }
        _animPlayer.Play(AnimName +
            _currAnimDir.GetAnimationString());
        _animPlayer.Seek(0f, true);
        _animPlayer.Pause();

        _velocitySet = false;
        CallDeferred(MethodName.SetJumpVelocity);
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
        if (_body.Velocity.X != 0 || _body.Velocity.Z != 0)
        {
            var velDir = _body.Velocity.GetOrthogDirection();
            if (velDir != _moveComp.GetFaceDirection())
            {
                var prevPos = _animPlayer.CurrentAnimationPosition;
                PlayAnim.AnimWithOrthog(BB, AnimName, velDir);
                _animPlayer.Seek(prevPos, true);
            }
        }
        //GD.Print("curr jump vel: ", _body.Velocity);
        // Add the gravity.
        //if (_body.IsOnFloor())
        //{
        //    EmitSignal(SignalName.TransitionState, this, _landFloorState);
        //    return;
        //}

        if (!_velocitySet) { return; }
        Vector3 velocity = _body.Velocity;
        

        velocity += _body.GetWeightedGravity() * delta;

        if (!_startedDescent && velocity.Y < 0)
        {
            EmitSignal(SignalName.TransitionState, this, _jumpFallState);
            return;
        }

        if (_inputDir.IsZeroApprox())
        {
            _body.Velocity = velocity;
            _body.MoveAndSlide();
            return;
        }

        var orthogDir = _inputDir.GetOrthogDirection();
        Vector3 direction = orthogDir.GetVector3();
        if (direction != Vector3.Zero)
        {
            velocity.X = Mathf.MoveToward(_body.Velocity.X, direction.X * Monster.AirMaxSpeed, Monster.AirAcceleration);
            velocity.Z = Mathf.MoveToward(_body.Velocity.Z, direction.Z * Monster.AirMaxSpeed, Monster.AirAcceleration);
            //direction.X * Monster.AirSpeed;
            //velocity.Z = direction.Z * Monster.AirSpeed;
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
    private void SetJumpVelocity()
    {
        var velocity = _body.Velocity;
        if (_climberComp.EjectRequested)
        {
            var orthogDir = _climberComp.EjectDir;
            _currAnimDir = orthogDir.GetAnimDir();
            BB.GetVar<Sprite3D>(BBDataSig.Sprite).FlipH = orthogDir.GetFlipH();

            var orthogVec = orthogDir.GetVector2();
            velocity.X += Monster.ClimbJumpPushOff * orthogVec.X;
            velocity.Z += Monster.ClimbJumpPushOff * orthogVec.Y;

            _climberComp.EjectRequested = false;
        }
        velocity.Y = Monster.JumpVelocity;

        _body.Velocity = velocity;
        //GD.Print("body velocity: ", _body.Velocity);
        _body.MoveAndSlide();
        _velocitySet = true;
    }
    private void OnFoundClimbable(object sender, ClimbableComponent e)
    {
        if (_jumpCanInterupt)
        {
            EmitSignal(SignalName.TransitionState, this, _landWallState);
        }
    }
    #endregion
}
