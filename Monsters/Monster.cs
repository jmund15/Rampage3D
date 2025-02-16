using Godot;
using System;
using Godot.Collections;

public class OrthogDirChangedArgs : EventArgs
{
    public OrthogDirection OldDir { get; set; }
    public OrthogDirection NewDir { get; set; }
    public OrthogDirChangedArgs(OrthogDirection oldDir, OrthogDirection newDir)
    {
        OldDir = oldDir;
        NewDir = newDir;
    }
}
public partial class Monster : CharacterBody3D, IMovementComponent
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
    private Dictionary<OrthogDirection, Vector3> MonsterPosOffsetMap = new Dictionary<OrthogDirection, Vector3>()
    {
        { OrthogDirection.UpRight, new Vector3(0, 0, -1) },
        { OrthogDirection.UpLeft, new Vector3(0, 0, -1) },
        { OrthogDirection.DownLeft, new Vector3(-0, 0, 1) },
        { OrthogDirection.DownRight, new Vector3(0, 0, 1) }
    };
    private Dictionary<OrthogDirection, Vector3> CollisionPositionMap = new Dictionary<OrthogDirection, Vector3>()
    {
        { OrthogDirection.UpRight, new Vector3(0, 0.8f, -0) },
        { OrthogDirection.UpLeft, new Vector3(0, 0.8f, -0) },
        { OrthogDirection.DownLeft, new Vector3(-0, 0.8f, -0.2f) },
        { OrthogDirection.DownRight, new Vector3(0, 0.8f, -0.2f) }
    };
    private Dictionary<OrthogDirection, Vector3> CollisionRotationMap = new Dictionary<OrthogDirection, Vector3>()
    {
        { OrthogDirection.UpRight, new Vector3(0, 45f, 90) },
        { OrthogDirection.UpLeft, new Vector3(0, -45f, 90) },
        { OrthogDirection.DownLeft, new Vector3(0, 45, 90) },
        { OrthogDirection.DownRight, new Vector3(0, -45f, 90) }
    };

    private Dictionary<OrthogDirection, Vector3> SpritePositionMap = new Dictionary<OrthogDirection, Vector3>()
    {
        { OrthogDirection.UpRight, new Vector3(0, -0, 0.8f) },
        { OrthogDirection.UpLeft, new Vector3(0, -0, 0.8f) },
        { OrthogDirection.DownLeft, new Vector3(0, -0, 0.8f) },
        { OrthogDirection.DownRight, new Vector3(0, -0, 0.8f) }
    };

    public SpriteOrthogComponent Sprite { get; protected set; }
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

    private OrthogDirection _currOrthogDir;
    public OrthogDirection CurrOrthogDir
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
    public static float Speed { get; private set; } = 6.0f;
    public static float AirMaxSpeed { get; private set; } = 3.75f;
    public static float AirHorizontalFriction { get; private set; } = 0.2f;
    public static float AirAcceleration { get; private set; } = 0.5f;
    public static float ClimbSpeed { get; private set; } = 4.5f;
    public static float JumpVelocity { get; private set; } = 7.5f;
    public static float ClimbJumpPushOff { get; private set; } = 5.0f;
    public static float FallHeight { get; private set; } = 3f;
    public static float SkidFriction { get; private set; } = 1f;
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
        AnimationPlayer animPlayer;
        AnimatedSprite3D animSprite;

        MonsterF = MonsterFormStart;
        MonsterId = new MonsterIdentifier(MonsterT, MonsterF);
        OrthogDirectionChanged += OnOrthogDirChanged;

        BB = this.GetFirstChildOfType<Blackboard>();
        BB.SetPrimVar(BBDataSig.SelfInteruptible, true);
        BB.SetVar(BBDataSig.Agent, this);
        BB.SetVar(BBDataSig.MoveComp, this); //since we implement "IMovementComponent" within this class we just send this object
        HealthComp = this.GetFirstChildOfType<HealthComponent>();
        BB.SetVar(BBDataSig.HealthComp, HealthComp);

        GD.Print("Before set sprite.");
        CollisionShape = this.GetFirstChildOfType<CollisionShape3D>();
        Sprite = this.GetFirstChildOfType<SpriteOrthogComponent>();
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
    public OrthogDirection GetFaceDirection()
    {
        return CurrOrthogDir;
    }
    public OrthogDirection GetDesiredFaceDirection()
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
        if (Input.IsActionPressed("attack_test"))
        {
            return true;
        }
        else if (Input.IsActionPressed("sattack_test"))
        {
            return true;
        }
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
