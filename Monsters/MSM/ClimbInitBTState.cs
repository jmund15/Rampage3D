using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class ClimbInitBTState : BTState
{
    #region STATE_VARIABLES
    private CharacterBody3D _body;
    private ClimbableComponent _climbComp;
    private AnimationPlayer _animPlayer;
    #endregion
    #region STATE_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        base.Init(agent, bb);
        _body = Agent as CharacterBody3D;
        _animPlayer = BB.GetVar<AnimationPlayer>(BBDataSig.Anim);
    }
    public override void Enter(Dictionary<State, bool> parallelStates)
	{
		base.Enter(parallelStates);
        _climbComp = BB.GetVar<ClimbableComponent>(BBDataSig.CurrClimbComp);
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
    
    #endregion
}
