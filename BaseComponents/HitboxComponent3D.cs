using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

[GlobalClass, Tool]
public partial class HitboxComponent3D : Area3D
{
    #region CLASS_VARIABLES
    public const string HitboxComponentNodeName = "HitboxComponent";
    [Export]
    private HurtboxComponent3D _ignoreHurtbox = null;

    //[Export]
    //public float BaseDamage { get; private set; }
    //[Export]
    //public float BaseForce { get; private set; }
    public RampageHitboxAttack CurrentAttack { get; private set; }
    private bool _attackActive = false;
    public bool AttackActive //{ get; private set; } = false;
    {
        get => _attackActive;
        set
        {
            if (_attackActive == value) { return; }
            _attackActive = value;
            if (_attackActive )
            {
                EmitSignal(SignalName.AttackStarted);
            }
            else
            {
                EmitSignal(SignalName.AttackFinished);
            }
        }
    }
    private bool _velocityAttackActive = false;
    public HitboxVelocityAttack VelocityAttack { get; private set; }
    public CollisionShape3D CollisionShape { get; private set; }
    public List<HurtboxComponent3D> HurtboxesInHitbox { get; private set; } = new List<HurtboxComponent3D>();

    [Signal]
    public delegate void AttackHitEventHandler(RampageHitboxAttack attack);
    [Signal]
    public delegate void HurtboxEnteredEventHandler(HurtboxComponent3D hurtbox);
    [Signal]
    public delegate void AttackStartedEventHandler();
    [Signal]
    public delegate void AttackFinishedEventHandler();
    
    #endregion

    #region BASE_GODOT_OVERRIDEN_FUNCTIONS
    public override void _Ready()
    {
        base._Ready();
        CollisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
        //BodyEntered += OnBodyEntered;
        //BodyExited += OnBodyExited;

        Monitorable = false;
        Monitoring = false;
        AttackActive = false;
        
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_velocityAttackActive)
        {
            //CurrentAttack = new RampageHitboxAttack(
            //    damage: VelocityAttack.BaseDamage + VelocityAttack.GetBodyVelocity().Length() * VelocityAttack.VelDamageMult,
            //    force: VelocityAttack.BaseForce + VelocityAttack.GetBodyVelocity().Length() * VelocityAttack.VelForceMult,
            //    direction: VelocityAttack.GetBodyVelocity().Normalized()
            //    );
            //GD.Print("current velocity attack - \ndamage: ", CurrentAttack.Damage, 
            //    "\nforce: ", CurrentAttack.Force,
            //    "\ndirection: ", CurrentAttack.Direction);
        }
    }
    #endregion

    #region COMPONENT_FUNCTIONS
    public void SetCurrentAttack(float damage, float force, Vector3 direction, AttackBuildingEffect buildingEffect)
    {
        CurrentAttack = new RampageHitboxAttack(damage, force, direction, buildingEffect);
        _velocityAttackActive = false;
    }
    public void SetCurrentAttack(RampageHitboxAttack hitboxAttack)
    {
        CurrentAttack = hitboxAttack;
        _velocityAttackActive = false;
    }
    public void SetVelocityAttack(PhysicsBody3D velBody, float baseDamage, float baseForce, float velDamageMult, float velForceMult)
    {
        VelocityAttack = new HitboxVelocityAttack(velBody, baseDamage, baseForce, velDamageMult, velForceMult);
        _velocityAttackActive = true;
        //TODO: MAKE 'IVelocityComponent' and FIX
    }
    public void HitboxActivate()
    {
        SetDeferred(PropertyName.Monitorable, true);
        SetDeferred(PropertyName.Monitoring, true);
        SetDeferred(PropertyName.AttackActive, true);
    }
    public void HitboxDeactivate()
    {
        SetDeferred(PropertyName.Monitorable, false);
        SetDeferred(PropertyName.Monitoring, false);
        SetDeferred(PropertyName.AttackActive, false);
        SetDeferred(PropertyName._velocityAttackActive, false);

        //CallDeferred(MethodName.EmitSignal, SignalName.AttackFinished);
        //do something with current attack?
    }
    public void HitboxActivateWithTimer(float attackTime)
    {
        Monitorable = true;
        Monitoring = true;
        AttackActive = true;
        GetTree().CreateTimer(attackTime).Timeout += HitboxDeactivate;
    }
    //public void StartNewAttack(float damage, float force, Vector2 direction, AttackRecoilType armType = AttackRecoilType.Hit)
    //{
    //    CurrentAttack = new HitboxAttack(damage, force, direction, armType);
    //    Monitorable = true;
    //    Monitoring = true;
    //    AttackActive = true;
    //}
    //public void StartNewAttackWithTimer(float damage, float force, Vector2 direction, float attackTime, AttackRecoilType armType = AttackRecoilType.Hit)
    //{
    //    CurrentAttack = new HitboxAttack(damage, force, direction, armType);
    //    Monitorable = true;
    //    Monitoring = true;
    //    AttackActive = true;
    //    GetTree().CreateTimer(attackTime).Timeout += HitboxDeactivate;
    //}
    //public void StartVelocityAttack(PhysicsBody3D velBody, float baseDamage, float baseForce, float velDamageMult, float velForceMult)
    //{
    //    VelocityAttack = new HitboxVelocityAttack(velBody, baseDamage, baseForce, velDamageMult, velForceMult);
    //    Monitorable = true;
    //    Monitoring = true;
    //    AttackActive = true;
    //    _velocityAttackActive = true;
    //}
    
    #endregion

    #region SIGNAL_LISTENERS
    private void OnAreaEntered(Area3D area)
    {
        if (area is HurtboxComponent3D hurtboxComponent &&
            hurtboxComponent != _ignoreHurtbox)
        {
            //GD.Print($"HURTBOX ENTERED HITBOX OF '{GetOwner().Name}'!");
            //GD.Print($"HURTBOX OWNER: {hurtboxComponent.GetOwner().Name}\nHITBOX OWNER: {GetOwner().Name}");

            HurtboxesInHitbox.Add(hurtboxComponent);
            EmitSignal(SignalName.AttackHit, CurrentAttack);
            EmitSignal(SignalName.HurtboxEntered, hurtboxComponent);
        }
    }
    private void OnAreaExited(Area3D area)
    {
        if (area is HurtboxComponent3D hurtboxComponent &&
            hurtboxComponent != _ignoreHurtbox)
        {
            HurtboxesInHitbox.Remove(hurtboxComponent);
        }
    }
    private void OnBodyEntered(Node3D body)
    {
        //throw new NotImplementedException();
    }

    private void OnBodyExited(Node3D body)
    {
        //throw new NotImplementedException();
    }
    #endregion

    #region HELPER_CLASSES
    public partial class HitboxVelocityAttack : Resource
    {
        public PhysicsBody3D VelocityBody { get; set; }
        public float BaseDamage { get; private set; }
        public float BaseForce { get; private set; }
        public float VelDamageMult { get; private set; }
        public float VelForceMult { get; private set; }
        public HitboxVelocityAttack()
        {
            VelocityBody = null;
            BaseDamage = 0.0f;
            BaseForce = 0.0f;
            VelDamageMult = 0.0f;
            VelForceMult = 0.0f;
        }
        public HitboxVelocityAttack(PhysicsBody3D velBody, float damage, float force, float velDamageMult, float velForceMult)//, Array<AttackEffect>? attackEffects = null)
        {
            VelocityBody = velBody;
            BaseDamage = damage;
            BaseForce = force;
            VelDamageMult = velDamageMult;
            VelForceMult = velForceMult;
        }
        public Vector3 GetBodyVelocity()
        {
            switch (VelocityBody)
            {
                case Node3D n when n is CharacterBody3D charBody:
                    return charBody.Velocity;
                case Node3D n when n is StaticBody3D staticBody:
                    return staticBody.ConstantLinearVelocity;
                case Node3D n when n is RigidBody3D rigidBody:
                    return rigidBody.LinearVelocity;
                default:
                    GD.PrintErr("Velocity Body isn't one of the three main physics bodies??");
                    return Vector3.Zero;
            }
        }
    }
}
public enum AttackEffect
{
    Poison,
    Stun
}
#endregion