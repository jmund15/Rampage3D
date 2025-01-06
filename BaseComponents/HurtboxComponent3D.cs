using Godot;
using System;
using System.Collections.Generic;

[GlobalClass, Tool]
public partial class HurtboxComponent3D : Area3D
{
    #region CLASS_VARIABLES
    public const string HurtboxComponentNodeName = "HurtboxComponent";

    [Export]
    public HealthComponent HealthComponent { get; private set; }
    [Export]
    private HitboxComponent3D _ignoreHitbox = null;
    public bool HasHealthComponent { get; private set; }
    public CollisionShape3D CollisionShape { get; private set; }
    public RampageHitboxAttack LatestAttackBase { get; set; }
    public RampageHitboxAttack LatestAttackModified { get; set; }
    public Node3D LatestAttacker { get; set; }
    public List<HitboxComponent3D> HitboxesInHurtbox { get; private set; } = new List<HitboxComponent3D>();
    //public List<DamageBodyComponent> DamageBodiesInHurtbox { get; private set; } = new List<DamageBodyComponent>();
    //public HurtboxState State { get; set; }
    public bool Invulnerable { get; set; } = false;
    public bool Defending { get; set; } = false;
    public float TimeSinceAttacked = 0f;

    [Signal]
    public delegate void HitboxEnteredEventHandler(HitboxComponent3D hitbox);
    //[Signal]
    //public delegate void DamageBodyEnteredEventHandler(DamageBodyComponent damageBody);
    [Signal]
    public delegate void AttackedEventHandler(RampageHitboxAttack attack);
    #endregion
    #region BASE_GODOT_OVERRIDEN_FUNCTIONS
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        base._Ready();
        CollisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
        //BodyEntered += OnBodyEntered;
        //BodyExited += OnBodyExited;
        //HasHealthComponent = (_healthComponent == null);
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        base._Process(delta);
        if (HitboxesInHurtbox.Count == 0) // needed??
        {
            TimeSinceAttacked += (float)delta;
        }
	}
    #endregion

    #region COMPONENT_FUNCTIONS
    public void DeactivateHurtbox()
    {
        SetDeferred(PropertyName.Monitorable, false);
        SetDeferred(PropertyName.Monitoring, false);
    }
    public void ReactivateHurtbox()
    {
        SetDeferred(PropertyName.Monitorable, true);
        SetDeferred(PropertyName.Monitoring, true);
    }
    private void HurtImpactEffect(HitboxComponent3D hitbox)
    {

    }
    #endregion
    #region SIGNAL_LISTENERS
    private void OnAreaEntered(Area3D area)
    {
        if (area is HitboxComponent3D hitboxComponent &&
            hitboxComponent != _ignoreHitbox) 
        {
            //GD.Print($"'{GetOwner().Name}' IS BEING ATTACKED bY HITBOX!");
            //GD.Print($"HITBOX OWNER: {hitboxComponent.GetOwner().Name}\nHURTBOX OWNER: {GetOwner().Name}");
            TimeSinceAttacked = 0f;
            HitboxesInHurtbox.Add(hitboxComponent);

            LatestAttackBase = hitboxComponent.CurrentAttack;
            LatestAttackModified = LatestAttackBase;// _attackReceiveStrat == null ? LatestAttackBase :
                //_attackReceiveStrat.ReceiveAttack(LatestAttackBase);

            //GD.Print("ATTACK BASE DMG: ", LatestAttackBase.Damage);
            //GD.Print("ATTACK MOD DMG: ", LatestAttackModified.Damage);

            HealthComponent?.DamageWithAttack(LatestAttackModified);

            LatestAttacker = hitboxComponent.GetOwner<Node3D>();

            EmitSignal(SignalName.Attacked, LatestAttackModified);
            EmitSignal(SignalName.HitboxEntered, hitboxComponent);
        }
    }
    private void OnAreaExited(Area3D area)
    {
        if (area is HitboxComponent3D hitboxComponent &&
            hitboxComponent != _ignoreHitbox)
        {
            HitboxesInHurtbox.Remove(hitboxComponent);
        }
    }
    //private void OnBodyEntered(Node3D body)
    //{
    //    //// OLD AND SLOW
    //    //if (body is PhysicsBody2D physicsBody)
    //    //{
    //    //    if (physicsBody.GetNode<DamageBodyComponent>(DamageBodyComponent.DamageBodyComponentSceneName) is DamageBodyComponent damageBody)
    //    //    {

    //    //    }
    //    //}
    //    if (body.GetNodeOrNull<DamageBodyComponent>(DamageBodyComponent.DamageBodyComponentNodeName) is not null &&
    //        body.GetNode<DamageBodyComponent>(DamageBodyComponent.DamageBodyComponentNodeName) is DamageBodyComponent damageBody)
    //    {
    //        DamageBodiesInHurtbox.Add(damageBody);
    //        LatestAttackBase = damageBody.CurrentAttack;
    //        LatestAttackModified = _attackReceiveStrat == null ? LatestAttackBase :
    //            _attackReceiveStrat.ReceiveAttack(LatestAttackBase);
    //        HealthComponent?.DamageWithAttack(damageBody.CurrentAttack);

    //        //HurtImpactEffect(hitboxComponent);
    //        EmitSignal(SignalName.Attacked, LatestAttackModified);
    //        EmitSignal(SignalName.DamageBodyEntered, damageBody);
    //    }
    //}
    //private void OnBodyExited(Node3D body)
    //{
    //    if (body.GetNodeOrNull<DamageBodyComponent>(DamageBodyComponent.DamageBodyComponentNodeName) is not null &&
    //        body.GetNode<DamageBodyComponent>(DamageBodyComponent.DamageBodyComponentNodeName) is DamageBodyComponent damageBody)
    //    {
    //        DamageBodiesInHurtbox.Remove(damageBody);
    //    }
    //}
    #endregion

    #region HELPER_CLASSES
}
public enum DefenseEffect
{
    Armour,
    WeightBoost
}
#endregion




