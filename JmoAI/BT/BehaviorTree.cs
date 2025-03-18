using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[GlobalClass, Tool]
public partial class BehaviorTree : Node
{
    #region TREE_VARIABLES
    public string TreeName { get; protected set; }
    public bool Initialized { get; protected set; } = false;

    [Export]
    private Node _exportedBB;
    [Export]
    public Node AgentNode { get; set; }
    public IBlackboard BB { get; set; }

    private bool _enabled = false;
    [Export]
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value) { return; }
            _enabled = value;
            if (_enabled) { EmitSignal(SignalName.TreeEnabled); }
            else { EmitSignal(SignalName.TreeDisabled); }
        }
    }
    [Export] // Runs without needing init'd from another node
    public bool SelfSuffecient { get; protected set; } = false;
    public BehaviorTask RootTask { get; set; }
    public BehaviorAction RunningLeaf { get; protected set; }
    public float ProcTimeMetric { get; private set; } = 0.0f;

    [Signal]
    public delegate void TreeInitializedEventHandler();
    [Signal]
    public delegate void TreeEnabledEventHandler();
    [Signal]
    public delegate void TreeDisabledEventHandler();
    [Signal]
    public delegate void TreeFinishedLoopEventHandler(TaskStatus treeStatus);
    [Signal]
    public delegate void TreeResetEventHandler();
    #endregion
    #region TREE_UPDATES
    public override void _Ready()
    {
        base._Ready();
        //TreeInitialized += () => { Initialized = true; GD.Print("TREE INITIALIZD"); };

        if (_enabled && SelfSuffecient)
        {
            if (Engine.IsEditorHint())
            {
                TreeName = "Editor's Behavior Tree";
            }
            else if (!AgentNode.IsValid())
            {
                GD.PrintErr($"BehaviorTree ERROR || AgentNode is not valid!");
                return;
            }
            else
            {
                TreeName = AgentNode.Name + "'s Behavior Tree";
            }
            if (_exportedBB is not IBlackboard bb)
            {
                GD.PrintErr($"BehaviorTree ERROR || Exported Blackboard doesn't implement \"IBlackboard\"!");
                return;
            }
            //CallDeferred(MethodName.Init, AgentNode, bb);
            //CallDeferred(MethodName.Enter);

            Init(AgentNode, bb);
            Enter();

        }
    }
    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint()) 
        { 
            return; 
        }
        base._Process(delta);
        if (Enabled)
        {
            ProcessFrame((float)delta);
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint()) { return; }
        base._PhysicsProcess(delta);
        if (Enabled)
        {
            ProcessPhysics((float)delta);
        }
    }
    public virtual void Init(Node agent, IBlackboard bb)
    {
        AgentNode = agent; BB = bb;
        var rt = this.GetFirstChildOfType<BehaviorTask>();
        if (!rt.IsValid())
        {
            Enabled = false;
            GD.PrintErr("BEHAVIOR TREE HAS NO ROOT TASK");
            return;
        }
        RootTask = rt;
        RootTask.Init(AgentNode, BB);
        EmitSignal(SignalName.TreeInitialized);
        Initialized = true;
        //GD.Print("root task of tree: ", RootTask.TaskName);
    }
    public virtual void Enter()
    {
        Enabled = true;
        RootTask.Enter();
        RootTask.TaskStatusChanged += OnRootTaskStatusChanged;

        //GetRunningLeaf();
    }
    public virtual void Exit()
    {
        Enabled = false;
        RootTask.Exit();
        RootTask.TaskStatusChanged -= OnRootTaskStatusChanged;
        //GD.Print($"BTree {Name} Exited.");
    }
    public virtual void ProcessFrame(float delta)
    {
        RootTask.ProcessFrame(delta);
    }
    public virtual void ProcessPhysics(float delta)
    {
        RootTask.ProcessPhysics(delta);
    }

    #endregion
    #region TREE_HELPER
    private void OnRootTaskStatusChanged(TaskStatus newStatus)
    {
        GD.Print($"Tree root node {RootTask.Name} status changed to {newStatus}");
        switch (newStatus)
        {
            case TaskStatus.RUNNING or TaskStatus.FRESH:
                break;
            case TaskStatus.SUCCESS:
                EmitSignal(SignalName.TreeFinishedLoop, Variant.From(TaskStatus.SUCCESS));
                GD.Print("EMITTED TREE FINISHED WITH SUCCESS");
                if (Enabled)
                {
                    RootTask.Exit();
                    RootTask.Enter();
                    EmitSignal(SignalName.TreeReset);
                }
                break;
            case TaskStatus.FAILURE:
                EmitSignal(SignalName.TreeFinishedLoop, Variant.From(TaskStatus.FAILURE));
                GD.Print("EMITTED TREE FINISHED WITH FAILURE");
                if (Enabled)
                {
                    RootTask.Exit();
                    RootTask.Enter();
                    EmitSignal(SignalName.TreeReset);
                }
                break;
        }
    }
    protected virtual void GetRunningLeaf() // TODO: DOESN'T WORK (signal will emit before running leaf has actually switched (probably))
    {
        var currLeaf = RootTask;
        while (currLeaf is not BehaviorAction) //&& currLeaf.Status != TaskStatus.RUNNING) //UNNECESARY
        {
            if (currLeaf is not CompositeTask compT)
            {
                GD.PrintErr("WEIRD BT ERROR HELP!");
                return;
            }
            currLeaf = compT.RunningChild;
        }
        RunningLeaf = currLeaf as BehaviorAction;
        RunningLeaf.TaskStatusChanged += (status) => GetRunningLeaf();
    }
    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (GetChildren().Count > 1)
        {
            warnings.Add("BehaviorTree nodes should only have one child (the root BehaviorTask)");
        }
        if (GetChildren().Any(x => x is not BehaviorTask))
        {
            warnings.Add("Root BehaviorTree should inherit from BehaviorTask class.");
        }
        if (BB is not null && BB is not IBlackboard bb)
        {
            warnings.Add("The exported Blackboard must implement \"IBlackboard\"!");
        }
        return warnings.ToArray();
    }
    #endregion
}
