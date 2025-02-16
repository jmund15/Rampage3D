using Godot;
using Godot.Collections;

[GlobalClass, Tool]
public partial class Base3DState : State
{
    #region STATE_VARIABLES
    protected CharacterBody3D Body;
    protected IMovementComponent MoveComp;
    protected IAnimComponent AnimPlayer;
    #endregion
    #region STATE_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        base.Init(agent, bb);
        Body = Agent as CharacterBody3D;
        MoveComp = BB.GetVar<IMovementComponent>(BBDataSig.MoveComp);
        AnimPlayer = BB.GetVar<IAnimComponent>(BBDataSig.Anim);
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
    }
    #endregion
    #region STATE_HELPER


    #endregion
}
