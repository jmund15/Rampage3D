using Godot;
using System;
using System.Collections.Generic;
public partial class AIAreaDetector3D : Area3D, IAIDetector3D
{
    public IEnumerable<Node3D> GetDetectedBodies()
    {
        foreach (var body in GetOverlappingBodies())
        {
            if (body is Node3D node)
                yield return node;
        }
    }
    public IEnumerable<Vector3> GetDirectionsDetected()
    {
        // For area, return directions to each detected body
        foreach (var body in GetOverlappingBodies())
        {
            if (body is Node3D node)
                yield return (node.GlobalPosition - GlobalPosition).Normalized();
        }
    }
}
