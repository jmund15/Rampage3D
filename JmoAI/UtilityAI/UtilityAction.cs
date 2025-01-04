using Godot;
using System.Collections.Generic;
using System.Linq;
using TimeRobbers.JmoAI.UtilityAI;

[GlobalClass, Tool]
public partial class UtilityAction : BehaviorAction, IUtilityTask
{
	#region TASK_VARIABLES
	[Export]
	public UtilityConsideration Consideration { get; set; }
	[Export] // = 0 always interruptible, < 0 never interruptible (USE WITH CAUTION)
    public float NonInterruptTime { get; protected set; } 
	public bool Interruptible { get; set; }
	#endregion
	#region TASK_UPDATES
	public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
		TaskName += "Utility";
	}
	public override void Enter()
	{
		base.Enter();
		switch (NonInterruptTime)
		{
			case < 0f:
				Interruptible = false;
				break;
			case 0f:
				Interruptible = true;
				break;
			case > 0f:
				Interruptible = false;
				GetTree().CreateTimer(NonInterruptTime).Timeout += () =>
				{
					Interruptible = true;
				};
				break;
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
    public override void _Process(double delta)
    {
		//GD.Print("UTILITY ACTION EDITOR");
        base._Process(delta);
   //     if (Engine.IsEditorHint())
   //     {
			//GD.Print("utiltiy act editor");
   //         var cc = Consideration as CurveConsideration;
   //         GD.Print("utiltiy act cc def");

   //         if (cc.IsValid() && cc.CallResetCurve)
			//{
   //             GD.Print("UTILITY ACTION RESETTING CURVE");
   //             cc.ResetCurve();
   //             cc.CallResetCurve = false;
   //         }
   //     }
    }
    public override string[] _GetConfigurationWarnings()
	{
		var warnings = new List<string>();



		return warnings.Concat(base._GetConfigurationWarnings()).ToArray();
	}
	#endregion
}

