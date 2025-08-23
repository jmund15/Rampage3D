using Godot;
using Jmo.AI.Core;
using Jmo.AI.Perception;

namespace Jmo.AI.Core;

/// <summary>
/// The primary coordinator node for an AI entity. It owns all core AI components and
/// orchestrates the main perceive-decide-act loop every physics frame. It is responsible
/// for initializing dependencies and assembling the DecisionContext.
/// </summary>
[GlobalClass]
public partial class AIAgent : Node3D
{
    [Export] public AIPersonality Personality { get; private set; }

    // Core Components - assign in the editor
    private IBlackboard _blackboard;
    private AIPerceptionManager _perceptionManager;
    private AISteeringProcessor _steeringProcessor;
    private AINavigator _navigator;
    // Assume a BehaviorTreePlayer component also exists

    public override void _Ready()
    {
        // In a real project, use a more robust method for finding components
        _blackboard = GetNode<Blackboard>("Blackboard");
        _perceptionManager = GetNode<AIPerceptionManager>("AIPerceptionManager");
        _steeringProcessor = GetNode<AISteeringProcessor>("AISteeringProcessor");
        _navigator = GetNode<AINavigator>("AINavigator");

        // Initialize dependencies
        _steeringProcessor.Initialize(Personality);
    }

    public override void _PhysicsProcess(double delta)
    {
        // The Behavior Tree would have already run and updated the Blackboard
        // with the current high-level goal.
        Vector3 highLevelTarget = _blackboard.GetPrimVar<Vector3>(BBDataSig.TargetPosition);

        // 1. ASSEMBLE the context for this frame's decision.
        var context = new DecisionContext(
            _perceptionManager,
            this.GlobalPosition,
            -this.GlobalBasis.Z, // Standard forward vector in Godot
            _navigator.Velocity, // Get current velocity from the navigator
            highLevelTarget
        );

        // 2. DECIDE: The steering processor calculates the best direction.
        Vector3 desiredDirection = _steeringProcessor.CalculateSteering(context);

        // 3. ACT: The navigator executes the movement.
        _navigator.SetMovementDirection(desiredDirection);
    }
}