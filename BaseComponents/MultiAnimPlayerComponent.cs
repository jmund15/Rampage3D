using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[GlobalClass, Tool]
public partial class MultiAnimPlayerComponent : Node3D, IAnimPlayerComponent, ISpriteComponent
{
    private string _editorPlayAnimName = "";
    [Export]
    public string EditorPlayAnimName
    {
        get => _editorPlayAnimName;
        set
        {
            if (value == _editorPlayAnimName) { return; }
            _editorPlayAnimName = value;
            //if (AnimationExists(_editorPlayAnimName))
            //{
            //    StartAnim(_editorPlayAnimName);
            //}
        }
    }
    public List<IAnimPlayerComponent> AnimPlayers { get; private set; } = new List<IAnimPlayerComponent>();
    public List<ISpriteComponent> Sprites { get; private set; } = new List<ISpriteComponent>();
    
    public bool FlipH 
    {
        get => Sprites[0].FlipH; 
        set
        {
            foreach (var sprite in Sprites)
            {
                sprite.FlipH = value;
            }
        }
    }
    public bool FlipV 
    {
        get => Sprites[0].FlipV;
        set
        {
            foreach (var sprite in Sprites)
            {
                sprite.FlipV = value;
            }
        }
    }

    public Vector2 Offset
    {
        get => Sprites[0].Offset;
        set
        {
            foreach (var sprite in Sprites)
            {
                sprite.Offset = value;
            }
        }
    }

    public event EventHandler<string> AnimStarted;
    public event EventHandler<string> AnimFinished;
    
    #region COMPONENT_VARIABLES
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
	{
		base._Ready();
        foreach (var child in this.GetChildrenOfType<Node>())
        {
            //GD.Print("child name: ", child.Name);
            if (child is IAnimPlayerComponent animPlayerComp)
            {
                AnimPlayers.Add(animPlayerComp);
            }
            else if (child is ISpriteComponent spriteComp)
            {
                Sprites.Add(spriteComp);
            }
        }
        if (AnimPlayers.Count == 0)
        {
            if (Engine.IsEditorHint()) { return; }
            else
            {
                GD.PrintErr("ERROR || MultiAnimPlayerComponent has no children that implement IAnimPlayerComponent!!");
                return;
            }
        }
        AnimPlayers[0].AnimStarted += (sender, animName) =>
        {
            AnimStarted?.Invoke(this, animName);
        };
        AnimPlayers[0].AnimFinished += (sender, animName) =>
        {
            AnimFinished?.Invoke(this, animName);
        };
    }
	public override void _Process(double delta)
	{
		base._Process(delta);
	}
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
	}
    #endregion
    #region COMPONENT_HELPER
    #endregion
    #region SIGNAL_LISTENERS
    #endregion
    #region INTERFACE_IMPLEMENTATIONS
    public string GetCurrAnimation()
    {
        return AnimPlayers[0].GetCurrAnimation();
    }
    public float GetCurrAnimationPosition()
    {
        return AnimPlayers[0].GetCurrAnimationPosition();
    }
    public void StartAnim(string animName)
    {
        foreach (var animPlayer in AnimPlayers)
        {
            animPlayer.StartAnim(animName);
        }
    }
    public void StopAnim()
    {
        foreach (var animPlayer in AnimPlayers)
        {
            animPlayer.StopAnim();
        }
    }
    public void UpdateAnim(string animName)
    {
        //if (GetCurrAnimation() == animName) { return; }
        if (!AnimPlayers[0].IsPlaying()) { StartAnim(animName); }

        var currAnimPos = GetCurrAnimationPosition();
        StartAnim(animName);
        SeekPos(currAnimPos);
    }
    public bool IsPlaying()
    {
        return AnimPlayers[0].IsPlaying();
    }
    public bool HasAnimation(string animName)
    {
        return AnimPlayers[0].HasAnimation(animName);
    }
    public void SeekPos(float time, bool updateNow = true)
    {
        foreach (var animPlayer in AnimPlayers)
        {
            animPlayer.SeekPos(time, updateNow);
        }
    }
    public void FastForward(float time)
    {
        foreach (var animPlayer in AnimPlayers)
        {
            animPlayer.FastForward(time);
        }
    }
    public float GetCurrAnimationLength()
    {
        return AnimPlayers[0].GetCurrAnimationLength();
    }
    public void PauseAnim()
    {
        foreach (var animPlayer in AnimPlayers)
        {
            animPlayer.PauseAnim();
        }
    }
    public float GetSpeedScale()
    {
        return AnimPlayers[0].GetSpeedScale();
    }
    public void SetSpeedScale(float speedScale)
    {
        foreach (var animPlayer in AnimPlayers)
        {
            animPlayer.SetSpeedScale(speedScale);
        }
    }

    public Node GetInterfaceNode()
    {
        return this;
    }

    public float GetSpriteHeight()
    {
        throw new NotImplementedException();
        //TODO: given sprites, get their combined width/height??
    }

    public float GetSpriteWidth()
    {
        throw new NotImplementedException();
    }

    public Texture2D GetTexture()
    {
        return Sprites[0].GetTexture();
    }

    #endregion
}
