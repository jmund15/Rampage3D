using Godot;
using System;
using Godot.Collections;

[Tool]
public partial class MSM : CompoundState
{
    #region STATE_VARIABLES
    private CharacterBody3D _body;

    [Export(PropertyHint.NodeType, "State")]
    protected State AttackState;
    [Export(PropertyHint.NodeType, "State")]
    protected State WallAttackState;
    [Export(PropertyHint.NodeType, "State")]
    protected State OnEjectClimberState;

    private ClimberComponent _climberComp;
    #endregion
    #region STATE_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        base.Init(agent, bb);
        _body = Agent as CharacterBody3D;
        _climberComp = BB.GetVar<ClimberComponent>(BBDataSig.ClimberComp);
    }
    public override void Enter(Dictionary<State, bool> parallelStates)
    {
        base.Enter(parallelStates);
        _climberComp.EjectRequestSent += OnRequestEjectClimber;
    }
    public override void Exit()
    {
        base.Exit();
        _climberComp.EjectRequestSent -= OnRequestEjectClimber;
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
        if (!BB.GetPrimVar<bool>(BBDataSig.SelfInteruptible).Value )
        {
            return;
        }
        if (PrimarySubState is AttackState)
        {
            return;
        }
        //GD.Print("handling input: ", @event.IsAction("attack_test"));
        if (Input.IsActionJustPressed("attack_test"))
        {
            if (_climberComp.IsClimbing)
            {
                BB.SetPrimVar(BBDataSig.CurrentAttackType, AttackType.WallNormal);
                TransitionFiniteSubState(PrimarySubState, WallAttackState);
            }
            else if (_body.IsOnFloor()) 
            {
                BB.SetPrimVar(BBDataSig.CurrentAttackType, AttackType.GroundNormal);
                TransitionFiniteSubState(PrimarySubState, AttackState);
            }
            //TODO: ALLOW ONE ATTACK IN THE AIR?

            GD.Print("TRANSITIONED TO AtTACK STATE: ", BB.GetPrimVar<AttackType>(BBDataSig.CurrentAttackType));

        }
        if (Input.IsActionJustPressed("sattack_test"))
        {
            if (_climberComp.IsClimbing)
            {
                BB.SetPrimVar(BBDataSig.CurrentAttackType, AttackType.WallSpecial);
                TransitionFiniteSubState(PrimarySubState, WallAttackState);
            }
            else if (_body.IsOnFloor())
            {
                BB.SetPrimVar(BBDataSig.CurrentAttackType, AttackType.GroundSpecial);
                TransitionFiniteSubState(PrimarySubState, AttackState);
            }
            

            
            GD.Print("TRANSITIONED TO AtTACK STATE: ", BB.GetPrimVar<AttackType>(BBDataSig.CurrentAttackType));

        }
    }
    #endregion
    #region STATE_HELPER
    private void OnRequestEjectClimber(object sender, EventArgs e)
    {
        GD.Print("EJECTING CLIMBER");
        _climberComp.StopClimb();
        TransitionFiniteSubState(PrimarySubState, OnEjectClimberState);
    }
    public override void TransitionFiniteSubState(State oldSubState, State newSubState)
    {
        base.TransitionFiniteSubState(oldSubState, newSubState);
    }
    public override void AddParallelSubState(State state)
    {
        base.AddParallelSubState(state);
    }
    public override void RemoveParallelSubState(State state)
    {
        base.RemoveParallelSubState(state);
    }
    #endregion
}
