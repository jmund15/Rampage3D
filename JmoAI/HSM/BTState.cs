using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public partial class BTState : State
{
	#region STATE_VARIABLES
	protected BehaviorTree Tree;

    [Export(PropertyHint.NodeType, "State")]
    protected State OnTreeSuccessState;
    [Export(PropertyHint.NodeType, "State")]
    protected State OnTreeFailureState;
    #endregion
    #region STATE_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        base.Init(agent, bb);
		var bt = this.GetFirstChildOfType<BehaviorTree>();
        if (!bt.IsValid())
		{
            GD.PrintErr("BTSTATE HAS NO ROOT BEHAVIORTREE");
            //throw new System.Exception(); //TODO: More verbose
			return;
        }
        Tree = bt;
        Tree.Init(agent, bb);

		Tree.TreeFinishedLoop += OnTreeFinishLoop;
    }
    public override void Enter(Godot.Collections.Dictionary<State, bool> parallelStates)
	{
		base.Enter(parallelStates);
		Tree.Enter();
	}
	public override void Exit()
	{
		base.Exit();
		Tree.Exit();
	}
	public override void ProcessFrame(float delta)
	{
		base.ProcessFrame(delta);
	}
	public override void ProcessPhysics(float delta)
	{
		base.ProcessPhysics(delta);
	}
	public override void HandleInput(InputEvent @event)
	{
		base.HandleInput(@event);
	}
	#endregion
	#region STATE_HELPER
	protected virtual void OnTreeFinishLoop(TaskStatus treeStatus)
	{
		GD.Print("TREE FINISHED LOOP!");
		switch (treeStatus)
		{
			case TaskStatus.FAILURE:
				GD.Print("BTState BehaviorTree Finished on status FAILURE");
                EmitSignal(SignalName.TransitionState, this, OnTreeFailureState); break;
			case TaskStatus.SUCCESS:
                EmitSignal(SignalName.TransitionState, this, OnTreeSuccessState); break;
			case TaskStatus.RUNNING or TaskStatus.FRESH:
				Global.LogError("HOW DID TREE FINISH LOOP ON NON SUCCeSS OR FAILURE STATUS?"); break;
        }
	}
    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

  //      if (GetChildren().Any(x => x is not BehaviorTree))
  //      {
  //          warnings.Add("The BTState class should only contain a singular child of BehaviorTree class.");
  //      }
		//else
		if (!this.GetFirstChildOfType<BehaviorTree>(false).IsValid())
		{
            warnings.Add("The BTState must contain a child of BehaviorTree class!");
        }

        return warnings.ToArray();
    }
    #endregion
}
