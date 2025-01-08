using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
