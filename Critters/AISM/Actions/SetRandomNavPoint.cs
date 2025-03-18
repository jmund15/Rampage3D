using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public partial class SetRandomNavPoint : BehaviorAction
{
    #region TASK_VARIABLES
    private AINav3DComponent _aiNavComp;
	private Vector3 _navPoint;

	private bool _mapLoaded;
    #endregion
    #region TASK_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        base.Init(agent, bb);
        _aiNavComp = BB.GetVar<AINav3DComponent>(BBDataSig.AINavComp);
    }
    public override void Enter()
	{
		base.Enter();
		if (NavigationServer3D.MapGetIterationId(_aiNavComp.GetNavigationMap()) == 0) // not setup yet
		{
            _mapLoaded = false;
            NavigationServer3D.MapChanged += OnMapChanged;
        }
		else
		{
            _mapLoaded = true;
            GetRandomPoint();
        }
		
	}
    public override void Exit()
	{
		base.Exit();
        if (_mapLoaded == false)
        {
            NavigationServer3D.MapChanged -= OnMapChanged;
        }
	}
	public override void ProcessFrame(float delta)
	{
		base.ProcessFrame(delta);
	}
	public override void ProcessPhysics(float delta)
	{
		base.ProcessPhysics(delta);
	}
    #endregion
    #region TASK_HELPER
    private void OnMapChanged(Rid map)
    {
        if (map == _aiNavComp.GetNavigationMap())
        {
            GetRandomPoint();
        }
    }
    private void GetRandomPoint()
    {
        do
        {
            _navPoint = NavigationServer3D.MapGetRandomPoint(_aiNavComp.GetNavigationMap(), _aiNavComp.NavigationLayers, true);
            GD.Print("calc'd nav point: ", _navPoint);
        } while (_navPoint.Y >= 1);
        _aiNavComp.SetTarget(_navPoint, true);
        //GD.Print("Nav point found: ", _navPoint);
        Status = TaskStatus.SUCCESS;
    }
    public override string[] _GetConfigurationWarnings()
	{
		var warnings = new List<string>();

		//

		return warnings.Concat(base._GetConfigurationWarnings()).ToArray();
	}
	#endregion
}

