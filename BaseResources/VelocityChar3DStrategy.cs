using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class VelocityChar3DStrategy : Resource, IVelocity3DComponent
{
    [Export]
    public float Speed { get; private set; }
    [Export]
    public float Friction { get; private set; }
    public float Acceleration { get; private set; }
    [Export]
    public float TurnSpeed { get; private set; }
    [Export]
    public float JumpForce { get; private set; }

}

