using Godot;
using System;

public enum DirectionType
{
    EightDir,
    FourDir,
    UpDown,
    LeftRight
}

[GlobalClass, Tool]
public partial class AAAParameters : Resource
{
    [Export]
    public Godot.Collections.Dictionary<SpriteType, int> SpriteTypePixelMap { get; private set; } 
        = new Godot.Collections.Dictionary<SpriteType, int>()
    {
        { SpriteType.Monster, 32},
        { SpriteType.Critter, 10}
    };
    [Export]
    public Godot.Collections.Dictionary<string, Animation.LoopModeEnum> AnimLoopMap { get; private set; } 
        = new Godot.Collections.Dictionary<string, Animation.LoopModeEnum>()
    {
        { "idle", Animation.LoopModeEnum.Linear },
        { "walk", Animation.LoopModeEnum.Linear },
        { "run", Animation.LoopModeEnum.Linear },
        { "land", Animation.LoopModeEnum.None },
        { "land2", Animation.LoopModeEnum.None },
        { "jump", Animation.LoopModeEnum.None },
        { "punchStartup", Animation.LoopModeEnum.None },
        { "punch1", Animation.LoopModeEnum.None },
        { "punch2", Animation.LoopModeEnum.None },
        { "wallPunch", Animation.LoopModeEnum.None },
        { "wallKick", Animation.LoopModeEnum.None },
        { "lift", Animation.LoopModeEnum.None },
        { "grab", Animation.LoopModeEnum.None },
        { "eat", Animation.LoopModeEnum.None },
    };
    [Export(PropertyHint.SaveFile, "*.tscn")]
    public string SavePath { get; private set; }
    
    [Export]
    public SpriteType SpriteType { get; private set; }
    [Export]
    public Godot.Collections.Dictionary<string, int> BodyParts { get; private set; } = new Godot.Collections.Dictionary<string, int>()
    {
        { "body", 2 },
        { "pants", 2 },
        { "shirt", 2 },
        { "hat", 2 },
    };
    [Export]
    public Godot.Collections.Dictionary<string, Godot.Collections.Array<string>> PartConfigLabels { get; private set; } =
        new Godot.Collections.Dictionary<string, Godot.Collections.Array<string>>();

    //[Export]
    //public DirectionType DirType { get; private set } = DirectionType.UpDown;

    [Export]
    public Godot.Collections.Array<AAADirection> AnimDirections { get; private set; } = new Godot.Collections.Array<AAADirection>()
    {
        AAADirection.UP,
        AAADirection.DOWN,
    };

    public AAAParameters()
    {

    }
}
