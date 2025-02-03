using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public partial class GroundVehicleComponent : VehicleBody3D
{
	#region COMPONENT_VARIABLES
	private CollisionShape3D _collShape;
	private List<VehicleWheel3D> _wheels = new List<VehicleWheel3D>();
	#endregion
	#region COMPONENT_UPDATES
	public override void _Ready()
	{
		base._Ready();
		if (Engine.IsEditorHint()) { return; }
		_wheels = this.GetChildrenOfType<VehicleWheel3D>().ToList();
		foreach (var wheel in _wheels)
		{
			var mesh = wheel.GetFirstChildOfType<MeshInstance3D>();
			mesh.Reparent(this);
		}

        _collShape = this.GetFirstChildOfType<CollisionShape3D>();
		_collShape.MakeConvexFromSiblings();
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
