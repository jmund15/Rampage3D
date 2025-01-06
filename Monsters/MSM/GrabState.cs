using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class GrabState : Base3DState
{
	#region STATE_VARIABLES
	[Export]
	private string _animName = "grab";
	[Export]
	private float _throwEatableTime = 0.12f;

	[Export(PropertyHint.NodeType, "State")]
	private State _transitionState;

	private EaterComponent _eaterComp;
	private EatableComponent _eatableGrabbed;
	#endregion
	#region STATE_UPDATES
	public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
		_eaterComp = BB.GetVar<EaterComponent>(BBDataSig.EaterComp);

    }
	public override void Enter(Dictionary<State, bool> parallelStates)
	{
		base.Enter(parallelStates);
		_eatableGrabbed = _eaterComp.CurrEatable;
		_eatableGrabbed.GrabbedByEater();

        AnimDirection animDir = MoveComp.GetAnimDirection();
        
        AnimPlayer.Play(_animName + IMovementComponent.GetFaceDirectionString(animDir));
        AnimPlayer.AnimationFinished += OnAnimationFinished;

		//TODO: GUARENTEE STATE HASN'T LEFT WHEN THROWING
		GetTree().CreateTimer(_throwEatableTime).Timeout += OnThrowEatable;
    }

    public override void Exit()
	{
		base.Exit();
        AnimPlayer.AnimationFinished -= OnAnimationFinished;
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

    private void OnThrowEatable()
    {
		_eaterComp.ThrowEatable();
		_eaterComp.ThrowPathFinished += OnThrowPathFinished;
    }
    private void OnThrowPathFinished(object sender, EatableComponent e)
    {
        _eaterComp.ThrowPathFinished -= OnThrowPathFinished;
        EmitSignal(SignalName.TransitionState, this, _transitionState);
    }

    private void OnAnimationFinished(StringName animName)
    {
		//EmitSignal(SignalName.TransitionState, this, _transitionState);
    }
    #endregion
}
