using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class WallAttackState : AttackState
{
	#region STATE_VARIABLES
	[Export(PropertyHint.NodeType, "State")]
	private State _onClimbableEjectState;

	private ClimbableComponent _climbableComp;
	#endregion
	#region STATE_UPDATES
	public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
	}
	public override void Enter(Dictionary<State, bool> parallelStates)
	{
		base.Enter(parallelStates);
		_climbableComp = BB.GetVar<ClimbableComponent>(BBDataSig.CurrClimbComp);
		_climbableComp.EjectClimbers += OnEjectClimbers;
    }
    public override void Exit()
	{
		base.Exit();
        _climbableComp.EjectClimbers -= OnEjectClimbers;
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
    private void OnEjectClimbers(object sender, EventArgs e)
    {
		GD.Print("EJECTING CLIMBER");
		EmitSignal(SignalName.TransitionState, this, _onClimbableEjectState);
    }
    #endregion
}
