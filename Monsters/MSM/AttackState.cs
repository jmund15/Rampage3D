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
    protected EaterComponent EaterComp;

	[Export(PropertyHint.NodeType, "State")]
    protected State PostAttackState;
    [Export(PropertyHint.NodeType, "State")]
    protected State OnEatableHitState;

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
        EaterComp = BB.GetVar<EaterComponent>(BBDataSig.EaterComp);

    }
	public override void Enter(Dictionary<State, bool> parallelStates)
	{
        EaterComp.EatableHit += OnEatableHit;

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
        EaterComp.EatableHit -= OnEatableHit;
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
    private void OnEatableHit(object sender, EatableComponent e)
    {
        EmitSignal(SignalName.TransitionState, this, OnEatableHitState);
    }
    protected override void OnTreeFinishLoop(TaskStatus treeStatus)
    {
        base.OnTreeFinishLoop(treeStatus);
        switch (treeStatus)
        {
            case TaskStatus.FAILURE:
                GD.Print("BTState BehaviorTree Finished on status FAILURE");
                EmitSignal(SignalName.TransitionState, this, OnTreeFailureState); break;
            case TaskStatus.SUCCESS:
                EmitSignal(SignalName.TransitionState, this, OnTreeSuccessState); break;
            case TaskStatus.RUNNING or TaskStatus.FRESH:
                Global.LogError("HOW DID TREE FINISH LOOP ON NON SUCCeSS OR FAILURE STATUS?"); break;
        }
    }
    private void OnAnimationFinished(StringName animName)
    {
    }

    #endregion
}
