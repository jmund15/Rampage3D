using Godot;
using Godot.Collections;

[GlobalClass, Tool]
public partial class NavigationRegionManager : NavigationRegion3D
{
	#region COMPONENT_VARIABLES
	[Export]
	private Node _areaScene;
	#endregion
	#region COMPONENT_UPDATES
	public override void _Ready()
	{
		base._Ready();
		if (_areaScene == null) { return; }

		var obstacles = _areaScene.GetAllChildrenNodesInGroup(Global.NAV_OBSTACLE_GROUP_NAME);
		foreach (var obstacle in obstacles) 
		{ 

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
