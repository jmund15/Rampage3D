using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class MonsterVelocity3DStrategy : CharacterBody3D, IVelocity3DComponent
{
    [ExportGroup("Velocity Properties")]
    [Export]
    protected float GroundSpeed { get; set; }
    [Export]
    protected float JumpForce { get; set; }
    [Export]
    protected float AirFallSpeed { get; set; }
    [Export]
    protected float AirMoveSpeed { get; set; }
    [Export]
    protected float ClimbSpeed { get; set; }
    //[Export]
    //public float Friction { get; private set; }
    //public float Acceleration { get; private set; }
    [Export]
    public float TurnSpeed { get; private set; }

    public Dictionary<VelocityType, float> VelocityMap { get; private set; } = new Dictionary<VelocityType, float>();
    //{ get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Dictionary<VelocityType, float> VelModMap { get; private set; } = new Dictionary<VelocityType, float>();
    //{ get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    [Export]
    public int JumpsAllowed { get; set; }
    public float MaxLandVelocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Node GetInterfaceNode()
    {
        return this;
    }

    //public Vector3 GetVelocity()
    //{
    //    return Velocity;
    //}
    public void CustomMove(Vector3 velocity)
    {
        Velocity = velocity;
        MoveAndSlide();
    }
    public void SetMovement(Vector3 direction, VelocityType moveVel)
    {
        Velocity = direction * (VelModMap[moveVel] + VelModMap[moveVel]);
        MoveAndSlide();
    }
    public void AppendVelocityMod(VelocityType velType, float mod)
    {
        if (!VelModMap.ContainsKey(velType))
        {
            VelModMap[velType] = mod;
        }
        VelModMap[velType] += mod;
    }
    public void SetVelocityMod(VelocityType velType, float mod)
    {
        VelModMap[velType] = mod;
    }
    public void AppendAllVelocityMods(float mod)
    {
        foreach (var modType in Global.GetEnumValues<VelocityType>())
        {
            if (!VelModMap.ContainsKey(modType))
            {
                VelModMap[modType] = mod;
            }
            VelModMap[modType] += mod;
        }
    }
    public void SetAllVelocityMods(float mod)
    {
        foreach (var modType in Global.GetEnumValues<VelocityType>())
        {
            VelModMap[modType] = mod;
        }
    }
    public Dictionary<VelocityType, float> GetAllTotalVelocities()
    {
        var totalVels = new Dictionary<VelocityType, float>();
        foreach (var velType in Global.GetEnumValues<VelocityType>())
        {
            if (VelocityMap.ContainsKey(velType))
            {
                totalVels[velType] = VelocityMap[velType];
                if (VelModMap.ContainsKey(velType))
                {
                    totalVels[velType] += VelModMap[velType];
                }
            }
            else if (VelModMap.ContainsKey(velType))
            {
                totalVels[velType] = VelModMap[velType];
            }
            else
            {
                totalVels[velType] = 0f;
            }
        }
        return totalVels;
    }

    public Dictionary<VelocityType, float> GetVelocityMap()
    {
        return VelocityMap;
    }

    public Dictionary<VelocityType, float> GetVelModMap()
    {
        return VelModMap;
    }

    public float GetBaseVelocityOfType(VelocityType type)
    {
        return GetVelocityMap()[type];
    }
    public float GetVelocityModID(VelocityType modType)
    {
        return GetVelModMap()[modType];
    }
    public float GetTotalVelocityID(VelocityType type)
    {
        return GetAllTotalVelocities()[type];
    }

    public void SetMovement(float delta, Vector3 direction, VelocityType velType)
    {
        throw new NotImplementedException();
    }

    public void ApplyGravity(float delta)
    {
        throw new NotImplementedException();
    }

    public void ApplyImpulse(Vector3 direction, ImpulseType forceType)
    {
        throw new NotImplementedException();
    }

    public void ApplyCustomVelocity(Vector3 velocity)
    {
        throw new NotImplementedException();
    }

    public void Move()
    {
        throw new NotImplementedException();
    }

    public void ResetVelocity()
    {
        throw new NotImplementedException();
    }

    public void ApplyCustomGravity(float delta, Vector3 customGravity, float weightPercentage = 0)
    {
        throw new NotImplementedException();
    }
}

