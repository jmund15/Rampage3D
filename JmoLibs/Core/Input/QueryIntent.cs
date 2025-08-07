using Godot;
using System.Collections.Generic;
using Jmo.Core.Input;
using System;
using System.Runtime.Intrinsics.X86;
[GlobalClass]
public partial class PlayerIntentSource : Node, IIntentSource
{
    // A designer can now create any number of mappings directly in the Godot Inspector.
    // They drag an InputAction.tres resource into the 'Key' and type the corresponding
    // Godot InputMap action string into the 'Value'.
    [Export]
    private Dictionary<InputAction, string> _actionMap = new();

    // Special case for analog movement, as it's not a simple action.
    [Export] private InputAction _moveAction;
    [Export] private string _moveLeftAction = "move_left";
    [Export] private string _moveRightAction = "move_right";
    [Export] private string _moveForwardAction = "move_forward";
    [Export] private string _moveBackAction = "move_back";

    public void QueryIntent(Dictionary<InputAction, object> intentCollection)
    {
        // Handle all simple boolean actions (buttons/keys)
        foreach (var mapping in _actionMap)
        {
            // Check for continuous presses
            if (Input.IsActionPressed(mapping.Value))
            {
                intentCollection[mapping.Key] = true;
            }
            // You can expand this to check for IsActionJustPressed/Released
            // and store more complex data if needed.
        }

        // Handle the special case of analog movement vector
        if (_moveAction != null)
        {
            Vector2 moveVector = Input.GetVector(_moveLeftAction, _moveRightAction, _moveForwardAction, _moveBackAction);
            intentCollection[_moveAction] = moveVector;
        }
    }
}