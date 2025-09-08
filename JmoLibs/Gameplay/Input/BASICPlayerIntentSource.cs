// PlayerIntentSource.cs
using Godot;
using Jmo.Core;
using Jmo.Core.Input;
using System.Collections.Generic;

namespace Clonepage.JmoLibs.Gameplay.Input;

/// <summary>
/// An adapter that implements the IIntentSource interface by reading
/// from Godot's Input singleton.
/// </summary>
public class BASICPlayerIntentSource : IIntentSource
{
    private readonly Dictionary<InputAction, IntentData> _intents = new();

    public IReadOnlyDictionary<InputAction, IntentData> GetIntents()
    {
        _intents.Clear();

        // TODO: UPDATE THIS LATER WITH CORRECT INPUTS AND UTILIZING GAME REGISTRY

        // --- Movement Intent ---
        Vector2 moveInput = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        if (!moveInput.IsZeroApprox())
        {
            _intents[Registry.DB.MoveAction] = new IntentData(moveInput);
        }

        // --- Jump Intent ---
        if (Input.IsActionJustPressed("ui_accept")) // Replace with your jump action
        {
            _intents[Registry.DB.JumpAction] = new IntentData(true);
        }

        // ... add other intents for attack, defend, etc.

        return _intents;
    }
}