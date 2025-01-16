using Godot;
using System;
using System.Collections.Generic;

public enum FloorHealthState
{
    Stable,
    Cracked,
    Crumbling,
    Destroyed
}

[GlobalClass, Tool]
public partial class BuildingFloorComponent : MeshInstance3D
{
    #region COMPONENT_VARIABLES
    [Export]
    public float FloorMaxHealth { get; private set; } = 0f;
    public HealthComponent HealthComp { get; private set; }

    public float YCenter { get; private set; }
    public FloorHealthState HealthState { get; private set; }

    private float _healthStateChangeAmt;

    private Dictionary<FloorHealthState, float> _healthStateMap = new Dictionary<FloorHealthState, float>();
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
    {
        base._Ready();
        HealthComp = this.GetFirstChildOfType<HealthComponent>();
        HealthComp.SetMaxHealth(FloorMaxHealth);
        HealthComp.Damaged += OnDamaged;

        _healthStateChangeAmt = HealthComp.MaxHealth / 3f;
        _healthStateMap.Add(FloorHealthState.Stable, HealthComp.MaxHealth);
        _healthStateMap.Add(FloorHealthState.Cracked, HealthComp.MaxHealth - _healthStateChangeAmt);
        _healthStateMap.Add(FloorHealthState.Crumbling, HealthComp.MaxHealth - _healthStateChangeAmt * 2);
        _healthStateMap.Add(FloorHealthState.Destroyed, 0f);


        //define ycenter
        YCenter = Mesh.GetAabb().GetCenter().Y + GlobalPosition.Y;
        //GD.Print($"floor {Name} YCENTER: {YCenter}.");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
    #endregion
    #region SIGNAL_LISTENERS
    private void OnDamaged(object sender, HealthUpdate healthUpdate)
    {
        GD.Print($"FLOOR DAMAGED TO HEALTH {healthUpdate.NewHealth} out of {healthUpdate.MaxHealth}");
        var hitDirection = Vector3.Zero;
        if (healthUpdate.Attack == null)
        {
            hitDirection = Global.GetRndVector3(); //TODO: MAKE Y 0?
        }
        else
        {
            hitDirection = healthUpdate.Attack.Direction; //TODO: MAKE Y 0?
        }

        var damage = healthUpdate.HealthChange;
        if (healthUpdate.NewHealth <= 0)
        {
            //var polyPlayback = _sfxComponent.GetStreamPlayback() as AudioStreamPlaybackPolyphonic;
            //_damageSfxStreamNum = polyPlayback.PlayStream(OnDamageSfx);
        }
        //var scaleMult = damage * 0.5f;
        var posMult = ((damage + 0.2f) / 2f) * 0.08f;

        //var scaleShift = scaleMult * hitDirection;
        var posShift = posMult * hitDirection;
        GD.Print("POS SHIFTING: ", posShift);
        //WIGGLE
        var scaleTween = CreateTween();
        scaleTween.TweenProperty(this, "position", Position + posShift, 0.1f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
        scaleTween.TweenProperty(this, "position", Position - (posShift / 2), 0.1f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
        scaleTween.TweenProperty(this, "position", Position, 0.1f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);

    }
    #endregion
    #region COMPONENT_HELPER
    #endregion
}
