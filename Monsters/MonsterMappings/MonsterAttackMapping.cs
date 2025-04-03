using Godot;
using Godot.Collections;
using System;
using static System.Formats.Asn1.AsnWriter;
//using System.Collections.Generic;


public enum MonsterType
{
    Rambucho,
    Rufus,
    Gawkus,
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
// Records/Structs are more performant than Godot Resources (which are required to export)
// So we use resources for exported, and then pass to records/structs for program runtime
public record MonsterIdentifier(MonsterType Monster, MonsterForm Form);
public record MonsterAttackIdentifier(MonsterType Monster, MonsterForm Form, AttackType Attack);
public partial class MonsterAttackMapping : Node
{
    [Export]
    private bool _usePrebuilt = true;
    [Export]
    private Dictionary<MonsterAttackIDResource, PackedScene> _attackSceneMap =
        new Dictionary<MonsterAttackIDResource, PackedScene>()
        {
            //{ new MonsterAttackIDResource(MonsterType.Rambucho, MonsterForm.F2, AttackType.GroundNormal),
            //    "res://Monsters/MonsterAttacks/grizzle_2_gna.tscn"},
            //{ new MonsterAttackIDResource(MonsterType.Rambucho, MonsterForm.F2, AttackType.GroundSpecial),
            //    "res://Monsters/MonsterAttacks/grizzle_2_gsa.tscn"},
            //{ new MonsterAttackIDResource(MonsterType.Rambucho, MonsterForm.F2, AttackType.WallNormal),
            //    "res://Monsters/MonsterAttacks/grizzle_2_wna.tscn"},
            //{ new MonsterAttackIDResource(MonsterType.Rambucho, MonsterForm.F2, AttackType.WallSpecial),
            //    "res://Monsters/MonsterAttacks/grizzle_2_wsa.tscn"},
            
        };
    private System.Collections.Generic.Dictionary<MonsterAttackIdentifier, string> _attackScenePrebuiltMap =
        new System.Collections.Generic.Dictionary<MonsterAttackIdentifier, string>()
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
    public static System.Collections.Generic.Dictionary<MonsterAttackIdentifier, BehaviorTree> AttackTreeMap { get; private set; } =
        new System.Collections.Generic.Dictionary<MonsterAttackIdentifier, BehaviorTree>();

    public override void _Ready()
    {
        base._Ready();
        if (_usePrebuilt)
        {
            foreach (var maiScenePair in _attackScenePrebuiltMap)
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
        else
        {
            foreach (var maiScenePair in _attackSceneMap)
            {
                var mai = maiScenePair.Key;
                var scene = maiScenePair.Value;
                var tree = scene.Instantiate<BehaviorTree>();
                AddChild(tree);
                GD.Print($"INSTANTIATED TREE FOR {mai.ToString()}, TREE name: {tree.Name}");
                AttackTreeMap.Add(mai.GetMonsterAttackIdentifier(), tree);

            }
        }
        

    }
}
