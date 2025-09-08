// CharacterBodyController3D.cs
using Godot;
using Jmo.Core.Movement;

namespace Jmo.Gameplay.Movement;

/// <summary>
/// An adapter that implements the ICharacterController3D interface
/// for a standard Godot CharacterBody3D node.
/// </summary>
public class CharacterBodyController3D : ICharacterController3D
{
    private readonly CharacterBody3D _body;

    public Node GetInterfaceNode() => _body;
    public Vector3 GlobalPosition => _body.GlobalPosition;
    public Vector3 Velocity => _body.Velocity;
    public bool IsOnFloor => _body.IsOnFloor();

    public CharacterBodyController3D(CharacterBody3D body)
    {
        _body = body;
    }

    public void SetVelocity(Vector3 newVelocity)
    {
        _body.Velocity = newVelocity;
    }

    public void AddVelocity(Vector3 additiveVelocity)
    {
        _body.Velocity += additiveVelocity;
    }

    public void Move()
    {
        _body.MoveAndSlide();
    }

    public void Teleport(Vector3 newGlobalPosition)
    {
        _body.GlobalPosition = newGlobalPosition;
    }
}