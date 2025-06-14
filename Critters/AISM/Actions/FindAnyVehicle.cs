using Godot;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class FindAnyVehicle : BehaviorAction
{
    #region TASK_VARIABLES
    [Export]
    private bool _needsDrive = true;

	private AINav3DComponent _aiNav;
	private City _residingCity;
    private LocatorComponent3D _vehicleLocator;

	private VehicleOccupantsComponent _vehicleOccupants;

    private bool _mapLoaded;

    #endregion
    #region TASK_UPDATES
    public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
		_aiNav = BB.GetVar<AINav3DComponent>(BBDataSig.AINavComp);
        _residingCity = NodeExts.GetFirstNodeOfTypeInScene<City>();
		_vehicleLocator = _residingCity.LocatorComp;
    }
	public override void Enter()
	{
		base.Enter();
        if (_vehicleLocator == null)
        {
            Status = TaskStatus.FAILURE;
            return;
        }
		var vehicle = _vehicleLocator.FindClosestAvailableVehicle(_aiNav.ParentAgent.GlobalPosition, _needsDrive);
        if (vehicle == null)
        {
			GD.Print($"Couldn't find available vehicle relative to {Agent.Name} @ {_aiNav.ParentAgent.GlobalPosition}");
            Status = TaskStatus.FAILURE;
            return;
        }
        else
        {
            GD.Print($"Found vehicle: {vehicle.Name} relative to {Agent.Name} @ {_aiNav.ParentAgent.GlobalPosition}." +
                $"\nVehicle Position: {vehicle.VehicleGeometry.GlobalPosition}");
        }
        _vehicleOccupants = vehicle;

        if (NavigationServer3D.MapGetIterationId(_aiNav.GetNavigationMap()) == 0) // not setup yet
        {
            _mapLoaded = false;
            NavigationServer3D.MapChanged += OnMapChanged;
        }
        else
        {
            _mapLoaded = true;
            CallDeferred(MethodName.SetVehicleEntranceTarget);
        }
    }
	public override void Exit()
	{
		base.Exit();
	}
	public override void ProcessFrame(float delta)
	{
		base.ProcessFrame(delta);
	}
	public override void ProcessPhysics(float delta)
	{
		base.ProcessPhysics(delta);
	}
    #endregion
    #region TASK_HELPER
    private void OnMapChanged(Rid map)
    {
        if (map == _aiNav.GetNavigationMap())
        {
            SetVehicleEntranceTarget();
        }
    }
    private void SetVehicleEntranceTarget()
    {
        if (_needsDrive)
        {
            if (!_aiNav.SetTarget(_vehicleOccupants.GetDriverEntryPosition(), true))
            {
                GD.Print($"Couldn't set vehicle driver seat entrance target...");
                Status = TaskStatus.FAILURE;
            }
        }
        else
        {
            if (!_aiNav.SetTarget(_vehicleOccupants.GetNextAvailableSeatEntryPosition(), true))
            {
                GD.Print($"Couldn't set vehicle available seat entrace target");
                Status = TaskStatus.FAILURE;
            }
        }

        GD.Print($"Set vehicle entrance target Sucessfully!!");
        BB.SetVar(BBDataSig.TargetVehicle, _vehicleOccupants);
        Status = TaskStatus.SUCCESS;
    }
    public override string[] _GetConfigurationWarnings()
	{
		var warnings = new List<string>();

		//

		return warnings.Concat(base._GetConfigurationWarnings()).ToArray();
	}
	#endregion
}

