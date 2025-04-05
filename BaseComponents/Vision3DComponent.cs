//using Godot;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using TimeRobbers.Interfaces;
//public enum FovType
//{
//    Narrow,
//    Normal,
//    Wide,
//    Full
//}

//[GlobalClass, Tool]
//public partial class Vision3DComponent : Area3D
//{
//    #region CLASS_VARIABLES
//    public const string NodeName = "Vision3DComponent";

//    [Export]
//    public bool BodyHasYMovement { get; private set; } = false;
//    public Node3D VisionBody { get; set; }
//    public IMovementComponent MoveComp { get; set; }
//    public static Dictionary<FovType, float> FovAngleMap { get; private set; } = new Dictionary<FovType, float>()
//    {
//        { FovType.Narrow, Mathf.Pi / 12f }, //15 deg per side, 30 total
//        { FovType.Normal, Mathf.Pi / 6f }, //30 deg per side, 60 total
//        { FovType.Wide, Mathf.Pi / 3f }, //60 deg per side, 120 total
//        { FovType.Full, Mathf.Pi * 2 } //360 deg (not needed probably)
//    };

//    // TODO: draw circles on enity's wary and full detect amts
//    // TODO: create function for falloff hearing strength that correlates with changing detect amts
//    //[Export]
//    //public float DetectRadius { get; private set; }

//    [Export]
//    public FovType VisionFov { get; private set; } = FovType.Normal;
//    [Export(PropertyHint.Range, "0, 1000, 1, or_greater")]
//    public float SuspicionDist { get; private set; } = 400f;
//    [Export(PropertyHint.Range, "0, 1000, 1, or_greater")]
//    public float DetectionDist { get; private set; } = 150f;
//    [Export]
//    public Vector2 VisionStartingPoint { get; private set; } = Vector2.Zero;

//    [Export]
//    public float BaseVisionStrength { get; private set; }
//    [Export(PropertyHint.ExpEasing)]
//    public float VisionStrengthFalloff { get; private set; }

//    [Export]
//    public float TimeForFullDetect { get; private set; } = 1.0f; // seconds for enemy to fully detect if they stay in sus conditions

//    public CollisionShape3D ColShape { get; private set; }
//    public SphereShape3D DetectCircle { get; private set; }

//    public Dictionary<Node3D, AwareLevel> ObjsInVision { get; private set; } = new Dictionary<Node3D, AwareLevel>();

//    [Signal]
//    public delegate void SuspicionAlertEventHandler(Node3D sightedObj);
//    [Signal]
//    public delegate void ObjExitedVisionEventHandler(Node3D sightedObj);
//    [Signal]
//    public delegate void DetectionAlertEventHandler(Node3D sightedObj);
//    //[Signal]
//    //public delegate void DetectionAreaExitedEventHandler(Node3D sightedObj);

//    private SphereShape3D fullFovCircle = new SphereShape3D();
//    private ConvexPolygonShape3D fovConvex = new ConvexPolygonShape3D();
//    private Vector2[] fovPolygon;


//    private float _turnAngPerSec = 2*Mathf.Pi; // TODO: MAKE THIS IN BASE CHAR SCRIPT OR MOVEMENT COMP
//    #endregion

//    #region BASE_GODOT_OVERRIDEN_FUNCTIONS
//    public override void _Ready()
//    {
//        ColShape = GetNode<CollisionShape3D>("VisionDetectShape");
//        if (!Engine.IsEditorHint())
//        {
//            ColShape.Visible = true;
//        }

//        //SetDeferred(PropertyName._moveComp, VisionBody as IMovementComponent);
//        //_moveComp = VisionBody as IMovementComponent;

//        SetVisionCollision();
//        //QueueRedraw();

//        AreaEntered += OnAreaEnterVision;
//        AreaExited += OnAreaExitVision;
//        BodyEntered += OnBodyEnterVision;
//        BodyExited += OnBodyExitVision;
//    }
//    public override void _Process(double delta)
//    {
//        base._Process(delta);
//        if (Engine.IsEditorHint())
//        {
//            DetectionDist = Mathf.Clamp(DetectionDist, 0, SuspicionDist);
//            Name = NodeName;
//            SetVisionCollision();
//            //QueueRedraw();
//            return;
//        }
//    }
//    public override void _PhysicsProcess(double delta)
//    {
//        base._PhysicsProcess(delta);
//        if (Engine.IsEditorHint()) { return; }
//        SetVisionDirection((float)delta);
//    }
//    //public override void _Draw()
//    //{
//    //    //return;
//    //    base._Draw();
//    //    if (!Engine.IsEditorHint())
//    //    {
//    //        return;
//    //    }

//    //    if (VisionFov == FovType.Full)
//    //    {
//    //        DrawCircle(Position, fullFovCircle.Radius, new Color(Colors.Yellow, 0.3f));
//    //        DrawCircle(Position, DetectionDist, new Color(Colors.Red, 0.3f));
//    //        return;
//    //    }
        
//    //    DrawColoredPolygon(fovPolygon, new Color(Colors.Yellow, 0.3f));
//    //    //DrawColoredPolygon(ColShape.Position, FullDetectAmt, new Color(Colors.Red, 0.3f));
//    //}
//    #endregion

//    #region COMPONENT_FUNCTIONS
//    public void DeactivateDetection()
//    {
//        Monitorable = false;
//        Monitoring = false;
//    }
//    public void ActivateDetection()
//    {
//        Monitorable = true;
//        Monitoring = true;
//    }
//    private void SetVisionDirection(float delta)
//    {
//        float currRotation = ColShape.Rotation;
//        float desiredRotation;
//        // if velocity and face direction are similar, base vision off of acceleration
//        if (VisionBody is IVelocity3DComponent DetectChar //&&
//            D//etectChar.GetVelocity.Length() > Global.IdleVelocity &&
//            //IMovementComponent.AreDirectionsSimilar(
//            //    IMovementComponent.GetDirectionFromVector(DetectChar.Velocity), MoveComp.GetFaceDirection()))
//        {
//            //var velDir = IMovementComponent.GetDirectionFromVector(DetectChar.Velocity);
//            desiredRotation = DetectChar.GetVelocity().Angle();
//        }
//        else // otherwise base solely off of face direction
//        {
//            desiredRotation = IMovementComponent.GetAngleFromEightDirection(MoveComp.GetFaceDirection());
//        }
//        var diff = desiredRotation - currRotation;
//        while (diff > Mathf.Pi)
//        {
//            diff -= 2 * Mathf.Pi;
//        }
//        while (diff < -Mathf.Pi)
//        {
//            diff += 2 * Mathf.Pi;
//        }
//        // TODO: IF ANGLE IS OVER A CERTAIN POSITIVE OR NEGATIVE NUMBER,
//        // SWITCH ITS SIGNS AND VALUE TO OPTIMAL NUMBER (quickest path to angle basically)
//        var angPerPhysics = _turnAngPerSec * delta;
//        if (diff > angPerPhysics)
//        {
//            ColShape.Rotation += angPerPhysics;
//        }
//        else if (diff < -angPerPhysics)
//        {
//            ColShape.Rotation -= angPerPhysics;
//        }
//        else
//        {
//            ColShape.Rotation = desiredRotation;
//        }
//    }
//    private void SetVisionCollision()
//    {
//        if (VisionFov == FovType.Full)
//        {
//            fullFovCircle.Radius = SuspicionDist;
//            ColShape.Shape = fullFovCircle;
//            return;
//        }

//        fovPolygon= new Vector2[]
//        {
//            VisionStartingPoint,
//            VisionStartingPoint + new Vector2(SuspicionDist, Mathf.Sin(FovAngleMap[VisionFov]) * SuspicionDist),
//            VisionStartingPoint + new Vector2(SuspicionDist, Mathf.Sin(FovAngleMap[VisionFov]) * -SuspicionDist)
//        };
//        //fovPolygon = new Vector2[] allows for cool scaling of width/height
//        //{
//        //    VisionStartingPoint,
//        //    VisionStartingPoint + new Vector2(WaryDetectAmt, Mathf.Sin(FovAngleMap[VisionFov]) * FullDetectAmt),
//        //    VisionStartingPoint + new Vector2(WaryDetectAmt, Mathf.Sin(FovAngleMap[VisionFov]) * -FullDetectAmt)
//        //};

//        fovConvex.Points = fovPolygon;
//        ColShape.Shape = fovConvex;

//        //GD.Print("fov polygon: ", fovPolygon[1]);
//    }
//    //public override string[] _GetConfigurationWarnings()
//    //{
//    //    var warnings = new List<string>();

//    //    if (Name != NodeName)
//    //    {
//    //        warnings.Add($"Component should be named its original component name, :\" {NodeName} \", for purposes of identification. \nNote: An entity should never require multiple components of the same type.");
//    //    }
//    //    if (GetParent() is not DetectComponent)
//    //    {
//    //        warnings.Add($"Component must be nested under a {DetectComponent.NodeName} to have detecting capabilities.");
//    //    }

//    //    return base._GetConfigurationWarnings().Concat(warnings).ToArray();
//    //}
//    private void UpdateObjsInVision()
//    {
//        foreach (var objPair in ObjsInVision)
//        {
//            var dist = objPair.Key.GlobalPosition.DistanceTo(VisionBody.GlobalPosition);
//            if (dist <= DetectionDist && objPair.Value == AwareLevel.Suspicious)
//            {
//                ObjsInVision[objPair.Key] = AwareLevel.Detected;
//                EmitSignal(SignalName.DetectionAlert, objPair.Key);
//            }
//            else if (dist > DetectionDist && objPair.Value == AwareLevel.Suspicious)
//            {
//                ObjsInVision[objPair.Key] = AwareLevel.Suspicious;
//                EmitSignal(SignalName.SuspicionAlert, objPair.Key); // is this necessary?
//            }
//        }
//    }
//    private void NewObjInVision(Node3D obj)
//    {
//        if (obj == VisionBody) { return; }
//        if (ObjsInVision.ContainsKey(obj)) { return; }
//        if (obj.GlobalPosition.DistanceTo(VisionBody.GlobalPosition) <= DetectionDist)
//        {
//            ObjsInVision.Add(obj, AwareLevel.Detected);
//            //GD.Print($"{Name}'S VISION DETECTED obj {obj.Name}");
//            EmitSignal(SignalName.DetectionAlert, obj);
//        }
//        else
//        {
//            ObjsInVision.Add(obj, AwareLevel.Suspicious);
//            //GD.Print($"{Name}'S VISION SAW SUSPICIOUS obj {obj.Name}");
//            EmitSignal(SignalName.SuspicionAlert, obj);
//        }
//    }
//    #endregion

//    #region SIGNAL_LISTENERS
//    private void OnAreaEnterVision(Area3D area)
//    {
//        var obj = area.GetOwner() as Node3D;
//        //GD.Print("body entered vision: ", body.Name);
//        NewObjInVision(obj);
//    }
//    private void OnAreaExitVision(Area3D area)
//    {
//        var body = area.GetOwner() as Node3D;
//        if (body == VisionBody) { return; }
//        if (!ObjsInVision.Remove(body)) { return; }
//        EmitSignal(SignalName.ObjExitedVision, body);
//    }

//    private void OnBodyEnterVision(Node3D obj)
//    {
//        NewObjInVision(obj);
//    }

//    private void OnBodyExitVision(Node3D body)
//    {
//        if (body == VisionBody) { return; }
//        if (!ObjsInVision.Remove(body)) { return; }
//        EmitSignal(SignalName.ObjExitedVision, body);
//    }
//    #endregion

//    #region HELPER_CLASSES
//    #endregion
//}
