using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

[GlobalClass, Tool]
public partial class DebugSMComponent : Node2D
{
    #region CLASS_VARIABLES
    [Export]
    private CompoundState _stateMachine;
    [Export]
    private float _labelTimeout = 5.0f;
    [Export]
    private bool _labelMoveUp = true;
    [Export]
    private OrthogDirection _displayDirection;

    private List<Label> _stateLabels = new List<Label>();
    private string _currStateName;
    private Label _currStateLabel;// = new Label();
    private float _currStateTime = 0f;

    private bool _active = false;
    #endregion

    #region BASE_GODOT_OVERRIDEN_FUNCTIONS
    public override void _Ready()
    {
        _stateMachine.EnteredCompoundState += OnEnteredCompoundState;
        _stateMachine.ExitedCompoundState += OnExitedCompoundState;
        _stateMachine.TransitionedState += OnTransitionedState;

        switch (_displayDirection)
        {
            case OrthogDirection.UpLeft:
                Position = new Vector2(75, 0); break;
            case OrthogDirection.UpRight:
                Position = new Vector2(-150, 0); break;
            case OrthogDirection.DownLeft:
                Position = new Vector2(0, -100); break;
            case OrthogDirection.DownRight:
                Position = new Vector2(0, 100); break;
        }
    }
    public override void _Process(double delta)
    {
        if (_active)
        {
            _currStateTime += (float)delta;
            SetCurrentStateLabelText();
        }
    }
    #endregion

    #region COMPONENT_FUNCTIONS
    protected virtual void SetCurrentStateLabelText()
    {
        _currStateLabel.Text = _currStateName + ": " + _currStateTime.ToString("n2");
    }
    protected virtual void CreateCurrentStateLabel(State currState)
    {
        _currStateLabel = new Label();
        _currStateLabel.Scale = new Vector2(1.5f, 1.5f);
        AddChild(_currStateLabel);
        _currStateName = currState.Name;
        //_currStateLabel.Position = ?;
        _stateLabels.Add(_currStateLabel);
    }
    protected virtual void FadeStateLabel(Label oldLabel)
    {
        var labelFadeTween = CreateTween();
        labelFadeTween.TweenProperty(oldLabel, "modulate:a", 0.0f, _labelTimeout).SetEase(Tween.EaseType.In);
        labelFadeTween.TweenCallback(Callable.From(() => _stateLabels.Remove(oldLabel)));
        labelFadeTween.TweenCallback(Callable.From(oldLabel.QueueFree));
    }
    #endregion

    #region SIGNAL_LISTENERS
    protected virtual void OnEnteredCompoundState()
    {
        _active = true;
        CreateCurrentStateLabel(_stateMachine.PrimarySubState);
    }
    protected virtual void OnExitedCompoundState()
    {
        _active = false;
        FadeStateLabel(_currStateLabel);
    }
    protected virtual void OnTransitionedState(State oldState, State newState)
    {
        _currStateTime = 0f;
        foreach (var pastLabel in _stateLabels)
        {
            if (_labelMoveUp)
            {
                pastLabel.Position += new Vector2(0, -30);
            }
            else
            {
                pastLabel.Position += new Vector2(0, 30);
            }
        }
        FadeStateLabel(_currStateLabel);
        CreateCurrentStateLabel(newState);
        SetCurrentStateLabelText();
    }
    #endregion

    #region HELPER_CLASSES
    #endregion
}
