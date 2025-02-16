using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeRobbers.Interfaces;


public partial class MultiConfigAnimPlayerComponent : IAnimPlayerComponent
{
    public Godot.Collections.Dictionary<AnimationPlayer, string> AnimationPlayerTypeMap { get; private set; } =
        new Godot.Collections.Dictionary<AnimationPlayer, string>();
    private List<AnimationPlayer> _animPlayers;

    public event EventHandler<string> AnimStarted;
    public event EventHandler<string> AnimFinished;

    public MultiConfigAnimPlayerComponent(List<AnimationPlayer> animPlayers)
    {
        _animPlayers = animPlayers;
        _animPlayers[0].AnimationStarted += (animName) =>
        {
            AnimStarted?.Invoke(this, animName);
        };
        _animPlayers[0].AnimationFinished += (animName) =>
        {
            AnimFinished?.Invoke(this, animName);
        };

        foreach(var animPlayer in _animPlayers)
        {
            var animList = animPlayer.GetAnimationList();

            HashSet<char> styles = new HashSet<char>();
            foreach (var anim in animList)
            {
                var styleChar = anim[anim.Length - 1];
                if (styles.Contains(styleChar)) { continue; }
                styles.Add(styleChar);
            }

            AnimationPlayerTypeMap.Add(animPlayer, "");
        }
    }
    public string GetCurrAnimation()
    {
        return _animPlayers[0].CurrentAnimation;
    }

    public float GetCurrAnimationPosition()
    {
        return (float)_animPlayers[0].CurrentAnimationPosition;
    }


    public void StartAnim(string animName)
    {
        foreach (var animType in AnimationPlayerTypeMap )
        {
            var animPlayer = animType.Key;
            var type = animType.Value;
            animPlayer.Play(animName + type);
        }
    }

    public void StopAnim()
    {
        foreach (var animPlayer in _animPlayers)
        {
            animPlayer.Stop();
        }
    }

    public void UpdateAnim(string animName)
    {
        if (GetCurrAnimation() == animName) { return; }
        if (!_animPlayers[0].IsPlaying()) { StartAnim(animName); }

        var currAnimPos = GetCurrAnimationPosition();
        StartAnim(animName);
        SeekPos(currAnimPos);
    }

    public void SeekPos(float time, bool updateNow = true)
    {
        foreach (var animPlayer in _animPlayers)
        {
            animPlayer.Seek(time, updateNow);
        }
    }

    public void FastForward(float time)
    {
        foreach(var animPlayer in _animPlayers)
        {
            animPlayer.Advance(time);
        }
    }

    public float GetCurrAnimationLength()
    {
        return (float)_animPlayers[0].CurrentAnimationLength;
    }

    public void PauseAnim()
    {
        foreach (var animPlayer in _animPlayers)
        {
            animPlayer.Pause();
        }
    }

    public float GetSpeedScale()
    {
        return _animPlayers[0].GetPlayingSpeed();
    }

    public void SetSpeedScale(float speedScale)
    {
        foreach (var animPlayer in _animPlayers)
        {
            animPlayer.SpeedScale = speedScale;
        }
    }

    public bool IsAnimating()
    {
        throw new NotImplementedException();
    }

    public bool AnimationExists(string animName)
    {
        throw new NotImplementedException();
    }
}
