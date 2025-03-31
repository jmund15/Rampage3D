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
    private IVelocityChar3DComponent _velComp;

    [Export(PropertyHint.NodeType, "State")]
    private State _jumpFallState;
    [Export(PropertyHint.NodeType, "State")]
    private State _landFloorState;
    [Export(PropertyHint.NodeType, "State")]
    private State _landWallState;

    private IAnimPlayerComponent _animPlayer;
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
        _velComp = BB.GetVar<IVelocityChar3DComponent>(BBDataSig.VelComp);
        _animPlayer = BB.GetVar<IAnimPlayerComponent>(BBDataSig.Anim);
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
        _animPlayer.StartAnim(AnimName +
            _currAnimDir.GetAnimationString());
        _animPlayer.SeekPos(0f, true);
        _animPlayer.PauseAnim();

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
        if (Mathf.Abs(_body.Velocity.X) > Global.CHANGE_DIR_VEL_REQ || Mathf.Abs(_body.Velocity.Z) > Global.CHANGE_DIR_VEL_REQ)
        {
            var velDir = _body.Velocity.GetOrthogDirection();
            //GD.Print("vel: ", _body., "\nvelDir: ", velDir, "; curr dir: ", _moveComp.GetFaceDirection());
            if (velDir != _moveComp.GetFaceDirection())
            {
                //GD.Print("Velocity when changing dirs: ", _body.Velocity);
                BB.GetVar<Sprite3D>(BBDataSig.Sprite).FlipH = velDir.GetFlipH();
                _animPlayer.UpdateAnim(AnimName + velDir.GetAnimDir());
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

        //velocity += _body.GetWeightedGravity() * delta;

        if (!_startedDescent && velocity.Y < 0)
        {
            EmitSignal(SignalName.TransitionState, this, _jumpFallState);
            return;
        }
        Vector3 direction;
        if (_inputDir.IsZeroApprox())
        {
            direction = Vector3.Zero;
        }
        else
        {
            var orthogDir = _inputDir.GetOrthogDirection();
            direction = orthogDir.GetVector3();
        }
        _velComp.SetHorizantalMovement(delta, direction, VelocityType.Air);
        _velComp.ApplyGravity(delta);
        _velComp.Move();
        //if (direction != Vector3.Zero)
        //{
        //    velocity.X = Mathf.MoveToward(_body.Velocity.X, direction.X * Monster.AirMaxSpeed, Monster.AirAcceleration);
        //    velocity.Z = Mathf.MoveToward(_body.Velocity.Z, direction.Z * Monster.AirMaxSpeed, Monster.AirAcceleration);
        //    //direction.X * Monster.AirSpeed;
        //    //velocity.Z = direction.Z * Monster.AirSpeed;
        //}
        //else
        //{
        //    velocity.X = Mathf.MoveToward(_body.Velocity.X, 0, Monster.AirHorizontalFriction);
        //    velocity.Z = Mathf.MoveToward(_body.Velocity.Z, 0, Monster.AirHorizontalFriction);
        //}

        //_body.Velocity = velocity;
        
        //_body.MoveAndSlide();
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

            var orthogVec3 = orthogDir.GetVector3();
            _velComp.ApplyImpulse(orthogVec3, ImpulseType.WallJump);
            var orthogVec = orthogDir.GetVector2();
            //velocity.X += Monster.ClimbJumpPushOff * orthogVec.X;
            //velocity.Z += Monster.ClimbJumpPushOff * orthogVec.Y;

            _climberComp.EjectRequested = false;
        }
        _velComp.ApplyImpulse(Vector3.Up, ImpulseType.Jump);
        _velComp.Move();
        //velocity.Y = Monster.JumpVelocity;

        //_body.Velocity = velocity;
        ////GD.Print("body velocity: ", _body.Velocity);
        //_body.MoveAndSlide();
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
