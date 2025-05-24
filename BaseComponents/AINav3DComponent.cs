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
    Building,
    EnvObject,
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
    Peripheral,
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
/* TODO:
 * Look into using Area3D instead of Raycasts, 
 * ESPECIALLY for 3D, as having a bunch of rays going up into the Y may cause performance issues
 */
[GlobalClass, Tool]
public partial class AINav3DComponent : NavigationAgent3D
{
    #region CLASS_VARIABLES
    [Export]
    public bool HasYMovement { get; private set; } = false;
    [Export]
    public bool ShowNavigationArrows { get; private set; } = true;
    #region HELPER_VARS
    private Dictionary<Dir8, bool> _eightToOrthogMap = new Dictionary<Dir8, bool>()
    {
        { Dir8.Right, true },
        { Dir8.DownRight, false },
        { Dir8.Down, true },
        { Dir8.DownLeft, false },
        { Dir8.Left, true },
        { Dir8.UpLeft, false },
        { Dir8.Up, true },
        { Dir8.UpRight, false }
    };
    #endregion
    
    [Export]
    public Node3D ParentAgent { get; private set; }
    [Export]
    private NavType _navMethod;
    [Export]
    public AIRayDetector3D AIRayDetector { get; private set; }
    [Export]
    public AIAreaDetector3D AIAreaDetector { get; private set; }

    [Export]
    public bool ResponbsibleForDetection { get; private set; } = true;
    private IMovementComponent _moveComp;
    private IBlackboard _bb;
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
    public bool HasPath { get; private set; } = true;
    public Vector3 WeightedNextPathDirection { get; private set; } = Vector3.Zero;
    [Export]
    public GDCol.Array<AIEntityConsideration3D> EntityConsiderations { get; private set; }
    public static Dictionary<AINavWeight, uint> NavLayerMap { get; private set; } = new Dictionary<AINavWeight, uint>()
    {
        { AINavWeight.Monster, 1 },
        { AINavWeight.Building, 2 },
        { AINavWeight.EnvObject, 3 },
        { AINavWeight.Critter, 4 },
        { AINavWeight.Vehicle, 5 },
        { AINavWeight.Military, 14 }
    };

    public Dictionary<AINavWeight, float> NavWeights { get; private set; } = new Dictionary<AINavWeight, float>()
    {
        { AINavWeight.Monster, 2f },
        { AINavWeight.Building, 1f },
        { AINavWeight.EnvObject, 1 },
        { AINavWeight.Vehicle, 2f },
        { AINavWeight.Critter, 0.5f },
        { AINavWeight.Military, 2f }
    };
    public Dictionary<AINavWeight, float> NavDistThresh { get; private set; } = new Dictionary<AINavWeight, float>()
    {
        { AINavWeight.Monster, 5 },
        { AINavWeight.Building, 0.5f },
        { AINavWeight.EnvObject, 0.5f },
        { AINavWeight.Vehicle, 3f },
        { AINavWeight.Critter, 0.25f },
        { AINavWeight.Military, 2f }
    };
    // TODO: CREATE CUSTOM SETTER FOR SPECIFIC SITUATIONS


    //public Dictionary<EightDirection, float> DirectionWeights { get; private set; } = new Dictionary<EightDirection, float>() {
    //    { EightDirection.Right, 0f },
    //    { EightDirection.DownRight, 0f },
    //    { EightDirection.Down, 0f },
    //    { EightDirection.DownLeft, 0f },
    //    { EightDirection.Left, 0f },
    //    { EightDirection.UpLeft, 0f },
    //    { EightDirection.Up, 0f },
    //    { EightDirection.UpRight, 0f }
    //};
    public Dictionary<Vector3, float> ConsiderationWeights { get; private set; } = new Dictionary<Vector3, float>();
    public Dictionary<Vector3, float> DirectionWeights { get; private set; } = new Dictionary<Vector3, float>() {
        //{ Dir8.Right, 0f },
        //{ Dir8.DownRight, 0f },
        //{ Dir8.Down, 0f },
        //{ Dir8.DownLeft, 0f },
        //{ Dir8.Left, 0f },
        //{ Dir8.UpLeft, 0f },
        //{ Dir8.Up, 0f },
        //{ Dir8.UpRight, 0f }
    };
    public Dictionary<AIFacing, float> SpatialAwarenessWeights { get; private set; } = new Dictionary<AIFacing, float>()
    {
        { AIFacing.Facing, 1.0f },
        { AIFacing.Peripheral, 1 },
        { AIFacing.Opposite, 1 }
        //{ AIFacing.Perpindicular, 1 },
        //{ AIFacing.Distant, 1 },
    };
    public Node3D CurrentTarget { get; set; }
    // Should use avoidance/avoidance layers? or weight with this
    public List<Node3D> CurrentAvoidTargets { get; set; } = new List<Node3D>(); 

    private bool _canCalcPath = true;

    [Export]
    public bool UseOrthogNavOnly { get; private set; } = true;

    //public event EventHandler<AIDetectionArgs> AIDetection;
    public event EventHandler<Node3D> BodyDetected;
    public event EventHandler<Area3D> AreaDetected;

    private Timer _debugTimer;


    // --- Randomness / Noise Parameters ---
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    [ExportGroup("Randomness Tuning")]
    [Export] // For Option B: Noise on Consideration Weight
    private FastNoiseLite _considerationNoise; // Assign in Inspector!
    [Export(PropertyHint.Range, "0.0, 0.5, 0.01")]
    private float _considerationNoiseIntensity = 0.15f;
    [Export]
    private float _considerationNoiseTimeScale = 0.4f;
    [Export(PropertyHint.Range, "0.1, 1.0, 0.05")] // Base weight before noise
    private float _baseConsiderationInfluence = 0.5f;
    [Export(PropertyHint.Range, "0.0, 0.5, 0.05")] // Minimum clamped influence
    private float _minConsiderationInfluence = 0.1f;
    [Export(PropertyHint.Range, "0.5, 1.5, 0.05")] // Maximum clamped influence
    private float _maxConsiderationInfluence = 1.0f;
    #endregion

    #region BASE_GODOT_OVERRIDEN_FUNCTIONS
    public override void _Ready()
    {
        base._Ready();
        if (!Engine.IsEditorHint())
        {
            if (!ParentAgent.IsValid() || ParentAgent is not IMovementComponent)
            {
                GD.PrintErr("AINAVCOMP ERROR || INVALID PARENT FOR NAVIGATION!");
            }
            _debugTimer = GetNode<Timer>("DebugTimer");
            _debugTimer.Timeout += OnDebugTimeout;
            _debugTimer.Start(1.0f);
        }
        //foreach (var dir in Global.GetEnumValues<Dir16>())
        //{
        //    ConsiderationWeights.Add(dir, 0f);
        //}
        
        _moveComp = ParentAgent as IMovementComponent;
        _bb = ParentAgent.GetFirstChildOfInterface<IBlackboard>();

        NavMethod = _navMethod;

        _baseAvoidanceEnabled = AvoidanceEnabled;

        PathTimer = GetNode<Timer>("PathTimer");
        PathTimer.Timeout += OnPathTimeout;

        NavigationEnabled = false;
        CallDeferred(MethodName.InitNavigation);
        //EnableNavigation();
    }

    private void OnDebugTimeout()
    {
        if (HasPath)
        {
            var globalPathP = GetNextPathPosition();
            var unweightedPathPoint = globalPathP - ParentAgent.GlobalPosition;
            //var unweightedPathPoint = ParentAgent.ToLocal(globalPathP);
            
            var normVec = unweightedPathPoint.Normalized();
            //GD.Print("global path p: ", globalPathP);
            //GD.Print("local path p: ", unweightedPathPoint);
            //GD.Print("parent p: ", ParentAgent.Position);
            //GD.Print("parent global p: ", ParentAgent.GlobalPosition);
            //GD.Print($"Base Path Direction: {normVec}");

            //return normVec;
        }
        var lastDir = Dir8.DownLeft;
        foreach (var dir in DirectionWeights.Keys)
        {
            if (dir.GetDir16().GetDir8() == null) { continue; }
            GD.Print($"{dir.GetDir16()}'s Weight: {DirectionWeights[dir]}; Consid: {ConsiderationWeights[dir]}");
        }
        //GD.Print($"Danger Weights:\n" +
        //    $"Up: {ConsiderationWeights[Dir16.U]:F2} " +
        //    $"UpRight: {ConsiderationWeights[Dir16.UR]:F2} " +
        //    $"Right: {ConsiderationWeights[Dir16.R]:F2} " +
        //    $"DownRight: {ConsiderationWeights[Dir16.DR]:F2} " +
        //    $"Down: {ConsiderationWeights[Dir16.D]:F2} " +
        //    $"DownLeft: {ConsiderationWeights[Dir16.DL]:F2} " +
        //    $"Left: {ConsiderationWeights[Dir16.L]:F2} " +
        //    $"UpLeft: {ConsiderationWeights[Dir16.UL]:F2} "
        //    );

        //GD.Print($"Direction Weights:\n" +
        //    $"Up: {DirectionWeights[Dir8.Up]:F2} " +
        //    $"Right: {DirectionWeights[Dir8.Right]:F2} " +
        //    $"Down: {DirectionWeights[Dir8.Down]:F2} " +
        //    $"Left: {DirectionWeights[Dir8.Left]:F2} " +
        //    $"UpRight: {DirectionWeights[Dir8.UpRight]:F2} " +
        //    $"DownRight: {DirectionWeights[Dir8.DownRight]:F2} " +
        //    $"DownLeft: {DirectionWeights[Dir8.DownLeft]:F2} " +
        //    $"UpLeft: {DirectionWeights[Dir8.UpLeft]:F2} "
        //    );

        GD.Print($"Chosen Dirction: {ChooseDirection()}");

        //foreach (var pair in _rays.Raycasts)
        //{
        //    var dir = pair.Key;
        //    var raycast = pair.Value;
        //    var castLength = raycast.TargetPosition.Length();
        //    if (raycast.IsColliding())
        //    {
        //        var rayCollision = raycast.GetCollider() as CollisionObject3D;
        //        if (rayCollision == ParentAgent) { continue; } //TODO: better fix
        //        var collDist = (raycast.GetCollisionPoint() - raycast.GlobalPosition).Length();
        //        // the closer the collision is to the raycast, the higher the "danger" weight
        //        // at max dist, weight is 0. TODO: change s.t. it's not zero, just lower (i.e. 0.1)
        //        // at dist 0, weight is 1.0. TODO: change s.t. weight being 0 occurs not only at dist 0, since that is impossible and too late
        //        //var distWeight = 1f - (collisionDist / castLength); 
        //        var minWeight = 0.1f;
        //        var k = 2.5f;
        //        var distDropThresh = 1.5f;
        //        float distWeight;
        //        if (collDist <= distDropThresh)
        //        {
        //            distWeight = 1.0f;  // Ensure max weight
        //        }
        //        else
        //        {
        //            distWeight = minWeight + (1.0f - minWeight) * (float)Math.Exp(-k * (collDist - distDropThresh) / (castLength - distDropThresh));
        //        }
                
        //        GD.Print($"Raycast colliding with {rayCollision.Name} @ dir {dir}!" +
        //            $"\n\tColl layer val: {rayCollision.CollisionLayer}" +
        //            $"\n\tColl Dist: {collDist}, weight: {distWeight}");
        //    }
        //}
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
            WeightedNextPathDirection = Vector3.Zero;//ParentAgent.GlobalPosition; // stay same pos
            return; 
        }
        WeightedNextPathDirection = GetWeightedPathPosition() * 10f; // TODO: change 10 to intelliget value
    
        if (!ShowNavigationArrows)
        {
            return;
        }
        var _time = Time.GetTicksMsec() / 1000.0f;
        var arrowSize = 4f;
        var arrowheadSize = 0.1f;

        
        foreach (var dirWeight in DirectionWeights)
        {
            var weight = dirWeight.Value;
            var arrowColor = Colors.Yellow;
            if (weight < 0.2f)
            {
                weight = 0.2f;
                arrowColor = Colors.Red;
            }
            else if (weight > 0.5f)
            {
                arrowColor = Colors.Green;
            }
            var dirArrow = dirWeight.Key * weight * arrowSize;
            DebugDraw3D.DrawArrow(ParentAgent.GlobalPosition,
                ParentAgent.GlobalPosition + dirArrow,
                arrowColor,
                arrowheadSize,
                true);
        }
        var chosenDirArrow = WeightedNextPathDirection * 0.1f * arrowSize;
        chosenDirArrow.Y = 0;
        DebugDraw3D.DrawArrow(ParentAgent.GlobalPosition,
                ParentAgent.GlobalPosition + chosenDirArrow,
                Colors.Black,
                arrowheadSize,
                true);

        //DebugDraw3D.DrawLine(line_begin, line_end, new Color(1, 1, 0));
        DebugDraw2D.SetText("Time", _time);
        DebugDraw2D.SetText("Frames drawn", Engine.GetFramesDrawn());
        DebugDraw2D.SetText("FPS", Engine.GetFramesPerSecond());
        DebugDraw2D.SetText("delta", delta);
    }
    #endregion

    #region COMPONENT_FUNCTIONS
    public void InitNavigation()
    {
        foreach (var entityConsid in EntityConsiderations)
        {
            entityConsid.InitializeResources(_bb);
        }

        foreach (var dir in AIRayDetector.Directions)
        {
            ConsiderationWeights.Add(dir, 0f);
            DirectionWeights.Add(dir, 0f);
            GD.Print("AINav Added Direction: ", dir);
        }

        EnableNavigation();
    }
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
        Vector3 normVec = Vector3.Zero;
        //INITIAL PATH
        if (HasPath)
        {
            var globalPathP = GetNextPathPosition();
            var unweightedPathPoint = globalPathP - ParentAgent.GlobalPosition;
            //var unweightedPathPoint = ParentAgent.ToLocal(globalPathP);
            //GD.Print("global path p: ", globalPathP);
            //GD.Print("local path p: ", unweightedPathPoint);
            //GD.Print("parent global p: ", ParentAgent.GlobalPosition);
            normVec = unweightedPathPoint.Normalized();
            //return normVec;
        }

        if (ResponbsibleForDetection)
        {
            SignalRaycastDetections();
        }
        //List<Dir8> dirs = Global.GetEnumValues<Dir8>().ToList();
        List<Vector3> dirs = AIRayDetector.Directions;
        ConsiderationWeights = GetConsiderationVector();

        float maxWeight = float.MinValue;
        Vector3 weightedDir = Vector3.Zero;
        foreach (var dir in dirs)
        {
            float dotProd;
            if (HasYMovement)
            {
                dotProd = normVec.Dot(new Vector3(dir.X, dir.Y, dir.Z));
            }
            else
            {
                dotProd = normVec.Dot(new Vector3(dir.X, 0f, dir.Z));
            }
            DirectionWeights[dir] = dotProd > 0 ? dotProd : 0; // no below zero!
            //var weight = 0.5f;
            DirectionWeights[dir] += ConsiderationWeights[dir];// * weight;
        }
        //foreach (var dir in dirs)
        //{
        //    var eightDirVec = dir;
        //    var dotProd = normVec.Dot(new Vector3(eightDirVec.X, 0f, eightDirVec.Y));
        //    DirectionWeights[dir] = dotProd > 0 ? dotProd : 0; // no below zero!
        //    var neighborDirs = dir.GetNeighboring16Dirs();
        //    var weight = 0.5f;
        //    DirectionWeights[dir] += ConsiderationWeights[dir.GetDir16()] * weight;
        //    DirectionWeights[dir] += ConsiderationWeights[neighborDirs.Item1] * weight * weight;
        //    DirectionWeights[dir] += ConsiderationWeights[neighborDirs.Item2] * weight * weight;
        //    //if (UseOrthogNavOnly)
        //    //{
        //    //    if (_eightToOrthogMap[dir]) // add neighboring dir weights for better orthog weighted movement
        //    //    {
        //    //        var orthogAddWeight = 0.5f;
        //    //        var orthogNeighbors = dir.GetNeighboringDirs();
        //    //        DirectionWeights[dir] += DirectionWeights[orthogNeighbors.Item1] * orthogAddWeight;
        //    //        DirectionWeights[dir] += DirectionWeights[orthogNeighbors.Item2] * orthogAddWeight;
        //    //    }
        //    //    else
        //    //    {
        //    //        continue; // don't set non orthogdir as move dir;
        //    //    }
        //    //}
        //    //if (DirectionWeights[dir] > maxWeight)
        //    //{
        //    //    weightedDir = new Vector3(eightDirVec.X, normVec.Y, eightDirVec.Y);
        //    //    maxWeight = DirectionWeights[dir];
        //    //}
        //}
        var chosenDir = ChooseDirection();
        if (HasYMovement)
        {
            weightedDir = new Vector3(chosenDir.X, chosenDir.Y, chosenDir.Z);
        }
        else
        {
            weightedDir = new Vector3(chosenDir.X, 0f, chosenDir.Z);
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
        return weightedDir;//unweightedPathPoint;//
    }
    private Vector3 ChooseDirection()
    {
        var chosenDir = Vector3.Zero;
        foreach (var dir in DirectionWeights.Keys)
        {
            //if (ConsiderationWeights[dir.GetDir16()] < 0)
            //{
            //    DirectionWeights[dir.Key] = 0;
            //}
            //Choose direction based on remaining interest
            chosenDir += dir * DirectionWeights[dir];
        }
        chosenDir = chosenDir.Normalized();
        return chosenDir;
    }
    // --- Weighted Random Selection ---
    private Vector3 ChooseDirectionWeightedRandom(Dictionary<Vector3, float> weights)
    {
        // --- Option B: Calculate Noisy Consideration Influence ---
        float currentConsiderationInfluence = _baseConsiderationInfluence;
        if (_considerationNoise != null)
        {
            float time = (float)Time.GetUnixTimeFromSystem() * _considerationNoiseTimeScale;
            // Get noise value (usually -1 to 1)
            float noiseValue = _considerationNoise.GetNoise1D(time);
            // Map noise to a +/- range based on intensity, add to base weight
            currentConsiderationInfluence += noiseValue * _considerationNoiseIntensity;
            // Clamp the final influence weight
            currentConsiderationInfluence = Mathf.Clamp(currentConsiderationInfluence, _minConsiderationInfluence, _maxConsiderationInfluence);
        }
        // --- End Option B ---

        float totalWeight = 0f;
        foreach (float weight in weights.Values)
        {
            // Ensure weight is positive for probability calculation
            if (weight > 0) totalWeight += weight;
        }

        // If total weight is near zero, no direction has interest
        if (totalWeight < 0.001f)
        {
            return Vector3.Zero; // Indicate no direction chosen
        }

        // Get a random value scaled by the total weight
        float randomValue = (float)_rng.RandfRange(0f, totalWeight);

        // Iterate through weights, subtracting until randomValue goes below zero
        foreach (KeyValuePair<Vector3, float> pair in weights)
        {
            if (pair.Value <= 0) continue; // Skip directions with zero or negative weight

            randomValue -= pair.Value;
            if (randomValue <= 0f)
            {
                return pair.Key; // This is the chosen direction
            }
        }

        // Fallback (should theoretically not be reached if totalWeight > 0, but safer)
        // Return the direction with the highest weight as a deterministic fallback
        Vector3 fallbackDir = Vector3.Zero;
        float maxW = -1f;
        foreach (KeyValuePair<Vector3, float> pair in weights)
        {
            if (pair.Value > maxW)
            {
                maxW = pair.Value;
                fallbackDir = pair.Key;
            }
        }
        return fallbackDir;
    }
    protected virtual Dictionary<Vector3, float> GetConsiderationVector()
    {
        var considerationVec = new Dictionary<Vector3, float>();
        considerationVec = AIRayDetector.Raycasts.Keys.ToDictionary(v => v, v => 0f);
        //{
        //    { Dir16.U, 0f }, { Dir16.UUR, 0f }, { Dir16.UR, 0f }, { Dir16.URR, 0f },
        //    { Dir16.R, 0f }, { Dir16.DRR, 0f }, { Dir16.DR, 0f }, { Dir16.DDR, 0f },
        //    { Dir16.D, 0f }, { Dir16.DDL, 0f }, { Dir16.DL, 0f }, { Dir16.DLL, 0f },
        //    { Dir16.L, 0f }, { Dir16.ULL, 0f }, { Dir16.UL, 0f }, { Dir16.UUL, 0f }
        //};



        foreach (var entityConsid in EntityConsiderations)
        {
            var entityConsidVec = entityConsid.GetConsiderationVector(AIRayDetector);
            foreach (var dir in considerationVec.Keys)
            {
                considerationVec[dir] += entityConsidVec[dir];
            }
        }
        //foreach (var pair in Rays.Raycasts)
        //{
        //    var dir = pair.Key;
        //    var raycast = pair.Value;
        //    var castLength = raycast.TargetPosition.Length();
        //    if (raycast.IsColliding())
        //    {
        //        var rayCollision = raycast.GetCollider() as CollisionObject3D;
        //        if (rayCollision == ParentAgent) { continue; } //TODO: better fix


        //        //GD.Print($"Raycast colliding with {rayCollision.Name} @ dir {dir}!" +
        //        //    $"\n\tColl layer val: {rayCollision.CollisionLayer}" +
        //        //    $"\n\tColl Dist: {collDist}, weight: {distWeight}");
        //        foreach (var navLayer in NavLayerMap)
        //        {
        //            if (rayCollision.GetCollisionLayerValue((int)navLayer.Value))
        //            {
        //                var collDist = (raycast.GetCollisionPoint() - raycast.GlobalPosition).Length();
        //                // the closer the collision is to the raycast, the higher the "danger" weight
        //                // at max dist, weight is 0. TODO: change s.t. it's not zero, just lower (i.e. 0.1)
        //                // at dist 0, weight is 1.0. TODO: change s.t. weight being 0 occurs not only at dist 0, since that is impossible and too late
        //                //var distWeight = 1f - (collisionDist / castLength); 
        //                var minWeight = 0.1f;
        //                var k = 2.5f;
        //                var distDropThresh = NavDistThresh[navLayer.Key];//1.5f; 
        //                float distWeight;
        //                if (collDist <= distDropThresh)
        //                {
        //                    distWeight = 1.0f;  // Ensure max weight
        //                }
        //                else
        //                {
        //                    distWeight = minWeight + (1.0f - minWeight) * (float)Math.Exp(-k * (collDist - distDropThresh) / (castLength - distDropThresh));
        //                }
        //                //GD.Print($"Raycast found danger {navLayer.Key} @ dir {dir}!");
        //                //var spatialAwarenessMod = SpatialAwarenessWeights[dir.GetAIFacing(_moveComp.GetFaceDirection())];
        //                var dangerAmt = NavWeights[navLayer.Key] * distWeight;
        //                    //* spatialAwarenessMod;

        //                dangerVector[dir] += dangerAmt;

        //                //PROPOGATE DANGER OUT
        //                var propogateNum = 3;
        //                var propLDir = dir;
        //                var propRDir = dir;
        //                var weightDrop = 0.75f;
        //                var propWeight = 0.5f;
        //                while (propogateNum > 0)
        //                {
        //                    propLDir = propLDir.GetLeftDir();
        //                    propRDir = propRDir.GetRightDir();
        //                    dangerVector[propLDir] += dangerAmt * propWeight;
        //                    dangerVector[propRDir] += dangerAmt * propWeight;

        //                    propWeight *= weightDrop;
        //                    propogateNum--;
        //                }


        //                // add danger to any danger dir in a 45 degree sweep
        //                //TODO: to increase performance, static these comparisons for quicker calcs
        //                //foreach (var dir8 in dangerVector.Keys)
        //                //{
        //                    //var angle = Mathf.Abs(dir8.GetVector2().GetAngleToVector(dir.GetVector2()));
        //                    //var dangerAngle = 30f;
        //                    //if (angle <= dangerAngle)
        //                    //{
        //                    //    // base is 0.5, max is 1
        //                    //    var angleDangerMod = 1.0f - ((angle / 2) / dangerAngle);
        //                    //    //GD.Print($"{dir8} danger amt for cast {dir}: {NavWeights[navLayer.Key]} * {distWeight} " +
        //                    //    //    $"* {spatialAwarenessMod} * {angleDangerMod}");
        //                    //    dangerVector[dir8] += dangerAmt * angleDangerMod;
        //                    //}
        //                //}

        //            }
        //        }
        //    }
        //}

        return considerationVec;
    }
    private void SignalRaycastDetections()
    {
        foreach (var raycast in AIRayDetector.GetAllRaycasts())
        {
            if (!raycast.IsColliding()) { continue; }
            var detectObj = raycast.GetCollider();
            if (detectObj is Area3D detectArea)
            {
                AreaDetected?.Invoke(this, detectArea);
            }
            else if (detectObj is Node3D detectBody)
            {
                BodyDetected?.Invoke(this, detectBody);
            }
            else
            {
                GD.PrintErr($"ERROR || Collider of raycast isn't Node3D?\nCollider object: {detectObj}");
            }
        }
    }
    protected virtual Dictionary<Dir8, float> GetInterestVector()
    {
        var interestVector = new Dictionary<Dir8, float>();



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
            var mapDistAllowance = 1f; // 0.01f
            GD.Print("setting path, map dist: ", mapDist);
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
            GD.Print("failed to calc path");
            return false; // wasn't able to set path
        }
        else
        {
            GD.Print("successfully calc path");
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
