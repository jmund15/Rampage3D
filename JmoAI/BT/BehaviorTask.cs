using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Godot.WebSocketPeer;

public enum TaskStatus
{
    FRESH,
    RUNNING,
    FAILURE,
    SUCCESS
};
[GlobalClass, Tool]
public partial class BehaviorTask : Node
{
    #region TASK_VARIABLES
    public string TaskName { get; protected set; }
    public Node Agent { get; private set; }
    public IBlackboard BB { get; private set; }
    private TaskStatus _status;
    public TaskStatus Status
    {
        get => _status;
        set
        {
            if (_status == value) { return; }
            //Global.LogError($"STATUS CHANGED ON: {Name}");
            _status = value; 
            EmitSignal(SignalName.TaskStatusChanged, Variant.From(_status));
        }
    }
    [Export]
    public Godot.Collections.Array<BTCondition> Conditions { get; private set; } = new Godot.Collections.Array<BTCondition>();
    [Signal]
    public delegate void TaskStatusChangedEventHandler(TaskStatus newStatus);
    #endregion
    #region TASK_UPDATES
    public virtual void Init(Node agent, IBlackboard bb)
    {
        Agent = agent;
        BB = bb;
        Status = TaskStatus.FRESH;
        TaskName += Name;
        foreach (var condition in Conditions)
        {
            condition.Init(agent, bb);
            TaskName += condition.ConditionName;
        }
        
    }
    public virtual void Enter()
    {
        //Status = TaskStatus.FRESH;
        Status = TaskStatus.RUNNING;
        //TODO: make sure is ok? currently deferring to allow proper entering and exiting of tasks.
        //But if conditions fail, do you really want them to even enter?
        //Solution could be for enter to return a enum (Enter_success, enter_failure, enter_running)?
        CallDeferred(MethodName.EnterConditions);
        //GD.Print($"Task {TaskName} entered");
    }
    public virtual void Exit()
    {
        //GD.Print($"Task {TaskName} exited with status {Status}");
        CallDeferred(MethodName.ExitConditions);
    }
    public virtual void ProcessFrame(float delta)
    {
        // Status = TaskStatus.RUNNING;
        //GD.Print("Processing frame for task: ", TaskName);
        foreach (var condition in Conditions)
        {
            //GD.Print("RUNNING CONDITION FOR: ", condition.ConditionName);
            condition.ProcessFrame(delta);
        }
    }
    public virtual void ProcessPhysics(float delta)
    {
        // Status = TaskStatus.RUNNING;
        foreach (var condition in Conditions)
        {
            condition.ProcessPhysics(delta);
        }
    }
    #endregion
    #region TASK_HELPER
    private void EnterConditions()
    {
        foreach (var condition in Conditions)
        {
            condition.ExitTaskEvent += OnConditionExit;
            condition.Enter();
            GD.Print("entered condition: ", condition.ResourceName);
        }
    }
    private void ExitConditions()
    {
        foreach (var condition in Conditions)
        {
            condition.ExitTaskEvent -= OnConditionExit;
            condition.Exit();
        }
    }
    private void OnConditionExit(object sender, bool succeedTask)
    {
        //if (!Conditions.Contains(sender)) { return; } //safeguard, may be slow and unecessary tho
        
        if (succeedTask)
        {
            Status = TaskStatus.SUCCESS;
        }
        else
        {
            Status = TaskStatus.FAILURE;
        }
        GD.Print("EXITED TASK DUE TO CONDITION");
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        //if (GetChildren().Any(x => x is not BehaviorTask)) {
        //    warnings.Add("All children of this node should inherit from BehaviorTask class.");
        //}

        return warnings.ToArray();
    }
    #endregion
}
