using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public enum VehiclePosition
{
    FrontLeft,
    FrontRight,
    BackLeft,
    BackRight,
    MiddleLeft,
    MiddleRight,
    Trunk,
    Hood
}

[GlobalClass, Tool]
public partial class VehicleOccupantComponent : Node
{
    #region COMPONENT_VARIABLES

    private IVelocity3DComponent _vehicleVelComp;
    [Export]
    private MeshInstance3D _vehicleGeometry;

    [Export]
    public Godot.Collections.Array<VehicleSeat> VehicleSeats { get; protected set; } = new();

    //TODO: MAKE A NODE3D THAT CORRESPONDS TO ENTERABLE POSITIONS

    [Export]
    public Godot.Collections.Array<VehiclePosition> EnterablePositions { get; protected set; } = new Godot.Collections.Array<VehiclePosition>
    {
        VehiclePosition.FrontLeft,
        VehiclePosition.FrontRight,
        VehiclePosition.BackLeft,
        VehiclePosition.BackRight,
        VehiclePosition.MiddleLeft,
        VehiclePosition.MiddleRight,
        VehiclePosition.Trunk,
        VehiclePosition.Hood
    };
    [Export]
    public VehiclePosition DriverEntryAnchor { get; protected set; } = VehiclePosition.FrontLeft; // Default entry point for the driver
    [Export]
    public bool HasDriver { get; protected set; } = false;
    [Export]
    public int MaxOccupants { get; protected set; } = 5;

    [Export]
    public NpcType VehicleType { get; protected set; } = NpcType.Critter; // Default type for the vehicle

    //public Godot.Collections.Dictionary<NpcType, bool> AllowedOccupantTypesMap
    //{ get; protected set; } = new Godot.Collections.Dictionary<NpcType, bool>
    //{
    //    { NpcType.Critter, false },
    //    { NpcType.Police, false },
    //    { NpcType.Army, false },
    //    { NpcType.General, false }
    //};
    [Export]
    public bool RandomizeInitialOccupants { get; protected set; } = false;
    [Export]
    public Godot.Collections.Array<PackedScene> StaticInitialOccupants { get; protected set; } = new();
    public HashSet<Node3D> CurrentOccupants { get; protected set; } = new();

    public event EventHandler<List<VehicleSeat>> OccupantsChanged;
    public event EventHandler<VehicleSeat> OccupantEmbarked;
    public event EventHandler<VehicleSeat> OccupantDisembarked;
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
	{
		base._Ready();

        _vehicleVelComp = GetOwner<IVelocity3DComponent>();
        if (_vehicleVelComp == null)
        {
            GD.PrintErr("VehicleOccupantComponent requires an owner that implements IVelocity3DComponent.");
            return;
        }

        // Randomize initial occupants if enabled
        if (RandomizeInitialOccupants)
        {
            RandomizeOccupants();
        }
        else
        {
            InitializeStaticOccupants();
        }
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
    private void RandomizeOccupants()
    {
        int numInitOccupants = GD.RandRange(1, MaxOccupants);
        switch (VehicleType)
        {
            case NpcType.Critter:
                bool sameSpecies;
                if (GD.RandRange(0, 1) == 1) { sameSpecies = true; }
                else { sameSpecies = false; }

                break;
        }
    }
    private void InitializeStaticOccupants()
    {
        foreach (var scene in StaticInitialOccupants)
        {
            var inst = scene.Instantiate<CharacterBody3D>();
            inst.Hide();
            AddChild(inst);
            // TODO: FIX INITIALizAION
            foreach (var seat in VehicleSeats)
            {
                if (!seat.IsOccupied)
                {
                    inst.Reparent(this);
                    if (seat.IsDriverSeat)
                    {
                        HasDriver = true;
                    }
                    break;
                }
            }
            CurrentOccupants.Add(inst);
            //if (AllowedOccupantTypesMap.ContainsKey(npcType) && AllowedOccupantTypesMap[npcType])
            //{
            // Create a new occupant and add it to the list
            //var occupant = new Occupant(npcType, null); // Reference can be set later
            //Occupants.Add(occupant);
            //}
        }
    }

    public void DisembarkAllOccupants()
    {

    }
    public void DisembarkOccupant(CharacterBody3D occupant, VehicleSeat seat)
    {
        if (seat.IsDriverSeat && !HasDriver)
        {
            GD.Print("Vehicle does not have a driver to disembark.");
            return;
        }
        if (occupant == null || !CurrentOccupants.Contains(occupant))
        {
            GD.Print("Occupant not found or invalid for disembarking.");
            return;
        }

        occupant.GlobalPosition = _vehicleGeometry.GlobalPosition + seat.EntrancePosition;
        CurrentOccupants.Remove(occupant);
        //occupant.Reparent(Global.CurrentCity); // Remove from vehicle
        occupant.Show(); // Show the occupant after disembarking
        if (seat.IsDriverSeat)
        {
            HasDriver = false; // Update driver status if the disembarked occupant was the driver
        }
        OccupantDisembarked?.Invoke(this, seat);
    }
    public void EmbarkOccupant(Node3D occupant, VehicleSeat seat)
    {
        if (seat.IsDriverSeat && HasDriver)
        {
            //TODO: Throw out driver??
            GD.Print("Vehicle already has a driver.");
            return;
        }
        if (CurrentOccupants.Count >= MaxOccupants ||
            seat.IsOccupied)
        {
            GD.Print("Cannot embark more occupants, maximum reached.");
            return;
        }
        if (occupant == null || !occupant.IsValid())
        {
            GD.Print("Invalid occupant provided for embarking.");
            return;
        }
        seat.Occupant = occupant;
        CurrentOccupants.Add(occupant);
        occupant.Reparent(this);
        occupant.Hide();
        if (seat.IsDriverSeat)
        {
            HasDriver = true; // Update driver status if the occupant is driving
        }
        OccupantEmbarked?.Invoke(this, seat);
    }
    public Vector3 GetDriverEntryPosition()
    {
        foreach (var seat in VehicleSeats)
        {
            if (seat.IsDriverSeat)
            {
                return seat.EntrancePosition;
            }
        }
        return Vector3.Zero; // Default if no driver seat found
    }
    #endregion
    #region SIGNAL_LISTENERS
    #endregion
}
