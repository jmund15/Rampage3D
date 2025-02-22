using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public interface IAnimComponent
{
    public event EventHandler<string> AnimStarted;
    public event EventHandler<string> AnimFinished;

    public void StartAnim(string animName = "");//, Enum? direction = null);
    public void PauseAnim();
    public void StopAnim();
    public void UpdateAnim(string animName);//, Enum? direction = null);
    //public void UpdateAnimationDirection(Enum? direction = null);
    public bool IsPlaying();
    public bool HasAnimation(string animName);
    public string GetCurrAnimation();
    //public Enum GetAnimationDirection();
    public float GetSpeedScale();
    public void SetSpeedScale(float speedScale);

    public Node GetInterfaceNode();
}
