using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
