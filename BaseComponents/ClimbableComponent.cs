using Godot;
using System;
using System.Collections.Generic;

[GlobalClass, Tool]
public partial class ClimbableComponent : Node
{
    private BuildingComponent _buildingComp;
    public RoofComponent RoofComp { get; private set; }
 //   [Export]
	//private CollisionShape3D _collShape;
	//private BoxShape3D _boxShape;
    [Export]
    public bool CanClimbOnTop { get; private set; } = true;
    public float ClimbOnFrontBuffer { get; private set; } = 1f;
    public float ClimbOnBackBuffer { get; private set; } = 0.75f;
    public float MaxClimbHeight;
	public Dictionary<Dir4, List<Vector2>> ClimbOnPosMap { get; private set; } 
        = new Dictionary<Dir4, List<Vector2>>();
    public List<Vector2> UpLeftClimbOnPoses { get; private set; }
        = new List<Vector2>();
    public List<Vector2> UpRightClimbOnPoses { get; private set; }
        = new List<Vector2>();
    public List<Vector2> DownLeftClimbOnPoses { get; private set; }
        = new List<Vector2>();
    public List<Vector2> DownRightClimbOnPoses { get; private set; }
        = new List<Vector2>();

    public event EventHandler EjectClimbers;
    public override void _Ready()
	{
        _buildingComp = GetOwner<BuildingComponent>();
        if (_buildingComp == null )
        {
            GD.PrintErr("ERROR || Climbable Component Needs a buildng comp as a parent!");
        }
        RoofComp = _buildingComp.GetFirstChildOfType<RoofComponent>();

        CallDeferred(MethodName.InitializeClimbPosMap);

        
        //if (CanClimbOnTop)
        //{
        //    var centerTopPoint = new Vector3
        //        (
        //        _buildingComp.Position.X /*+ (_boxShape.Size.X / 2)) * _buildingComp.Scale.X*/,
        //        MaxClimbHeight,
        //        _buildingComp.Position.Z /*+ (_boxShape.Size.Z / 2)) * _buildingComp.Scale.Z*/
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
    //public Vector2 GetClosestClimbablePos(Vector2 pos)
    //{

    //}
    public Vector2 GetClosestClimbablePosOfDir(Vector2 xzPos, Dir4 dir)
    {
        var closestDist = float.MaxValue;
        Vector2 closestPos = Vector2.Zero;

        //GD.Print("startign climbing at dir: ", dir,
        //    "\nbody pos: ", xzPos,
        //    "\nclamp pos options:");

        foreach (var climbOnPos in ClimbOnPosMap[dir])
        {
            //GD.Print("\t", climbOnPos);
            var dist = xzPos.DistanceTo(climbOnPos);
            if (dist <= closestDist)
            {
                closestDist = dist;
                closestPos = climbOnPos;
            }
        }
        return closestPos;
    }
    public Vector2 GetClosestClimbablePosOfDir(Vector3 pos, Dir4 dir)
    {
        Vector2 xzPos = new Vector2(pos.X, pos.Z);
        var closestDist = float.MaxValue;
        Vector2 closestPos = Vector2.Zero;

        //GD.Print("startign climbing at dir: ", dir,
        //    "\nbody pos: ", xzPos,
        //    "\nclamp pos options:");

        foreach (var climbOnPos in ClimbOnPosMap[dir])
        {
            GD.Print("\t", climbOnPos);
            var dist = xzPos.DistanceTo(climbOnPos);
            if (dist <= closestDist)
            {
                closestDist = dist;
                closestPos = climbOnPos;
            }
        }
        return closestPos;
    }
    private void InitializeClimbPosMap()
    {
        //GD.Print("INITIALIZING CLIMB POSES FOR: ", _buildingComp.Name);
        var structPos = _buildingComp.GlobalPosition;

        foreach (var frontFacePos in _buildingComp.XFacePoses)
        {
            UpRightClimbOnPoses.Add(new Vector2(frontFacePos.X, frontFacePos.Y + ClimbOnFrontBuffer));
            var backFacePos = new Vector2(frontFacePos.X, 
                frontFacePos.Y - _buildingComp.Dimensions.Z - ClimbOnBackBuffer);
            DownLeftClimbOnPoses.Add(backFacePos);
            var ownerName = GetOwner().Name;
            //if (ownerName == "Testing")
            //{
            //    GD.Print("Up right climb pos: ", frontFacePos, "\nDown Left climb pos: ", backFacePos);
            //}
            //GD.Print("Up right climb pos: ", frontFacePos, "\nDown Left climb pos: ", backFacePos);
        }
        foreach (var frontFacePos in _buildingComp.ZFacePoses)
        {
            UpLeftClimbOnPoses.Add(new Vector2(frontFacePos.X + ClimbOnFrontBuffer, frontFacePos.Y));
            var backFacePos = new Vector2(frontFacePos.X - _buildingComp.Dimensions.X - ClimbOnBackBuffer,
                frontFacePos.Y);
            DownRightClimbOnPoses.Add(backFacePos);
            var ownerName = GetOwner().Name;
            //if (ownerName == "Testing")
            //{
            //    GD.Print("UP left climb pos: ", frontFacePos, "\nDown Right climb pos: ", backFacePos);
            //}
            //GD.Print("UP left climb pos: ", frontFacePos, "\nDown Right climb pos: ", backFacePos);
        }

        ClimbOnPosMap.Add(Dir4.Left, UpLeftClimbOnPoses);
        ClimbOnPosMap.Add(Dir4.Up, UpRightClimbOnPoses);
        ClimbOnPosMap.Add(Dir4.Down, DownLeftClimbOnPoses);
        ClimbOnPosMap.Add(Dir4.Right, DownRightClimbOnPoses);

        //var uLPos = new Vector2(structPos.X + _buildingComp.Dimensions.X, structPos.Z);
        //      var uRPos = new Vector2(structPos.X, structPos.Z + _buildingComp.Dimensions.Z);
        //      var dLPos = new Vector2(structPos.X, structPos.Z - _buildingComp.Dimensions.Z);
        //      var dRPos = new Vector2(structPos.X - _buildingComp.Dimensions.X, structPos.Z);
        //      ClimbOnPosMap.Add(OrthogDirection.UpLeft, uLPos);
        //      ClimbOnPosMap.Add(OrthogDirection.UpRight, uRPos);
        //      ClimbOnPosMap.Add(OrthogDirection.DownLeft, dLPos);
        //      ClimbOnPosMap.Add(OrthogDirection.DownRight, dRPos);

        MaxClimbHeight = _buildingComp.Dimensions.Y; //+ structPos.Y 
    }
    private void OnClimbableDestroyed(object sender, EventArgs e)
    {
        EjectClimbers?.Invoke(this, EventArgs.Empty);
        GD.Print("EJECT CLIMBERS INVOKED");
    }
}
