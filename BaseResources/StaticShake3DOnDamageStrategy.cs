using Godot;
using System;
using System.Collections.Generic;

[GlobalClass, Tool]
public partial class StaticShake3DOnDamageStrategy : BreakableOnDamageStrategy
{
    [Export]
    public float ShakeDist { get; private set; } = 0.5f;
    [Export]
    public int ShakeCycles { get; private set; } = 2;
    [Export(PropertyHint.Range, "0.0,0.25,0.01,or_greater")]
    public float PerShakeTime { get; private set; } = 0.05f;
    [Export]
    public bool ShakeY { get; private set; } = false;
    [Export]
    public Tween.EaseType TweenEase { get; private set; } = Tween.EaseType.InOut;
    [Export]
    public Tween.TransitionType TweenTransition { get; private set; } = Tween.TransitionType.Elastic;
    public StaticShake3DOnDamageStrategy() : base()
    {
    }
    public override void Damage()
    {
        if (Breakable is not Node3D shakeable3D)
        {
            throw new Exception("Breakable OnDamage ERROR || Breakable is not Node3D!");
        }
        var shakePoses = new List<Vector3>();
        for (int i = 0; i < ShakeCycles; i++)
        {
            var shakePos = new Vector3
                (Global.GetRndInRange(-ShakeDist, ShakeDist), 0f, 
                Global.GetRndInRange(-ShakeDist, ShakeDist));
            if (ShakeY)
            {
                shakePos.Y += Global.GetRndInRange(-ShakeDist, ShakeDist);
            }
            shakePoses.Add(shakeable3D.Position + shakePos);
        }
        var shakeTween = shakeable3D.GetTree().CreateTween();
        foreach (var pos in shakePoses)
        {
            shakeTween.TweenProperty(shakeable3D, "position:x",
                    pos.X, PerShakeTime).SetEase(TweenEase).SetTrans(TweenTransition);
            shakeTween.Parallel().TweenProperty(shakeable3D, "position:z",
                pos.Z, PerShakeTime).SetEase(TweenEase).SetTrans(TweenTransition);
            if (ShakeY)
            {
                shakeTween.Parallel().TweenProperty(shakeable3D, "position:y",
                    pos.Y, PerShakeTime).SetEase(TweenEase).SetTrans(TweenTransition);
            }
        }
        shakeTween.TweenProperty(shakeable3D, "position:x",
             shakeable3D.Position.X, PerShakeTime).SetEase(TweenEase).SetTrans(TweenTransition);
        shakeTween.Parallel().TweenProperty(shakeable3D, "position:z",
            shakeable3D.Position.Z, PerShakeTime).SetEase(TweenEase).SetTrans(TweenTransition);
        if (ShakeY)
        {
            shakeTween.Parallel().TweenProperty(shakeable3D, "position:y",
                shakeable3D.Position.Y, PerShakeTime).SetEase(TweenEase).SetTrans(TweenTransition);
        }
    }
}
