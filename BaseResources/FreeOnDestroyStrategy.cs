using Godot;
using System;

[GlobalClass, Tool]
public partial class FreeOnDestroyStrategy : BreakableOnDestroyStrategy
{
    public FreeOnDestroyStrategy() : base()
    {
    }
    public override void Destroy()
    {
        if (Breakable == null) { throw new Exception("FreeOnDestroy ERROR || Breakable is null!"); }
        Breakable.QueueFree();
    }
}
