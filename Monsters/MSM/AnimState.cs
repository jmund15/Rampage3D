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
	protected IAnimPlayerComponent AnimPlayer;
	#endregion
	#region STATE_UPDATES
	public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
		MoveComp = BB.GetVar<IMovementComponent>(BBDataSig.MoveComp);
        AnimPlayer = BB.GetVar<IAnimPlayerComponent>(BBDataSig.Anim);
    }
	public override void Enter(Dictionary<State, bool> parallelStates)
	{
		base.Enter(parallelStates);
		AnimStateStart();
	}
    public override void Exit()
	{
		base.Exit();
        AnimPlayer.AnimFinished -= OnAnimationFinished;
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
        AnimPlayer.StartAnim(AnimName + animDir.GetAnimationString());
        AnimPlayer.SeekPos(AnimStartTime, true);

        AnimPlayer.AnimFinished += OnAnimationFinished;
    }
    protected virtual void OnAnimationFinished(object sender, string animName)
    {
		EmitSignal(SignalName.TransitionState, this, AnimTransitionState);
    }

    #endregion
}
