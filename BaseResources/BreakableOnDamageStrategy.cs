using Godot;
using System;

[GlobalClass, Tool]
public abstract partial class BreakableOnDamageStrategy : Resource
{
    public Node Breakable { get; set; }
    public IBlackboard BB { get; set; }
    public BreakableOnDamageStrategy()
    {
        Breakable = null;
        BB = null;
    }
    public abstract void Damage();
}
