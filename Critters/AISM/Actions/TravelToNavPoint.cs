using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public partial class TravelToNavPoint : BehaviorAction
{
    #region TASK_VARIABLES
    protected AINav3DComponent AINavComp;

    [Export]
    protected bool DisableNavOnTargetReached = false;
    #endregion
    #region TASK_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        base.Init(agent, bb);
        AINavComp = BB.GetVar<AINav3DComponent>(BBDataSig.AINavComp);
    }
    public override void Enter()
	{
		base.Enter();
		AINavComp.TargetReached += OnTargetReached; // or NavigationFinished?
        //GD.Print("init nav path: ");
        //foreach (var p in _aiNavComp.GetCurrentNavigationPath())
        //{
        //    GD.Print("\t", p);
        //}
        //GD.Print("init curr pos: ", _aiNavComp.ParentAgent.GlobalPosition);
        //GD.Print("init target pos: ", _aiNavComp.TargetPosition);
        //GD.Print("init curr dir: ", _aiNavComp.WeightedNextPathDirection);

    }
    public override void Exit()
	{
		base.Exit();
        AINavComp.TargetReached -= OnTargetReached;
	}
    public override void ProcessFrame(float delta)
	{
		base.ProcessFrame(delta);
	}
	public override void ProcessPhysics(float delta)
	{
		base.ProcessPhysics(delta);
        //GD.Print("dist to nav point: ", _aiNavComp.DistanceToTarget());
        //GD.Print("targ pos: ", _aiNavComp.TargetPosition);
        //GD.Print("agent pos: ", _aiNavComp.ParentAgent.GlobalPosition);
	}
    #endregion
    #region TASK_HELPER
    private void OnTargetReached()
    {
        if (DisableNavOnTargetReached)
        {
            AINavComp.DisableNavigation();
        }
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

