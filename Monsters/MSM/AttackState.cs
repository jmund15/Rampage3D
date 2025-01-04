using Godot;
using Godot.Collections;
using System;

public enum AttackType
{
	GroundNormal,
	GroundSpecial,
	WallNormal,
	WallSpecial
}
[GlobalClass, Tool]
public partial class AttackState : BTState
{
	#region STATE_VARIABLES
	protected Dictionary<AttackType, BBDataSig> AttackToBBMap = new Dictionary<AttackType, BBDataSig>()
	{
		{ AttackType.GroundNormal, BBDataSig.GroundNormalAttack },
		{ AttackType.GroundSpecial, BBDataSig.GroundSpecialAttack },
		{ AttackType.WallNormal, BBDataSig.WallNormalAttack },
		{ AttackType.WallSpecial, BBDataSig.WallSpecialAttack },
	};

    protected AttackType AttackType;
    protected BehaviorTree AttackTree;
	protected BBDataSig AttackSig;

    protected CharacterBody3D Body;
    protected IMovementComponent MoveComp;
	protected AnimationPlayer AnimPlayer;

	[Export(PropertyHint.NodeType, "State")]
    protected State PostAttackState;


    protected Vector2 InputDir;
    protected AnimDirection AttackAnimDir;
    #endregion
    #region STATE_UPDATES
    public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
        Body = Agent as CharacterBody3D;
		MoveComp = BB.GetVar<IMovementComponent>(BBDataSig.MoveComp);
        AnimPlayer = BB.GetVar<AnimationPlayer>(BBDataSig.Anim);


    }
	public override void Enter(Dictionary<State, bool> parallelStates)
	{
        AttackType = BB.GetPrimVar<AttackType>(BBDataSig.CurrentAttackType).Value;
        //GD.Print("ATTACK STATE TYPE: ", AttackType);
        AttackSig = AttackToBBMap[AttackType];
        //GD.Print("ATTACK STATE SIG: ", AttackSig);
        AttackTree = BB.GetVar<BehaviorTree>(AttackSig);
        //GD.Print("ATTACK STATE TREE NAME: ", AttackTree.Name);
        if (Tree != AttackTree)
        {
            Tree = AttackTree;
            Tree.TreeFinishedLoop += OnTreeFinishLoop;
        }
        if (!Tree.Initialized)
        {
            Tree.Init(Agent, BB);
        }
        
        base.Enter(parallelStates);
    }
    public override void Exit()
	{
		base.Exit();
        //AnimPlayer.AnimationFinished -= OnAnimationFinished;
        Tree.TreeFinishedLoop -= OnTreeFinishLoop;
    }
    public override void ProcessFrame(float delta)
	{
		base.ProcessFrame(delta);
        InputDir = MoveComp.GetDesiredDirection();
    }
    public override void ProcessPhysics(float delta)
	{
		base.ProcessPhysics(delta);
        
        if (!MoveComp.WantsAttack() &&
            BB.GetPrimVar<bool>(BBDataSig.SelfInteruptible).Value)
        {
            EmitSignal(SignalName.TransitionState, this, PostAttackState);
        }
    }
	public override void HandleInput(InputEvent @event)
	{
		base.HandleInput(@event);
	}
    #endregion
    #region STATE_HELPER
    private void OnAnimationFinished(StringName animName)
    {
    }

    #endregion
}
