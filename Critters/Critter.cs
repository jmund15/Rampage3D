using Godot;
using System;
using System.Collections.Generic;
using TimeRobbers.Interfaces;

public partial class Critter : CharacterBody3D, IMovementComponent
{
    public IBlackboard BB { get; protected set; }
    public AINav3DComponent AINavComp { get; protected set; }
    public ISpriteComponent Sprite { get; protected set; }
    public IAnimPlayerComponent AnimPlayer { get; protected set; }
    public List<IConfigAnimComponent> ConfigComps { get; protected set; } = new List<IConfigAnimComponent>();
    public HealthComponent HealthComp { get; protected set; }
    public HurtboxComponent3D HurtboxComp { get; protected set; }
    public EatableComponent EatableComp { get; protected set; }

    private CompoundState _csm;
    private Godot.Collections.Dictionary<State, bool> _parallelStateMachines = 
        new Godot.Collections.Dictionary<State, bool>();
    public State PrimaryState { get; protected set; }
    public Godot.Collections.Dictionary<State, bool> ParallelStates { get; protected set; } =
        new Godot.Collections.Dictionary<State, bool>();

    private CompoundState _aism;
    private Godot.Collections.Dictionary<State, bool> _aiParallelStateMachines =
        new Godot.Collections.Dictionary<State, bool>();
    public State AIPrimaryState { get; protected set; }
    public Godot.Collections.Dictionary<State, bool> AIParallelStates { get; protected set; } =
        new Godot.Collections.Dictionary<State, bool>();

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

        AINavComp = this.GetFirstChildOfType<AINav3DComponent>();
        BB.SetVar(BBDataSig.AINavComp, AINavComp);

        HealthComp = this.GetFirstChildOfType<HealthComponent>();
        BB.SetVar(BBDataSig.HealthComp, HealthComp);

        Sprite = this.GetFirstChildOfInterface<ISpriteComponent>();
        Sprite.Show();

        //CharacterSize = new Vector2(Sprite.Texture.GetWidth() / Sprite.Hframes, Sprite.Texture.GetHeight() / Sprite.Vframes);
        
        //AnimPlayer.AnimationStarted += OnAnimationStarted;
        //AnimPlayer.AnimationFinished += OnAnimationFinished;

        AnimPlayer = this.GetFirstChildOfInterface<IAnimPlayerComponent>();
        // GET CONFIGS AND RANDOMIZE
        foreach (var child in this.GetChildrenOfType<Node>())
        {
            if (child is IConfigAnimComponent config)
            {
                ConfigComps.Add(config);
                var configOpts = config.GetConfigOptions();
                var configCount = configOpts.Count;
                var randConfig = Global.Rnd.Next(0, configCount);
                config.SetConfig(configOpts[randConfig]);
                GD.Print($"Sprite {child.GetParent().Name}'s config set to {config.GetConfig()}");
            }
            //if (child is IAnimComponent anim)
            //{
            //    AnimPlayer = anim;
            //}
        }

        BB.SetVar(BBDataSig.Sprite, Sprite.GetInterfaceNode());
        BB.SetVar(BBDataSig.Anim, AnimPlayer.GetInterfaceNode());
        //foreach (var config in ConfigComps)
        //{
        //    var configOpts = config.GetConfigOptions();
        //    var configCount = configOpts.Count;
        //    var randConfig = Global.Rnd.Next(0, configCount);
        //    config.SetConfig(configOpts[randConfig]);
        //    GD.Print($"");
        //}

        HurtboxComp = this.GetFirstChildOfType<HurtboxComponent3D>();
        EatableComp = HurtboxComp.GetFirstChildOfType<EatableComponent>();
        BB.SetVar(BBDataSig.HurtboxComp, HurtboxComp);
        BB.SetVar(BBDataSig.EatableComp, EatableComp);


        _csm = GetNode<CompoundState>("CSM");
        _csm.Init(this, BB);
        PrimaryState = _csm.InitialSubState;
        ParallelStates = _csm.ParallelSubStates;
        _csm.Enter(_parallelStateMachines);

        _aism = GetNode<CompoundState>("AISM");
        _aism.Init(this, BB);
        AIPrimaryState = _aism.InitialSubState;
        AIParallelStates = _aism.ParallelSubStates;
        _aism.Enter(_aiParallelStateMachines);

        GD.Print("raycast angle = ", this.GetFirstChildOfType<AIRays16Dir>().RayU.GlobalRotation);
    }
    public override void _ExitTree()
    {
        base._ExitTree();
        _csm.Exit();
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        _csm.ProcessFrame((float)delta);
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _csm.ProcessPhysics((float)delta);
    }


    public AnimDirection GetAnimDirection()
    {
        if (AnimPlayer.GetCurrAnimation().Contains("Down"))
        {
            return AnimDirection.Down;
        }
        else /*if (AnimPlayer.GetCurrAnimation().Contains("Up"))*/
        {
            return AnimDirection.Up;
        }
        //else
        //{
        //    throw new Exception("ERROR || Can't get anim direction from current animation: " +
        //        AnimPlayer.GetCurrAnimation());
        //}
    }
    public Dir4 GetFaceDirection()
    {
        return IMovementComponent.GetOrthogDirection(GetAnimDirection(), Sprite.FlipH);
    }

    public Dir4 GetDesiredFaceDirection()
    {
        throw new NotImplementedException();
    }

    public Vector2 GetDesiredDirection()
    {
        var dir = AINavComp.WeightedNextPathDirection;
        return new Vector2(dir.X, dir.Z);
    }

    public Vector2 GetDesiredDirectionNormalized()
    {
        return GetDesiredDirection().Normalized();
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
