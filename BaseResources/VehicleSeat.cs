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
    public Vector3 EntrancePosition { get; private set; }
    public Node Occupant { get; set; } = null;
    public bool IsOccupied => Occupant != null;

    public VehicleSeat()
    {
        IsDriverSeat = false;
        EntrancePosition = Vector3.Zero;
        Occupant = null;    
    }
    public VehicleSeat(bool isDriverSeat, Vector3 entrancePosition)
    {
        IsDriverSeat = isDriverSeat;
        EntrancePosition = entrancePosition;
        Occupant = null;
    }
    public VehicleSeat(bool isDriverSeat, Vector3 entrancePosition, Node occupant)
    {
        IsDriverSeat = isDriverSeat;
        EntrancePosition = entrancePosition;
        Occupant = occupant;
    }
}

