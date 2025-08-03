
using Godot;

/// <summary>
/// A component to be placed on any Node3D that should be specifically identifiable by AI sensors.
/// It holds the entity's type, making perception logic more abstract and modular.
/// </summary>
[GlobalClass]
public partial class DetectableComponent_gemini : Node
{
    [Export]
    public DetectableType Type { get; private set; }

    /// <summary>
    /// A reference to the parent Node3D this component is attached to.
    /// </summary>
    public Node3D OwnerNode { get; private set; }

    public override void _Ready()
    {
        OwnerNode = GetParent<Node3D>();
        if (OwnerNode == null)
        {
            GD.PrintErr("DetectableComponent_gemini must be a child of a Node3D.");
        }
    }
}
