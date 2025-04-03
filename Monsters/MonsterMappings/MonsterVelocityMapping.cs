using Godot;
using Godot.Collections;
using System;
//using System.Collections.Generic;
public partial class MonsterVelocityMapping : Node
{
    [Export]
    private Dictionary<MonsterIDResource, Char3DVelocityProperties> _velocityPropertyMap =
        new Dictionary<MonsterIDResource, Char3DVelocityProperties>()
        {
            //{ new MonsterIDResource(MonsterType.Rambucho, MonsterForm.F2),
            //    "res://Monsters/MonsterVelocityIDs/rambucho_med_velocity_props.tres"},
        };
    public static System.Collections.Generic.Dictionary<MonsterIdentifier, Char3DVelocityProperties> VelocityPropertyMap { get; private set; } =
        new System.Collections.Generic.Dictionary<MonsterIdentifier, Char3DVelocityProperties>();

    public override void _Ready()
    {
        base._Ready();
        foreach (var maiScenePair in _velocityPropertyMap)
        {
            var mvp = maiScenePair.Key;
            var velProps = maiScenePair.Value;
            VelocityPropertyMap.Add(mvp.GetMonsterIdentifier(), velProps);
        }
    }
}
