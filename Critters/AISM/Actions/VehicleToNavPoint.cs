using BaseInterfaces;
using Godot;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class VehicleToNavPoint : SetRandomNavPoint
{
	#region TASK_VARIABLES
	private IVehicleComponent3D _occupiedVehicle;
	private VehicleOccupantsComponent _vehicleOccComp;
	private VehicleSeat _currentSeat;
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
        _currentSeat = BB.GetVar<VehicleSeat>(BBDataSig.TargetVehicleSeat);

		if (_occupiedVehicle == null || _currentSeat == null)
		{
			GD.PrintErr($"VehicleToNavPoint: {Agent.Name} couldn't get its occupied vehicle or current seat from BB.");
			Status = TaskStatus.FAILURE;
			return;
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
	public override string[] _GetConfigurationWarnings()
	{
		var warnings = new List<string>();

		//

		return warnings.Concat(base._GetConfigurationWarnings()).ToArray();
	}
	#endregion
}

