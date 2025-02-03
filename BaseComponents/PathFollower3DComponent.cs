using Godot;
using Godot.Collections;
using System.Collections.Generic;

[GlobalClass, Tool]
public partial class PathFollower3DComponent : Node
{
    #region COMPONENT_VARIABLES
    private Node3D _followAgent;
	[Export]
	private AINav3DComponent _aiNavComp;


	private Path3D _currPath;
    public int NumPathControlPoints { get; private set; }
    public int NumPathBakedPoints { get; private set; }

    private Vector3 _currTargetPoint;
    private int _currPathPIdx;

    private bool _searchingForPath = false;
    private Timer _searchForPathTimer;
    private float _researchTimeInt = 0.1f;
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
	{
		base._Ready();
        _followAgent = _aiNavComp.ParentAgent;
        _searchForPathTimer = GetNode<Timer>("SearchForPathTimer");
        _searchForPathTimer.WaitTime = _researchTimeInt;
        _searchForPathTimer.Timeout += OnSearchForPathTimeout;

    }
	public override void _Process(double delta)
	{
		base._Process(delta);
	}
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
        if (_searchingForPath)
        {
            var newClosestSearch = _currPath.Curve.GetClosestPoint(_followAgent.Position);
            if (_currTargetPoint != newClosestSearch) // handle nav vs real dist differences,d
            {
                _currTargetPoint = newClosestSearch;
                _aiNavComp.SetTarget(_currTargetPoint, true);
                _searchingForPath = false;
                _searchForPathTimer.Start();
            }
        }
    }
	#endregion
	#region COMPONENT_HELPER
	public void SetPath(Path3D path)
	{
        _currPath = path;
        NumPathControlPoints = _currPath.Curve.PointCount;
        NumPathBakedPoints = _currPath.Curve.GetBakedPoints().Length;
    }
    public void FollowPath()
    {
        _aiNavComp.NavigationFinished += OnPathPointReached;
        //(_currTargetPoint, _currPathPIdx)  = AINav.FindNearestPathPoint();
        _currTargetPoint = _currPath.Curve.GetClosestPoint(_followAgent.Position);
        _currPathPIdx = GetPathPointIdx(_currTargetPoint);
        _searchingForPath = false;
        _searchForPathTimer.Start();
        _aiNavComp.SetTarget(_currTargetPoint, true);
        //_currPathPIdx = AINav.GetPathPointIdx(_currTargetPoint); //AINav._currPath.Curve.GetClosestPoint(_currTargetPoint);
        GD.Print("found nearest path point: ", _currTargetPoint, "\npath idx: ", _currPathPIdx);
    }
    public void StopFollow()
    {
        _searchingForPath = false;
        _aiNavComp.NavigationFinished -= OnPathPointReached;
    }
    public (Vector3, int) FindNearestPathPoint()
    {
        // "_currPath.Curve.GetClosestPoint()" would work, but it doesn't account for navigation distance
        float lowestDist = 9999f;
        Vector3 nearingPathPoint = Vector3.Inf; //returns inf if can't find path point
        int idx = -1;
        for (int i = 0; i < NumPathBakedPoints; i++)
        {
            var pathP = _currPath.Curve.GetBakedPoints()[i];
            var navDist = _aiNavComp.NavDistanceToPoint(pathP);
            if (navDist < lowestDist)
            {
                lowestDist = navDist;
                nearingPathPoint = pathP;
                idx = i;
            }
            GD.Print("path point (", pathP, ") is at nav dist: ", navDist);
        }
        return (nearingPathPoint, idx);
    }
    public int GetPathPointIdx(Vector3 pathP, float withinDist = -1f)
    {
        if (withinDist < 0f)
        {
            withinDist = _currPath.Curve.BakeInterval;
        }
        var bakedLength = _currPath.Curve.GetBakedPoints().Length; //_currPath.Curve.GetBakedLength();
        GD.Print("searching for path point: ", pathP);
        for (int i = 0; i < bakedLength; i++)
        {
            var p = _currPath.Curve.GetBakedPoints()[i];
            //GD.Print("scanning point ", i, ": ", _currPath.Curve.GetBakedPoints()[i]);
            if (pathP.DistanceTo(p) <= withinDist)
            {
                GD.Print("found sufficient path point: ", p);
                return i;
            }
        }
        return -1; // if no idx found, return -1
    }
    #endregion
    #region SIGNAL_LISTENERS
    private void OnPathPointReached()
    {
        _searchingForPath = false;
        _searchForPathTimer.Stop();
        if (++_currPathPIdx == NumPathBakedPoints)
        {
            _currPathPIdx = 0;
        }
        _currTargetPoint = _currPath.Curve.GetBakedPoints()[_currPathPIdx];
        _aiNavComp.SetTarget(_currTargetPoint, true);
    }
    private void OnSearchForPathTimeout()
    {
        _searchingForPath = true;
    }
    #endregion

}
