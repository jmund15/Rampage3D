using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
namespace BaseInterfaces
{
    public interface IDriver
    {
        public DriverBehavior GetDriverBehavior();
        public IVehicleComponent3D? GetVehicleComponent();
        public VehicleOccupantsComponent? GetVehicleOccupantsComponent();
        public VehicleSeat? GetOccupiedSeat();
        public void SetDriveTargetLocation(Vector3 targetPosition);
        public void SetDriveTargetRotation(Vector3 targetRotation);
        public Vector3 GetDriveTargetLocation();
        public Vector3 GetDriveTargetRotation();
        public bool WantsDrive();

        public event EventHandler<bool> WantsDriveChanged;
        public event EventHandler<Vector3> DriveTargetLocationChanged;
        public event EventHandler<Vector3> DriveTargetRotationChanged;
    }
}
