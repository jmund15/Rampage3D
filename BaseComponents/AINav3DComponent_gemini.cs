
using Godot;
using System;

/// <summary>
/// A simplified, modular navigation component responsible for pathfinding.
/// It acts as a "GPS" for an AI agent, providing the path to a target
/// without any steering or environmental awareness logic.
/// Inherits from NavigationAgent3D to leverage Godot's built-in navigation system.
/// </summary>
[GlobalClass]
public partial class AINav3DComponent_gemini : NavigationAgent3D
{
    /// <summary>
    /// Timer to control how frequently a new path can be requested.
    /// </summary>
    private Timer _pathRequestTimer;
    private bool _canRequestPath = true;

    [Export]
    public float PathRequestInterval { get; set; } = 0.25f;

    [Export]
    public Node3D ParentAgent { get; private set; }

    public override void _Ready()
    {
        base._Ready();

        if (ParentAgent == null)
        {
            ParentAgent = GetParent<Node3D>();
            if (ParentAgent == null)
            {
                GD.PrintErr("AINav3DComponent_gemini requires a Node3D parent or to have its ParentAgent property set.");
                SetProcess(false);
                SetPhysicsProcess(false);
                return;
            }
        }

        // Timer to prevent spamming path requests
        _pathRequestTimer = new Timer();
        AddChild(_pathRequestTimer);
        _pathRequestTimer.WaitTime = PathRequestInterval;
        _pathRequestTimer.OneShot = true;
        _pathRequestTimer.Timeout += () => _canRequestPath = true;

        // Ensure the agent is ready for navigation commands.
        CallDeferred(MethodName.EnableNavigation);
    }

    /// <summary>
    /// Sets the target position for the navigation agent.
    /// Returns true if the path request is accepted, false if on cooldown.
    /// </summary>
    /// <param name="globalPosition">The global position to navigate to.</param>
    /// <returns>Boolean indicating if the request was successful.</returns>
    public bool RequestPath(Vector3 globalPosition)
    {
        if (!_canRequestPath)
        {
            return false;
        }

        TargetPosition = globalPosition;
        _canRequestPath = false;
        _pathRequestTimer.Start();
        return true;
    }

    /// <summary>
    /// Enables the navigation agent.
    /// </summary>
    public void EnableNavigation()
    {
        SetProcess(true);
        SetPhysicsProcess(true);
    }

    /// <summary>
    /// Disables the navigation agent and clears its path.
    /// </summary>
    public void DisableNavigation()
    {
        TargetPosition = ParentAgent.GlobalPosition;
        SetProcess(false);
        SetPhysicsProcess(false);
    }

    /// <summary>
    /// Returns the ideal next direction to move in, without any context or steering.
    /// This is the pure direction from the navigation mesh path.
    /// </summary>
    /// <returns>A normalized Vector3 direction.</returns>
    public Vector3 GetIdealDirection()
    {
        if (IsNavigationFinished())
        {
            return Vector3.Zero;
        }

        Vector3 nextPathPos = GetNextPathPosition();
        return ParentAgent.GlobalPosition.DirectionTo(nextPathPos);
    }

    /// <summary>
    /// Calculates the total remaining distance along the current navigation path.
    /// </summary>
    /// <returns>The path distance to the target in meters.</returns>
    public float GetRemainingPathDistance()
    {
        if (IsNavigationFinished())
        {
            return 0f;
        }

        Vector3[] path = GetCurrentNavigationPath();
        int currentIndex = GetCurrentNavigationPathIndex();
        if (currentIndex >= path.Length)
        {
            return 0f;
        }

        float distance = ParentAgent.GlobalPosition.DistanceTo(path[currentIndex]);

        for (int i = currentIndex; i < path.Length - 1; i++)
        {
            distance += path[i].DistanceTo(path[i + 1]);
        }

        return distance;
    }
}
