using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[GlobalClass, Tool]
public partial class DriverBehavior : Resource
{
    [Export(PropertyHint.Range, "0.1,2.0,0.1,or_greater")]
    public float DriverAggression { get; set; } = 1.0f; // 1.0 = normal, >1 more aggressive, <1 more cautious
    [Export(PropertyHint.Range, "0.0,1.0,0.1")]
    public float DriverAwareness { get; private set; } = 0.8f; // 1.0 = perfect (no mistakes), 0.0 = oblivious

    // Add vehicle time to eject here? tie to Driver awareness?

    //[Export]
    //public bool WantsDrive { get; private set; } = true;

    public DriverBehavior()
    {
    }
    public DriverBehavior(float driverAggression, float driverAwareness)
    {
        DriverAggression = driverAggression;
        DriverAwareness = driverAwareness;
    }
}

