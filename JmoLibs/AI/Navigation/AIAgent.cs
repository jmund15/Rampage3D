using Godot;
using Jmo.AI.Core;
using Jmo.AI.Perception;
using Jmo.Shared;
using System.Collections.Generic;

namespace Jmo.AI.Navigation;
/// <summary>
/// The primary coordinator node for an AI entity. It serves as the root of the AI's scene
/// tree, owning all core components and orchestrating the main perceive-decide-act loop
/// every physics frame. It is responsible for initializing dependencies and assembling the
/// DecisionContext for the steering system.
/// </summary>
[Tool]
[GlobalClass]
public partial class AIAgent : Node3D
{
    [ExportGroup("Core Components")]
    [Export] private Blackboard _blackboard;
    [Export] private AIPerceptionManager _perceptionManager;
    [Export] private AISteeringProcessor _steeringProcessor;
    [Export] private AINavigator _navigator;
    [Export] private AIAffinitiesComponent _affinities;

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();
        if (_blackboard == null) warnings.Add("Component reference '_blackboard' is not set.");
        if (_perceptionManager == null) warnings.Add("Component reference '_perceptionManager' is not set.");
        if (_steeringProcessor == null) warnings.Add("Component reference '_steeringProcessor' is not set.");
        if (_navigator == null) warnings.Add("Component reference '_navigator' is not set.");
        if (_affinities == null) warnings.Add("Component reference '_affinities' is not set.");
        return warnings.ToArray();
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;

        // --- Configuration Validation ---
        // A robust agent validates its own dependencies at runtime.
        if (_blackboard == null || _perceptionManager == null || _steeringProcessor == null || _navigator == null || _affinities == null)
        {
            Logger.Error(this, "One or more core components are not assigned. The agent cannot function and will be disabled.");
            SetPhysicsProcess(false);
            return;
        }

        // --- Dependency Initialization ---
        // The agent is responsible for ensuring its components are ready.
        _steeringProcessor.Initialize();

        // --- Blackboard Registration ---
        // Ensure the blackboard has references to the core components for other systems (like BTs) to use.
        _blackboard.SetVar(BBDataSig.Agent, this);
        _blackboard.SetVar(BBDataSig.PerceptionComp, _perceptionManager);
        _blackboard.SetVar(BBDataSig.Affinities, _affinities);
        _blackboard.SetVar(BBDataSig.AINavComp, _navigator); // Assuming AINavComp is the navigator now
    }

    public override void _PhysicsProcess(double delta)
    {
        // High-level logic (e.g., a Behavior Tree) is assumed to have run, updating the blackboard.
        _blackboard.GetPrimVar<Vector3>(BBDataSig.TargetPosition, out var highLevelTarget);

        // --- 1. ASSEMBLE the context for this frame's decision. ---
        // This creates an immutable snapshot of the world state for consistent calculations.
        var context = new DecisionContext(
            _perceptionManager,
            this.GlobalPosition,
            -this.GlobalBasis.Z, // Standard forward vector in Godot
            _navigator.Velocity,
            highLevelTarget
        );

        // --- 2. DECIDE: The steering processor calculates the best direction. ---
        Vector3 desiredDirection = _steeringProcessor.CalculateSteering(context, _blackboard);

        // --- 3. ACT: The navigator executes the movement. ---
        _navigator.SetMovementDirection(desiredDirection);
    }

    public void EnableAgent(bool enable)
    {
        SetPhysicsProcess(enable);
        _navigator.ProcessMode = enable ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
        // You could also disable perception here if the AI is truly "off".
    }
}