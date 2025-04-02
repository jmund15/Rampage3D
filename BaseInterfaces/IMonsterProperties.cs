using Godot;
//using Godot.Collections;
using System;
using System.Collections.Generic;


public interface IMonsterProperties
{
    public Dictionary<VelocityType, float> VelocityMap { get; set; }
    public void SetVelocityMod(VelocityType velType, float mod);
    public int JumpsAllowed { get; set; }
}
