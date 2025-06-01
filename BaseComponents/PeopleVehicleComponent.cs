using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using static Godot.ClassDB;

[GlobalClass, Tool]
public partial class PeopleVehicleComponent : Node
{
    #region COMPONENT_VARIABLES
    [Export]
    public Godot.Collections.Dictionary<int, Vector3> VehicleDoorPositions { get; protected set; } = new Godot.Collections.Dictionary<int, Vector3>
    {
        { 0, new Vector3(0, 0, 0) }, // Default position for door 0
        { 1, new Vector3(1, 0, 0) }, // Default position for door 1
        { 2, new Vector3(-1, 0, 0) }, // Default position for door 2
        { 3, new Vector3(0, 0, -1) } // Default position for door 3
    };
    [Export]
    public int MaxOccupants { get; protected set; } = 5;
    [Export]
    public int MinOccupants { get; protected set; } = 1;
    [Export]
    public NpcType VehicleType { get; protected set; } = NpcType.Critter; // Default type for the vehicle

    //public Godot.Collections.Dictionary<NpcType, bool> AllowedOccupantTypesMap
    //{ get; protected set; } = new Godot.Collections.Dictionary<NpcType, bool>
    //{
    //    { NpcType.Critter, false },
    //    { NpcType.Police, false },
    //    { NpcType.Army, false },
    //    { NpcType.General, false }
    //};
    [Export]
    public bool RandomizeInitialOccupants { get; protected set; } = false;
    [Export]
    public Godot.Collections.Array<PackedScene> StaticInitialOccupants { get; protected set; } = new();
    public List<CharacterBody3D> Occupants { get; protected set; } = new();

    public event EventHandler<List<CharacterBody3D>> OccupantsChanges;
    public event EventHandler<CharacterBody3D> OccupantEmbarked;
    public event EventHandler<CharacterBody3D> OccupantDisembarked;
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
	{
		base._Ready();
        // Randomize initial occupants if enabled
        if (RandomizeInitialOccupants)
        {
            RandomizeOccupants();
        }
        else
        {
            InitializeStaticOccupants();
        }
    }
	public override void _Process(double delta)
	{
		base._Process(delta);
	}
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
	}
	#endregion
	#region COMPONENT_HELPER
    private void RandomizeOccupants()
    {
        int numInitOccupants = GD.RandRange(MinOccupants, MaxOccupants);
        switch (VehicleType)
        {
            case NpcType.Critter:
                bool sameSpecies;
                if (GD.RandRange(0, 1) == 1) { sameSpecies = true; }
                else { sameSpecies = false; }



                break;
        }
        


    }
    private void InitializeStaticOccupants()
    {
        foreach (var scene in StaticInitialOccupants)
        {
            var inst = scene.Instantiate<CharacterBody3D>();
            inst.Hide();
            AddChild(inst);
            Occupants.Add(inst);
            //if (AllowedOccupantTypesMap.ContainsKey(npcType) && AllowedOccupantTypesMap[npcType])
            //{
            // Create a new occupant and add it to the list
            //var occupant = new Occupant(npcType, null); // Reference can be set later
            //Occupants.Add(occupant);
            //}
        }
    }

    public void DisembarkAllOccupants()
    {

    }
    public void DisembarkOccupant(CharacterBody3D occupant)
    {
        if (occupant == null || !Occupants.Contains(occupant))
        {
            GD.Print("Occupant not found or invalid for disembarking.");
            return;
        }
        Occupants.Remove(occupant);
        //occupant.Reparent(Global.CurrentCity); // Remove from vehicle
        occupant.Show(); // Show the occupant after disembarking
        OccupantDisembarked?.Invoke(this, occupant);
    }

    public void EmbarkOccupant(CharacterBody3D occupant)
    {
        if (Occupants.Count >= MaxOccupants)
        {
            GD.Print("Cannot embark more occupants, maximum reached.");
            return;
        }
        if (occupant == null || !occupant.IsValid())
        {
            GD.Print("Invalid occupant provided for embarking.");
            return;
        }
        Occupants.Add(occupant);
        occupant.Reparent(this);
        occupant.Hide();
    }
    #endregion
    #region SIGNAL_LISTENERS
    #endregion
}
