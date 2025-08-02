using Godot;
using Godot.Collections;
using BaseInterfaces;
using System;

[GlobalClass, Tool]
public partial class OccupantComponent3D : Node3D, IDriver
{
    #region COMPONENT_VARIABLES
    public Vector3 DriveTargetLocation { get; private set; }
    public Vector3 DriveTargetRotation { get; private set; }

    private Node _originalDriverParent;
    private Node3D _driver;
    private AINav3DComponent _driverAI;

    [Export]
    public bool CanDrive { get; private set; } = true;
    [Export]
	public DriverBehavior DriverBehavior { get; private set; }
    public IVehicleComponent3D? VehicleComponent { get; set; }
    public VehicleOccupantsComponent? VehicleOccupantsComponent { get; set; }
    public VehicleSeat? OccupiedSeat { get; set; }


    public event EventHandler<bool> WantsDriveChanged;
    public event EventHandler<Vector3> DriveTargetLocationChanged;
    public event EventHandler<Vector3> DriveTargetRotationChanged;
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
	{
		base._Ready();
        _driver = GetOwner<Node3D>();
        _originalDriverParent = _driver.GetParent();
        _driverAI = _driver.GetFirstChildOfType<AINav3DComponent>();
    }
	public override void _Process(double delta)
	{
		base._Process(delta);
	}
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
	}
    public DriverBehavior GetDriverBehavior()
    {
        return DriverBehavior;
    }
    public IVehicleComponent3D GetVehicleComponent()
    {
        return VehicleComponent;
    }
    public VehicleOccupantsComponent GetVehicleOccupantsComponent()
    {
        return VehicleOccupantsComponent;
    }

    public Vector3 GetDriveTargetLocation()
    {
        return DriveTargetLocation;
    }
    public Vector3 GetDriveTargetRotation()
    {
        return DriveTargetRotation;
    }
    public void SetDriveTargetLocation(Vector3 targetPosition)
    {
        if (VehicleComponent != null)
        {
            DriveTargetLocation = targetPosition;
        }
    }
    public void SetDriveTargetRotation(Vector3 targetRotation)
    {
        DriveTargetRotation = targetRotation;
    }
    public bool WantsDrive()
    {
        throw new System.NotImplementedException();
    }
    #endregion
    #region COMPONENT_HELPER
    public bool EmbarkInVehicle(IVehicleComponent3D vehicle, VehicleSeat seat)
    {
        try
        {
            if (seat.IsOccupied)
            {
                if (seat.IsDriverSeat)
                {
                    GD.Print("Vehicle already has a driver.");
                    return false;
                }
                else
                {
                    GD.Print("Seat is already occupied.");
                    return false;
                }
            }
            if (!seat.VOccupantComp.CloseEnoughToEmbark(this, seat))
            {
                GD.Print("Occupant is too far from the vehicle to embark.");
                return false;
            }
            if (seat.IsDriverSeat)
            {
                if (!CanDrive)
                {
                    GD.Print("Occupant cannot drive this vehicle.");
                    return false;
                }

                // more logic?
            }


            
            _driver.Hide();
            _driver.Reparent(VehicleOccupantsComponent);
            _driverAI.DisableNavigation();
            VehicleComponent.SetDriverBehavior(DriverBehavior);
            VehicleComponent = vehicle;
            VehicleOccupantsComponent = seat.VOccupantComp;
            OccupiedSeat = seat;
            OccupiedSeat.Occupant = this;
            return true;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error during embarkation: {e.Message}");
            return false;
        }
    }
    public bool DisembarkFromVehicle()
    {
        // Global Pos check for if it's possible to disembark
        GlobalPosition = GlobalPosition + OccupiedSeat.EntrancePosition.GetVector3();

        OccupiedSeat.Occupant = null;
        VehicleComponent = null;
        VehicleOccupantsComponent = null;
        OccupiedSeat = null;
        _driver.Show();
        _driver.Reparent(_originalDriverParent);
        _driverAI.EnableNavigation();

        return true;
    }

    public VehicleSeat GetOccupiedSeat()
    {
        return OccupiedSeat;
    }

    #endregion
    #region SIGNAL_LISTENERS
    #endregion
}
