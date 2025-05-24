using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IAIDetector3D
{
    IEnumerable<Node3D> GetDetectedBodies();
    IEnumerable<Vector3> GetDirectionsDetected();
}
