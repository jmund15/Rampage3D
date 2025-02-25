using Godot;
using Godot.Collections;

[GlobalClass, Tool]
public partial class Area : Node3D
{
	#region COMPONENT_VARIABLES
	#endregion
	#region COMPONENT_UPDATES
	public override void _Ready()
	{
		base._Ready();
		if (Engine.IsEditorHint())
		{
			foreach (var building in this.GetChildrenOfType<BuildingComponent>())
			{
				//EditorInterface.Singleton.CallDeferred(EditorInterface.MethodName.EditNode, child);
				//EditorInterface.Singleton.EditNode(building);
				EditorInterface.Singleton.GetSelection().AddNode(building);
				//GD.Print("editing building: ", building.Name);
			}
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
	#endregion
	#region COMPONENT_HELPER
	#endregion
	#region SIGNAL_LISTENERS
	#endregion
}
