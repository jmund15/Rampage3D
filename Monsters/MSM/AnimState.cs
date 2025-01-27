using Godot;
using Godot.Collections;
using System;

[GlobalClass, Tool]
public partial class AnimState : State
{
	#region STATE_VARIABLES
	[Export]
	protected string AnimName;
	[Export] 
	protected float AnimStartTime = 0f;
	[Export]
	protected float AnimEndDelay = 0f;


	[Export(PropertyHint.NodeType, "State")]
	protected State AnimTransitionState;

	protected IMovementComponent MoveComp;
	protected AnimationPlayer AnimPlayer;
	#endregion
	#region STATE_UPDATES
	public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
		MoveComp = BB.GetVar<IMovementComponent>(BBDataSig.MoveComp);
        AnimPlayer = BB.GetVar<AnimationPlayer>(BBDataSig.Anim);
    }
	public override void Enter(Dictionary<State, bool> parallelStates)
	{
		base.Enter(parallelStates);
		AnimStateStart();
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
	protected virtual void AnimStateStart()
	{
		AnimDirection animDir;
        var inputDir = MoveComp.GetDesiredDirection();
        if (inputDir.Y > 0)
        {
            animDir = MoveComp.GetDesiredDirection().GetAnimDir();
        }
        else
        {
            animDir = MoveComp.GetAnimDirection();
        }
        AnimPlayer.Play(AnimName + animDir.GetAnimationString());
        AnimPlayer.Seek(AnimStartTime, true);

        AnimPlayer.AnimationFinished += OnAnimationFinished;
    }
    protected virtual void OnAnimationFinished(StringName animName)
    {
		EmitSignal(SignalName.TransitionState, this, AnimTransitionState);
    }

    #endregion
}
