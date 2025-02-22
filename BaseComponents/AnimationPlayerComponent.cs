using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public partial class AnimationPlayerComponent : AnimationPlayer, IAnimPlayerComponent, IConfigAnimComponent
{
    #region COMPONENT_VARIABLES
    public string BaseAnimName { get; private set; } = "";
    [Export]
    private Godot.Collections.Array<string> _configOptions = new Godot.Collections.Array<string>();
    private int _configIdx = 0;
    private string _configLabel = "";
    [Export]
    private string ConfigLabel
    {
        get => _configLabel;
        set
        {
            if (_configLabel == value || _configOptions.Count == 0) { return; }
            for (int i = 0; i < _configOptions.Count; i++)
            {
                var config = _configOptions[i];
                if (value == config)
                {
                    _configIdx = i;
                    _configLabel = value;
                    ConfigChanged?.Invoke(this, _configLabel);
                    return;
                }
            }
            GD.PrintErr($"IConfigAnimComponent ERROR || Provided config '{value}' isn't in config option list!");
        }
    }
    private string _directionLabel = "";

    public event EventHandler<string> AnimStarted;
    public event EventHandler<string> AnimFinished;
    public event EventHandler<string> ConfigChanged;
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
	{
		base._Ready();
        AnimationStarted += (animName) =>
        {
            AnimStarted?.Invoke(this, animName);
        };
        AnimationFinished += (animName) =>
        {
            AnimFinished?.Invoke(this, animName);
        };

        //ConfigChanged += OnConfigChanged;
        if (_configOptions.Count > 0)
        {
            ConfigLabel = _configOptions[0];
        }
    }
    private void OnConfigChanged(object sender, string e)
    {
        if (IsPlaying())
        {
            GD.Print("updating anim with config label: ", ConfigLabel, "!");
            UpdateAnim(BaseAnimName);
        }
    }

    public override void _Process(double delta)
	{
		base._Process(delta);
	}
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
	}

    public void StartAnim(string animName)
    {
        if (!HasAnimation(animName))
        {
            GD.PrintErr("ERROR || Animation doesn't Exist!!!");
        }
        Play(animName + ConfigLabel);
        BaseAnimName = animName;
    }
    public void PauseAnim()
    {
        Pause();
    }
    public void StopAnim()
    {
        Stop();
    }
    public void SeekPos(float time, bool updateNow = true)
    {
        Seek(time, updateNow);
    }
    public void FastForward(float time)
    {
        Advance(time);
    }

    public string GetCurrAnimation()
    {
        return CurrentAnimation;
    }

    public float GetCurrAnimationLength()
    {
        return (float)CurrentAnimationLength;
    }
    public float GetCurrAnimationPosition()
    {
        return (float)CurrentAnimationPosition;
    }
    //public float GetSpeedScale()
    //{
    //    return GetPlayingSpeed();
    //}
    //public void SetSpeedScale(float speedScale)
    //{
    //    SpeedScale = speedScale;
    //}
    public void UpdateAnim(string animName)
    {
        if (GetCurrAnimation() == (animName + ConfigLabel)) { return; }
        if (!IsPlaying()) { StartAnim(animName); }

        var currAnimPos = GetCurrAnimationPosition();
        StartAnim(animName);
        SeekPos(currAnimPos);
    }
    //public bool IsPlaying()
    //{
    //    return IsPlaying();
    //}
    public bool HasAnimation(string animName)
    {
        return base.HasAnimation(animName + ConfigLabel);
    }
    public void SetConfig(string type)
    {
        ConfigLabel = type;
    }
    public void IterateConfig()
    {
        if (_configOptions.Count <= 1) { return; }

        _configIdx++;
        if (_configIdx == _configOptions.Count)
        {
            _configIdx = 0;
        }
        ConfigLabel = _configOptions[_configIdx];
    }
    public string GetConfig()
    {
        return ConfigLabel;
    }
    public List<string> GetConfigOptions()
    {
        return _configOptions.ToList();
    }

    public void ResetConfig()
    {
        if (_configOptions.Count > 0)
        {
            ConfigLabel = _configOptions[0];
        }
    }
    #endregion
    #region COMPONENT_HELPER
    public void SetConfigOptions(List<string> configOptions)
    {
        _configOptions = new Godot.Collections.Array<string>(configOptions);
    }

    public Node GetInterfaceNode()
    {
        return this;
    }

    //private string GetAnimName()
    //{
    //    return animName + _configLabel;
    //}
    #endregion
    #region SIGNAL_LISTENERS
    #endregion
}
