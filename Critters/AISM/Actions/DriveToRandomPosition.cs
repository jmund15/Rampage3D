using BaseInterfaces;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class DriveToRandomPosition : BehaviorAction
{
	#region TASK_VARIABLES
	private IVehicleComponent3D _vehicleComp;
	private IDriver _driver;
	
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
		_vehicleComp = BB.GetVar<IVehicleComponent3D>(BBDataSig.TargetOrOccupiedVehicle);
        if (NavigationServer3D.MapGetIterationId(_vehicleComp.GetNavigationMap()) == 0) // not setup yet
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
        if (map == _vehicleComp.GetNavigationMap())
        {
            GetRandomPoint();
        }
    }
    private void GetRandomPoint()
    {
        do
        {
            _navPoint = NavigationServer3D.MapGetRandomPoint(_vehicleComp.GetNavigationMap(), _vehicleComp.GetNavigationLayers(), true);
            GD.Print("calc'd nav point: ", _navPoint);
        } while (_navPoint.Y >= 1);
        _vehicleComp.SetDriveTargetLocation(_navPoint); //, true);
        //GD.Print("Nav point found: ", _navPoint);
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

