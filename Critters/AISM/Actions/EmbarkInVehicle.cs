using Godot;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class EmbarkInVehicle : BehaviorAction
{
	#region TASK_VARIABLES

	#endregion
	#region TASK_UPDATES
	public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
	}
	public override void Enter()
	{
		base.Enter();
		var rider = BB.GetVar<OccupantComponent3D>(BBDataSig.OccupantComp);//(Agent as Node3D);
		var targetSeat = BB.GetVar<VehicleSeat>(BBDataSig.TargetOrOccupiedVehicleSeat);
		var targetVehicleOccComp = targetSeat.VOccupantComp;
        if (!targetVehicleOccComp.CloseEnoughToEmbark(rider, targetSeat))
		{
			Status = TaskStatus.FAILURE;
        }

		var embarked = rider.EmbarkInVehicle(targetVehicleOccComp.VehicleComp, targetSeat);
		if (!embarked)
		{
			GD.PrintErr($"EmbarkInVehicle: {Agent.Name} failed to embark on vehicle {targetVehicleOccComp.Name} at seat {targetSeat.EntrancePosition}.");
            Status = TaskStatus.FAILURE;
		}
		else
		{
			BB.SetVar(BBDataSig.TargetOrOccupiedVehicle, targetVehicleOccComp.VehicleComp);
            Status = TaskStatus.SUCCESS;
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

