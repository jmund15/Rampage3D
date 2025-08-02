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
    private OccupantComponent3D _occupant = null;
    public OccupantComponent3D Occupant
    {
        get => _occupant;
        set
        {
            if (_occupant != value)
            {
                var oldOccupant = _occupant;
                _occupant = value;
                if (Occupant == null)
                {
                    OccupancyChanged?.Invoke(this, new OccupancyChangedEventArgs(IsOccupied, oldOccupant));
                }
                else
                {
                    OccupancyChanged?.Invoke(this, new OccupancyChangedEventArgs(IsOccupied, Occupant));
                }
                AvailabilityChanged?.Invoke(this, Availability);
            }
        }
    }
    private bool _queuedForEntry = false;
    public bool QueuedForEntry
    {
        get => _queuedForEntry;
        set
        {
            if (_queuedForEntry != value)
            {
                _queuedForEntry = value;
                AvailabilityChanged?.Invoke(this, Availability);
            }
        }
    }
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

    public class OccupancyChangedEventArgs : EventArgs
    {
        public bool OccupantEntered { get; }
        public OccupantComponent3D Occupant { get; }

        public OccupancyChangedEventArgs(bool occEntered, OccupantComponent3D occupant)
        {
            OccupantEntered = occEntered;
            Occupant = occupant;
        }
    }
    public event EventHandler<SeatAvailability> AvailabilityChanged;
    public event EventHandler<OccupancyChangedEventArgs> OccupancyChanged;
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
    public VehicleSeat(bool isDriverSeat, Vector2 entrancePosition, OccupantComponent3D occupant)
    {
        IsDriverSeat = isDriverSeat;
        EntrancePosition = entrancePosition;
        Occupant = occupant;
        SeatIndColor = new Color(GD.Randf(), GD.Randf(), GD.Randf());
    }
}

