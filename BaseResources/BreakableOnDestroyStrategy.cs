using Godot;
using System;

[GlobalClass, Tool]
public abstract partial class BreakableOnDestroyStrategy : Resource
{
    public Node Breakable { get; set; }
    public IBlackboard BB { get; set; }
    public BreakableOnDestroyStrategy()
    {
        Breakable = null;
        BB = null;
    }
    public abstract void Destroy();
}
