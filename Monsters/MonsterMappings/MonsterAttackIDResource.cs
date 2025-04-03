using Godot;
using Godot.Collections;
using System;

[GlobalClass, Tool]
public partial class MonsterAttackIDResource : MonsterIDResource
{
    [Export]
    public AttackType Attack { get; set; } 
    public MonsterAttackIDResource() : base()
    {
        Attack = AttackType.GroundNormal;
    }
    public MonsterAttackIDResource(MonsterType monster, MonsterForm form, AttackType attack) 
        : base(monster, form)
    {
        Attack = attack;
    }
    public MonsterAttackIdentifier GetMonsterAttackIdentifier()
    {
        return new MonsterAttackIdentifier(Monster, Form, Attack);
    }
}