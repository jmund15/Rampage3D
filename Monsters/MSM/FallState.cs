using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class FallState : State
{
    #region STATE_VARIABLES
    private string _animName = "jump";
    private float _animStartTime = 0.1f;

    private Monster _body;
    private IMovementComponent _moveComp;

    [Export(PropertyHint.NodeType, "State")]
    private State _landFloorState;
    [Export(PropertyHint.NodeType, "State")]
    private State _landWallState;
    [Export(PropertyHint.NodeType, "State")]
    private State _landFallState;

    private AnimationPlayer _animPlayer;
    private ClimberComponent _climberComp;
    private Vector2 _inputDir = new Vector2();
    private AnimDirection _currAnimDir;

    private float _fallHeight;
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

        _currAnimDir = _moveComp.GetAnimDirection();
        //GD.Print("on fall enter anim: ", _animPlayer.CurrentAnimation);
        //GD.Print("on fall enter anim direction: ", _currAnimDir);
        //GD.Print("FALL ANIM DIR: ", _currAnimDir);
        _animPlayer.Play(_animName + _currAnimDir.GetAnimationString());
        _animPlayer.Seek(_animStartTime, true);
        _animPlayer.Pause();

        _fallHeight = _body.Position.Y;

        _climberComp.FoundClimbable += OnFoundClimbable;
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
        // Add the gravity.
        if (_body.IsOnFloor())
        {
            var landHeight = _body.Position.Y;
            var fallDist = _fallHeight - landHeight;
            //GD.Print("jump fall dist: ", fallDist);
            if (fallDist > Monster.FallHeight)
            {
                EmitSignal(SignalName.TransitionState, this, _landFallState);
            }
            else
            {
                EmitSignal(SignalName.TransitionState, this, _landFloorState);
            }
        }

        if (_body.Velocity.X != 0 || _body.Velocity.Z != 0)
        {
            var velDir = _body.Velocity.GetOrthogDirection();
            if (velDir != _moveComp.GetFaceDirection())
            {
                var prevPos = _animPlayer.CurrentAnimationPosition;
                PlayAnim.AnimWithOrthog(BB, _animName, velDir);
                _animPlayer.Seek(prevPos, true);
            }
        }
        

        Vector3 velocity = _body.Velocity;
        

        velocity += _body.GetWeightedGravity() * delta;
        //GD.Print("body velocity after gravity: ", velocity);

        if (_inputDir.IsZeroApprox()) {
            velocity.X = Mathf.MoveToward(_body.Velocity.X, 0, Monster.AirHorizontalFriction);
            velocity.Z = Mathf.MoveToward(_body.Velocity.Z, 0, Monster.AirHorizontalFriction);
            _body.Velocity = velocity;
            _body.MoveAndSlide();
            return; }
        var orthogDir = _inputDir.GetOrthogDirection();
        Vector3 direction = orthogDir.GetVector3();

        //velocity.X = direction.X * Monster.AirSpeed;
        //velocity.Z = direction.Z * Monster.AirSpeed;
        velocity.X = Mathf.MoveToward(_body.Velocity.X, direction.X * Monster.AirMaxSpeed, Monster.AirAcceleration);
        velocity.Z = Mathf.MoveToward(_body.Velocity.Z, direction.Z * Monster.AirMaxSpeed, Monster.AirAcceleration);


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
        EmitSignal(SignalName.TransitionState, this, _landWallState);
    }

    #endregion
}
