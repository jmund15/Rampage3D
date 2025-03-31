using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public partial struct VelocityID
{
    //[Export]
    public float MaxSpeed { get; set; }
    //[Export]
    public float Acceleration { get; set; }
    //[Export]
    public float Friction { get; set; }
    //[Export]
    public bool InstantMaxSpeed { get; set; }
    public VelocityID()
    {
        MaxSpeed = 0;
        Acceleration = 0;
        Friction = 0;
        InstantMaxSpeed = false;
    }
    public VelocityID(float maxSpeed, float acceleration, float friction)
    {
        MaxSpeed = maxSpeed;
        Acceleration = acceleration;
        Friction = friction;
        InstantMaxSpeed = false;
    }
    public VelocityID(float maxSpeed/*, bool instantMovement = true*/)
    {
        MaxSpeed = maxSpeed;
        //if (instantMovement)
        //{
        Acceleration = -1;
        Friction = -1;
        //}
        //else
        //{
        //    Acceleration = MaxSpeed;
        //    Friction = MaxSpeed;
        //}
        InstantMaxSpeed = true;
    }

    // TODO: define edge case for 'InstantMaxSpeed' IDs
    public static VelocityID operator +(VelocityID a, VelocityID b)
    {
        if (a.InstantMaxSpeed)
        {
            return new VelocityID(a.MaxSpeed + b.MaxSpeed);
        }
        return new VelocityID(
            a.MaxSpeed + b.MaxSpeed,
            a.Acceleration + b.Acceleration,
            a.Friction + b.Friction
        );
    }
    public static VelocityID operator *(VelocityID a, VelocityID b)
    {
        if (a.InstantMaxSpeed)
        {
            return new VelocityID(a.MaxSpeed * b.MaxSpeed);
        }
        return new VelocityID(
            a.MaxSpeed * b.MaxSpeed,
            a.Acceleration * b.Acceleration,
            a.Friction * b.Friction
        );
    }

    // Choose a value appropriate for your scale and precision needs
    private const float CustomEpsilon = 0.0001f; // Adjust based on your needs
    // Overload the == operator
    public static bool operator ==(VelocityID a, VelocityID b)
    {
        return Math.Abs(a.MaxSpeed - b.MaxSpeed) < CustomEpsilon &&
                Math.Abs(a.Acceleration - b.Acceleration) < CustomEpsilon &&
                Math.Abs(a.Friction - b.Friction) < CustomEpsilon;
    }

    // When you overload ==, you must also overload !=
    public static bool operator !=(VelocityID a, VelocityID b)
    {
        return !(a == b);
    }

    // When overloading equality operators, you should also override Equals and GetHashCode
    public override bool Equals(object obj)
    {
        if (obj is VelocityID other)
        {
            return this == other;
        }
        return false;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(MaxSpeed, Acceleration, Friction);
    }

    // ToString for debugging
    public override string ToString()
    {
        return $"(MaxSpeed: {MaxSpeed}, Acceleration: {Acceleration}, Friction: {Friction}); InstantMaxSpeed: {InstantMaxSpeed}";
    }
}
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

