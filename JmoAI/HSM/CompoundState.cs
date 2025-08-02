using Godot;
using Godot.Collections;
using System;

[GlobalClass, Tool]
public partial class CompoundState : State
{
    #region STATE_VARIABLES
    [Export]
    public State InitialSubState { get; protected set; }
    public State PrimarySubState { get; protected set; }

    public Dictionary<State, bool> FiniteSubStates { get; protected set; } = new Dictionary<State, bool>();
    public Dictionary<State, bool> ParallelSubStates { get; protected set; } = new Dictionary<State, bool>();

    [Signal]
    public delegate void EnteredCompoundStateEventHandler();
    [Signal]
    public delegate void ExitedCompoundStateEventHandler();
    [Signal]
    public delegate void TransitionedStateEventHandler(State oldState, State newState);
    [Signal]
    public delegate void AddedParallelStateEventHandler(State parallelState);
    [Signal]
    public delegate void RemovedParallelStateEventHandler(State parallelState);
    #endregion
    #region STATE_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        GD.Print("compound state: ", this.Name, " entering init compound state for body: ", agent.Name);
        foreach (var child in GetChildren())
        {
            //GD.Print("parent state: ", Name, ", child state: ", child.Name);
            if (child is not State state) { continue; }
            state.Init(agent, bb);
            state.TransitionState += TransitionFiniteSubState;
            state.AddParallelState += AddParallelSubState;
            state.RemoveParallelState += RemoveParallelSubState;
            if (state is IParallelState stateParallel)
            {
                ParallelSubStates.Add(state, false);
            }
            else
            {
                //GD.Print("finite state: ", state.Name);
                FiniteSubStates.Add(state, false);
            }
        }
        base.Init(agent, bb);
    }
    public override void Enter(Dictionary<State, bool> parallelStates)
    {
        base.Enter(parallelStates);
        if (InitialSubState == null)
        {
            Global.LogError($"No initial sub state set for {Name}!");
        }
        //GD.Print("just entered compound state: ", Name, "\nParallel Sub States: ", ParallelSubStates);
        InitialSubState.Enter(ParallelSubStates);
        FiniteSubStates[InitialSubState] = true;
        PrimarySubState = InitialSubState;
        EmitSignal(SignalName.EnteredCompoundState);
    }
    public override void Exit()
    {
        base.Exit(); 
        PrimarySubState.Exit();
        FiniteSubStates[PrimarySubState] = false;
        foreach (var parallelState in ParallelSubStates)
        {
            if (parallelState.Value)
            {
                parallelState.Key.Exit();
            }
        }
        EmitSignal(SignalName.ExitedCompoundState);
    }
    public override void ProcessFrame(float delta)
    {
        base.ProcessFrame(delta);
        PrimarySubState.ProcessFrame(delta);
        foreach (var parallelState in ParallelSubStates)
        {
            if (parallelState.Value)
            {
                parallelState.Key.ProcessFrame(delta);
            }
        }
    }
    public override void ProcessPhysics(float delta)
    {
        base.ProcessPhysics(delta);
        PrimarySubState.ProcessPhysics(delta);
        foreach (var parallelState in ParallelSubStates)
        {
            if (parallelState.Value)
            {
                parallelState.Key.ProcessPhysics(delta);
            }
        }
    }
    public override void HandleInput(InputEvent @event)
    {
        base.HandleInput(@event);
        PrimarySubState.HandleInput(@event);
        foreach (var parallelState in ParallelSubStates)
        {
            if (parallelState.Value)
            {
                parallelState.Key.HandleInput(@event);
            }
        }
    }
    #endregion
    #region STATE_HELPER
    public virtual void TransitionFiniteSubState(State oldSubState, State newSubState)
    {
        if (newSubState == null)
        {
            throw new Exception($"COMPOUND STATE ERROR || NEW STATE TRANSITIONED FROM \"{oldSubState.Name}\" HAS NOT BEEN SET IN THE EDITOR AND IS NULL");
        }
        //if (ParallelSubStates[oldSubState])
        //{
        //    oldSubState.Exit();
        //    ParallelSubStates[oldSubState] = false;
        //    newSubState.Enter(ParallelStates);
        //    ParallelSubStates[newSubState] = true;
        //}
        if (PrimarySubState != null)
        {
            if (PrimarySubState != oldSubState) { return; }
            // STILL EXIT AND ENTER FOR BTSTATE PURPOSES (tree exiting and re-entering)
            //if (PrimarySubState == newSubState) { return; } 
            PrimarySubState.Exit();
            FiniteSubStates[PrimarySubState] = false;
        }
        PrimarySubState = newSubState;
        PrimarySubState.Enter(ParallelSubStates);
        FiniteSubStates[PrimarySubState] = true;
        //GD.Print("transitioning from ", oldSubState.Name, " to ", newSubState.Name);
        EmitSignal(SignalName.TransitionedState, oldSubState, newSubState);
    }
    public virtual void AddParallelSubState(State state)
    {
        if (state is not IParallelState parallelState)
        {
            GD.PrintErr("ERROR || Added Parallel State is NOT Parallel!");
            return;
        }
        if (ParallelSubStates[state])
        {
            GD.PrintErr("WARNING || Can't add parallel state that is already active!");
            return;
        }
        state.Enter(ParallelSubStates);
        ParallelSubStates[state] = true;
        EmitSignal(SignalName.AddedParallelState, state);
    }
    public virtual void RemoveParallelSubState(State state)
    {
        if (state is not IParallelState parallelState)
        {
            GD.PrintErr("ERROR || Added Parallel State is NOT Parallel!");
            return;
        }
        if (!ParallelSubStates[state])
        {
            GD.PrintErr("WARNING || Can't exit parallel state that is already disactive!");
            return;
        }
        state.Exit();
        ParallelSubStates[state] = false;
        EmitSignal(SignalName.RemovedParallelState, state);
    }
    #endregion
}
