using Godot;
using System;
using System.Collections.Generic;

public enum MonsterType
{
    Rambucho,
    Candace,
    Viktor,
    Swindell
}
public enum MonsterForm
{
    F1,
    F2,
    F3
}
public record MonsterIdentifier(MonsterType Monster, MonsterForm Form);
public record MonsterAttackIdentifier(MonsterType Monster, MonsterForm Form, AttackType Attack);
public partial class MonsterAttackMapping : Node
{
    private Dictionary<MonsterAttackIdentifier, string> _attackSceneMap =
        new Dictionary<MonsterAttackIdentifier, string>()
        {
            { new MonsterAttackIdentifier(MonsterType.Rambucho, MonsterForm.F2, AttackType.GroundNormal),
                "res://Monsters/MonsterAttacks/grizzle_2_gna.tscn"},
            { new MonsterAttackIdentifier(MonsterType.Rambucho, MonsterForm.F2, AttackType.GroundSpecial),
                "res://Monsters/MonsterAttacks/grizzle_2_gsa.tscn"},
            { new MonsterAttackIdentifier(MonsterType.Rambucho, MonsterForm.F2, AttackType.WallNormal),
                "res://Monsters/MonsterAttacks/grizzle_2_wna.tscn"},
            { new MonsterAttackIdentifier(MonsterType.Rambucho, MonsterForm.F2, AttackType.WallSpecial),
                "res://Monsters/MonsterAttacks/grizzle_2_wsa.tscn"},
            
        };
    public static Dictionary<MonsterAttackIdentifier, BehaviorTree> AttackTreeMap { get; private set; } =
        new Dictionary<MonsterAttackIdentifier, BehaviorTree>();

    public override void _Ready()
    {
        base._Ready();
        foreach (var maiScenePair in _attackSceneMap)
        {
            var mai = maiScenePair.Key;
            var scenePath = maiScenePair.Value;

            if (!ResourceLoader.Exists(scenePath))
            {
                GD.Print($"attack tree for {mai.ToString()} does not exist!");
                continue;
            }
            var scene = ResourceLoader.Load<PackedScene>(scenePath);
            var tree = scene.Instantiate<BehaviorTree>();
            AddChild(tree);
            GD.Print($"INSTANTIATED TREE FOR {mai.ToString()}, TREE name: {tree.Name}");
            AttackTreeMap.Add(mai, tree);

        }

    }
}
