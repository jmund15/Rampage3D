using Godot;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public partial class WaitIdle : BehaviorAction
{
	#region TASK_VARIABLES
	private AINav3DComponent _aiNavComp;
	#endregion
	#region TASK_UPDATES
	public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
		_aiNavComp = BB.GetVar<AINav3DComponent>(BBDataSig.AINavComp);
    }
	public override void Enter()
	{
		base.Enter();
		//_aiNavComp.SetTarget((Agent as Node3D).GlobalPosition, true);
		_aiNavComp.DisableNavigation();
        GD.Print("arrived at nav point, starting wait idle");
    }
	public override void Exit()
	{
		base.Exit();
        _aiNavComp.EnableNavigation();
		GD.Print("finished wait idle");
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

