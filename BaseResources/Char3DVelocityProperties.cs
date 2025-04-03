using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ***** VELOCITY FORMULA *****
// MaxSpeed = Acceleration / Friction
// ****************************
[GlobalClass, Tool]
public partial class Char3DVelocityProperties : Resource
{
    [Export]
    public Array<ImpulseIDResource> ImpulseIds { get; private set; }
    [Export]
    public Array<VelocityIDResource> VelocityIds { get; private set; }

    public Char3DVelocityProperties()
    {
        ImpulseIds = new Array<ImpulseIDResource>();
        VelocityIds = new Array<VelocityIDResource>();
    }
    public Char3DVelocityProperties(
        Array<ImpulseIDResource> impulseIds, 
        Array<VelocityIDResource> velocityIds)
    {
        ImpulseIds = impulseIds;
        VelocityIds = velocityIds;
    }
    public Char3DVelocityProperties(
        System.Collections.Generic.List<ImpulseIDResource> impulseIds,
        System.Collections.Generic.List<VelocityIDResource> velocityIds)
    {
        ImpulseIds = new Array<ImpulseIDResource>(impulseIds);
        VelocityIds = new Array<VelocityIDResource>(velocityIds);
    }
}

