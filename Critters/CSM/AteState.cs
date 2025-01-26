using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class AteState : Base3DState
{
	#region STATE_VARIABLES
	[Export(PropertyHint.NodeType, "State")]
	private State _transitionState;

	private EatableComponent _eatableComp;
	private Sprite3D _sprite;
	#endregion
	#region STATE_UPDATES
	public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
        _sprite = BB.GetVar<Sprite3D>(BBDataSig.Sprite);
		_eatableComp = BB.GetVar<EatableComponent>(BBDataSig.EatableComp);
    }
	public override void Enter(Dictionary<State, bool> parallelStates)
	{
		base.Enter(parallelStates);
		//GD.Print("CRITTER EATEN!!!");
		_sprite.Hide();
		GD.Print("HIDING SPIRTE");
		_eatableComp.Eaten += OnEaten;
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
    private void OnEaten(object sender, EaterComponent e)
    {
        _eatableComp.Eaten -= OnEaten;
		Agent.QueueFree();
		GD.Print("QUEUED EATABLE FOR DELETEION");
    }
    #endregion
}
