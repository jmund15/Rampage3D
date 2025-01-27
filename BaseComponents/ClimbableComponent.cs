using Godot;
using System;
using System.Collections.Generic;

[GlobalClass, Tool]
public partial class ClimbableComponent : Node
{
    private Node3D _structure;
    [Export]
    private BuildingComponent _buildingComp;
    public RoofComponent RoofComp { get; private set; }
 //   [Export]
	//private CollisionShape3D _collShape;
	//private BoxShape3D _boxShape;
    [Export]
    public bool CanClimbOnTop { get; private set; } = true;
    [Export]
    public float ClimbOnLedgeDist { get; private set; } = 0.5f;
    public float MaxClimbHeight;
	public Dictionary<OrthogDirection, Vector2> XZPositionMap { get; private set; } 
        = new Dictionary<OrthogDirection, Vector2>();
    public Dictionary<OrthogDirection, Vector3> ClimbOnPosMap { get; private set; } 
        = new Dictionary<OrthogDirection, Vector3>();

    public event EventHandler EjectClimbers;
    public override void _Ready()
	{
        _structure = GetOwner<Node3D>();
        RoofComp = _structure.GetFirstChildOfType<RoofComponent>();

        var structPos = _structure.GlobalPosition;


		var uLPos = new Vector2(structPos.X + _buildingComp.Dimensions.X, structPos.Z);
        var uRPos = new Vector2(structPos.X, structPos.Z + _buildingComp.Dimensions.Z);
        var dLPos = new Vector2(structPos.X, structPos.Z - _buildingComp.Dimensions.Z);
        var dRPos = new Vector2(structPos.X - _buildingComp.Dimensions.X, structPos.Z); 
		XZPositionMap.Add(OrthogDirection.UpLeft, uLPos);
        XZPositionMap.Add(OrthogDirection.UpRight, uRPos);
        XZPositionMap.Add(OrthogDirection.DownLeft, dLPos);
        XZPositionMap.Add(OrthogDirection.DownRight, dRPos);

        MaxClimbHeight = _buildingComp.Dimensions.Y; //+ structPos.Y 

        
        //if (CanClimbOnTop)
        //{
        //    var centerTopPoint = new Vector3
        //        (
        //        _structure.Position.X /*+ (_boxShape.Size.X / 2)) * _structure.Scale.X*/,
        //        MaxClimbHeight,
        //        _structure.Position.Z /*+ (_boxShape.Size.Z / 2)) * _structure.Scale.Z*/
        //        );


        //    var uLClimbPos = new Vector3(structPos.X + _buildingComp.Dimensions.X - ClimbOnLedgeDist, MaxClimbHeight, structPos.Z);
        //    var uRClimbPos = new Vector3(structPos.X, MaxClimbHeight, structPos.Z + _buildingComp.Dimensions.Z - ClimbOnLedgeDist);
        //    var dLClimbPos = new Vector3(structPos.X, MaxClimbHeight, structPos.Z - _buildingComp.Dimensions.Z + ClimbOnLedgeDist);
        //    var dRClimbPos = new Vector3(structPos.X - _buildingComp.Dimensions.X + ClimbOnLedgeDist, MaxClimbHeight, structPos.Z);

        //    //GD.Print("cener p: ", centerTopPoint, "\nUl climb pos: ", uLPos, "\nUL climb on p: ", uLClimbPos);
        //    ClimbOnPosMap.Add(OrthogDirection.UpLeft, uLClimbPos);
        //    ClimbOnPosMap.Add(OrthogDirection.UpRight, uRClimbPos);
        //    ClimbOnPosMap.Add(OrthogDirection.DownLeft, dLClimbPos);
        //    ClimbOnPosMap.Add(OrthogDirection.DownRight, dRClimbPos);
        //}

        _buildingComp.BuildingDestroyed += OnClimbableDestroyed;
    }

    public override void _Process(double delta)
	{
	}
    private void OnClimbableDestroyed(object sender, EventArgs e)
    {
        EjectClimbers?.Invoke(this, EventArgs.Empty);
        GD.Print("EJECT CLIMBERS INVOKED");
    }
}
