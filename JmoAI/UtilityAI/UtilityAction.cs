// --- UtilityAction.cs ---
// Your base action for all utility-based behaviors. It now implements the Priority property.
using Godot;
using JmoAI.UtilityAI;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public partial class UtilityAction : BehaviorAction, IUtilityTask
{
    #region TASK_VARIABLES
    [Export]
    public UtilityConsideration Consideration { get; set; }

    [Export(PropertyHint.Range, "-1,100,1")] // -1 means never interruptible
    public float NonInterruptibleTime { get; protected set; } = 0.25f; // Small delay by default

    [Export]
    public int Priority { get; private set; } = 0;
    
    public bool Interruptible { get; private set; } = true;

	public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
		TaskName += "_Utility";
	}
    public override void Enter()
    {
        base.Enter();
        if (NonInterruptibleTime < 0)
        {
            Interruptible = false;
        }
        else if (NonInterruptibleTime > 0)
        {
            Interruptible = false;
            GetTree().CreateTimer(NonInterruptibleTime).Timeout += () => Interruptible = true;
        }
        else
        {
            Interruptible = true;
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