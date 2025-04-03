using Godot;
using Godot.Collections;
using System;


[GlobalClass, Tool]
public partial class MonsterIDResource : Resource
{
    [Export]
    public MonsterType Monster { get; set; }
    [Export]
    public MonsterForm Form { get; set; }
    public MonsterIDResource()
    {
        Monster = MonsterType.Rambucho;
        Form = MonsterForm.F1;
    }
    public MonsterIDResource(MonsterType monster, MonsterForm form)
    {
        Monster = monster;
        Form = form;
    }
    public MonsterIdentifier GetMonsterIdentifier()
    {
        return new MonsterIdentifier(Monster, Form);
    }
}