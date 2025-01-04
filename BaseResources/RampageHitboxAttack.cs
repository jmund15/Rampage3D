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
    public OrthogDirection FaceDirection { get; private set; }

    public RampageHitboxAttack()
    {
        Damage = 0.0f;
        Force = 0.0f;
        Direction = Vector3.Zero;
        FaceDirection = OrthogDirection.DownRight;
    }
    public RampageHitboxAttack(float damage, float force, Vector3 direction)//, Array<AttackEffect>? attackEffects = null)
    {
        Damage = damage;
        Force = force;
        Direction = direction;
        //FaceDirection = IMovementComponent.GetDirectionFromVector(direction);

    }
    //public void SetNormForce(float minForce, float maxForce)
    //{
    //    NormForce = Global.NormalizeNumber(Force, minForce, maxForce);
    //}
}
