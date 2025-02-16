using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class GrabbedState : Base3DState
{
	#region STATE_VARIABLES
	[Export(PropertyHint.NodeType, "State")]
	private State _onEatenState;

	private EatableComponent _eatableComp;
	private EaterComponent _eater;
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
		_eater = _eatableComp.Eater;

		//var faceDir = IMovementComponent.GetOppositeDirection(_eater.)
		AnimPlayer.StartAnim("falling" + MoveComp.GetAnimDirection());

		_eatableComp.InMouth += OnInMouth;
    }

    public override void Exit()
	{
		base.Exit();
        _eatableComp.InMouth -= OnInMouth;
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

    private void OnInMouth(object sender, EaterComponent e)
    {
		EmitSignal(SignalName.TransitionState, this, _onEatenState);
    }
    #endregion
}
