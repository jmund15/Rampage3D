using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
public interface IVelocity3DComponent
{
    //public Vector3 Velocity { get; set; }
    public void SetDesiredVelocityAndMove(bool slide = true);
    public Vector3 GetVelocity();
    //public bool AxisLockLinearX { get; set; }
    //public bool AxisLockLinearY { get; set; }
    //public bool AxisLockLinearZ { get; set; }
    //public bool AxisLockAngularX { get; set; }
    //public bool AxisLockAngularY { get; set; }
    //public bool AxisLockAngularZ { get; set; }
    public void ApplyJump();
    public void SetRunning();
    public void SetWalking();
    public void Move();
    public float GetVelMult();
    public bool TestMove(Transform3D from, Vector3 motion, KinematicCollision3D collision = default, float safeMargin = 0.001f, bool recoveryAsCollision = false, int maxCollisions = 1);

    public Node GetInterfaceNode();
}
