using Godot;
using System;
using System.Collections.Generic;
using TimeRobbers.Interfaces;

public partial class Critter : CharacterBody3D, IMovementComponent, IVelocityChar3DComponent
{
    public IBlackboard BB { get; protected set; }
    public AINav3DComponent AINavComp { get; protected set; }
    public ISpriteComponent Sprite { get; protected set; }
    public IAnimPlayerComponent AnimPlayer { get; protected set; }
    public List<IConfigAnimComponent> ConfigComps { get; protected set; } = new List<IConfigAnimComponent>();
    public HealthComponent HealthComp { get; protected set; }
    public HurtboxComponent3D HurtboxComp { get; protected set; }
    public EatableComponent EatableComp { get; protected set; }
    public OccupantComponent3D OccupantComp { get; protected set; }

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
        BB.SetVar(BBDataSig.VelComp, this); // same
        foreach (var velProp in VelocityProperties.VelocityIds)
        {
            VelocityMap[velProp.VelocityType] = velProp.GetVelocityID();
            VelAddModMap[velProp.VelocityType] = new VelocityID();
            VelMultModMap[velProp.VelocityType] = new VelocityID(1, 1, 1);
            //GD.Print($"set vel prop for {velProp.VelocityType}");
        }
        foreach (var impProp in VelocityProperties.ImpulseIds)
        {
            ImpulseMap[impProp.ImpulseType] = impProp.ImpulseForce;
            ImpulseModMap[impProp.ImpulseType] = 0f;
            //GD.Print($"set impulse prop for {impProp.ImpulseType}");
        }

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

        OccupantComp = this.GetFirstChildOfType<OccupantComponent3D>();
        BB.SetVar(BBDataSig.OccupantComp, OccupantComp);


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

    [Export]
    protected Char3DVelocityProperties VelocityProperties { get; set; }
    [ExportGroup("Velocity Properties")]

    public System.Collections.Generic.Dictionary<VelocityType, VelocityID> VelocityMap { get; private set; }
        = new System.Collections.Generic.Dictionary<VelocityType, VelocityID>();
    //{ get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public System.Collections.Generic.Dictionary<VelocityType, VelocityID> VelAddModMap { get; private set; }
        = new System.Collections.Generic.Dictionary<VelocityType, VelocityID>();
    public System.Collections.Generic.Dictionary<VelocityType, VelocityID> VelMultModMap { get; private set; }
        = new System.Collections.Generic.Dictionary<VelocityType, VelocityID>();
    public System.Collections.Generic.Dictionary<ImpulseType, float> ImpulseMap { get; private set; }
        = new System.Collections.Generic.Dictionary<ImpulseType, float>();
    public System.Collections.Generic.Dictionary<ImpulseType, float> ImpulseModMap { get; private set; }
        = new System.Collections.Generic.Dictionary<ImpulseType, float>();
    //{ get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    [Export]
    public int JumpsAllowed { get; set; }
    [Export]
    public float MaxLandVelocity { get; set; }

    public Node GetInterfaceNode()
    {
        return this;
    }

    //public Vector3 GetVelocity()
    //{
    //    return Velocity;
    //}

    public void SetMovement(float delta, Vector3 direction, VelocityType velType, bool useYFriction = false)
    {
        var velId = (VelocityMap[velType] * VelMultModMap[velType]) + VelAddModMap[velType];
        //GD.Print($"Setting Movement for {velType} with velID of: {velId}");
        //GD.Print($"Ground Velocity MAP ID: \n{VelocityMap[VelocityType.Ground]}");

        Vector3 velocity = Velocity;
        if (velId.InstantMaxSpeed)
        {
            velocity = direction * velId.MaxSpeed;
        }
        else
        {
            //GD.Print("direction: ", direction, "dir is zero: ", direction.IsZeroApprox());
            if (direction.IsZeroApprox())
            {
                //GD.Print("USING BRAKE FRICTION");
                velocity.X -= Velocity.X * velId.Friction * velId.BrakeFrictionMod * delta;
                if (useYFriction)
                {
                    velocity.Y -= Velocity.Y * velId.Friction * velId.BrakeFrictionMod * delta;
                }
                velocity.Z -= Velocity.Z * velId.Friction * velId.BrakeFrictionMod * delta;
            }
            else
            {
                velocity.X -= Velocity.X * velId.Friction * delta;
                if (useYFriction)
                {
                    velocity.Y -= Velocity.Y * velId.Friction * delta;
                }
                velocity.Z -= Velocity.Z * velId.Friction * delta;
            }


            velocity.X += direction.X * velId.Acceleration * delta;
            velocity.Y += direction.Y * velId.Acceleration * delta;
            velocity.Z += direction.Z * velId.Acceleration * delta;

            //velocity -= Velocity * velId.Friction * delta;
            //velocity += direction * velId.Acceleration * delta;
        }

        //if (direction == Vector3.Zero)
        //{
        //    if (velType != VelocityType.Air)
        //    {
        //        velocity.X = Mathf.MoveToward(Velocity.X, 0, totalVelocity);
        //        velocity.Y = Mathf.MoveToward(Velocity.Y, 0, totalVelocity);
        //        velocity.Z = Mathf.MoveToward(Velocity.Z, 0, totalVelocity);
        //    }
        //    else
        //    {
        //        velocity.X = Mathf.MoveToward(Velocity.X, 0, AirFriction);
        //        //velocity.Y = Mathf.MoveToward(Velocity.Y, 0, AirFriction);
        //        velocity.Z = Mathf.MoveToward(Velocity.Z, 0, AirFriction);
        //    }
        //}
        //else
        //{
        //    if (velType != VelocityType.Air)
        //    {
        //        velocity = direction * totalVelocity;
        //    }
        //    else
        //    {
        //        velocity.X = Mathf.MoveToward(Velocity.X, direction.X * totalVelocity, AirAcceleration);
        //        velocity.Y = Mathf.MoveToward(Velocity.Y, direction.Y * totalVelocity, AirAcceleration);
        //        velocity.Z = Mathf.MoveToward(Velocity.Z, direction.Z * totalVelocity, AirAcceleration);
        //    }
        //}
        Velocity = velocity;
        //GD.Print("Set Velocity: ", Velocity);
    }
    public void ApplyGravity(float delta)
    {
        Velocity += this.GetWeightedGravity() * delta;
    }
    public void ApplyCustomGravity(float delta, Vector3 customGrav, float weightPercentage = 0)
    {
        Velocity += this.GetCustomWeightedGravity(customGrav, weightPercentage) * delta;
    }
    public void ApplyImpulse(Vector3 direction, ImpulseType impulseType)
    {
        //GD.Print($"Impulse of type {impulseType} applied for {direction}");
        var totalForce = ImpulseMap[impulseType] + ImpulseModMap[impulseType];
        //GD.Print("vel before impulse: ", Velocity);
        Velocity += (direction * totalForce);
        //GD.Print("vel after impulse: ", Velocity);
    }
    public void ApplyCustomVelocity(Vector3 velocity)
    {
        Velocity += velocity;
    }
    public void Move()
    {
        MoveAndSlide();
    }
    public void CustomMove(Vector3 velocity)
    {
        Velocity = velocity;
        MoveAndSlide();
    }
    public void AppendAddVelocityIDMod(VelocityType velType, VelocityID mod)
    {
        if (!VelAddModMap.ContainsKey(velType))
        {
            VelAddModMap[velType] = mod;
        }
        VelAddModMap[velType] += mod;
    }
    public void SetAddVelocityIDMod(VelocityType velType, VelocityID mod)
    {
        VelAddModMap[velType] = mod;
    }
    public void AppendMultVelocityIDMod(VelocityType velType, VelocityID mod)
    {
        if (!VelMultModMap.ContainsKey(velType))
        {
            VelMultModMap[velType] = mod;
        }
        VelMultModMap[velType] *= mod;
    }
    public void SetMultVelocityIDMod(VelocityType velType, VelocityID mod)
    {
        VelMultModMap[velType] = mod;
    }
    public void AppendAllAddVelocityIDMods(VelocityID mod)
    {
        foreach (var modType in Global.GetEnumValues<VelocityType>())
        {
            if (!VelAddModMap.ContainsKey(modType))
            {
                VelAddModMap[modType] = mod;
            }
            VelAddModMap[modType] += mod;
        }
    }
    public void SetAllAddVelocityIDMods(VelocityID mod)
    {
        foreach (var modType in Global.GetEnumValues<VelocityType>())
        {
            VelAddModMap[modType] = mod;
        }
    }
    public void AppendAllMultVelocityIDMods(VelocityID mod)
    {
        foreach (var modType in Global.GetEnumValues<VelocityType>())
        {
            if (!VelMultModMap.ContainsKey(modType))
            {
                VelMultModMap[modType] = mod;
            }
            VelMultModMap[modType] *= mod;
        }
    }
    public void SetAllMultVelocityIDMods(VelocityID mod)
    {
        foreach (var modType in Global.GetEnumValues<VelocityType>())
        {
            VelMultModMap[modType] = mod;
        }
    }
    public System.Collections.Generic.Dictionary<VelocityType, VelocityID> GetBaseVelocityMap()
    {
        return VelocityMap;
    }
    public System.Collections.Generic.Dictionary<VelocityType, VelocityID> GetVelAddModMap()
    {
        return VelAddModMap;
    }
    public System.Collections.Generic.Dictionary<VelocityType, VelocityID> GetVelMultModMap()
    {
        return VelMultModMap;
    }
    public System.Collections.Generic.Dictionary<VelocityType, VelocityID> GetAllTotalVelocities()
    {
        var totalVels = new System.Collections.Generic.Dictionary<VelocityType, VelocityID>();
        foreach (var velType in Global.GetEnumValues<VelocityType>())
        {
            if (VelocityMap.ContainsKey(velType))
            {
                totalVels[velType] = VelocityMap[velType];
                if (VelAddModMap.ContainsKey(velType))
                {
                    totalVels[velType] += VelAddModMap[velType];
                }
            }
            else if (VelAddModMap.ContainsKey(velType))
            {
                totalVels[velType] = VelAddModMap[velType];
            }
            else
            {
                totalVels[velType] = new VelocityID();
            }
        }
        return totalVels;
    }
    public VelocityID GetBaseVelocityID(VelocityType type)
    {
        return GetBaseVelocityMap()[type];
    }
    public VelocityID GetVelocityAddModID(VelocityType modType)
    {
        return GetVelAddModMap()[modType];
    }
    public VelocityID GetVelocityMultModID(VelocityType modType)
    {
        return GetVelMultModMap()[modType];
    }
    public VelocityID GetTotalVelocityID(VelocityType type)
    {
        return GetAllTotalVelocities()[type];
    }
    public void ResetVelocity()
    {
        Velocity = Vector3.Zero;
    }

    public void AppendAddMod(float mod)
    {
        throw new NotImplementedException();
    }

    public void AppendMultMod(float mod)
    {
        throw new NotImplementedException();
    }

    public void SetAddMod(float mod)
    {
        throw new NotImplementedException();
    }

    public void SetMultMod(float mod)
    {
        throw new NotImplementedException();
    }
}
