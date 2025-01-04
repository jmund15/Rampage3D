using Godot;
using System;
using static System.Net.Mime.MediaTypeNames;

public partial class DestroyableStructureComponent : Node
{
    #region COMPONENT_VARIABLES
    //[Export]
    //private HurtboxComponent3D _hurtboxComp;
    [Export]
    private HealthComponent _healthComp;
    [Export]
    private Node3D _structure;

    
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
	{
        base._Ready();
        _healthComp.HealthChanged += OnHealthChanged;
        _healthComp.Destroyed += OnDestroyed;
    }
    public override void _Process(double delta)
	{
        base._Process(delta);
	}
    #endregion
    #region SIGNAL_LISTENERS
    private void OnDestroyed(HealthUpdate destroyUpdate)
    {
        GetOwner().CallDeferred(Node.MethodName.QueueFree);
        //GetOwner().QueueFree();
    }
    private void OnHealthChanged(HealthUpdate healthUpdate)
    {
        GD.Print("ON HEALTH CHANGED FUNCTION ENTER");
        var hitDirection = healthUpdate.Attack.Direction;
        var damage = healthUpdate.HealthChange;
        if (_healthComp.Health != 0)
        {
            //var polyPlayback = _sfxComponent.GetStreamPlayback() as AudioStreamPlaybackPolyphonic;
            //_damageSfxStreamNum = polyPlayback.PlayStream(OnDamageSfx);
        }
        var scaleMult = damage * 0.5f;
        var posMult = ((damage + 0.2f) / 2f) * 10f;

        var scaleShift = scaleMult * hitDirection;
        var posShift = posMult * hitDirection;
        GD.Print("SCALE SHIFTING: ", scaleShift);
        //WIGGLE
        var scaleTween = CreateTween();
        scaleTween.TweenProperty(_structure, "scale", _structure.Scale - scaleShift, 0.1f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
        scaleTween.TweenProperty(_structure, "scale", _structure.Scale + (scaleShift / 2), 0.1f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
        scaleTween.TweenProperty(_structure, "scale", _structure.Scale, 0.1f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
    }
    #endregion
    #region COMPONENT_HELPER
    #endregion
}
