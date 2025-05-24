using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public partial class SetNavPoint : BehaviorAction
{
    #region TASK_VARIABLES
    [Export]
    private Vector3 _navPoint = new Vector3();
    private AINav3DComponent _aiNavComp;

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
        if (!_aiNavComp.NavigationEnabled)
        {
            _aiNavComp.EnableNavigation();
        }
        if (NavigationServer3D.MapGetIterationId(_aiNavComp.GetNavigationMap()) == 0) // not setup yet
		{
            _mapLoaded = false;
            NavigationServer3D.MapChanged += OnMapChanged;
            GD.Print("haven't loaded nav map");
        }
		else
		{
            _mapLoaded = true;
            GD.Print("loaded nav map, settng nav now");
            SetNav();
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
        GD.Print("MAP LOADED");
        if (map == _aiNavComp.GetNavigationMap())
        {
            SetNav();
        }
    }
    private void SetNav()
    {
        _aiNavComp.SetTarget(_navPoint, true);
        GD.Print("Nav point set: ", _navPoint);
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

