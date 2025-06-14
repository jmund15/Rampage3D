using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public partial class City : Node3D
{
	#region COMPONENT_VARIABLES
	public LocatorComponent3D? LocatorComp { get; private set; }
    public List<BuildingComponent> Buildings { get; private set; }
    public List<VehicleOccupantsComponent> OccupiableVehicles { get; private set; }

    public List<Area3D> VehicleExitAreas { get; private set; }
	public List<Area3D> CivilianExitAreas { get; private set; }


	public List<Area3D> VehicleParkingAreas { get; private set; }
	public List<Area3D> CivilianHubAreas { get; private set; }
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
	{
		base._Ready();

		LocatorComp = this.GetFirstChildOfType<LocatorComponent3D>(true);

        //GetTree().CallGroup(Global.BUILDING_GROUP_NAME, MethodName."");
        //GetTree().GetNodesInGroup
        //Buildings = this.GetAllChildrenNodesInGroup(Global.BUILDING_GROUP_NAME, true);
        Buildings = this.GetChildrenOfType<BuildingComponent>().ToList();
        OccupiableVehicles = this.GetChildrenOfType<VehicleOccupantsComponent>().ToList();
        if (Engine.IsEditorHint())
		{
			foreach (var building in Buildings)
			{
                //EditorInterface.Singleton.CallDeferred(EditorInterface.MethodName.EditNode, child);
                //EditorInterface.Singleton.EditNode(building);
				//EditorInterface.Singleton.GetSelection().AddNode(building);
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
