using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
// If Speed, Acceleration, and Friction are EQUAL,
// then acceleration to max speed is instant, and slowdown to zero is instant

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
    public void AppendAddVelocityMod(VelocityType velType, VelocityID mod);
    public void SetAddVelocityMod(VelocityType velType, VelocityID mod);
    public void AppendMultVelocityMod(VelocityType velType, VelocityID mod);
    public void SetMultVelocityMod(VelocityType velType, VelocityID mod);
    public void AppendAllAddVelocityMods(VelocityID mod);
    public void SetAllAddVelocityMods(VelocityID mod);
    public void AppendAllMultVelocityMods(VelocityID mod);
    public void SetAllMultVelocityMods(VelocityID mod);
}

