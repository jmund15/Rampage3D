using BaseInterfaces;
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
public partial class VehicleOccupantsComponent : Node
{
    #region COMPONENT_VARIABLES

    public IVehicleComponent3D VehicleComp { get; private set; }
    [Export]
    public MeshInstance3D VehicleGeometry { get; private set; }

    [Export]
    public Godot.Collections.Array<VehicleSeat> VehicleSeats { get; protected set; } = new();
    
    public VehicleSeat DriverSeat { get; protected set; }
    
    //public HashSet<VehicleSeat> OccupiedSeats { get; protected set; } = new();
    public HashSet<VehicleSeat> AvailableSeats { get; protected set; } = new();
    //public HashSet<VehicleSeat> QueuedSeats { get; protected set; } = new();
    [Export]
    public float AllowedEmbarkDistance { get; private set; } = 0.25f;

    //TODO: MAKE A NODE3D THAT CORRESPONDS TO ENTERABLE POSITIONS

    //[Export]
    //public Godot.Collections.Dictionary<VehiclePosition, Vector3> EnterablePositions { get; protected set; } =
    //    new Godot.Collections.Dictionary<VehiclePosition, Vector3>
    //{
    //        { VehiclePosition.FrontLeft, new Vector3(-1, 0, 2) },
    //        { VehiclePosition.FrontRight, new Vector3(1, 0, 2) },
    //        { VehiclePosition.BackLeft, new Vector3(-1, 0, -2) },
    //        { VehiclePosition.BackRight, new Vector3(1, 0, -2) },
    //        { VehiclePosition.MiddleLeft, new Vector3(-1, 0, 0) },
    //        { VehiclePosition.MiddleRight, new Vector3(1, 0, 0) },
    //        { VehiclePosition.Trunk, new Vector3(0, 0, -3) },
    //        { VehiclePosition.Hood, new Vector3(0, 0, 3) }
    //};

    //[Export]
    //public VehiclePosition DriverEntryAnchor { get; protected set; } = VehiclePosition.FrontLeft; // Default entry point for the driver
    //[Export]
    private bool _isEmbarkable = true; // Whether occupants can embark on the vehicle
    public bool IsEmbarkable 
    { 
        get => _isEmbarkable; 
        protected set
        {
            if (value == _isEmbarkable) return;

            _isEmbarkable = value; // Update the embark status
            EmbarkableStatusChanged?.Invoke(this, _isEmbarkable); // Notify listeners of the change
        }
    } 
    public bool HasDriver { get; private set; }
    //{
    //    get => HasOpenDriverSeat(); // Check if there is an open driver seat
    //}
    public bool HasAvailableSeat
    {
        get => HasOpenSeat(); // Check if there is an open seat
    }
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
    public HashSet<OccupantComponent3D> CurrentOccupants { get; protected set; } = new();

    //public event EventHandler<bool> Parked?;
    public event EventHandler<List<VehicleSeat>> OccupantsChange;
    public event EventHandler<VehicleSeat> OccupantEmbark;
    public event EventHandler<VehicleSeat> OccupantDisembark;

    public event EventHandler<IDriver> DriverEmbark;
    public event EventHandler DriverDisembark;

    public event EventHandler<bool> EmbarkableStatusChanged;
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
	{
		base._Ready();

        VehicleComp = GetOwner<IVehicleComponent3D>();
        if (VehicleComp == null)
        {
            GD.PrintErr("VehicleOccupantComponent requires an owner that implements IVelocity3DComponent.");
            return;
        }

        // init seat occupants (TODO: is this ok?)
        bool foundDriversSeat = false;
        foreach (var seat in VehicleSeats)
        {
            seat.VOccupantComp = this;
            if (seat.IsDriverSeat)
            {
                if (foundDriversSeat) {
                    GD.PrintErr("Multiple driver seats found in VehicleOccupantsComponent. Only one driver seat is allowed.");
                    continue; // Skip if multiple driver seats are defined
                }
                GD.Print("SET DRIVER SEAT!"); 
                DriverSeat = seat; // Set the driver seat if it exists
                foundDriversSeat = true;
            }
        }
        if (!foundDriversSeat)
        {
            GD.PrintErr("No driver seat defined in VehicleOccupantsComponent. Please define a driver seat.");
            DriverSeat = null; // No driver seat found
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

        foreach (var seat in VehicleSeats)
        {
            if (seat.Availability == SeatAvailability.Available)
            {
                AvailableSeats.Add(seat);
                seat.AvailabilityChanged += OnSeatAvailabilityChanged;
                seat.OccupancyChanged += OnSeatOccupancyChanged;
            }
        }
        OccupantEmbark += OnOccupantEmbarked;
        OccupantDisembark += OnOccupantDisembarked;

        DriverEmbark += (sender, driver) =>
        {
            HasDriver = true; // Update driver status when a driver embarks
        };
        DriverDisembark += (sender, e) =>
        {
            HasDriver = false; // Update driver status when a driver disembarks
        };
    }

    private void Seat_OccupancyChanged(object sender, bool e)
    {
        throw new NotImplementedException();
    }
    public override void _Process(double delta)
	{
		base._Process(delta);
	}
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
        if (Engine.IsEditorHint())
        {
            if (VehicleGeometry == null)
            {
                return;
            }
            foreach (var seat in VehicleSeats)
            {
                if (seat == null) { continue; }
                DrawSeatEntrancePoint(seat);
            }
        }
    }
    #endregion
    #region COMPONENT_HELPER
    public bool HasOpenSeat(bool caresAboutQueue = false)
    {
        return VehicleSeats.Any(seat => 
            !seat.IsOccupied && !(seat.Availability == SeatAvailability.QueuedForEntry && caresAboutQueue));
    }
    public bool HasOpenDriverSeat(bool caresAboutQueue = false)
    {
        return VehicleSeats.Any(seat => 
            seat.IsDriverSeat && 
            !seat.IsOccupied && 
            !(seat.Availability == SeatAvailability.QueuedForEntry && caresAboutQueue));
    }
    private void DrawSeatEntrancePoint(VehicleSeat seat)
    {
        var _time = Time.GetTicksMsec() / 1000.0f;
        //var sphere = _doorEntranceIndicator.Mesh as SphereMesh;
        //sphere.Radius = 0.25f * Mathf.Sin(_time * 2f);
        //sphere.Height = sphere.Radius * 2;
        //_doorEntranceIndicator.GlobalPosition = new Vector3(
        //    _doorEntranceIndicator.GlobalPosition.X, 0, _doorEntranceIndicator.GlobalPosition.Z);

        //DoorEntranceOffset = new Vector2(
        //    _doorEntranceIndicator.Position.X, _doorEntranceIndicator.Position.Z);
        var pointRadius = Mathf.Abs(0.02f/*1f*/ * Mathf.Sin(_time * 4f)) + 0.15f;
        var pointLoc = VehicleGeometry.GlobalPosition +
            new Vector3(seat.EntrancePosition.X, pointRadius, seat.EntrancePosition.Y);
        DebugDraw3D.DrawSphere(pointLoc, pointRadius, seat.SeatIndColor);
        //GD.Print("DRAWING SEAT THINGY!");
        //DebugDraw3D.DrawBox(pointLoc, new Quaternion(), new Vector3(pointRadius, pointRadius, pointRadius), seat.SeatIndColor);
    }
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
                    //if (seat.IsDriverSeat)
                    //{
                    //    HasDriver = true;
                    //}
                    break;
                }
            }
            CurrentOccupants.Add(inst.GetFirstChildOfType<OccupantComponent3D>());
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
    public (VehicleSeat, SeatAvailability?) GetDriverSeat()
    {
        foreach (var seat in VehicleSeats)
        {
            if (seat == null) { continue; }
            if (seat.IsDriverSeat)
            {
                return (seat, seat.Availability);
            }
        }
        return (null, null); // No driver seat found
    }
    public Vector3 GetDriverEntryPosition()
    {
        foreach (var seat in VehicleSeats)
        {
            if (seat == null) { continue; }
            if (seat.IsDriverSeat)
            {
                return VehicleGeometry.ToGlobal(seat.EntrancePosition.GetVector3());
            }
        }
        return Vector3.Zero; // Default if no driver seat found
    }
    public Vector3 GetNextAvailableSeatEntryPosition()
    {
        foreach (var seat in VehicleSeats)
        {
            if (seat == null) { continue; }
            if (!seat.IsOccupied)
            {
                return VehicleGeometry.ToGlobal(seat.EntrancePosition.GetVector3());
            }
        }
        return Vector3.Zero; // Default if no driver seat found
    }

    public VehicleSeat GetClosestAvailableSeat(Vector3 position, bool caresAboutQueue = true)
    {
        VehicleSeat closestSeat = null;
        float closestDistance = float.MaxValue;
        foreach (var seat in VehicleSeats)
        {
            if (seat == null || seat.Availability == SeatAvailability.Occupied) { continue; }
            if (caresAboutQueue && seat.Availability == SeatAvailability.QueuedForEntry)
            {
                continue; // Skip seats that are queued for entry if it matters (aka in times of peace)
            }
            float distance = position.DistanceTo(VehicleGeometry.ToGlobal(seat.EntrancePosition.GetVector3()));
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSeat = seat;
            }
        }
        return closestSeat; // Returns the closest available seat or null if none found
    }
    public bool CloseEnoughToEmbark(Node3D occupant, VehicleSeat seat)
    {
        if (seat.IsOccupied)
        {
            GD.Print("Seat is already occupied.");
            return false;
        }
        if (occupant.GlobalPosition.DistanceTo(VehicleGeometry.ToGlobal(seat.EntrancePosition.GetVector3())) 
            > AllowedEmbarkDistance)
        {
            GD.Print("Occupant is too far from the vehicle.");
            return false;
        }
        return true;
    }

    public Vector3 GetSeatEntryPosition(VehicleSeat seat)
    {
        return VehicleGeometry.ToGlobal(seat.EntrancePosition.GetVector3());
    }
    #endregion
    #region SIGNAL_LISTENERS

    private void OnSeatAvailabilityChanged(object sender, SeatAvailability availability)
    {
        var seat = sender as VehicleSeat;
    }
    private void OnSeatOccupancyChanged(object sender, VehicleSeat.OccupancyChangedEventArgs occChangedArgs)
    {
        var seat = sender as VehicleSeat;
        var occupant = occChangedArgs.Occupant;
        if (occChangedArgs.OccupantEntered)
        {
            if (seat.IsDriverSeat)
            {
                var driver = occupant as IDriver;
                CurrentOccupants.Add(occupant);
                OccupantEmbark?.Invoke(this, seat);
                DriverEmbark?.Invoke(this, driver);
            }
            else
            {
                seat.Occupant = occupant;
                CurrentOccupants.Add(occupant);
                OccupantEmbark?.Invoke(this, seat);
            }
        }
        else
        {
            CurrentOccupants.Remove(occupant);
            if (seat.IsDriverSeat)
            {
                DriverDisembark?.Invoke(this, EventArgs.Empty);
            }
            OccupantDisembark?.Invoke(this, seat);
        }
    }
    private void OnOccupantEmbarked(object sender, VehicleSeat e)
    {
        AvailableSeats.Remove(e);
    }
    private void OnOccupantDisembarked(object sender, VehicleSeat e)
    {
        AvailableSeats.Add(e);
    }
    #endregion
}
