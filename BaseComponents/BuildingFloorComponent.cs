using Godot;
using System;

public partial class BuildingFloorComponent : MeshInstance3D
{
    #region COMPONENT_VARIABLES
    public HealthComponent HealthComp { get; private set; }

    public float YCenter { get; private set; }
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
    {
        base._Ready();
        HealthComp = this.GetFirstChildOfType<HealthComponent>();
        HealthComp.HealthChanged += OnHealthChanged;

        //define ycenter
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
    }
    #endregion
    #region SIGNAL_LISTENERS
    private void OnHealthChanged(HealthUpdate healthUpdate)
    {
        var hitDirection = healthUpdate.Attack.Direction; //TODO: MAKE Y 0?
        var damage = healthUpdate.HealthChange;
        if (healthUpdate.NewHealth <= 0)
        {
            //var polyPlayback = _sfxComponent.GetStreamPlayback() as AudioStreamPlaybackPolyphonic;
            //_damageSfxStreamNum = polyPlayback.PlayStream(OnDamageSfx);
        }
        //var scaleMult = damage * 0.5f;
        var posMult = ((damage + 0.2f) / 2f) * 10f;

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
