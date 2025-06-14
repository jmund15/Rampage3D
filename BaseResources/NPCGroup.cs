using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum GroupLoyalty
{
    Neutral,
    Tight,
    Loose,
}

//[GlobalClass, Tool]
public partial class NPCGroup //: Resource
{
    //[Export]
    public Godot.Collections.Array<Node> GroupMembers { get; set; } = new();
    //[Export]
    public GroupLoyalty Loyalty { get; set; } = GroupLoyalty.Neutral;
    public int GroupSize => GroupMembers.Count;

    public NPCGroup(Godot.Collections.Array<Node> groupMembers, GroupLoyalty groupLoyalty)
    {
        GroupMembers = groupMembers;
        Loyalty = groupLoyalty;
    }
    public NPCGroup()
    {
        GroupMembers = new();
        Loyalty = GroupLoyalty.Neutral;
    }
}

