using DotnetUtils;
using Godot;
using Jmo.AI.Navigation;
using Jmo.Shared;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;

namespace Jmo.AI.Navigation;

/* TODO:
 * Instead of a fixed timer, the AINavigator should recalculate its path based on specific conditions:
 * When the target moves too far: The navigator should store the position where it last calculated a path to (_lastCalculatedTargetPath). Every frame, it checks the distance between this stored position and the actual target's current position. If this distance exceeds a threshold (e.g., 2 meters), it's time to recalculate. This makes the AI highly responsive.
 * When the path is invalidated: If the current path becomes blocked (e.g., a dynamic door closes), the agent will get stuck. The navigator can detect this (e.g., if Velocity.IsZeroApprox() for more than a second while IsNavigationFinished() is false) and trigger a recalculation.
 * When the TargetReached signal is emitted: The navigator should stop all pathfinding until it is given a new target.
 * This conditional logic provides the same performance benefits as a timer but makes the AI far more responsive and efficient.
 */


/// <summary>
/// A pure "driver" component responsible for low-level agent movement. It takes a desired
/// steering direction from the AISteeringProcessor and uses Godot's NavigationAgent3D
/// to execute the final movement, handling pathfinding and velocity updates. It has no
/// knowledge of why it is moving, only how.
/// </summary>
[Tool]
[GlobalClass]
public partial class AINavigator : NavigationAgent3D
{
    private NavigationProfile _activeProfile;

    /// <summary>
    /// Gets the agent's current linear velocity.
    /// </summary>
    //public Vector3 Velocity { get; private set; }

    [Export(PropertyHint.Range, "0, 50, 0.1")]
    private float _maxSpeed = 10.0f;

    public override string[] _GetConfigurationWarnings()
    {
        if (GetParentOrNull<AIAgent>() == null)
        {
            return new string[] { "AINavigator should be a direct child of an AIAgent node for proper coordination." };
        }
        return base._GetConfigurationWarnings();
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;

        // Ensure the agent doesn't try to move itself. The parent body is the one that moves.
        // This is a common point of confusion with NavigationAgent3D.
        VelocityComputed += OnVelocityComputed;
    }

    /// <summary>
    /// Sets the desired movement direction for the current frame. The navigator will attempt
    /// to move in this direction, respecting pathfinding and physics.
    /// </summary>
    /// <param name="direction">The normalized direction vector for movement.</param>
    public void SetMovementDirection(Vector3 direction)
    {
        if (!IsNavigationFinished())
        {
            Vector3 nextPathPos = GetNextPathPosition();
            Vector3 navDirection = GetParent<Node3D>().GlobalPosition.DirectionTo(nextPathPos);
            // Simple blend between desired direction and navigation path direction
            Vector3 finalDirection = direction.Lerp(navDirection, 0.5f).Normalized();
            SetVelocity(finalDirection * _maxSpeed);
        }
        else
        {
            SetVelocity(direction * _maxSpeed);
        }
    }

    private void OnVelocityComputed(Vector3 safeVelocity)
    {
        Velocity = safeVelocity;
        // The parent Node3D (which must be a CharacterBody3D or similar) is responsible
        // for actually moving. This signal connection is key.
        // In your CharacterBody3D script: navigator.VelocityComputed += (vel) => SetVelocity(vel); MoveAndSlide();
    }

    #region HELPER_FUNCTIONS
    /// <summary>
    /// Changes the agent's active navigation profile at runtime.
    /// </summary>
    public void SetNavigationProfile(NavigationProfile newProfile)
    {
        if (newProfile == null)
        {
            Logger.Error(this, "Attempted to set a null NavigationProfile.");
            return;
        }
        _activeProfile = newProfile;
        SetNavigationLayers(_activeProfile.NavigationLayers);
    }

    /// <summary>
    /// Calculates the length of the current, active navigation path.
    /// </summary>
    /// <returns>The path distance in meters, or float.MaxValue if no path exists.</returns>
    public float GetCurrentPathDistance()
    {
        if (IsNavigationFinished()) return 0f;

        Vector3[] pathPoints = GetCurrentNavigationPath();
        if (pathPoints.Length < 2) return 0f;

        float distance = GetParent<Node3D>().GlobalPosition.DistanceTo(pathPoints[0]);
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            distance += pathPoints[i].DistanceTo(pathPoints[i + 1]);
        }
        return distance;
    }

    /// <summary>
    /// Finds the target node from a list that is closest via navigation path distance.
    /// </summary>
    /// <remarks>
    /// This is a synchronous operation and a potentially expensive call. 
    /// It may cause frame hitches if the target list is large.
    /// ONLY USE WITH SMALL TARGET LISTS OR DURING NON-REALTIME SCENARIOS.
    /// </remarks>
    /// <param name="targets">A list of potential target nodes.</param>
    /// <returns>The closest reachable target, or null if none are reachable.</returns>
    public Node3D? FindNearestNavTargetSyncronous(IEnumerable<Node3D> targets, bool optimize = true)
    {
        Node3D? closestTarget = null;
        float shortestDistance = float.MaxValue;
        Rid map = GetNavigationMap();

        foreach (var target in targets)
        {
            // Use the NavigationServer directly for a synchronous path query.
            Vector3[] path = NavigationServer3D.MapGetPath(map, GetParent<Node3D>().GlobalPosition, target.GlobalPosition, optimize, _activeProfile.NavigationLayers);

            if (path.Length > 0)
            {
                float distance = CalculatePathLength(path);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestTarget = target;
                }
            }
        }
        return closestTarget;
    }

    public async Task<Node3D?> FindNearestNavTargetAsync(IEnumerable<Node3D> targets, CancellationToken cancellationToken, bool optimize = true)
    {
        Node3D? closestTarget = null;
        float shortestDistance = float.MaxValue;
        Rid map = GetNavigationMap();

        foreach (var target in targets)
        {
            // Check if the task was cancelled from outside
            if (cancellationToken.IsCancellationRequested) return null;

            //NavigationServer3D.QueryPath // TODO: look into using this instead
            Vector3[] path = NavigationServer3D.MapGetPath(map, GetParent<Node3D>().GlobalPosition, target.GlobalPosition, optimize, _activeProfile.NavigationLayers);

            if (path.Length > 0)
            {
                float distance = CalculatePathLength(path);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestTarget = target;
                }
            }

            // This is the key part: wait for the next frame before starting the next expensive query.
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        }
        return closestTarget;
    }

    private float CalculatePathLength(Vector3[] path)
    {
        float distance = 0f;
        for (int i = 0; i < path.Length - 1; i++)
        {
            distance += path[i].DistanceTo(path[i + 1]);
        }
        return distance;
    }
    #endregion
}