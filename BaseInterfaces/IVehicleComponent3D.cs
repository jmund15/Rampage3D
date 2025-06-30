using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


namespace TimeRobbers.BaseInterfaces
{
    public interface IVehicleComponent3D
    {
        public DriverBehavior GetDriverBehavior();
        public void SetDriverBehavior(DriverBehavior driverBehavior);
        public bool IsParked { get; set; }

        public void Park();
        public void Drive();
        public void Drift();

        public event EventHandler<bool> ParkedStatusChanged;

        public Vector3 Position { get; set; }
        public Vector3 GlobalPosition { get; set; }
    }
}
