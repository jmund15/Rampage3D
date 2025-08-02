using Godot;
using Godot.Collections;
using System;

[GlobalClass, Tool]
public abstract partial class State : Node
{
    #region STATE_VARIABLES
    [Export]
    protected InteruptibleChange SelfInteruptible = InteruptibleChange.NoChange;
    public IBlackboard BB { get; protected set; }
    public Node Agent { get; set; }
    public bool Initialized { get; protected set; } = false;

    protected Dictionary<State, bool> ParallelStates = new Dictionary<State, bool>();

    [Signal]
    public delegate void TransitionStateEventHandler(State oldState, State newState);
    [Signal]
    public delegate void AddParallelStateEventHandler(State parallelState);
    [Signal]
    public delegate void RemoveParallelStateEventHandler(State parallelState);
    public event EventHandler InitializedState;
    #endregion
    #region STATE_UPDATES
    public virtual void Init(Node agent, IBlackboard bb)
    {
        Agent = agent;
        BB = bb;
        Initialized = true;
        InitializedState?.Invoke(this, EventArgs.Empty);
    }
    public virtual void Enter(Dictionary<State, bool> parallelStates)
    {
        // TODO: ADD PRINT TO DEBUG COMP ONLY
        //GD.Print($"{Name} entered by {Agent.Name}");

        ParallelStates = parallelStates;
        switch (SelfInteruptible)
        {
            case InteruptibleChange.NoChange:
                break;
            case InteruptibleChange.True:
                BB.SetPrimVar(BBDataSig.SelfInteruptible, true); break;
            case InteruptibleChange.False:
                BB.SetPrimVar(BBDataSig.SelfInteruptible, false); break;
        }
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
    public virtual void HandleInput(InputEvent @event)
    {
    }
    #endregion
    #region STATE_HELPER
    #endregion
}
