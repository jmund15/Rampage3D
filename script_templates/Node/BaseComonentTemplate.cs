using _BINDINGS_NAMESPACE_;
using Godot.Collections;

// meta-name: Base Component
// meta-description: Basic Component Template
// meta-default: true
// meta-space-indent: 4
[GlobalClass, Tool]
public partial class _CLASS_ : _BASE_
{
    #region COMPONENT_VARIABLES
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
    {
        base._Ready();
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
}