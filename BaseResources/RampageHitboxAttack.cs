using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class RampageHitboxAttack : Resource
{
    [Export]
    public float Damage { get; private set; }
    [Export]
    public float Force { get; private set; }
    //public float NormForce { get; private set; }
    [Export]
    public Vector3 Direction { get; private set; }
    [Export]
    public Dir4 FaceDirection { get; private set; }
    [Export]
    public AttackBuildingEffect BuildingEffect { get; private set; }

    public RampageHitboxAttack()
    {
        Damage = 0.0f;
        Force = 0.0f;
        Direction = Vector3.Zero;
        FaceDirection = Dir4.Right;
        BuildingEffect = new AttackBuildingEffect();
    }
    public RampageHitboxAttack(float damage, float force, Vector3 direction, 
        AttackBuildingEffect buildingEffect)//, Array<AttackEffect>? attackEffects = null)
    {
        Damage = damage;
        Force = force;
        Direction = direction;
        BuildingEffect = buildingEffect;
        FaceDirection = direction.GetOrthogDirection();
    }
    //public void SetNormForce(float minForce, float maxForce)
    //{
    //    NormForce = Global.NormalizeNumber(Force, minForce, maxForce);
    //}
}
