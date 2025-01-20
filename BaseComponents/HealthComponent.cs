using Godot;
using System;

[GlobalClass, Tool]
public partial class HealthComponent : Node
{
    #region CLASS_VARIABLES
    public const string NodeName = "HealthComponent";

    [Export]
    public float MaxHealth {
        get { return _maxHealth; }
        private set 
        { 
            if (_maxHealth != value)
            {
                if (!_healthInitialized)
                {
                    _maxHealth = value;
                    return;
                }
                if (ChangeHealthOnMaxChange)
                {
                    var change = value - _maxHealth;
                    Health += change;
                }
                _maxHealth = value;
                if (Health > _maxHealth)
                {
                    Health = _maxHealth; // cap health at max
                }
                EmitSignal(SignalName.MaxHealthChanged, _maxHealth);
            }
        }
    }
    public float Health
    {
        get { return _health; }
        private set 
        {
            if (_health == value) { return; }
            if (!_healthInitialized)
            {
                //_health = Mathf.Clamp(value, 0f, MaxHealth);
                _health = value;
                return;
            }
            var prevHealth = _health;
            if (AllowHealthBelowZero)
            {
                _health = Mathf.Min(value, MaxHealth);
            }
            else
            {
                _health = Mathf.Clamp(value, 0f, MaxHealth);
            }
            LastHealthUpdate = new HealthUpdate(_health, prevHealth, MaxHealth, _damageAttack);
            EmitSignal(SignalName.HealthChanged, LastHealthUpdate);

            if (LastHealthUpdate.HealthChange < 0) 
            { Damaged?.Invoke(this, LastHealthUpdate); }
            else 
            { Healed?.Invoke(this, LastHealthUpdate); }

            if (_health == 0f)
            {
                if (!IsDead)
                {
                    IsDead = true;
                    EmitSignal(SignalName.Destroyed, LastHealthUpdate);
                }
            }
            else
            {
                IsDead = false;
            }
            
        }
    }

    [Export]
    public bool ChangeHealthOnMaxChange { get; private set; } = true;
    [Export]
    public bool AllowHealthBelowZero { get; private set; } = false;

    public bool IsDead
    {
        get { return _isDead; }
        set
        {
            if (value != _isDead)
            {
                _isDead = value;
            }
        }
    }
    public HealthUpdate LastHealthUpdate { get; private set; }

    private float _maxHealth;
    private float _health;
    private RampageHitboxAttack? _damageAttack;
    private bool _isDead = false;
    private bool _healthInitialized = false;

    public event EventHandler HealthInitialized;
    public event EventHandler<HealthUpdate> Damaged;
    public event EventHandler<HealthUpdate> Healed;
    [Signal]
    public delegate void HealthChangedEventHandler(HealthUpdate healthUpdate);
    [Signal]
    public delegate void MaxHealthChangedEventHandler(float newMax);
    [Signal]
    public delegate void DestroyedEventHandler(HealthUpdate destroyUpdate);
    [Signal]
    public delegate void RessurectEventHandler(HealthUpdate ressurectUpdate);
    #endregion

    #region BASE_GODOT_OVERRIDEN_FUNCTIONS
    public override void _Ready()
    {
        base._Ready();
        CallDeferred(MethodName.InitializeHealth);
        //InitializeHealth();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
    #endregion

    #region COMPONENT_FUNCTIONS
    public virtual void InitializeHealth()
    {
        Health = MaxHealth;
        IsDead = false;
        _healthInitialized = true;
        HealthInitialized?.Invoke(this, EventArgs.Empty);
    }
    public virtual void Damage(float damage)
    {
        _damageAttack = null;
        Health -= damage;
    }
    public virtual void DamageWithAttack(RampageHitboxAttack attack)
    {
        _damageAttack = attack;
        Health -= attack.Damage;
    }
    public virtual void Heal(float healAmt)
    {
        Damage(-healAmt);
    }

    public virtual void SetMaxHealth(float newMax)
    {
        MaxHealth = newMax;
    }
    #endregion

    #region SIGNAL_LISTENERS
    #endregion

    #region HELPER_CLASSES
    
}
public partial class HealthUpdate : Resource
{
    public float NewHealth { get; private set; }
    public float PreviousHealth { get; private set; }
    public float HealthChange { get; private set; }
    public float MaxHealth { get; private set; }
    public RampageHitboxAttack? Attack { get; private set; }
    public HealthUpdate(float newHealth, float previousHealth, float maxHealth, RampageHitboxAttack? attack)
    {
        NewHealth = newHealth;
        PreviousHealth = previousHealth;
        HealthChange = NewHealth - PreviousHealth;
        MaxHealth = maxHealth;
        Attack = attack;
    }
}
#endregion
