using Godot;
using Jmo.Core.Input;
using System.Collections.Generic;

namespace Jmo.Gameplay.Input;

[GlobalClass]
public partial class PlayerIntentSource : Node, IIntentSource
{
    [Export] public bool IsActive { get; set; } = true;

    // This is now a unified, data-driven system. No more special-casing movement.
    [ExportGroup("Action Bindings")]
    [Export] private Godot.Collections.Array<ActionBinding> _actionBindings = new();
    [Export] private Godot.Collections.Array<VectorActionBinding> _vectorBindings = new();

    private readonly Dictionary<InputAction, IntentData> _currentIntents = new();

    /// <summary>
    /// Input polling now happens in _Process to ensure no inputs are missed.
    /// </summary>
    public override void _Process(double delta)
    {
        if (!IsActive)
        {
            if (_currentIntents.Count > 0) _currentIntents.Clear();
            return;
        }

        UpdateIntentState();
    }

    private void UpdateIntentState()
    {
        _currentIntents.Clear();

        // Process all boolean/pressed/released actions
        foreach (var binding in _actionBindings)
        {
            if (binding?.Action == null || string.IsNullOrEmpty(binding.GodotActionName)) continue;

            bool result = false;
            switch (binding.PollType)
            {
                case InputActionPollType.JustPressed:
                    result = Godot.Input.IsActionJustPressed(binding.GodotActionName);
                    break;
                case InputActionPollType.Pressed:
                    result = Godot.Input.IsActionPressed(binding.GodotActionName);
                    break;
                case InputActionPollType.JustReleased:
                    result = Godot.Input.IsActionJustReleased(binding.GodotActionName);
                    break;
            }

            if (result)
            {
                _currentIntents[binding.Action] = new IntentData(true);
            }
        }

        // Process all vector actions
        foreach (var binding in _vectorBindings)
        {
            if (binding?.Action == null) continue;

            var moveVector = Godot.Input.GetVector(binding.Left, binding.Right, binding.Up, binding.Down);
            if (moveVector.LengthSquared() > 0)
            {
                _currentIntents[binding.Action] = new IntentData(moveVector);
            }
        }
    }

    /// <summary>
    /// Returns a snapshot of the intents captured during the last _Process frame.
    /// This method is safe to call from _PhysicsProcess.
    /// </summary>
    public IReadOnlyDictionary<InputAction, IntentData> GetIntents()
    {
        return _currentIntents;
    }
}