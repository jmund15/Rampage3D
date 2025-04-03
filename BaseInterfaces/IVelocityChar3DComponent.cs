using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
// If Speed, Acceleration, and Friction are EQUAL,
// then acceleration to max speed is instant, and slowdown to zero is instant

// TODO: ADD VELOCITY TYPE TO PROPERTIES?
public partial struct VelocityID
{
    //[Export]
    public float MaxSpeed { get; set; }
    //[Export]
    public float Acceleration { get; set; }
    //[Export]
    public float Friction { get; set; }
    public float BrakeFrictionMod { get; set; }
    //[Export]
    public bool InstantMaxSpeed { get; set; }
    public VelocityID()
    {
        MaxSpeed = 0;
        Acceleration = 0;
        Friction = 0;
        BrakeFrictionMod = 1;
        InstantMaxSpeed = false;
    }
    public VelocityID(float maxSpeed, float acceleration, float friction, float brakeFrictMod = 1)
    {
        MaxSpeed = maxSpeed;
        Acceleration = acceleration;
        Friction = friction;
        BrakeFrictionMod = brakeFrictMod;
        InstantMaxSpeed = false;
    }
    public VelocityID(float maxSpeed/*, bool instantMovement = true*/)
    {
        MaxSpeed = maxSpeed;
        //if (instantMovement)
        //{
        Acceleration = -1;
        Friction = -1;
        BrakeFrictionMod = -1;
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
            a.Friction + b.Friction,
            a.BrakeFrictionMod//(a.BrakeFrictionMod + b.BrakeFrictionMod) / 2
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
            a.Friction * b.Friction,
            a.BrakeFrictionMod * b.BrakeFrictionMod
        );
    }
    public static VelocityID operator *(VelocityID a, float mod)
    {
        if (a.InstantMaxSpeed)
        {
            return new VelocityID(a.MaxSpeed * mod);
        }
        return new VelocityID(
            a.MaxSpeed * mod,
            a.Acceleration * mod,
            a.Friction * mod,
            a.BrakeFrictionMod
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
        return $"(MaxSpeed: {MaxSpeed}, Acceleration: {Acceleration}, Friction: {Friction}, BrakeFrictMod: {BrakeFrictionMod}); InstantMaxSpeed: {InstantMaxSpeed}";
    }
}
public interface IVelocityChar3DComponent : IVelocity3DComponent//<VelocityT> // i.e. <MonsterVelocityType>, etc.
{
    public int JumpsAllowed { get; set; }
    public float MaxLandVelocity { get; set; }
    //public void ApplyXMovement(float delta, float force, bool useFriction = true);
    //public void ApplyYMovement(float delta, float force, bool useFriction = true);
    //public void ApplyZMovement(float delta, float force, bool useFriction = true);
    public void SetMovement(float delta, Vector3 direction, VelocityType velType, bool useYFriction = false);
    public void ApplyImpulse(Vector3 direction, ImpulseType forceType);
    public Dictionary<VelocityType, VelocityID> GetBaseVelocityMap();
    public Dictionary<VelocityType, VelocityID> GetVelAddModMap();
    public Dictionary<VelocityType, VelocityID> GetVelMultModMap();
    public Dictionary<VelocityType, VelocityID> GetAllTotalVelocities();
    public VelocityID GetBaseVelocityID(VelocityType type);
    public VelocityID GetVelocityAddModID(VelocityType modType);
    public VelocityID GetVelocityMultModID(VelocityType modType);
    public VelocityID GetTotalVelocityID(VelocityType type);
    public void AppendAddVelocityIDMod(VelocityType velType, VelocityID mod);
    public void SetAddVelocityIDMod(VelocityType velType, VelocityID mod);
    public void AppendMultVelocityIDMod(VelocityType velType, VelocityID mod);
    public void SetMultVelocityIDMod(VelocityType velType, VelocityID mod);
    public void AppendAllAddVelocityIDMods(VelocityID mod);
    public void SetAllAddVelocityIDMods(VelocityID mod);
    public void AppendAllMultVelocityIDMods(VelocityID mod);
    public void SetAllMultVelocityIDMods(VelocityID mod);
}

