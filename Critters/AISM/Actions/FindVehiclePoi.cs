using BaseInterfaces;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class FindVehiclePoi : BehaviorAction
{
	#region TASK_VARIABLES
	private IVehicleComponent3D _occupiedVehicle;
	private VehicleSeat _occupiedSeat;

    private Vector3 _navPoint;
	private bool _mapLoaded;
    #endregion
    #region TASK_UPDATES
    public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
	}
	public override void Enter()
	{
		base.Enter();
        _occupiedVehicle = BB.GetVar<IVehicleComponent3D>(BBDataSig.OccupiedVehicle);
        _occupiedSeat = BB.GetVar<VehicleSeat>(BBDataSig.TargetVehicleSeat);
		if (!_occupiedSeat.IsDriverSeat)
		{
			var occComp = _occupiedSeat.VOccupantComp;
			if (occComp.DriverSeat.Availability == SeatAvailability.Available)
			{
				// fail because npc isn't in drivers seat and drivers seat isn't filled or going to be filled
				Status = TaskStatus.FAILURE;
			}
			else
			{
				_occupiedVehicle.GearChanged += OnVehicleGearChanged;
			}
		}

        if (NavigationServer3D.MapGetIterationId(_occupiedVehicle.GetNavigationMap()) == 0) // not setup yet
        {
            _mapLoaded = false;
            NavigationServer3D.MapChanged += OnMapChanged;
        }
        else
        {
            _mapLoaded = true;
            GetRandomPoint();
        }
    }

    private void OnVehicleGearChanged(object sender, VehicleGear e)
    {
        throw new NotImplementedException();
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
    private void GetRandomPoint()
    {
        do
        {
            _navPoint = NavigationServer3D.MapGetRandomPoint(_occupiedVehicle.GetNavigationMap(), _occupiedVehicle.GetNavigationLayers(), true);
            GD.Print("calc'd nav point: ", _navPoint);
        } while (_navPoint.Y >= 1);
        _occupiedVehicle.SetDriveTargetLocation(_navPoint); //, true);
        //GD.Print("Nav point found: ", _navPoint);
        Status = TaskStatus.SUCCESS;
    }
    private void OnMapChanged(Rid map)
    {
        if (map == _occupiedVehicle.GetNavigationMap())
        {
            GetRandomPoint();
        }
    }
    public override string[] _GetConfigurationWarnings()
	{
		var warnings = new List<string>();

		//

		return warnings.Concat(base._GetConfigurationWarnings()).ToArray();
	}
	#endregion
}

