using Godot;
using System;
using Godot.Collections;

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

	public SpriteOrthogComponent Sprite { get; protected set; }
	public AnimationPlayer AnimPlayer { get; protected set; }
    public HealthComponent HealthComp { get; protected set; }
    public HurtboxComponent3D HurtboxComp { get; protected set; }
    public HitboxComponent3D HitboxComp { get; protected set; }
    public EaterComponent EaterComp { get; protected set; }
    public ClimberComponent ClimberComp { get; protected set; }

    private CompoundState _stateMachine;
    private Dictionary<State, bool> _parallelStateMachines = new Dictionary<State, bool>();
    public State PrimaryState { get; protected set; }
    public Dictionary<State, bool> ParallelStates { get; protected set; } =
        new Dictionary<State, bool>();

    public static float Speed { get; private set; } = 7.0f;
    public static float AirSpeed { get; private set; } = 2.5f;
    public static float AirHorizontalFriction { get; private set; } = 0.2f;
    public static float ClimbSpeed { get; private set; } = 5.0f;
    public static float JumpVelocity { get; private set; } = 7.5f;
    public static float ClimbJumpPushOff { get; private set; } = 6.0f;
    public static float FallHeight { get; private set; } = 3f;
    public static float SkidFriction { get; private set; } = 1f;

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

    public override void _Ready()
    {
        base._Ready();
        MonsterF = MonsterFormStart;
        MonsterId = new MonsterIdentifier(MonsterT, MonsterF);

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

        HitboxComp = this.GetFirstChildOfType<HitboxComponent3D>();
        EaterComp = HitboxComp.GetFirstChildOfType<EaterComponent>();
        ClimberComp = this.GetFirstChildOfType<ClimberComponent>();
        BB.SetVar(BBDataSig.HitboxComp, HitboxComp);
        BB.SetVar(BBDataSig.EaterComp, EaterComp);
        BB.SetVar(BBDataSig.ClimberComp, ClimberComp);

        HurtboxComp = this.GetFirstChildOfType<HurtboxComponent3D>();
        BB.SetVar(BBDataSig.HurtboxComp, HurtboxComp);

        SetMonsterAttacks();

        //Shadow.Scale = BagStateComponent.CurrentRobberShadowScale;

        _stateMachine = GetNode<CompoundState>("MSM");
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
        return Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");//Input.GetVector(LeftInput, RightInput, UpInput, DownInput);
    }

    public Vector2 GetDesiredDirectionNormalized()
    {
        throw new NotImplementedException();
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
