using Godot;
using System.Collections.Generic;

[GlobalClass, Tool]
public partial class NpcGroupManager : Node
{
	#region COMPONENT_VARIABLES
    public HashSet<NPCGroup> NpcGroups { get; protected set; } = new();

    private Dictionary<Node, NPCGroup> _npcGroupMap = new();
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
	{
		base._Ready();
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
	public void RegisterGroup(NPCGroup group)
    {
        if (group == null) return;
        NpcGroups.Add(group);
        foreach (var npc in group.GroupMembers)
        {
            if (!_npcGroupMap.ContainsKey(npc))
            {
                _npcGroupMap[npc] = group;
            }
        }
    }
    public void UnregisterGroup(NPCGroup group)
    {
        if (group == null) return;
        NpcGroups.Remove(group);
        foreach (var npc in group.GroupMembers)
        {
            if (_npcGroupMap.ContainsKey(npc))
            {
                _npcGroupMap.Remove(npc);
            }
        }
    }
    public NPCGroup? GetGroup(Node npc)
    {
        if (_npcGroupMap.ContainsKey(npc))
        {
            return _npcGroupMap[npc];
        }
        return null;
    }
    #endregion
    #region SIGNAL_LISTENERS
    #endregion
}
