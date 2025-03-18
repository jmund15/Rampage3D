using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

//public enum ConditionStatus
//{
//	SuceedTask,
//	RunTask,
//	FailTask
//}

[GlobalClass, Tool]
public abstract partial class BTCondition : Resource
{
	#region TASK_VARIABLES
	public string ConditionName { get; protected set; }

	protected Node Agent;
	protected IBlackboard BB;

	[Export]
	protected bool SucceedTaskOnExit = false; // naturally all tasks will fail on condition exit

	//private TaskStatus _status = TaskStatus.RUNNING; // all conditions are successfull until proved failure
	//public TaskStatus Status {
 //       get => _status;
 //       set
 //       {
 //           if (_status == value) { return; }
 //           _status = value;
	//		ConditionStatusChangedEvent?.Invoke(this, _status);
 //           //EmitSignal(SignalName.ConditionStatusChanged, Variant.From(_status));
 //       }
 //   }

	//public event EventHandler<TaskStatus> ConditionStatusChangedEvent;

	public event EventHandler<bool> ExitTaskEvent;
	#endregion
	#region TASK_UPDATES
	public virtual void Init(Node agent, IBlackboard bb)
	{
		Agent = agent;
        BB = bb;
    }
    public virtual void Enter()
	{
		//GD.Print($"Entered Decorator '{ConditionName}'.");
	}
	public virtual void Exit()
	{
    }
	public virtual void ProcessFrame(float delta)
	{
	}
	public virtual void ProcessPhysics(float delta)
	{
	}
    #endregion
    #region TASK_HELPER
    protected virtual void OnExitTask()
	{
		if (SucceedTaskOnExit)
		{
            ExitTaskEvent?.Invoke(this, true);
            return;
        }

        ExitTaskEvent?.Invoke(this, false);
	}
    #endregion
}

