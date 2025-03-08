using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract partial class DistanceConsideration : Resource, IAIDistanceConsideration
{
    public DistanceConsideration()
    {

    }
    public abstract float GetDistanceConsideration(RayCast3D raycast);
}
