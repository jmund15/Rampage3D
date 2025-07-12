using BaseInterfaces;
using Godot;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class DriveToRandomPosition : BehaviorAction
{
	#region TASK_VARIABLES
	private IVehicleComponent3D _vehicleComp;
	private IDriver _driver;
	

    #endregion
    #region TASK_UPDATES
    public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
	}
	public override void Enter()
	{
		base.Enter();
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

