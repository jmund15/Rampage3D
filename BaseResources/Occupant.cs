using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public enum NpcType
{
    Critter,
    Police,
    Army,
    General
}

[GlobalClass, Tool]
public partial class Occupant : Resource
{
    [Export]
    public NpcType Type { get; set; }
    [Export]
    public PackedScene Scene { get; set; } // Scene to instantiate for this occupant type
    public Node NodeRef { get; set; } // Reference to the NPC node, if needed

    public Occupant()
    {
        Type = NpcType.Critter; // Default type
        Scene = null;
        NodeRef = null; // Default to null
    }
    public Occupant(NpcType type, PackedScene scene)
    {
        Type = type;
        Scene = scene;
        NodeRef = null; // Default to null
    }
    public Occupant(NpcType type, Node occupantRef)
    {
        Type = type;
        NodeRef = occupantRef;
    }
    //public void GenerateOccupant()
    //      {

    //      }
    // Override ToString for easier debugging
    public override string ToString()
    {
        return $"Occupant(Type: {Type}, Reference: {NodeRef?.Name ?? "null"})";
    }
}

