using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public enum VelocityType
{
    Ground,
    GroundRun,
    Air,
    Swim,
    Climb,
}
// TODO: in the future, force has its own itnerface/class with the 'GetForce' function
// (in case of randomly generated forces), maybe do the same with the 'VelocityID' struct
public enum ImpulseType // TODO: remove if unnecessary?
{
    Jump,
    WallJump,
    Glide
}
public interface IVelocity3DComponent//<VelocityT> // i.e. <MonsterVelocityType>, etc.
{
    public Vector3 GetVelocity();

    public void AppendAddMod(float mod);
    public void AppendMultMod(float mod);
    public void SetAddMod(float mod);
    public void SetMultMod(float mod);
    //public bool AxisLockLinearX { get; set; }
    //public bool AxisLockLinearY { get; set; }
    //public bool AxisLockLinearZ { get; set; }
    //public bool AxisLockAngularX { get; set; }
    //public bool AxisLockAngularY { get; set; }
    //public bool AxisLockAngularZ { get; set; }
    //public KinematicCollision3D MoveAndCollide();


    // /// <returns>Void, only appends velocity.</returns>
    // /// <exception cref="ExceptionType">No exception throws.</exception>
    /// <summary>
    /// Appends gravity to the body's velocity
    /// </summary>
    /// <param name="delta">Time since last frame</param>
    /// <remarks>
    /// Call prior to any "BasicMove" function calls,
    /// to add gravity to the move calculation.
    /// </remarks>
    /// <example>
    /// <code>
    /// SetMovement(direction, VelocityType.Air);
    /// ApplyGravity((float)delta);
    /// Move();
    /// </code>
    /// </example>
    public void ApplyGravity(float delta);
    public void ApplyCustomGravity(float delta, Vector3 customGravity, float weightPercentage = 0f);
    public void ApplyCustomVelocity(Vector3 velocity);
    //public void ApplyVerticalMovement();
    public void Move();
    public void CustomMove(Vector3 velocity);
    public void ResetVelocity();
    public bool TestMove(Transform3D from, Vector3 motion, KinematicCollision3D collision = default, float safeMargin = 0.001f, bool recoveryAsCollision = false, int maxCollisions = 1);
    public Node GetInterfaceNode();
}

