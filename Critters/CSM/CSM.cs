using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class CSM : CompoundState
{
    #region STATE_VARIABLES
    [Export(PropertyHint.NodeType, "State")]
    private State _onGrabbedState;
    private EatableComponent _eatableComp;
	#endregion
	#region STATE_UPDATES
	public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
		_eatableComp = BB.GetVar<EatableComponent>(BBDataSig.EatableComp);
	}
	public override void Enter(Dictionary<State, bool> parallelStates)
	{
		base.Enter(parallelStates);
		_eatableComp.Grabbed += OnGrabbedByEater;
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
    private void OnGrabbedByEater(object sender, EaterComponent e)
    {
		TransitionFiniteSubState(PrimarySubState, _onGrabbedState);
    }

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

