using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public enum VehicleGear
{
    Park,
    Neutral,
    Drive,
    //Reverse ?
}

namespace BaseInterfaces
{

    public interface IVehicleComponent3D
    {
        public VehicleGear Gear { get; set; }
        public bool SetDriveTargetLocation(Vector3 targetPosition);
        public bool SetDriveTargetRotation(Vector3 targetRotation);
        public void SetDriverBehavior(DriverBehavior driverBehavior);
        

        // TODO: decide to use this or instead integrate target position for braking if directly infront of the vehicle
        //public bool WantsBrake { get; set; }
        public DriverBehavior GetDriverBehavior();
        //public void SetDriveTargetLocation(Vector3 targetPosition);
        //public void SetDriveTargetRotation(Vector3 targetRotation);
        public Vector3 GetDriveTargetLocation();
        public Vector3 GetDriveTargetRotation();
        public bool IsParked { get; set; }

        public void Park();
        public void Drive();
        public void Drift();

        public event EventHandler<bool> ParkedStatusChanged;

        public Rid GetNavigationMap();
        public uint GetNavigationLayers();
        public Vector3 Position { get; set; }
        public Vector3 GlobalPosition { get; set; }
        public Node3D GetInterfaceNode();
    }
}
