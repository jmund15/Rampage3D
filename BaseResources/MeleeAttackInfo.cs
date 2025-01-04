using Godot;
using System;
using Godot.Collections;
// MAKE SURE ALL RESOURCES HAVE THE "GlobalClass" and "Tool" conditions!
[GlobalClass, Tool] 
public partial class MeleeAttackInfo : Resource
{
    [Export]
    public string AnimName { get; private set; }
    //[Export] // This could be a forward momentum or backwards recoil during the attack
    //public Vector2 Velocity { get; private set; }
    [Export] // This could be a forward momentum or backwards recoil during the attack
    public float Velocity { get; private set; }
    [Export]
    public float Damage { get; private set; }
    [Export]
    public float Knockback { get; private set; }
    ////[Export]
    ////public Shape2D HitboxShape { get; private set; }
    ////[Export]
    ////public Vector2 HitboxLocationUp { get; private set; }
    ////[Export]
    ////public Vector2 HitboxLocationUpRight { get; private set; }
    ////[Export] // -y is up relative to the player
    ////public Vector2 HitboxLocationRight { get; private set; }
    ////[Export]
    ////public Vector2 HitboxLocationDownRight { get; private set; }
    ////[Export]
    ////public Vector2 HitboxLocationDown { get; private set; }
    //[Export]
    //public Vector2 HitboxActiveTime { get; private set; }

    public MeleeAttackInfo()
    {
        AnimName = "";
        Velocity = 0f;// Vector2.Zero;
        Damage = 0f;
        Knockback = 0f;
    }
}
