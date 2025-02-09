using Godot;
using Godot.Collections;

[Tool]
public partial class Idle3DState : Base3DState
{
    #region STATE_VARIABLES
    [Export]
    public string AnimName { get; protected set; } = "idle";

    [Export(PropertyHint.NodeType, "State")]
    private State _onMoveInputState;
    [Export(PropertyHint.NodeType, "State")]
    private State _onJumpInputState;
    [Export(PropertyHint.NodeType, "State")]
    private State _onNotOnFloorState;

    private Vector2 _inputDirection = new Vector2();
    #endregion
    #region STATE_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        base.Init(agent, bb);
    }
    public override void Enter(Dictionary<State, bool> parallelStates)
    {
        base.Enter(parallelStates);

        AnimPlayer.Play(AnimName +
            MoveComp.GetAnimDirection().GetAnimationString());
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void ProcessFrame(float delta)
    {
        base.ProcessFrame(delta);
        _inputDirection = MoveComp.GetDesiredDirection();
        if (!_inputDirection.IsZeroApprox())
        {
            EmitSignal(SignalName.TransitionState, this, _onMoveInputState);
        }
        else if (MoveComp.WantsJump() && Body.IsOnFloor())
        {
            EmitSignal(SignalName.TransitionState, this, _onJumpInputState);
        }
    }
    public override void ProcessPhysics(float delta)
    {
        base.ProcessPhysics(delta);
        Vector3 velocity = Body.Velocity;

        //if (!Body.IsOnFloor())
        //{
        //    EmitSignal(SignalName.TransitionState, this, _onNotOnFloorState);
        //}

        velocity.X = 0; // TODO: slow down?
        velocity.Y = 0; // Fall state/gravity?
        velocity.Z = 0; // slow down

        Body.Velocity = velocity;
        Body.MoveAndSlide();
    }
    public override void HandleInput(InputEvent @event)
    {
    }
    #endregion
    #region STATE_HELPER


    #endregion
}
