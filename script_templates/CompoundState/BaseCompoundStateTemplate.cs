using _BINDINGS_NAMESPACE_;
using Godot.Collections;

// meta-name: Base Compound State
// meta-description: Basic Compound State Template
// meta-default: true
// meta-space-indent: 4
public partial class _CLASS_ : _BASE_
{
    //TEMPLATE FOR COMPOUND STATES
    #region STATE_VARIABLES
    [Export(PropertyHint.NodeType, "State")]
    private State _transitionState;
    #endregion
    #region STATE_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        base.Init(agent, bb);
    }
    public override void Enter(Dictionary<State, bool> parallelStates)
    {
        base.Enter(parallelStates);
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
    public override void HandleInput(InputEvent @event)
    {
        base.HandleInput(@event);
    }
    #endregion
    #region STATE_HELPER
    public override void TransitionFiniteSubState(State oldSubState, State newSubState)
    {
        base.TransitionFiniteSubState(oldSubState, newSubState);
    }
    public override void AddParallelSubState(State state)
    {
        base.AddParallelSubState(state);
    }
    public override void RemoveParallelSubState(State state)
    {
        base.RemoveParallelSubState(state);
    }
    #endregion
}
