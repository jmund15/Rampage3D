using Godot;
using System;
using Godot.Collections;

public class OrthogDirChangedArgs : EventArgs
{
    public Dir4 OldDir { get; set; }
    public Dir4 NewDir { get; set; }
    public OrthogDirChangedArgs(Dir4 oldDir, Dir4 newDir)
    {
        OldDir = oldDir;
        NewDir = newDir;
    }
}
/* TODO:
 * Add exported properties for -
 * * State Machine
 * Add Monster form dictionaries for -
 * * Coll shapes (main, hitbox, hurtbox)
 *
 */
public partial class Monster : CharacterBody3D, IMovementComponent, IVelocityChar3DComponent
{
    public int PlayerNumber { get; set; }
    public bool InputAllowed { get; set; } = true;
    public Vector2 InputDirection { get; private set; }
    public string LeftInput { get; private set; }
    public string RightInput { get; private set; }
    public string UpInput { get; private set; }
    public string DownInput { get; private set; }
    public string AttackInput { get; private set; }
    public string DefendInput { get; private set; }
    public string LeapInput { get; private set; }
    public string ThrowInput { get; private set; }
    public string ItemInput { get; private set; }

    public IBlackboard BB { get; protected set; }

    public CollisionShape3D CollisionShape { get; protected set; }
    private Vector3 _baseCollisionPos;
    private Dictionary<Dir4, Vector3> MonsterPosOffsetMap = new Dictionary<Dir4, Vector3>()
    {
        { Dir4.Up, new Vector3(0, 0, -1) },
        { Dir4.Left, new Vector3(0, 0, -1) },
        { Dir4.Down, new Vector3(-0, 0, 1) },
        { Dir4.Right, new Vector3(0, 0, 1) }
    };
    private Dictionary<Dir4, Vector3> CollisionPositionMap = new Dictionary<Dir4, Vector3>()
    {
        { Dir4.Up, new Vector3(0, 0.8f, -0) },
        { Dir4.Left, new Vector3(0, 0.8f, -0) },
        { Dir4.Down, new Vector3(-0, 0.8f, -0.2f) },
        { Dir4.Right, new Vector3(0, 0.8f, -0.2f) }
    };
    private Dictionary<Dir4, Vector3> CollisionRotationMap = new Dictionary<Dir4, Vector3>()
    {
        { Dir4.Up, new Vector3(0, 45f, 90) },
        { Dir4.Left, new Vector3(0, -45f, 90) },
        { Dir4.Down, new Vector3(0, 45, 90) },
        { Dir4.Right, new Vector3(0, -45f, 90) }
    };

    private Dictionary<Dir4, Vector3> SpritePositionMap = new Dictionary<Dir4, Vector3>()
    {
        { Dir4.Up, new Vector3(0, -0, 0.8f) },
        { Dir4.Left, new Vector3(0, -0, 0.8f) },
        { Dir4.Down, new Vector3(0, -0, 0.8f) },
        { Dir4.Right, new Vector3(0, -0, 0.8f) }
    };

    public Sprite3DComponent Sprite { get; protected set; }
	public AnimationPlayerComponent AnimPlayer { get; protected set; }
    public HealthComponent HealthComp { get; protected set; }
    public HurtboxComponent3D HurtboxComp { get; protected set; }
    public HitboxComponent3D HitboxComp { get; protected set; }
    private Vector3 _baseHitboxComp;
    public EaterComponent EaterComp { get; protected set; }
    public ClimberComponent ClimberComp { get; protected set; }

    private CompoundState _stateMachine;
    private Dictionary<State, bool> _parallelStateMachines = new Dictionary<State, bool>();
    public State PrimaryState { get; protected set; }
    public Dictionary<State, bool> ParallelStates { get; protected set; } =
        new Dictionary<State, bool>();

    private Dir4 _currOrthogDir;
    public Dir4 CurrOrthogDir
    {
        get => _currOrthogDir;
        set
        {
            if (value == _currOrthogDir) { return; }
            var oldDir = _currOrthogDir;
            _currOrthogDir = value;
            OrthogDirectionChanged?.Invoke(this, new OrthogDirChangedArgs(oldDir, _currOrthogDir));
        }
    }
    //public static float Speed { get; private set; } = 6.0f;
    //public static float AirMaxSpeed { get; private set; } = 3.75f;
    //public static float AirHorizontalFriction { get; private set; } = 0.2f;
    //public static float AirAcceleration { get; private set; } = 0.5f;
    //public static float ClimbSpeed { get; private set; } = 4.5f;
    //public static float JumpVelocity { get; private set; } = 7.5f;
    //public static float ClimbJumpPushOff { get; private set; } = 5.0f;
    //public static float FallHeight { get; private set; } = 3f;
    //public static float SkidFriction { get; private set; } = 1f;
    public float LastFrameVelocity { get; private set; } = 0f;
    [Export]
    private float _pushForce = 200f;
    [Export]
    private float _pushImpulse = 1000f;
    public MonsterIdentifier MonsterId { get; private set; }
    [Export]
    public MonsterType MonsterT { get; private set; }
    public MonsterForm MonsterF { get; private set; }
    [Export]
    public MonsterForm MonsterFormStart { get; private set; } = MonsterForm.F2;
    [Export]
    private Dictionary<MonsterForm, Sprite3D> _monsterFormMapping = new Dictionary<MonsterForm, Sprite3D>();

    private MonsterAttackIdentifier _gna;
    private MonsterAttackIdentifier _gsa;
    private MonsterAttackIdentifier _wna;
    private MonsterAttackIdentifier _wsa;

    public event EventHandler<OrthogDirChangedArgs> OrthogDirectionChanged;
    public override void _Ready()
    {
        base._Ready();
        foreach (var velProp in VelocityProperties.VelocityIds)
        {
            VelocityMap[velProp.VelocityType] = velProp.GetVelocityID();
        }
        foreach (var impProp in VelocityProperties.ImpulseIds)
        {
            ImpulseMap[impProp.ImpulseType] = impProp.ImpulseForce;
        }
        //VelocityMap[VelocityType.Ground] =
        //    GroundVelocity.GetVelocityID();
        //VelocityMap[VelocityType.Air] =
        //    AirVelocity.GetVelocityID();
        //VelocityMap[VelocityType.Climb] =
        //    ClimbVelocity.GetVelocityID();
        //VelocityMap[VelocityType.Swim] =
        //    SwimVelocity.GetVelocityID();


        GD.Print($"Air Velocity ID: \n{AirVelocity.GetVelocityID().ToString()}");
        //GD.Print($"Ground Velocity MAP ID: \n{VelocityMap[VelocityType.Ground]}");
        foreach (var velType in Global.GetEnumValues<VelocityType>())
        {
            VelAddModMap[velType] = new VelocityID();
            VelMultModMap[velType] = new VelocityID(1,1,1);
        }

        //ImpulseMap[ImpulseType.Jump] = JumpForce;
        //ImpulseMap[ImpulseType.WallJump] = WallJumpForce;
        //ImpulseMap[ImpulseType.Glide] = 0.5f; // TODO: MAKE VARIABLE
        //foreach (var forceType in Global.GetEnumValues<ImpulseType>())
        //{
        //    ImpulseModMap[forceType] = 0f;
        //}

        MonsterF = MonsterFormStart;
        MonsterId = new MonsterIdentifier(MonsterT, MonsterF);
        OrthogDirectionChanged += OnOrthogDirChanged;

        BB = this.GetFirstChildOfType<Blackboard>();
        BB.SetPrimVar(BBDataSig.QueuedNextAttack, false);
        BB.SetPrimVar(BBDataSig.SelfInteruptible, true);
        BB.SetVar(BBDataSig.Agent, this);
        BB.SetVar(BBDataSig.MoveComp, this); //since we implement "IMovementComponent" within this class we just send this object
        BB.SetVar(BBDataSig.VelComp, this); //same as above
        HealthComp = this.GetFirstChildOfType<HealthComponent>();
        BB.SetVar(BBDataSig.HealthComp, HealthComp);
        
        GD.Print("Before set sprite.");
        CollisionShape = this.GetFirstChildOfType<CollisionShape3D>();
        Sprite = this.GetFirstChildOfType<Sprite3DComponent>();
        Sprite.Show();
        //CharacterSize = new Vector2(Sprite.Texture.GetWidth() / Sprite.Hframes, Sprite.Texture.GetHeight() / Sprite.Vframes);
        AnimPlayer = Sprite.GetFirstChildOfType<AnimationPlayerComponent>();
        //AnimPlayer.AnimationStarted += OnAnimationStarted;
        //AnimPlayer.AnimationFinished += OnAnimationFinished;
        BB.SetVar(BBDataSig.Sprite, Sprite);
        BB.SetVar(BBDataSig.Anim, AnimPlayer);

        HitboxComp = this.GetFirstChildOfType<HitboxComponent3D>();
        EaterComp = HitboxComp.GetFirstChildOfType<EaterComponent>();
        ClimberComp = this.GetFirstChildOfType<ClimberComponent>();
        BB.SetVar(BBDataSig.HitboxComp, HitboxComp);
        BB.SetVar(BBDataSig.EaterComp, EaterComp);
        BB.SetVar(BBDataSig.ClimberComp, ClimberComp);

        HurtboxComp = this.GetFirstChildOfType<HurtboxComponent3D>();
        BB.SetVar(BBDataSig.HurtboxComp, HurtboxComp);

        _baseCollisionPos = CollisionShape.Position;
        _baseHitboxComp = HitboxComp.Position;

        SetMonsterAttacks();

        //Shadow.Scale = BagStateComponent.CurrentRobberShadowScale;

        _stateMachine = GetNode<CompoundState>("MSM");
        _stateMachine.Init(this, BB);
        PrimaryState = _stateMachine.InitialSubState;
        ParallelStates = _stateMachine.ParallelSubStates;
        _stateMachine.Enter(_parallelStateMachines);


        //AnimPlayer.AnimationStarted += (name) =>
        //{
        //    GD.Print($"Animation Started: {name}");
        //};
        CurrOrthogDir =
            IMovementComponent.GetOrthogDirection(AnimPlayer.GetAnimDirection(), Sprite.FlipH);
        OnOrthogDirChanged(this, new OrthogDirChangedArgs(CurrOrthogDir,CurrOrthogDir));
    }
    public override void _ExitTree()
    {
        base._ExitTree();
        _stateMachine.Exit();
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        _stateMachine.ProcessFrame((float)delta);
        CurrOrthogDir =
            IMovementComponent.GetOrthogDirection(AnimPlayer.GetAnimDirection(), Sprite.FlipH);

    }
    public override void _PhysicsProcess(double delta)
	{
        base._PhysicsProcess(delta);
        var preVel = Velocity;
        _stateMachine.ProcessPhysics((float)delta);
        if (GetDesiredDirection().IsZeroApprox()) { return; }
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            var coll = GetSlideCollision(i);
            var collBody = coll.GetCollider();
            var collidingDir = GetLastMotion();
            if (collBody is GroundVehicleComponent vehicle)
            {
                Vector3 force;
                if (preVel.Length() < 1)
                {
                    force = GetDesiredDirection().GetOrthogDirection().GetVector3();
                    //vehicle.ApplyForce(force * _pushForce * (float)delta, coll.GetPosition());
                    //GD.Print($"COLLIDED WITH prevel {preVel} @ force: {force}");
                }
                else
                {
                    force = preVel * GetDesiredDirection().GetOrthogDirection().GetVector3();
                    //vehicle.ApplyImpulse(force * _pushImpulse * (float)delta, coll.GetPosition());// - vehicle.GlobalPosition);
                    vehicle.ApplyCentralImpulse(force * _pushImpulse * (float)delta);

                    //GD.Print($"COLLIDED WITH prevel {preVel} @ force: {force}");
                }
            }
            
           
        }
        //ApplyFloorSnap();
		//Vector3 velocity = Velocity;

		

		//// Handle Jump.
		//if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		//{
		//	velocity.Y = JumpVelocity;
		//}

		//// Get the input direction and handle the movement/deceleration.
		//// As good practice, you should replace UI actions with custom gameplay actions.
		//Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		//Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		//if (direction != Vector3.Zero)
		//{
		//	velocity.X = direction.X * Speed;
		//	velocity.Z = direction.Z * Speed;
		//}
		//else
		//{
		//	velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		//	velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		//}

		//Velocity = velocity;
		//MoveAndSlide();
	}
    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        _stateMachine.HandleInput(@event);
        
    }
    private void SetMonsterAttacks()
    {
        MonsterId = new MonsterIdentifier(MonsterT, MonsterF);

        _gna = new MonsterAttackIdentifier(MonsterId.Monster, MonsterId.Form, AttackType.GroundNormal);
        _gsa = new MonsterAttackIdentifier(MonsterId.Monster, MonsterId.Form, AttackType.GroundSpecial);
        _wna = new MonsterAttackIdentifier(MonsterId.Monster, MonsterId.Form, AttackType.WallNormal);
        _wsa = new MonsterAttackIdentifier(MonsterId.Monster, MonsterId.Form, AttackType.WallSpecial);

        BB.SetVar(BBDataSig.GroundNormalAttack, MonsterAttackMapping.AttackTreeMap[_gna]);
        //BB.SetVar(BBDataSig.GroundNormalAttack, MonsterAttackMapping.AttackTreeMap[_gsa]);
        BB.SetVar(BBDataSig.WallNormalAttack, MonsterAttackMapping.AttackTreeMap[_wna]);
        BB.SetVar(BBDataSig.WallSpecialAttack, MonsterAttackMapping.AttackTreeMap[_wsa]);
    }
    private void OnOrthogDirChanged(object sender, OrthogDirChangedArgs dirArgs)
    {
        var oldDir = dirArgs.OldDir;
        var newDir = dirArgs.NewDir;
        //if (oldDir.GetAnimDir() != newDir.GetAnimDir())
        //{
        //    GD.Print("old monster pos: ", Position);
        //    Position += Transform.Basis * MonsterPosOffsetMap[newDir];
        //    GD.Print("new monster pos: ", Position);
        //}
        CollisionShape.Position = CollisionPositionMap[newDir];
        //HitboxComp.Position = _baseHitboxComp + CollisionPositionMap[orthogDir];
        CollisionShape.RotationDegrees = CollisionRotationMap[newDir];
        Sprite.Position = SpritePositionMap[newDir];

        //GD.Print("Sprite Pos: ", Sprite.Position);
        //GD.Print("Coll Pos: ", CollisionShape.Position);

        //Sprite.TopLevel = true; CollisionShape.TopLevel = true;
        //CallDeferred(MethodName.AdjustMonsterPos, MonsterPositionMap[orthogDir]);
    }
    private void AdjustMonsterPos(Vector3 pos)
    {
        Position = pos;
        Sprite.TopLevel = false; CollisionShape.TopLevel = false;
    }
    public AnimDirection GetAnimDirection()
    {
        return CurrOrthogDir.GetAnimDir();
    }
    public Dir4 GetFaceDirection()
    {
        return CurrOrthogDir;
    }
    public Dir4 GetDesiredFaceDirection()
    {
        return GetDesiredDirection().GetOrthogDirection();
    }
    public Vector2 GetDesiredDirection()
    {
        var input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        return new Vector2(input.X, input.Y); //Input.GetVector(LeftInput, RightInput, UpInput, DownInput);
    }

    public Vector2 GetDesiredDirectionNormalized()
    {
        return GetDesiredDirection().Normalized();
    }

    public bool WantsJump()
    {
        if (Input.IsActionJustPressed("ui_accept"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool WantsAttack()
    {
        if (Input.IsActionJustPressed("attack_test"))
        {
            return true;
        }
        else if (Input.IsActionJustPressed("sattack_test"))
        {
            return true;
        }
        return false;
    }
    public bool WantsStrafe()
    {
        throw new NotImplementedException();
    }

    [Export]
    protected Char3DVelocityProperties VelocityProperties { get; set; }
    [ExportGroup("Velocity Properties")]
    [Export]
    protected VelocityIDResource GroundVelocity { get; set; }
    [Export]
    protected VelocityIDResource AirVelocity { get; set; }
    [Export]
    protected VelocityIDResource ClimbVelocity { get; set; }
    //[Export]
    //protected float ClimbSkidFriction { get; set; }
    [Export]
    protected VelocityIDResource SwimVelocity { get; set; }

    [Export]
    protected float JumpForce { get; set; }
    [Export]
    protected float WallJumpForce { get; set; }

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
        GD.Print("Set Velocity: ", Velocity);
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
        GD.Print($"Impulse of type {impulseType} applied for {direction}");
        var totalForce = ImpulseMap[impulseType] + ImpulseModMap[impulseType];
        GD.Print("vel before impulse: ", Velocity);
        Velocity += (direction * totalForce);
        GD.Print("vel after impulse: ", Velocity);
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
    public void AppendAddVelocityMod(VelocityType velType, VelocityID mod)
    {
        if (!VelAddModMap.ContainsKey(velType))
        {
            VelAddModMap[velType] = mod;
        }
        VelAddModMap[velType] += mod;
    }
    public void SetAddVelocityMod(VelocityType velType, VelocityID mod)
    {
        VelAddModMap[velType] = mod;
    }
    public void AppendMultVelocityMod(VelocityType velType, VelocityID mod)
    {
        if (!VelMultModMap.ContainsKey(velType))
        {
            VelMultModMap[velType] = mod;
        }
        VelMultModMap[velType] *= mod;
    }
    public void SetMultVelocityMod(VelocityType velType, VelocityID mod)
    {
        VelMultModMap[velType] = mod;
    }
    public void AppendAllAddVelocityMods(VelocityID mod)
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
    public void SetAllAddVelocityMods(VelocityID mod)
    {
        foreach (var modType in Global.GetEnumValues<VelocityType>())
        {
            VelAddModMap[modType] = mod;
        }
    }
    public void AppendAllMultVelocityMods(VelocityID mod)
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
    public void SetAllMultVelocityMods(VelocityID mod)
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

    public float TimeSinceAttackRequest()
    {
        throw new NotImplementedException();
    }
}
