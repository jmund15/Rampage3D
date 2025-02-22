using Godot;
using Godot.Collections;

[Tool]
public partial class AISM : CompoundState
{
	#region STATE_VARIABLES
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

