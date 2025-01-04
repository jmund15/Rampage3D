using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class JumpState : State
{
    #region STATE_VARIABLES
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
    }
    public override void Enter(Dictionary<State, bool> parallelStates)
    {
        base.Enter(parallelStates);

        _inputDir = _moveComp.GetDesiredDirection();
        var velocity = _body.Velocity;

        if (!_body.IsOnFloor())
        {
            if (_moveComp.GetAnimDirection() == AnimDirection.Down) { _currAnimDir = AnimDirection.Up; }
            else { _currAnimDir = AnimDirection.Down; }
            BB.GetVar<Sprite3D>(BBDataSig.Sprite).FlipH = !BB.GetVar<Sprite3D>(BBDataSig.Sprite).FlipH;

            var orthogDir = IMovementComponent.GetOrthogDirection(_currAnimDir, BB.GetVar<Sprite3D>(BBDataSig.Sprite).FlipH);
            velocity.X += Monster.ClimbJumpPushOff * IMovementComponent.GetVectorFromDirection(orthogDir).X;
            velocity.Z += Monster.ClimbJumpPushOff * IMovementComponent.GetVectorFromDirection(orthogDir).Y;
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
        _body.MoveAndSlide();
        _startedDescent = false;
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void ProcessFrame(float delta)
    {
        base.ProcessFrame(delta);
        _inputDir = BB.GetVar<IMovementComponent>(BBDataSig.MoveComp).GetDesiredDirection();   
    }
    public override void ProcessPhysics(float delta)
    {   
        base.ProcessPhysics(delta);
        //GD.Print("curr jump vel: ", _body.Velocity);
        // Add the gravity.
        if (_body.IsOnFloor())
        {
            EmitSignal(SignalName.TransitionState, this, _landFloorState);
            return;
        }
        else if (_body.IsOnWall())
        {
            var wallCollider = _body.GetLastSlideCollision().GetCollider() as Node3D;
            //GD.Print("wall collider: ", wallCollider.Name);
            var climbComp = wallCollider.GetFirstChildOfType<ClimbableComponent>();
            if (climbComp == null) { return; }
            BB.SetVar(BBDataSig.CurrClimbComp, climbComp);
            EmitSignal(SignalName.TransitionState, this, _landWallState);
        }

        Vector3 velocity = _body.Velocity;
        Vector3 direction = (_body.Transform.Basis * new Vector3(_inputDir.X, 0, _inputDir.Y)).Normalized();

        velocity += _body.GetGravity() * delta;
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
    #endregion
}
