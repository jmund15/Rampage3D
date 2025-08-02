using BaseInterfaces;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class VehicleToNavPoint : SetRandomNavPoint
{
	#region TASK_VARIABLES
	private IVehicleComponent3D _occupiedVehicle;
	private IVelocity3DComponent _vehVelComp;
	private VehicleOccupantsComponent _vehicleOccComp;
	private VehicleSeat _currentSeat;
	private OccupantComponent3D _occupantComp;

	private float _timeStopped;
	private float _timeToEject;
	private float _baseEjectTime = 2.5f;
    #endregion
    #region TASK_UPDATES
    public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
	}
	public override void Enter()
	{
		base.Enter();
		_occupiedVehicle = BB.GetVar<IVehicleComponent3D>(BBDataSig.TargetOrOccupiedVehicle);
		_vehVelComp = BB.GetVar<IVelocity3DComponent>(BBDataSig.TargetOrOccupiedVehicle);
        _currentSeat = BB.GetVar<VehicleSeat>(BBDataSig.TargetOrOccupiedVehicleSeat);
        _occupantComp = BB.GetVar<OccupantComponent3D>(BBDataSig.OccupantComp);

        if (_occupiedVehicle == null || _currentSeat == null)
		{
			GD.PrintErr($"VehicleToNavPoint: {Agent.Name} couldn't get its occupied vehicle or current seat from BB.");
			Status = TaskStatus.FAILURE;
			return;
        }

        _timeStopped = 0f;
        _timeToEject = Global.GetRndInRange(_baseEjectTime - 0.5f, _baseEjectTime + 0.5f);

		_occupiedVehicle.GearChanged += OnVehicleGearChanged;
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
		if (_vehVelComp.GetVelocity().LengthSquared() < 0.1f)
		{
			_timeStopped += delta;
        }
		else
		{
			_timeStopped = 0f;
		}

		if (_timeStopped > _timeToEject)
		{
			Status = TaskStatus.FAILURE;
			// check for setting vehicle gear to topsided?
		}
	}
    #endregion
    #region TASK_HELPER
    private void OnVehicleGearChanged(object sender, VehicleGear gear)
    {
		if (gear == VehicleGear.Park)
		{
			Status = TaskStatus.SUCCESS;
		}
		// other?
    }
    public override string[] _GetConfigurationWarnings()
	{
		var warnings = new List<string>();

		//

		return warnings.Concat(base._GetConfigurationWarnings()).ToArray();
	}
	#endregion
}

