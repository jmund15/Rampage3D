using Godot;
using GDCol = Godot.Collections;
using System;
using System.Collections.Generic;
using TimeRobbers.Interfaces;
using System.Linq;
public enum NavType
{
    Walk,
    Drive,
    Fly,
    GroundChaos
}
public enum AINavWeight
{
    #region BODIES
    Monster,
    Wall,
    Building,
    Vehicle,
    Critter,
    Military,
    #endregion
    #region AREAS
    InterestArea
    #endregion
}
public enum AIFacing
{
    Facing,
    Next,
    Perpindicular,
    Distant,
    Opposite
}
public enum DetectionTypes
{
    Enterable,
    Vehicle,
    Monster,
    Military,
}

public class AIDetectionArgs : EventArgs
{
    public GodotObject Detectable;
    public List<DetectionTypes> DetectTypes;
    public AIDetectionArgs(GodotObject detectable, List<DetectionTypes> types = null)
    {
        Detectable = detectable;
        if (types == null) { DetectTypes = new  List<DetectionTypes>(); }
        else { DetectTypes = types; }
    }
}

[GlobalClass, Tool]
public partial class AINav3DComponent : NavigationAgent3D
{
    //TEMPLATE FOR COMPONENTS
    #region CLASS_VARIABLES
    [Export]
    public Node3D ParentAgent { get; private set; }
    [Export]
    private NavType _navMethod;
    public NavType NavMethod 
    {
        get => _navMethod;
        private set
        {
            if (_navMethod == value) { return; }
            _navMethod = value;
            //TODO: ADD SIGNAL IF NEEDED LATER
            OnNavMethodChanged(_navMethod);
        }
    }
    public Timer PathTimer { get; private set; }
    [Export]
    public float FindPathInterval { get; private set; } = 0.25f;
    private bool _baseAvoidanceEnabled;
    public bool NavigationEnabled { get; private set; } = false;

    public Vector3 WeightedNextPathDirection { get; private set; } = Vector3.Zero;
    public static Dictionary<AINavWeight, uint> NavLayerMap { get; private set; } = new Dictionary<AINavWeight, uint>()
    {
        { AINavWeight.Monster, 1 },
        { AINavWeight.Building, 4 },
        { AINavWeight.Wall, 5 },
        { AINavWeight.Vehicle, 7 },
        { AINavWeight.Critter, 9 },
        { AINavWeight.Military, 14 }
    };

    public Dictionary<AINavWeight, float> NavWeights { get; private set; } = new Dictionary<AINavWeight, float>()
    {
        { AINavWeight.Monster, 1 },
        { AINavWeight.Building, 1 },
        { AINavWeight.Wall, 1 },
        { AINavWeight.Vehicle, 1 },
        { AINavWeight.Critter, 1 },
        { AINavWeight.Military, 1 }
    }; // TODO: CREATE CUSTOM SETTER FOR SPECIFIC SITUATIONS


    public Dictionary<EightDirection, float> DirectionWeights { get; private set; } = new Dictionary<EightDirection, float>() {
        { EightDirection.Right, 0f },
        { EightDirection.DownRight, 0f },
        { EightDirection.Down, 0f },
        { EightDirection.DownLeft, 0f },
        { EightDirection.Left, 0f },
        { EightDirection.UpLeft, 0f },
        { EightDirection.Up, 0f },
        { EightDirection.UpRight, 0f }
    };
    public Dictionary<AIFacing, float> SpatialAwarenessWeights { get; private set; } = new Dictionary<AIFacing, float>();
    public RayCast3D RayUp { get; private set; }
    public RayCast3D RayUpRight { get; private set; }
    public RayCast3D RayRight { get; private set; }
    public RayCast3D RayDownRight { get; private set; }
    public RayCast3D RayDown { get; private set; }  
    public RayCast3D RayDownLeft { get; private set; }
    public RayCast3D RayLeft { get; private set; }
    public RayCast3D RayUpLeft { get; private set; }
    public Dictionary<EightDirection, RayCast3D> Raycasts { get; private set; } = new Dictionary<EightDirection, RayCast3D>();

    public Node3D CurrentTarget { get; set; }
    // Should use avoidance/avoidance layers? or weight with this
    public List<Node3D> CurrentAvoidTargets { get; set; } = new List<Node3D>(); 

    private bool _canCalcPath = true;


    public event EventHandler<AIDetectionArgs> AIDetection;
    #endregion

    #region BASE_GODOT_OVERRIDEN_FUNCTIONS
    public override void _Ready()
    {
        base._Ready();
        if (!Engine.IsEditorHint())
        {
            if (!ParentAgent.IsValid())
            {
                GD.PrintErr("AINAVCOMP ERROR || INVALID PARENT FOR NAVIGATION!");
            }
        }
        NavMethod = _navMethod;

        _baseAvoidanceEnabled = AvoidanceEnabled;

        RayUp = GetNode<RayCast3D>("RayUp");
        RayUpRight = GetNode<RayCast3D>("RayUpRight");
        RayRight = GetNode<RayCast3D>("RayRight");
        RayDownRight = GetNode<RayCast3D>("RayDownRight");
        RayDown = GetNode<RayCast3D>("RayDown");
        RayDownLeft = GetNode<RayCast3D>("RayDownLeft");
        RayLeft = GetNode<RayCast3D>("RayLeft");
        RayUpLeft = GetNode<RayCast3D>("RayUpLeft");
        Raycasts.Add(EightDirection.Up, RayUp);
        Raycasts.Add(EightDirection.UpRight, RayUpRight);
        Raycasts.Add(EightDirection.Right, RayRight);
        Raycasts.Add(EightDirection.DownRight, RayDownRight);
        Raycasts.Add(EightDirection.Down, RayDown);
        Raycasts.Add(EightDirection.DownLeft, RayDownLeft);
        Raycasts.Add(EightDirection.Left, RayLeft);
        Raycasts.Add(EightDirection.UpLeft, RayUpLeft);

        PathTimer = this.GetFirstChildOfType<Timer>();
        PathTimer.Timeout += OnPathTimeout;
        
        EnableNavigation();
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Engine.IsEditorHint()) { return; }
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (Engine.IsEditorHint()) { return; }
        if (NavigationServer3D.MapGetIterationId(GetNavigationMap()) == 0 || // if map is not sync'd, don't try and calc path
            !NavigationEnabled)// ||
            //IsNavigationFinished())
        {
            WeightedNextPathDirection = ParentAgent.GlobalPosition; // stay same pos
            return; 
        }
        WeightedNextPathDirection = GetWeightedPathPosition() * 10f; // TODO: change 10 to intelliget value
    }
    #endregion

    #region COMPONENT_FUNCTIONS
    public void DisableNavigation()
    {
        TargetPosition = ParentAgent.GlobalPosition;
        AvoidanceEnabled = false;
        NavigationEnabled = false;
    }
    public void EnableNavigation()
    {
        AvoidanceEnabled = _baseAvoidanceEnabled;
        NavigationEnabled = true;
    }
    //TODO: ADD PROCESSING FOR FLYING/Y-ENABLED NAV AGENTS
    protected virtual Vector3 GetWeightedPathPosition()
    {
        //INITIAL PATH
        var unweightedPathPoint = ParentAgent.ToLocal(GetNextPathPosition());
        var normVec = unweightedPathPoint.Normalized();
        List<EightDirection> dirs = Global.GetEnumValues<EightDirection>().ToList();

        var dangerWeights = GetDangerVector();

        float maxWeight = float.MinValue;
        Vector3 weightedDir = Vector3.Zero;
        foreach (var dir in dirs)
        {
            var eightDirVec = dir.GetVector();
            DirectionWeights[dir] = normVec.Dot(new Vector3(eightDirVec.X, 0f, eightDirVec.Y));
            DirectionWeights[dir] -= dangerWeights[dir];
            if (DirectionWeights[dir] > maxWeight)
            {
                weightedDir = new Vector3(eightDirVec.X, unweightedPathPoint.Y, eightDirVec.Y);
                maxWeight = DirectionWeights[dir];
            }
        }
        //GD.PrintS($"(Right: {DirectionWeights[EightDirection.RIGHT]}, " +
        //    $"DownRight: {DirectionWeights[EightDirection.DOWNRIGHT]}, " +
        //    $"Down: {DirectionWeights[EightDirection.DOWN]}, " +
        //    $"DownLeft: {DirectionWeights[EightDirection.DOWNLEFT]}, " +
        //    $"Left: {DirectionWeights[EightDirection.LEFT]}, " +
        //    $"UpLeft: {DirectionWeights[EightDirection.UPLEFT]}, " +
        //    $"Up: {DirectionWeights[EightDirection.UP]}, " +
        //    $"UpRight: {DirectionWeights[EightDirection.UPRIGHT]})");

        // TODO: DON'T WEIGHT AGAINST OBJECT IF IT IS THE CURRENT TARGET 

        //var result = Enumerable.Zip(DirectionWeights, DirectionWeights, (a, b) => (a.Key, a.Value + b.Value));
        return weightedDir;
    }
    protected virtual Dictionary<EightDirection, float> GetDangerVector()
    {
        var dangerVector = new Dictionary<EightDirection, float>()
        {
            { EightDirection.Right, 0f },
            { EightDirection.DownRight, 0f },
            { EightDirection.Down, 0f },
            { EightDirection.DownLeft, 0f },
            { EightDirection.Left, 0f },
            { EightDirection.UpLeft, 0f },
            { EightDirection.Up, 0f },
            { EightDirection.UpRight, 0f }
        };
        foreach (var pair in Raycasts)
        {
            var dir = pair.Key;
            var raycast = pair.Value;
            var castLength = raycast.TargetPosition.Length();
            if (raycast.IsColliding())
            {
                var collisionDist = (raycast.GetCollisionPoint() - raycast.GlobalPosition).Length();
                // the closer the collision is to the raycast, the higher the "danger" weight
                // at max dist, weight is 0. TODO: change s.t. it's not zero, just lower (i.e. 0.1)
                // at dist 0, weight is 1.0. TODO: change s.t. weight being 0 occurs not only at dist 0, since that is impossible and too late
                var distWeight = 1f - (collisionDist / castLength); 

                var rayCollision = raycast.GetCollider() as CollisionObject2D;
                foreach (var navLayer in NavLayerMap)
                {
                    if (rayCollision.GetCollisionLayerValue((int)navLayer.Value))
                    {
                        dangerVector[dir] += NavWeights[navLayer.Key] * distWeight;
                    }
                }
            }
        }

        return dangerVector;
    }
    protected virtual Dictionary<EightDirection, float> GetInterestVector()
    {
        var interestVector = new Dictionary<EightDirection, float>();



        return interestVector;
    }
    public bool SetTarget(Vector3 position, bool overrideInterval = false) 
    {
        if (!_canCalcPath && !overrideInterval) { return false; } // allow path creation if override bool is true
        //Check if point is in a nav mesh
        GDCol.Array<Rid> rIDs = NavigationServer2D.GetMaps();
        foreach (Rid rID in rIDs)
        {
            var mapDist = NavigationServer3D.MapGetClosestPoint(rID, position).DistanceTo(position);
            var mapDistAllowance = 0.01f;
            //GD.Print("setting path, map dist: ", mapDist);
            //bool isNavMesh = mapDist <= float.Epsilon; //if point is in a nav region, its distance should be ~0.0
            if (mapDist <= mapDistAllowance)
            {
                TargetPosition = position; //Allow for path to be set if it is on a nav mesh
                _canCalcPath = false;
                PathTimer.Start(FindPathInterval);
                break;
            }
        }
        if (_canCalcPath)
        {
            //GD.Print("failed to calc path");
            return false; // wasn't able to set path
        }
        else
        {
            //GD.Print("successfully calc path");
            return true; // successfully set path
        }
    }
    public float NavDistanceToTarget()
    {
        //GD.Print("target Pos: ", TargetPosition);
        var navPath = GetCurrentNavigationPath();
        if (navPath.Length == 0) { return 0f; }
        //var currNavPos = navPath[GetCurrentNavigationPathIndex()];
        //var currNavIdx = 0;
        //var dist = 0.0f;
        //foreach (var navPoint in navPath)
        //{
        //    if (currNavIdx <= GetCurrentNavigationPathIndex())
        //    {
        //        currNavIdx++;
        //        continue;
        //    }
        //    dist += currNavPos.DistanceTo(navPoint);
        //    currNavPos = navPoint;
        //}

        var realNavPath = navPath.Skip(GetCurrentNavigationPathIndex()).ToList();
        var currNavPos = ParentAgent.GlobalPosition; //realNavPath[0];
        var dist = 0f; //_parentAgent.GlobalPosition.DistanceTo(currNavPos);
        foreach (var navPoint in realNavPath)
        {
            dist += currNavPos.DistanceTo(navPoint);
            currNavPos = navPoint;
        }

        return dist;
    }
    public float NavDistanceToPoint(Vector3 toPoint)
    {
        var currTargetPos = TargetPosition;
        TargetPosition = toPoint;
        var navBuff = GetNextPathPosition();
        GD.Print("calc'ing dist to point ", toPoint, ", next path pos: ", navBuff);
        var navPath = GetCurrentNavigationPath();
        GD.Print("calc'ing dist to point ", toPoint, ", nav path: ", navPath);
        if (navPath.Length == 0) { TargetPosition = currTargetPos; return 0f; }
        var currNavPos = navPath[GetCurrentNavigationPathIndex()];
        var currNavIdx = 0;
        var dist = 0.0f;

        foreach (var navPoint in navPath)
        {
            if (currNavIdx <= GetCurrentNavigationPathIndex())
            {
                currNavIdx++;
                continue;
            }
            dist += currNavPos.DistanceTo(navPoint);
            currNavPos = navPoint;
        }
        TargetPosition = currTargetPos;
        return dist;
    }
    
    //public Vector2 GetPathPointFromIdx(int idx)
    //{
    //    return _currPath.Curve.GetBakedPoints()[idx];
    //}
    public Node3D FindNearestNavTarget(List<Node3D> targets)
    {
        float lowestDist = 9999f;
        Node3D nearestTarget = null;
        foreach (var target in targets)
        {
            var navDist = NavDistanceToPoint(target.Position);
            if (navDist < lowestDist)
            {
                lowestDist = navDist;
                nearestTarget = target;
            }
        }
        if (nearestTarget  == null) { throw new Exception("AINavComponent ERROR || No nav targets are reachable!"); }
        return nearestTarget;
    }
    public PhysicsBody3D FindNearestNavTarget(List<PhysicsBody3D> targets)
    {
        float lowestDist = 9999f;
        PhysicsBody3D nearestTarget = null;
        foreach (var target in targets)
        {
            var navDist = NavDistanceToPoint(target.Position);
            if (navDist < lowestDist)
            {
                lowestDist = navDist;
                nearestTarget = target;
            }
        }
        if (nearestTarget == null) { throw new Exception("AINavComponent ERROR || No nav targets are reachable!"); }
        return nearestTarget;
    }
    #endregion

    #region SIGNAL_LISTENERS
    private void OnPathTimeout()
    {
        _canCalcPath = true;
    }
    private void OnNavMethodChanged(NavType navMethod)
    {
        switch (navMethod)
        {
            case NavType.Walk:
                SetNavigationLayerValue(1, true);
                SetNavigationLayerValue(2, false);
                SetNavigationLayerValue(3, false);
                SetNavigationLayerValue(4, false);
                break;
            case NavType.Drive:
                SetNavigationLayerValue(1, false);
                SetNavigationLayerValue(2, true);
                SetNavigationLayerValue(3, false);
                SetNavigationLayerValue(4, false);
                break;
            case NavType.Fly:
                SetNavigationLayerValue(1, false);
                SetNavigationLayerValue(2, false);
                SetNavigationLayerValue(3, true);
                SetNavigationLayerValue(4, false);
                break;
            case NavType.GroundChaos:
                SetNavigationLayerValue(1, false);
                SetNavigationLayerValue(2, false);
                SetNavigationLayerValue(3, false);
                SetNavigationLayerValue(4, true);
                break;
            default:
                GD.PrintErr($"NAVIGATION TYPE ERROR || Selected NavType {navMethod} is not supported!");
                break;
        }
    }
    #endregion

    #region HELPER_CLASSES
    #endregion
}
