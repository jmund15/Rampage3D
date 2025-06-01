using Godot;
using Godot.Collections;
using System;
//using System.Collections.Generic;
public partial class MonsterVelocityMapping : Node
{
    [Export]
    private bool _usePrebuilt = false;
    [Export]
    private Dictionary<MonsterIDResource, Char3DVelocityProperties> _velocityPropertyMap =
        new Dictionary<MonsterIDResource, Char3DVelocityProperties>()
        {
            //{ new MonsterIDResource(MonsterType.Rambucho, MonsterForm.F2),
            //    "res://Monsters/MonsterVelocityIDs/rambucho_med_velocity_props.tres"},
        };
    private System.Collections.Generic.Dictionary<MonsterIdentifier, string> _velocityPropertyPrebuiltMap =
        new System.Collections.Generic.Dictionary<MonsterIdentifier, string>()
        {
            { new MonsterIdentifier(MonsterType.Rambucho, MonsterForm.F2),
                "res://Monsters/MonsterVelocityIDs/rufus_velocity_med.tres" }//"res://Monsters/MonsterVelocityIDs/rambucho_med_velocity_props.tres"},
        };
    public static System.Collections.Generic.Dictionary<MonsterIdentifier, Char3DVelocityProperties> VelocityPropertyMap { get; private set; } =
        new System.Collections.Generic.Dictionary<MonsterIdentifier, Char3DVelocityProperties>();

    public override void _Ready()
    {
        base._Ready();
        if (_usePrebuilt)
        {
            foreach (var mvpScenePair in _velocityPropertyPrebuiltMap)
            {
                var mvp = mvpScenePair.Key;
                var resourcePath = mvpScenePair.Value;

                if (!ResourceLoader.Exists(resourcePath))
                {
                    GD.Print($"attack tree for {mvp.ToString()} does not exist!");
                    continue;
                }
                var velProps = ResourceLoader.Load<Char3DVelocityProperties>(resourcePath);
                VelocityPropertyMap.Add(mvp, velProps);
            }
        }
        else
        {
            foreach (var mvpScenePair in _velocityPropertyMap)
            {
                var mvp = mvpScenePair.Key;
                var velProps = mvpScenePair.Value;
                VelocityPropertyMap.Add(mvp.GetMonsterIdentifier(), velProps);
            }
        }
    }
}
