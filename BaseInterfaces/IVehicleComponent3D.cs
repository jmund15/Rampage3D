using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BaseInterfaces
{
    public interface IVehicleComponent3D
    {
        public DriverBehavior GetDriverBehavior();
        public void SetDriveTargetLocation(Vector3 targetPosition);
        public void SetDriveTargetRotation(Vector3 targetRotation);
        public Vector3 GetDriveTargetLocation();
        public Vector3 GetDriveTargetRotation();
        public bool IsParked { get; set; }

        public void Park();
        public void Drive();
        public void Drift();

        public event EventHandler<bool> ParkedStatusChanged;

        public Vector3 Position { get; set; }
        public Vector3 GlobalPosition { get; set; }
    }
}
