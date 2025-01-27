using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TimeRobbers.Interfaces;

public enum AnimDirectionStrategy
{
	CurrDirection,
	InputDirection
}

[GlobalClass, Tool]
public partial class PlayAnim : BehaviorAction
{
	#region TASK_VARIABLES
	[Export]
	protected string AnimName;
	[Export]
	protected float AnimSpeed = 1.0f;
	[Export]
	protected AnimDirectionStrategy AnimDirStrategy;

	protected AnimationPlayer AnimPlayer;

    #endregion
    #region TASK_UPDATES
    public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
		AnimPlayer = BB.GetVar<AnimationPlayer>(BBDataSig.Anim);
    }
	public override void Enter()
	{
        AnimPlayer.AnimationFinished += OnAnimationFinished; // so we guarentee signal connect and disconnect
        base.Enter();
		GD.Print("entered play anim");
        AnimPlayer.SpeedScale = AnimSpeed;

		AnimDirection animDir = AnimDirection.Down;
		switch (AnimDirStrategy)
		{
			case AnimDirectionStrategy.CurrDirection:
				animDir = CurrDirectionStrat(BB);
                break;
			case AnimDirectionStrategy.InputDirection:
				animDir = InputDirectionStrat(BB);
				break;
		}
		var dirName = animDir.GetAnimationString();
        AnimPlayer.Play(AnimName + dirName);
    }
	public override void Exit()
	{
		base.Exit();
        AnimPlayer.AnimationFinished -= OnAnimationFinished;

        AnimPlayer.SpeedScale = 1f;
    }
    public override void ProcessFrame(float delta)
	{
		base.ProcessFrame(delta);
	}
	public override void ProcessPhysics(float delta)
	{
		base.ProcessPhysics(delta);
	}
    #endregion
    #region TASK_HELPER
    public static AnimDirection CurrDirectionStrat(IBlackboard bb)
	{
        return bb.GetVar<IMovementComponent>(BBDataSig.MoveComp).GetAnimDirection();
    }
	public static AnimDirection InputDirectionStrat(IBlackboard bb)
	{
        var moveComp = bb.GetVar<IMovementComponent>(BBDataSig.MoveComp);
        var inputDir = moveComp.GetDesiredDirection();
        //Attack = BB.GetVar<MeleeAttackInfo>(AttackSig);
		if (inputDir.X > 0)
		{
            bb.GetVar<Sprite3D>(BBDataSig.Sprite).FlipH = inputDir.GetFlipH();
        }
        if (inputDir.Y > 0)
        {
            return inputDir.GetAnimDir();
        }
        return moveComp.GetAnimDirection();
    }
	public static void AnimWithOrthog(IBlackboard bb, string animName, OrthogDirection orthogDir)
	{
		var animDir = orthogDir.GetAnimDir();
		var dirName = animDir.GetAnimationString();
		bb.GetVar<Sprite3D>(BBDataSig.Sprite).FlipH = orthogDir.GetFlipH();
        bb.GetVar<AnimationPlayer>(BBDataSig.Anim).Play(animName + dirName);
    }
    private void OnAnimationFinished(StringName animName)
    {
		GD.Print("startup anim finished!");
		Status = TaskStatus.SUCCESS;
    }
    public override string[] _GetConfigurationWarnings()
	{
		var warnings = new List<string>();

		//

		return warnings.Concat(base._GetConfigurationWarnings()).ToArray();
	}
	#endregion
}

