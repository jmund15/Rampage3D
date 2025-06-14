using Godot;
using Godot.Collections;
using System.Linq;

[GlobalClass, Tool]
public partial class LocatorComponent3D : Node3D
{
    #region COMPONENT_VARIABLES
    // TODO: Have city have a nullable ref to this
    // TODO: If neeeded, have this component have exported ref to resources for each object to locate (e.g., vehicles, buildings, etc.)

    /// <summary>
    /// A manager node responsible for efficiently finding vehicles using Godot's
    /// built-in physics engine for spatial queries.
    /// </summary>
    // By exporting this, you can easily tweak the search radius from the Godot editor.
    [Export]
    public float VehicleSearchRadius { get; set; } = 200.0f;

    // We define the collision layer for vehicles here. It must match the layer
    // you set up in Project Settings (e.g., Layer 2).
    private const int VehicleCollisionLayer = 2;

    
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
	{
		base._Ready();
	}
	public override void _Process(double delta)
	{
		base._Process(delta);
	}
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
	}
    #endregion
    #region COMPONENT_HELPER
    /// <summary>
    /// Finds the closest "Available" vehicle to a given position.
    /// </summary>
    /// <param name="userPosition">The 3D world position to search from.</param>
    /// <returns>The closest available Vehicle node, or null if none are found.</returns>
    public VehicleOccupantsComponent FindClosestAvailableVehicle(Vector3 userPosition, bool needsToBeDriver = false,
        bool isCivilian = true)
    {
        // --- Step 1: Access the physics world ---
        // GetWorld3D().DirectSpaceState gives us access to low-level, high-performance
        // physics queries that don't require creating temporary physics bodies.
        var spaceState = GetWorld3D().DirectSpaceState;

        // --- Step 2: Define the search area (our "net") ---
        // We will query for any objects intersecting with a sphere.
        var queryShape = new SphereShape3D { Radius = VehicleSearchRadius };

        // --- Step 3: Configure the query parameters ---
        var query = new PhysicsShapeQueryParameters3D
        {
            // Set the shape we are querying with.
            Shape = queryShape,

            // Set the shape's position and orientation in the world.
            // We want the sphere to be centered on the user.
            Transform = new Transform3D(Basis.Identity, userPosition),

            // **THIS IS THE KEY TO EFFICIENCY**
            // We set a collision mask to ONLY check against the layer where our
            // vehicles reside. This tells the physics engine to ignore everything else
            // (the ground, buildings, players, etc.), making the query much faster.
            // The mask is a bitmask. (1 << 0) is layer 1, (1 << 1) is layer 2, etc.
            CollisionMask = 1 << (VehicleCollisionLayer - 1),
        };

        // --- Step 4: Execute the query ---
        // IntersectShape is extremely fast. It leverages the engine's internal BVH
        // (Bounding Volume Hierarchy) to quickly discard large parts of the world
        // that are too far away.
        // It returns an array of dictionaries, one for each object found.
        Array<Dictionary> intersectingObjects = spaceState.IntersectShape(query);

        // If the initial, broad-phase query found nothing, we can stop immediately.
        if (intersectingObjects.Count == 0)
        {
            GD.Print($"No vehicles found within {VehicleSearchRadius}m.");
            return null;
        }

        // --- Step 5: Process the results to find the best match ---
        // Now we have a small list of candidates. We can process this short list in C#.
        VehicleOccupantsComponent closestAvailableVehicle = intersectingObjects
            // The actual node is stored in the "collider" key of the result dictionary.
            .Select(resultDict => resultDict["collider"].As<Node>())

            // Safely cast to our Vehicle type and filter out any other objects that might
            // have been on the same layer by mistake.
            //.OfType<RigidBody3D>()
            //.Where(vehicle => vehicle.GetFirstChildOfType<VehicleOccupantComponent>() is VehicleOccupantComponent voc)
            .Select(vehicle => vehicle.GetFirstChildOfType<VehicleOccupantsComponent>(false))
            .Where(voc => voc != null) // Filter out cases where vehicles can't be occupied

            // From the remaining candidates, filter for only those that have open seats available.
            .Where(voc => voc.HasOpenSeat())

            // If the caller is needing to be a driver, filter as such
            .Where(voc => !needsToBeDriver || voc.HasOpenDriverSeat())

            // If the caller is a civilian, filter as such
            //.Where(voc => !isCivilian || voc.VehicleType == NpcType.Critter)

            // Now, on this very small, pre-filtered list, we do the precise distance check.
            // We use DistanceSquaredTo because it avoids a costly square root operation,
            // making the sort slightly faster. The relative order is the same.
            .OrderBy(voc => voc.VehicleGeometry.GlobalPosition.DistanceSquaredTo(userPosition))
            // Finally, select the first one in the sorted list.
            .FirstOrDefault();

        // --- Step 6: Return the result ---
        if (closestAvailableVehicle == null)
        {
            GD.Print("Found vehicles in radius, but none were available.");
        }

        return closestAvailableVehicle;
    }
    #endregion
    #region SIGNAL_LISTENERS
    #endregion
}
