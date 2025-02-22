using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeRobbers.Interfaces;


public partial class AnimSprite3DAnimComponent : IAnimSpriteComponent
{
    private AnimatedSprite3D _animSprite;

    public event EventHandler<string> AnimStarted;
    public event EventHandler<string> AnimFinished;

    public AnimSprite3DAnimComponent(AnimatedSprite3D animSprite)
    {
        _animSprite = animSprite;
        _animSprite.AnimationChanged += () =>
        {
            AnimStarted?.Invoke(this, GetCurrAnimation());
        };
        _animSprite.AnimationFinished += () =>
        {
            AnimFinished?.Invoke(this, GetCurrAnimation());
        };
    }

    public void StartAnim(string animName)
    {
        _animSprite.Play(animName);
    }

    public void StopAnim()
    {
        _animSprite.Stop();
    }
    public void PauseAnim()
    {
        _animSprite.Pause();
    }
    public string GetCurrAnimation()
    {
        return _animSprite.Animation;
    }
    public void UpdateAnim(string animName)
    {
        if (GetCurrAnimation() == animName) { return; }
        if (!_animSprite.IsPlaying()) { StartAnim(animName); }

        var currFrame = GetFrame();
        var currProg = GetFrameProgress();
        StartAnim(animName);
        SetFrameAndProgress(currFrame, currProg);
    }
    public bool IsPlaying()
    {
        return _animSprite.IsPlaying();
    }
    public float GetSpeedScale()
    {
        return _animSprite.GetPlayingSpeed();
    }

    public void SetSpeedScale(float speedScale)
    {
        _animSprite.SpeedScale = speedScale;
    }

    public void SetFrame(int frame)
    {
        _animSprite.Frame = frame;
    }

    public void SetFrameAndProgress(int frame, float progress)
    {
        _animSprite.SetFrameAndProgress(frame, progress);
    }

    public int GetFrame()
    {
        return _animSprite.Frame;
    }

    public float GetFrameProgress()
    {
        return _animSprite.FrameProgress;
    }

    public bool HasAnimation(string animName)
    {
        return _animSprite.SpriteFrames.HasAnimation(animName);
    }

    public Node GetInterfaceNode()
    {
        throw new NotImplementedException();
    }
}
