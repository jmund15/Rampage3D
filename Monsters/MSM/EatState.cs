using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class EatState : Base3DState
{
    #region STATE_VARIABLES

    [Export(PropertyHint.NodeType, "State")]
    private State _onFinishedEatingState;

    private EaterComponent _eaterComp;
    #endregion
    #region STATE_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        base.Init(agent, bb);
        _eaterComp = BB.GetVar<EaterComponent>(BBDataSig.EaterComp);
    }
    public override void Enter(Dictionary<State, bool> parallelStates)
    {
        base.Enter(parallelStates);
        _eaterComp.AllowEating = true;
        _eaterComp.FinishedEatingCycle += OnFinishedEatingCycle;
    }
    public override void Exit()
    {
        base.Exit();
        _eaterComp.FinishedEatingCycle -= OnFinishedEatingCycle;
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
    private void OnFinishedEatingCycle(object sender, EventArgs e)
    {
        EmitSignal(SignalName.TransitionState, this, _onFinishedEatingState);
    }

    #endregion
}
