using Godot;
using Godot.Collections;

[GlobalClass, Tool]
public partial class VehicleRiderComponent : Node
{
	#region COMPONENT_VARIABLES

	[Export]
	public bool CanDrive { get; private set; } = true;

	[Export]
	public Node OwnedVehicle { get; private set; }
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
