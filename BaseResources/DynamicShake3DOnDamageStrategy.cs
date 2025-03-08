using Godot;
using System;
using System.Collections.Generic;

public abstract partial class DynamicShake3DOnDamageStrategy : BreakableOnDamageStrategy
{
    [Export]
    public float ShakeForce { get; private set; } = 0.5f;
    //[Export]
    //public int ShakeCycles { get; private set; } = 1;
    //[Export]
    //public float ShakeCycleTime { get; private set; } = 0.5f;
    [Export]
    public bool ShakeY { get; private set; } = false;
    public DynamicShake3DOnDamageStrategy()
    {
    }
    public override void Damage()
    {
        if (Breakable is not Node3D shakeable3D)
        {
            throw new Exception("Breakable OnDamage ERROR || Breakable is not Node3D!");
        }
        //float timeToCollapse = Global.GetRndInRange(2.0f, 3.0f);

        //var shakePoses = new List<Vector3>();
        //for (int i = 0; i < ShakeCycles; i++)
        //{
        //    var shakePos = new Vector3
        //        (Global.GetRndInRange(-ShakeForce, ShakeForce), 0f, 
        //        Global.GetRndInRange(-ShakeForce, ShakeForce));
        //    if (ShakeY)
        //    {
        //        shakePos.Y += Global.GetRndInRange(-ShakeForce, ShakeForce);
        //    }
        //    shakePoses.Add(shakeable3D.Position + shakePos);
        //}
        //var shakeTween = shakeable3D.GetTree().CreateTween();
        //while (timeToCollapse > 0f)
        //{
        //    foreach (var pos in shakePoses)
        //    {
        //        shakeTween.TweenProperty(this, "position:x",
        //            pos.X, 0.05f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
        //        shakeTween.Parallel().TweenProperty(this, "position:z",
        //            pos.Z, 0.05f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
        //        timeToCollapse -= 0.05f;
        //    }
        //}
        //shakeTween.TweenProperty(this, "position:x",
        //     this.Position.X, 0.05f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
        //shakeTween.Parallel().TweenProperty(this, "position:z",
        //    this.Position.Z, 0.05f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);

    }
}
