using Godot;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class FindOwnedVehicle : BehaviorAction
{
	#region TASK_VARIABLES
	private Node3D _ownedVehicle;
	#endregion
	#region TASK_UPDATES
	public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
	}
	public override void Enter()
	{
		base.Enter();
        _ownedVehicle = BB.GetVar<Node3D>(BBDataSig.OwnedVehicle);
		if (_ownedVehicle == null)
		{
			Status = TaskStatus.FAILURE;
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

