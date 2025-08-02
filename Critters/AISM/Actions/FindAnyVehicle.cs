using Godot;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class FindAnyVehicle : BehaviorAction
{
    #region TASK_VARIABLES
    [Export]
    private bool _needsParked = true;
    [Export]
    private bool _needsDrive = true;
    [Export]
    private bool _caresAboutQueue = true;
    [Export]
    private bool _caresAboutMilitary = true;

    private AINav3DComponent _aiNav;
	private City _residingCity;
    private LocatorComponent3D _vehicleLocator;

	private VehicleOccupantsComponent _vehicleOccupants;
    private VehicleSeat _vehicleSeat;

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
            GD.Print($"Current City {Global.CurrentCity.Name} has no LocatorComponent3D!");
            Status = TaskStatus.FAILURE;
            return;
        }
		_vehicleSeat = _vehicleLocator.FindClosestAvailableVehicleSeat(
            _aiNav.ParentAgent.GlobalPosition,
            _needsParked, 
            _needsDrive,
            _caresAboutQueue,
            _caresAboutMilitary);

        if (_vehicleSeat == null)
        {
			GD.Print($"Couldn't find available vehicle relative to {Agent.Name} @ {_aiNav.ParentAgent.GlobalPosition}");
            Status = TaskStatus.FAILURE;
            return;
        }
        else
        {
            GD.Print($"Found vehicle: {_vehicleSeat.VOccupantComp.Name} relative to {Agent.Name} @ {_aiNav.ParentAgent.GlobalPosition}." +
                $"\nVehicle Position: {_vehicleSeat.VOccupantComp.VehicleGeometry.GlobalPosition}");
        }
        _vehicleOccupants = _vehicleSeat.VOccupantComp;

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
        //if (_needsDrive)
        //{
        //    // LOGIC SHOULD BE DONE IN LOCATOR???
        //    (var driverSeat, var availability) = _vehicleOccupants.GetDriverSeat();
        //    if (availability == SeatAvailability.Occupied)
        //    {
        //        Status = TaskStatus.FAILURE;
        //    }
        //    else if (availability == SeatAvailability.QueuedForEntry &&
        //        _aiNav.NavMethod != NavType.GroundChaos)
        //    {
        //        Status = TaskStatus.FAILURE;
        //    }

        //    //GetDriverEntryPosition()
        //    if (!_aiNav.SetTarget(_vehicleOccupants.GetDriverEntryPosition(), true))
        //    {
        //        GD.Print($"Couldn't set vehicle driver seat entrance target...");
        //        Status = TaskStatus.FAILURE;
        //    }
        //    BB.SetVar(BBDataSig.TargetVehicleSeat, driverSeat);
        //}
        //else
        //{
        //    var targetSeat = _vehicleOccupants.GetClosestAvailableSeat(_aiNav.ParentAgent.GlobalPosition);
        //    if (!_aiNav.SetTarget(_vehicleOccupants.GetSeatEntryPosition(targetSeat), true))
        //    {
        //        GD.Print($"Couldn't set vehicle available seat entrace target");
        //        Status = TaskStatus.FAILURE;
        //    }
        //    BB.SetVar(BBDataSig.TargetVehicleSeat, targetSeat);
        //}

        _vehicleSeat.QueuedForEntry = true;
        GD.Print($"Set vehicle entrance target Sucessfully!!");
        BB.SetVar(BBDataSig.TargetOrOccupiedVehicle, _vehicleOccupants.VehicleComp);
        BB.SetVar(BBDataSig.TargetOrOccupiedVehicleSeat, _vehicleSeat);
        _aiNav.SetTarget(_vehicleOccupants.GetSeatEntryPosition(_vehicleSeat), true);   
        GD.Print("Seat Entry Position: ", _vehicleOccupants.GetSeatEntryPosition(_vehicleSeat));
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

