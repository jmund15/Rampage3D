using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IAIDistanceConsideration
{
    public float GetDistanceConsideration(RayCast3D raycast);
}
