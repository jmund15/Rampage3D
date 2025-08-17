using Godot;
using Jmo.Core;
//using Godot.Collections;
using System.Collections.Generic;

namespace Jmo.Core.IntentInput
{
    /// <summary>Defines the link between an abstract action and a Godot InputMap string.</summary>
    [GlobalClass]
    public partial class ActionBinding : Resource
    {
        [Export] public InputAction Action { get; private set; }
        /// <summary>
        /// The name of the action as defined in Godot's InputMap.
        /// </summary>
        [Export] public string GodotActionName { get; private set; }
    }
    [GlobalClass]
    public partial class PlayerIntentSource : Node, IIntentSource
    {
        /// <summary>
        /// Controls whether this component is active. When false, _PhysicsProcess will be skipped,
        /// preventing any input polling. This should be set to false during cutscenes, menus, or
        /// any game state where the player should not have control.
        /// </summary>
        [Export] public bool IsActive { get; set; } = true;

        private readonly Dictionary<InputAction, IntentData> _currentIntents = new();

        // ... (Export properties for bindings remain the same) ...
        [ExportGroup("Movement Configuration")]
        [Export] private Godot.Collections.Array<ActionBinding> _booleanActionBindings = new();
        [Export] private InputAction _moveAction;
        [Export] private string _moveLeft = "move_left";
        [Export] private string _moveRight = "move_right";
        [Export] private string _moveUp = "move_up";
        [Export] private string _moveDown = "move_down";

        public override void _PhysicsProcess(double delta)
        {
            // If the component is disabled, we do nothing and ensure our state is clear.
            if (!IsActive)
            {
                if (_currentIntents.Count > 0)
                {
                    _currentIntents.Clear();
                }
                return;
            }

            // We are active, so poll the input state for this frame.
            UpdateIntentState();
        }

        private void UpdateIntentState()
        {
            _currentIntents.Clear();

            foreach (var binding in _booleanActionBindings)
            {
                if (binding?.Action != null && Input.IsActionJustPressed(binding.GodotActionName))
                {
                    _currentIntents[binding.Action] = new IntentData(true);
                }
            }

            if (_moveAction != null)
            {
                var moveVector = Input.GetVector(_moveLeft, _moveRight, _moveUp, _moveDown);
                if (moveVector.LengthSquared() > 0)
                {
                    _currentIntents[_moveAction] = new IntentData(moveVector);
                }
            }
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<InputAction, IntentData> GetIntents()
        {
            return _currentIntents;
        }
    }
}
