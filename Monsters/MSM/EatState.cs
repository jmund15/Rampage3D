using Godot;
using Godot.Collections;

[Tool]
public partial class EatState : Base3DState
{
    #region STATE_VARIABLES
    [Export]
    private string _animName = "eat";

    [Export(PropertyHint.NodeType, "State")]
    private State _onFinishedEatingState;

    private EaterComponent _eaterComp;
    private EatableComponent _eatableGrabbed;
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
        _eatableGrabbed = _eaterComp.CurrEatable;

        AnimDirection animDir = MoveComp.GetAnimDirection();

        AnimPlayer.Play(_animName + IMovementComponent.GetFaceDirectionString(animDir));
        AnimPlayer.AnimationFinished += OnAnimationFinished;

        _eaterComp.CommenceConsumption();
    }
    public override void Exit()
    {
        base.Exit();
        AnimPlayer.AnimationFinished -= OnAnimationFinished;
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
    private void OnAnimationFinished(StringName animName)
    {
        _eaterComp.CompletedConsumption();
        EmitSignal(SignalName.TransitionState, this, _onFinishedEatingState);
    }
    #endregion
}
