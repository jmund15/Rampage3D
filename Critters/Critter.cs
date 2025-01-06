using Godot;
using Godot.Collections;
using System;
using TimeRobbers.Interfaces;

public partial class Critter : CharacterBody3D, IMovementComponent
{
    public IBlackboard BB { get; protected set; }
    public SpriteOrthogComponent Sprite { get; protected set; }
    public AnimationPlayer AnimPlayer { get; protected set; }
    public HealthComponent HealthComp { get; protected set; }
    public HurtboxComponent3D HurtboxComp { get; protected set; }
    public EatableComponent EatableComp { get; protected set; }

    private CompoundState _stateMachine;
    private Dictionary<State, bool> _parallelStateMachines = new Dictionary<State, bool>();
    public State PrimaryState { get; protected set; }
    public Dictionary<State, bool> ParallelStates { get; protected set; } =
        new Dictionary<State, bool>();

    public static float Speed { get; private set; } = 7.5f;
    public static float AirSpeed { get; private set; } = 2.0f;
    public static float AirHorizontalFriction { get; private set; } = 0.2f;
    public static float ClimbSpeed { get; private set; } = 5.0f;
    public static float JumpVelocity { get; private set; } = 7.5f;
    public static float ClimbJumpPushOff { get; private set; } = 6.0f;
    public static float FallHeight { get; private set; } = 3f;

    public override void _Ready()
    {
        base._Ready();
        BB = this.GetFirstChildOfType<Blackboard>();
        BB.SetPrimVar(BBDataSig.SelfInteruptible, true);
        BB.SetVar(BBDataSig.Agent, this);
        BB.SetVar(BBDataSig.MoveComp, this); //since we implement "IMovementComponent" within this class we just send this object
        HealthComp = this.GetFirstChildOfType<HealthComponent>();
        BB.SetVar(BBDataSig.HealthComp, HealthComp);

        GD.Print("Before set sprite.");

        Sprite = this.GetFirstChildOfType<SpriteOrthogComponent>();
        Sprite.Show();
        //CharacterSize = new Vector2(Sprite.Texture.GetWidth() / Sprite.Hframes, Sprite.Texture.GetHeight() / Sprite.Vframes);
        AnimPlayer = Sprite.GetFirstChildOfType<AnimationPlayer>();
        //AnimPlayer.AnimationStarted += OnAnimationStarted;
        //AnimPlayer.AnimationFinished += OnAnimationFinished;
        BB.SetVar(BBDataSig.Sprite, Sprite);
        BB.SetVar(BBDataSig.Anim, AnimPlayer);

        HurtboxComp = this.GetFirstChildOfType<HurtboxComponent3D>();
        EatableComp = HurtboxComp.GetFirstChildOfType<EatableComponent>();
        BB.SetVar(BBDataSig.HurtboxComp, HurtboxComp);
        BB.SetVar(BBDataSig.EatableComp, EatableComp);


        _stateMachine = GetNode<CompoundState>("CSM");
        _stateMachine.Init(this, BB);
        PrimaryState = _stateMachine.InitialSubState;
        ParallelStates = _stateMachine.ParallelSubStates;
        _stateMachine.Enter(_parallelStateMachines);
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        _stateMachine.ProcessFrame((float)delta);
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _stateMachine.ProcessPhysics((float)delta);
    }


    public AnimDirection GetAnimDirection()
    {
        return IMovementComponent.GetAnimPlayerDirection(AnimPlayer);
    }
    public OrthogDirection GetFaceDirection()
    {
        return IMovementComponent.GetSpriteOrthogDirection(Sprite, AnimPlayer);
    }

    public OrthogDirection GetDesiredFaceDirection()
    {
        throw new NotImplementedException();
    }

    public Vector2 GetDesiredDirection()
    {
        return Vector2.Zero;
    }

    public Vector2 GetDesiredDirectionNormalized()
    {
        throw new NotImplementedException();
    }

    public bool WantsJump()
    {
        return false;
    }

    public bool WantsAttack()
    {
        return false;
    }

    public float TimeSinceAttackRequest()
    {
        throw new NotImplementedException();
    }

    public bool WantsStrafe()
    {
        throw new NotImplementedException();
    }

    public float GetRunSpeedMult()
    {
        throw new NotImplementedException();
    }
}
