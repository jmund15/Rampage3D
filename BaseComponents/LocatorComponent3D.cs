﻿using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
using BaseInterfaces;

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
    // you set up in Project Settings (e.g., Layer 5).
    private const int VehicleCollisionLayer = 5;

    
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
    // TODO: Adjust locator to have two separate methods:
    // 1. for Finding general vehicles with parameters (closest general, needs to be able to be occupied, etc.)
    // 2. for finding vehicle seats available based on parameters (driver, queued, etc.)
    // 3. Make locator more modular? like instead of just for veicles, have user put in their own desired query parameters/collision layer
    public IVehicleComponent3D FindClosestAvailableVehicle(
        Vector3 callerPosition,
        bool needsParked = true
        )
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
            Transform = new Transform3D(Basis.Identity, callerPosition),

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
        IVehicleComponent3D closestAvailableVehicle = intersectingObjects

            // The actual node is stored in the "collider" key of the result dictionary.
            .Select(resultDict => resultDict["collider"].As<Node>())

            // cast to Vehicle type and filter out any other objects that might
            .Select(vehicle => vehicle as IVehicleComponent3D) // TODO: abstract this to base vehicle comp

            // Filter out driving vehicles if needsParked is true
            .Where(vehicle => vehicle.Gear == VehicleGear.Park || !needsParked)

            .OrderBy(vehicle => vehicle.GlobalPosition.DistanceSquaredTo(callerPosition))

            // Finally, select the first one in the sorted list.
            .FirstOrDefault();

        // --- Step 6: Return the result ---
        if (closestAvailableVehicle == null)
        {
            GD.Print("Found vehicles in radius, but none were available.");
            return null;
        }

        return closestAvailableVehicle;
    }

    /// <summary>
    /// Finds the closest "Available" vehicle to a given position.
    /// </summary>
    /// <param name="callerPosition">The 3D world position to search from.</param>
    /// <returns>The closest available Vehicle node, or null if none are found.</returns>
    public VehicleSeat FindClosestAvailableVehicleSeat(
        Vector3 callerPosition,
        bool needsParked = true,
        bool needsToBeDriver = false,
        bool caresAboutQueue = true,
        bool caresAboutMilitary = true // tbd if best way to handle
        )
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
            Transform = new Transform3D(Basis.Identity, callerPosition),

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

        //testing 
        intersectingObjects
            // The actual node is stored in the "collider" key of the result dictionary.
            .Select(resultDict => resultDict["collider"].As<Node>())
            //.ToList().ForEach(vehicle => GD.Print($"Found vehicle: {vehicle.Name}"));

            // cast to Vehicle type and filter out any other objects that might
            .Select(vehicle => vehicle as IVehicleComponent3D) // TODO: abstract this to base vehicle comp

            //.ToList().ForEach(vehicle => GD.Print($"Found vehicle: {vehicle.GetInterfaceNode().Name} at position {vehicle.GlobalPosition}"));
            
            // Filter out driving vehicles if needsParked is true
            .Where(vehicle => vehicle.Gear == VehicleGear.Park || !needsParked)

            //.ToList().ForEach(vehicle => GD.Print($"Found vehicle: {vehicle.GetInterfaceNode().Name} at position {vehicle.GlobalPosition}"));

            //.OfType<RigidBody3D>()
            //.Where(vehicle => vehicle.GetFirstChildOfType<VehicleOccupantComponent>() is VehicleOccupantComponent voc)
            .Select(vehicle => (vehicle as Node3D).GetFirstChildOfType<VehicleOccupantsComponent>(false))
            .Where(voc => voc != null) // Filter out cases where vehicles can't be occupied

            .ToList().ForEach(vehicle => GD.Print($"Found vehicle: {vehicle.Name} at position {vehicle.VehicleGeometry.GlobalPosition}"));


            //// Driver checks
            //.Where(voc => (voc.HasOpenDriverSeat(caresAboutQueue) || !needsToBeDriver) &&
            //                voc.HasOpenSeat(caresAboutQueue))


        // --- Step 5: Process the results to find the best match ---
        // Now we have a small list of candidates. We can process this short list in C#.
        VehicleOccupantsComponent closestAvailableVehicle = intersectingObjects
            // The actual node is stored in the "collider" key of the result dictionary.
            .Select(resultDict => resultDict["collider"].As<Node>())

            // cast to Vehicle type and filter out any other objects that might
            .Select(vehicle => vehicle as IVehicleComponent3D) // TODO: abstract this to base vehicle comp

            // Filter out driving vehicles if needsParked is true
            .Where(vehicle => vehicle.Gear == VehicleGear.Park || !needsParked)

            //.OfType<RigidBody3D>()
            //.Where(vehicle => vehicle.GetFirstChildOfType<VehicleOccupantComponent>() is VehicleOccupantComponent voc)
            .Select(vehicle => (vehicle as Node3D).GetFirstChildOfType<VehicleOccupantsComponent>(false))
            .Where(voc => voc != null) // Filter out cases where vehicles can't be occupied

            // Driver checks
            .Where(voc => (voc.HasOpenDriverSeat(caresAboutQueue) || !needsToBeDriver) &&
                            voc.HasOpenSeat(caresAboutQueue))


            // If the caller is a civilian, filter as such
            //.Where(voc => !isCivilian || voc.VehicleType == NpcType.Critter)

            // Now, on this very small, pre-filtered list, we do the precise distance check.
            // We use DistanceSquaredTo because it avoids a costly square root operation,
            // making the sort slightly faster. The relative order is the same.
            .OrderBy(voc => voc.VehicleGeometry.GlobalPosition.DistanceSquaredTo(callerPosition))
            // Finally, select the first one in the sorted list.
            .FirstOrDefault();

        // --- Step 6: Return the result ---
        if (closestAvailableVehicle == null)
        {
            GD.Print("Found vehicles in radius, but none were available.");
            return null;
        }
        else
        {
            GD.Print($"Found closeset available vehicle: {closestAvailableVehicle.Name} at position {closestAvailableVehicle.VehicleGeometry.GlobalPosition}");
        }

        if (needsToBeDriver)
        {
            // we already know the driver seat is available, so we can return it directly
            return closestAvailableVehicle.DriverSeat;
        }
        else
        {
            // If we don't need to be the driver, we can check all available seats
            return closestAvailableVehicle.GetClosestAvailableSeat(callerPosition, caresAboutQueue);
        }
    }



    private List<VehicleSeat> DriverCheckVehicleSeats(VehicleOccupantsComponent voc, bool caresAboutQueue)
    {
        if (voc.DriverSeat.Availability == SeatAvailability.Available ||
                (voc.DriverSeat.Availability == SeatAvailability.QueuedForEntry && !caresAboutQueue))
        {
            return new List<VehicleSeat> { voc.DriverSeat };
        }
        else
        {
            return new List<VehicleSeat>();
        }
    }

    #endregion
    #region SIGNAL_LISTENERS
    #endregion
}
