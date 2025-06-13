using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[GlobalClass, Tool]
public partial class VehicleSeat : Resource
{
    [Export]
    public bool IsDriverSeat { get; private set; }
    [Export]
    public Vector2 EntrancePosition { get; private set; }
    public Color SeatIndColor { get; set; }
    public Node Occupant { get; set; } = null;
    public bool IsOccupied => Occupant != null;

    public VehicleSeat()
    {
        IsDriverSeat = false;
        EntrancePosition = Vector2.Zero;
        Occupant = null;
        SeatIndColor = new Color(GD.Randf(), GD.Randf(), GD.Randf());
    }
    public VehicleSeat(bool isDriverSeat, Vector2 entrancePosition)
    {
        IsDriverSeat = isDriverSeat;
        EntrancePosition = entrancePosition;
        Occupant = null;
        SeatIndColor = new Color(GD.Randf(), GD.Randf(), GD.Randf());
    }
    public VehicleSeat(bool isDriverSeat, Vector2 entrancePosition, Node occupant)
    {
        IsDriverSeat = isDriverSeat;
        EntrancePosition = entrancePosition;
        Occupant = occupant;
        SeatIndColor = new Color(GD.Randf(), GD.Randf(), GD.Randf());
    }
}

