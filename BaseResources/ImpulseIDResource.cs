using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ***** VELOCITY FORMULA *****
// MaxSpeed = Acceleration / Friction
// ****************************
[GlobalClass, Tool]
public partial class ImpulseIDResource : Resource
{
    [Export]
    public ImpulseType ImpulseType { get; private set; }
    //private float _maxSpeed;
    [Export]
    public float ImpulseForce { get; private set; }

    public ImpulseIDResource()
    {
        ImpulseType = ImpulseType.Jump;
        ImpulseForce = 0f;
    }
}

