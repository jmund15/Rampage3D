using Godot;
using System;

[GlobalClass, Tool]
public partial class AIAffinitiesComponent : Node
{
    [Export]
    public float Fear { get; private set; }
    [Export]
    public float Pride { get; private set; } //Hubris?
    [Export]
    public float Anger { get; private set; }
    [Export]
    public float SingleMindedness { get; private set; } // affinity to change targets or become fixated?

    // MELEE VS. RANGE 
    // DO WE INSTEAD ASSERT THESE FROM OTHER EMOTIONS???
    // i.e. catious = ranged, bold = melee ?
    [Export]
    public float MeleeAffinity { get; private set; }
    [Export]
    public float RangeAffinity { get; private set; }

    [Export] //reckless vs catious
    public float Recklessness { get; private set; }


    [Export] //smart vs dumb
    public float Intelligence { get; private set; }

    [Export] //support vs solo (no collabing)
    public float Supportiveness { get; private set; }  
}
