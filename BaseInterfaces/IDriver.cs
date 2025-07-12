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
        public Vector3 GetDesiredDriveLoc();
        public bool WantsDrive();
    }
}
