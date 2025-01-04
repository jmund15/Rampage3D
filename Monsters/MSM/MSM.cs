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
    #endregion
    #region STATE_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        base.Init(agent, bb);
        _body = Agent as CharacterBody3D;
    }
    public override void Enter(Dictionary<State, bool> parallelStates)
    {
        base.Enter(parallelStates);
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
            if (_body.IsOnFloor())
            {
                BB.SetPrimVar(BBDataSig.CurrentAttackType, AttackType.GroundNormal);
                TransitionFiniteSubState(PrimarySubState, AttackState);
            }
            else //TODO: FIX!!!!!!
            {
                BB.SetPrimVar(BBDataSig.CurrentAttackType, AttackType.WallNormal);
                TransitionFiniteSubState(PrimarySubState, WallAttackState);
            }
            GD.Print("TRANSITIONED TO AtTACK STATE: ", BB.GetPrimVar<AttackType>(BBDataSig.CurrentAttackType));

        }
        if (Input.IsActionJustPressed("sattack_test"))
        {
            if (_body.IsOnFloor())
            {
                BB.SetPrimVar(BBDataSig.CurrentAttackType, AttackType.GroundSpecial);
                TransitionFiniteSubState(PrimarySubState, AttackState);
            }
            else 
            {
                BB.SetPrimVar(BBDataSig.CurrentAttackType, AttackType.WallSpecial);
                TransitionFiniteSubState(PrimarySubState, WallAttackState);
            }

            
            GD.Print("TRANSITIONED TO AtTACK STATE: ", BB.GetPrimVar<AttackType>(BBDataSig.CurrentAttackType));

        }
    }
    #endregion
    #region STATE_HELPER
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
