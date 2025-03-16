using Godot;
using Godot.Collections;

[GlobalClass, Tool]
public partial class BreakablePiece : RigidBody3D
{
	#region COMPONENT_VARIABLES
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
    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        base._IntegrateForces(state);
		//GD.Print("pre rot: ", Rotation);
		Rotation = new Vector3(Rotation.X, -Mathf.Pi / 2, 0f);
		//GD.Print("post rot: ", Rotation);
    }
    #endregion
    #region COMPONENT_HELPER
    #endregion
    #region SIGNAL_LISTENERS
    #endregion
}
