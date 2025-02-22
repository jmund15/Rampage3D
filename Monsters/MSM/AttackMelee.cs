using Godot;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public partial class AttackMelee : BehaviorAction
{
    #region TASK_VARIABLES
    protected CharacterBody3D Body;
    protected IMovementComponent MoveComp;
    protected HitboxComponent3D HitboxComp;
    protected IAnimComponent AnimPlayer;

    [Export]
    protected AnimDirectionStrategy AnimDirStrategy = AnimDirectionStrategy.InputDirection;
    [Export]
    protected MeleeAttackInfo AttackInfo;

    protected Vector2 InputDir;
    protected Vector3 AttackDirection;
    protected AnimDirection AttackAnimDir;

    protected Shape3D PrevHitboxShape;
    protected Vector3 PrevHitboxLocation;
    #endregion
    #region TASK_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        base.Init(agent, bb);
        Body = BB.GetVar<CharacterBody3D>(BBDataSig.Agent);
        HitboxComp = BB.GetVar<HitboxComponent3D>(BBDataSig.HitboxComp);
        MoveComp = BB.GetVar<IMovementComponent>(BBDataSig.MoveComp);
        AnimPlayer = BB.GetVar<IAnimComponent>(BBDataSig.Anim);

    }
    public override void Enter()
    {
        base.Enter();
        InputDir = MoveComp.GetDesiredDirection();

        switch(AnimDirStrategy)
        {
            case AnimDirectionStrategy.CurrDirection:
                AttackAnimDir = PlayAnim.CurrDirectionStrat(BB);
                var orthogDir = IMovementComponent.GetOrthogDirection(AttackAnimDir,
                    BB.GetVar<ISpriteComponent>(BBDataSig.Sprite).FlipH);
                AttackDirection = orthogDir.GetVector3();
                break;
            case AnimDirectionStrategy.InputDirection:
                AttackAnimDir = PlayAnim.InputDirectionStrat(BB);
                var inputDir = MoveComp.GetDesiredDirection();
                if (!inputDir.IsZeroApprox())
                {
                    AttackDirection = new Vector3(inputDir.X, 0, inputDir.Y);
                }
                else
                {
                    orthogDir = IMovementComponent.GetOrthogDirection(AttackAnimDir,
                    BB.GetVar<ISpriteComponent>(BBDataSig.Sprite).FlipH);
                    AttackDirection = orthogDir.GetVector3();
                }
                break;
        }
        if (AttackInfo.AnimName == "punch1")
        {
            GD.Print("PUNCH 1 ATTACK STARTING");
        }

        AnimPlayer.StartAnim(AttackInfo.AnimName + AttackAnimDir.GetAnimationString());
        AnimPlayer.AnimFinished += OnAnimationFinished;

        //if (AttackInfo.AnimName == "punch1")
        //{
        //    GD.Print("PUNCH 1 ANIM POS: ", AnimPlayer.CurrentAnimationPosition, "/", AnimPlayer.CurrentAnimationLength);
        //}

        PrevHitboxShape = HitboxComp.CollisionShape.Shape;
        PrevHitboxLocation = HitboxComp.CollisionShape.Position;

        HitboxComp.SetCurrentAttack(AttackInfo.Damage,
                AttackInfo.Knockback,
                AttackDirection.Normalized(),
                AttackInfo.BuildingEffect
        );

        //TODO: REPLACE WITH ANIM
        HitboxComp.HitboxActivate();
    }
    public override void Exit()
    {
        base.Exit();
        AnimPlayer.AnimFinished -= OnAnimationFinished;

        //TODO: REPLACE WITH ANIM
        HitboxComp.HitboxDeactivate();

        //HitboxComp.CollisionShape.SetDeferred(CollisionShape2D.PropertyName.Shape, PrevHitboxShape);
        //HitboxComp.CollisionShape.SetDeferred(CollisionShape2D.PropertyName.Position, PrevHitboxLocation);

        //HitboxComp.CollisionShape.Shape = PrevHitboxShape;
        //HitboxComp.CollisionShape.Position = PrevHitboxLocation;
    }
    public override void ProcessFrame(float delta)
    {
        base.ProcessFrame(delta);
        InputDir = MoveComp.GetDesiredDirection();
    }
    public override void ProcessPhysics(float delta)
    {
        base.ProcessPhysics(delta);
        //if (!Body.IsOnFloor())
        //{
        //    Vector3 velocity = Body.Velocity;
        //    Vector3 direction = (Body.Transform.Basis * new Vector3(InputDir.X, 0, InputDir.Y)).Normalized();

        //    velocity += Body.GetWeightedGravity() * delta;

        //    if (direction != Vector3.Zero)
        //    {
        //        velocity.X = direction.X * Monster.AirSpeed;
        //        velocity.Z = direction.Z * Monster.AirSpeed;
        //    }
        //    else
        //    {
        //        velocity.X = Mathf.MoveToward(Body.Velocity.X, 0, Monster.AirSpeed);
        //        velocity.Z = Mathf.MoveToward(Body.Velocity.Z, 0, Monster.AirSpeed);
        //    }

        //    Body.Velocity = velocity;
        //    Body.MoveAndSlide();
        //}

        //Body.Velocity -= Body.Velocity * Body.Friction * delta;
        //if (HitboxComp.AttackActive) // might need overidden for larger attacks where velocity only happens at certain times
        //{
        //    //GD.Print("hitbox active!");
        //    Body.Velocity += AttackDirection /** Body.Acceleration*/ * AttackInfo.Velocity * delta;
        //}
        //Body.MoveAndSlide();
        //GD.Print("attack velocity: ", Body.Velocity, "\nMag: ", Body.Velocity.Length());
    }
    #endregion
    #region TASK_HELPER
    private void OnAnimationFinished(object sender, string animName)
    {
        //GD.Print("FINISHED ATTACK ANIM");
        Status = TaskStatus.SUCCESS;
    }

    protected Vector2 GetFlippedHitboxPos(Vector2 hitboxPos)
    {
        return hitboxPos * new Vector2(-1, 1);
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        //

        return warnings.Concat(base._GetConfigurationWarnings()).ToArray();
    }
    #endregion
}

