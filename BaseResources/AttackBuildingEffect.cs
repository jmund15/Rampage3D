using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[GlobalClass, Tool]
public partial class AttackBuildingEffect : Resource
{
    [Export] // 1 spillover rate means 100% of damage goes to each 
    public float SpilloverRate { get; private set; }
    [Export]
    public int FloorsEffected { get; private set; }
    public AttackBuildingEffect()
    {
        SpilloverRate = 0f;
        FloorsEffected = 0;

        RigidBody3D _rigBody;
        StaticBody3D _statBod;
        CharacterBody3D _charBod;

        //_charBod.
        //_statBod.Ax
        //_rigBody.Velo
    }
}
