using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public partial class LastAnimName : BTCondition
{
	#region TASK_VARIABLES
	[Export]
	private string _lastAnimName;
	private AnimationPlayer _animPlayer;

	public LastAnimName(string lastAnimName) // REAL????
	{
        _lastAnimName = lastAnimName;
    }
	public LastAnimName()
	{
		_lastAnimName = "";
	}
	#endregion
	#region TASK_UPDATES
	public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
		_animPlayer = BB.GetVar<AnimationPlayer>(BBDataSig.Anim);
    }
    public override void Enter()
	{
		base.Enter();
		var assignedAnim = _animPlayer.AssignedAnimation;
		GD.Print("assigned anim name: ", assignedAnim, "\nlast anim key: ", _lastAnimName);
		if (assignedAnim.Contains(_lastAnimName))
		{
			GD.Print("LAST ANIM KEY MATCHES, EXITING...");
            OnExitTask();
        }
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
	#endregion
	#region TASK_HELPER
    #endregion
}

