using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum SeatAvailability
{
    Available,
    Occupied,
    QueuedForEntry
}

[GlobalClass, Tool]
public partial class VehicleSeat : Resource
{
    [Export]
    public bool IsDriverSeat { get; private set; }
    [Export]
    public Vector2 EntrancePosition { get; private set; }
    public Color SeatIndColor { get; set; }
    public VehicleOccupantsComponent VOccupantComp { get; set; } = null;
    public Node Occupant { get; set; } = null;
    public bool QueuedForEntry { get; set; }
    public bool IsOccupied => Occupant != null;
    public SeatAvailability Availability
    {
        get
        {
            if (QueuedForEntry)
                return SeatAvailability.QueuedForEntry;
            if (IsOccupied)
                return SeatAvailability.Occupied;
            return SeatAvailability.Available;
        }
    }

    public VehicleSeat()
    {
        IsDriverSeat = false;
        EntrancePosition = Vector2.Zero;
        Occupant = null;
        SeatIndColor = new Color(GD.Randf(), GD.Randf(), GD.Randf());
        QueuedForEntry = false;
    }
    public VehicleSeat(bool isDriverSeat, Vector2 entrancePosition)
    {
        IsDriverSeat = isDriverSeat;
        EntrancePosition = entrancePosition;
        Occupant = null;
        SeatIndColor = new Color(GD.Randf(), GD.Randf(), GD.Randf());
        QueuedForEntry = false;
    }
    public VehicleSeat(bool isDriverSeat, Vector2 entrancePosition, Node occupant)
    {
        IsDriverSeat = isDriverSeat;
        EntrancePosition = entrancePosition;
        Occupant = occupant;
        SeatIndColor = new Color(GD.Randf(), GD.Randf(), GD.Randf());
    }
}

