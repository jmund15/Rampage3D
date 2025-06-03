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
    public record OccupantInfo
    {
        public CharacterBody3D Occupant;
        public VehiclePosition PositionOccupied;
        public bool IsDriving;
        public override string ToString()
        {
            return $"Occupant: {Occupant?.Name}, Position: {PositionOccupied}, Is Driving: {IsDriving}";
        }
    }

    private IVelocity3DComponent _vehicleVelComp;
    [Export]
    private MeshInstance3D _vehicleGeometry;

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
    public List<OccupantInfo> Occupants { get; protected set; } = new();
    private Dictionary<CharacterBody3D, OccupantInfo> _occupantMap = new Dictionary<CharacterBody3D, OccupantInfo>();


    public event EventHandler<List<OccupantInfo>> OccupantsChanged;
    public event EventHandler<OccupantInfo> OccupantEmbarked;
    public event EventHandler<OccupantInfo> OccupantDisembarked;
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
            var occInfo = new OccupantInfo
            {
                Occupant = inst,
                PositionOccupied = GetOccupantEntryPosition(inst.GlobalPosition),
                IsDriving = false // Default to not driving
            };
            Occupants.Add(inst);
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
    public void DisembarkOccupant(CharacterBody3D occupant, bool isDriving)
    {
        if (isDriving && !HasDriver)
        {
            GD.Print("Vehicle does not have a driver to disembark.");
            return;
        }
        if (occupant == null || !Occupants.Contains(occupant))
        {
            GD.Print("Occupant not found or invalid for disembarking.");
            return;
        
        }

        var occInfo = _occupantMap[occupant];
        occupant.GlobalPosition = GetVehiclePositionGlobal(occInfo.PositionOccupied); // Move occupant to their position before disembarking
        Occupants.Remove(occInfo);
        //occupant.Reparent(Global.CurrentCity); // Remove from vehicle
        occupant.Show(); // Show the occupant after disembarking
        if (isDriving)
        {
            HasDriver = false; // Update driver status if the disembarked occupant was the driver
        }
        OccupantDisembarked?.Invoke(this, occInfo);
    }

    public void EmbarkOccupant(CharacterBody3D occupant, bool isDriving)
    {
        if (isDriving && HasDriver)
        {
            GD.Print("Vehicle already has a driver.");
            return;
        }
        if (Occupants.Count >= MaxOccupants)
        {
            GD.Print("Cannot embark more occupants, maximum reached.");
            return;
        }
        if (occupant == null || !occupant.IsValid())
        {
            GD.Print("Invalid occupant provided for embarking.");
            return;
        }
        var occInfo = new OccupantInfo
        {
            Occupant = occupant,
            PositionOccupied = GetOccupantEntryPosition(occupant.GlobalPosition),
            IsDriving = isDriving
        };
        Occupants.Add(occInfo);
        occupant.Reparent(this);
        occupant.Hide();
        if (isDriving)
        {
            HasDriver = true; // Update driver status if the occupant is driving
        }
        OccupantEmbarked?.Invoke(this, occInfo);
    }
    // TODO: FIX TO USE VEHICLE MESH
    private VehiclePosition GetOccupantEntryPosition(Vector3 occPos)
    {
        var vehiclePos = _vehicleGeometry.GlobalPosition;
        var occVehOffset = occPos - vehiclePos;

        switch (occVehOffset)
        {
            case var offset when offset.X > 0 && offset.Z > 0:
                return VehiclePosition.FrontRight;
            case var offset when offset.X < 0 && offset.Z > 0:
                return VehiclePosition.FrontLeft;
            case var offset when offset.X > 0 && offset.Z < 0:
                return VehiclePosition.BackRight;
            case var offset when offset.X < 0 && offset.Z < 0:
                return VehiclePosition.BackLeft;
            case var offset when Math.Abs(offset.X) < 1f && Math.Abs(offset.Z) > 1f:
                return VehiclePosition.MiddleRight;
            case var offset when Math.Abs(offset.X) > 1f && Math.Abs(offset.Z) < 1f:
                return VehiclePosition.MiddleLeft;
            case var offset when Math.Abs(offset.X) < 1f && Math.Abs(offset.Z) < 1f:
                return VehiclePosition.Trunk;
            default:
                return VehiclePosition.Hood; // Default position if none match
        }
    }
    // TODO: FIX TO USE VEHICLE MESH
    public Vector3 GetVehiclePositionGlobal(VehiclePosition desiredEntryPos)
    {
        switch (desiredEntryPos)
        {
            case VehiclePosition.FrontLeft:
                return _vehicleGeometry.GlobalPosition + new Vector3(-1, 0, 1);
            case VehiclePosition.FrontRight:
                return _vehicleGeometry.GlobalPosition + new Vector3(1, 0, 1);
            case VehiclePosition.BackLeft:
                return _vehicleGeometry.GlobalPosition + new Vector3(-1, 0, -1);
            case VehiclePosition.BackRight:
                return _vehicleGeometry.GlobalPosition + new Vector3(1, 0, -1);
            case VehiclePosition.MiddleLeft:
                return _vehicleGeometry.GlobalPosition + new Vector3(-1, 0, 0);
            case VehiclePosition.MiddleRight:
                return _vehicleGeometry.GlobalPosition + new Vector3(1, 0, 0);
            case VehiclePosition.Trunk:
                return _vehicleGeometry.GlobalPosition + new Vector3(0, 0, -2);
            case VehiclePosition.Hood:
                return _vehicleGeometry.GlobalPosition + new Vector3(0, 0, 2);
            default:
                return _vehicleGeometry.GlobalPosition; // Default to vehicle's global position
        }
    }
    public Vector3 GetDriverEntryPosition()
    {
        return GetVehiclePositionGlobal(DriverEntryAnchor);
    }
    #endregion
    #region SIGNAL_LISTENERS
    #endregion
}
