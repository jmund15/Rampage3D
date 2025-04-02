using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class GlideState : State
{
    #region STATE_VARIABLES
    private string _animName = "jump";//"glide";
    private float _animStartTime = 0.1f;

    private Monster _body;
    private IMovementComponent _moveComp;
    private IVelocityChar3DComponent _velComp;

    [Export(PropertyHint.NodeType, "State")]
    private State _jumpState;
    [Export(PropertyHint.NodeType, "State")]
    private State _landFloorState;
    [Export(PropertyHint.NodeType, "State")]
    private State _landWallState;

    private IAnimPlayerComponent _animPlayer;
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
        _velComp = BB.GetVar<IVelocityChar3DComponent>(BBDataSig.VelComp);
        _animPlayer = BB.GetVar<IAnimPlayerComponent>(BBDataSig.Anim);
        _climberComp = BB.GetVar<ClimberComponent>(BBDataSig.ClimberComp);
    }
    public override void Enter(Dictionary<State, bool> parallelStates)
    {
        base.Enter(parallelStates);

        _currAnimDir = _moveComp.GetAnimDirection();
        //GD.Print("on fall enter anim: ", _animPlayer.CurrentAnimation);
        //GD.Print("on fall enter anim direction: ", _currAnimDir);
        //GD.Print("FALL ANIM DIR: ", _currAnimDir);
        _animPlayer.StartAnim(_animName + _currAnimDir.GetAnimationString());
        _animPlayer.SeekPos(_animStartTime, true);
        _animPlayer.PauseAnim();

        _fallHeight = _body.Position.Y;

        _climberComp.FoundClimbable += OnFoundClimbable;
    }
    public override void Exit()
    {
        base.Exit();
        if (!_moveComp.WantsJump())
        {
            BB.SetPrimVar(BBDataSig.JumpsLeft, _velComp.JumpsAllowed);
        }
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
            ////GD.Print("jump fall dist: ", fallDist);
            //if (fallDist > _velComp.MaxLandVelocity)
            //{
            //    EmitSignal(SignalName.TransitionState, this, _landFallState);
            //}
            //else
            //{
            EmitSignal(SignalName.TransitionState, this, _landFloorState);
            //}
        }

        if (Mathf.Abs(_body.Velocity.X) > Global.CHANGE_DIR_VEL_REQ || Mathf.Abs(_body.Velocity.Z) > Global.CHANGE_DIR_VEL_REQ)
        {
            var velDir = _body.Velocity.GetOrthogDirection();
            //GD.Print("vel: ", _body., "\nvelDir: ", velDir, "; curr dir: ", _moveComp.GetFaceDirection());
            if (velDir != _moveComp.GetFaceDirection())
            {
                //GD.Print("Velocity when changing dirs: ", _body.Velocity);
                BB.GetVar<Sprite3D>(BBDataSig.Sprite).FlipH = velDir.GetFlipH();
                _animPlayer.UpdateAnim(_animName + velDir.GetAnimDir());
            }
        }

        if (_moveComp.WantsJump() && BB.GetPrimVar<int>(BBDataSig.JumpsLeft) > 1)
        {
            BB.SetPrimVar(BBDataSig.JumpsLeft, BB.GetPrimVar<int>(BBDataSig.JumpsLeft).Value - 1);
            EmitSignal(SignalName.TransitionState, this, _jumpState);
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
        _velComp.SetMovement(delta, direction, VelocityType.Air);
        //_velComp.ApplyGravity(delta);
        //_velComp.Move();

        //// GAWKUS GLIDE!!!!
        var glideForce = _moveComp.GetFaceDirection().GetVector3();
        if (direction.IsZeroApprox())
        {
            // depending on face direction should ALWAYS be gliding forwards
            _velComp.ApplyImpulse(glideForce, ImpulseType.Glide);
        }
        _velComp.ApplyCustomGravity(delta, 
            (_velComp.GetInterfaceNode() as CharacterBody3D).GetGravity() / 4,
            MovementExtensions.DEFAULT_WEIGHT_PERCENTAGE / 2); 
        // should start gliding downward FASTER as he goes down
        // custom apply gravity function for custom gravity value and weight value
        _velComp.Move();

        //Vector3 velocity = _body.Velocity;

        //velocity += _body.GetWeightedGravity() * delta;
        ////GD.Print("body velocity after gravity: ", velocity);

        //if (_inputDir.IsZeroApprox()) {
        //    velocity.X = Mathf.MoveToward(_body.Velocity.X, 0, Monster.AirHorizontalFriction);
        //    velocity.Z = Mathf.MoveToward(_body.Velocity.Z, 0, Monster.AirHorizontalFriction);
        //    _body.Velocity = velocity;
        //    _body.MoveAndSlide();
        //    return; }
        //var orthogDir = _inputDir.GetOrthogDirection();
        //Vector3 direction = orthogDir.GetVector3();

        ////velocity.X = direction.X * Monster.AirSpeed;
        ////velocity.Z = direction.Z * Monster.AirSpeed;
        //velocity.X = Mathf.MoveToward(_body.Velocity.X, direction.X * Monster.AirMaxSpeed, Monster.AirAcceleration);
        //velocity.Z = Mathf.MoveToward(_body.Velocity.Z, direction.Z * Monster.AirMaxSpeed, Monster.AirAcceleration);


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
    private void OnFoundClimbable(object sender, ClimbableComponent e)
    {
        EmitSignal(SignalName.TransitionState, this, _landWallState);
    }

    #endregion
}
