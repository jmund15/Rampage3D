using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class FindEnterableBuilding : BehaviorAction
{
	#region TASK_VARIABLES
	private AINav3DComponent _aiNav;
	private City _residingCity;

	private bool _setNav;
	private bool _mapLoaded;
	#endregion
	#region TASK_UPDATES
	public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
		_aiNav = BB.GetVar<AINav3DComponent>(BBDataSig.AINavComp);

    }
	public override void Enter()
	{
		base.Enter();
		_residingCity = NodeExts.GetFirstNodeOfTypeInScene<City>();
		_setNav = false;

        if (NavigationServer3D.MapGetIterationId(_aiNav.GetNavigationMap()) == 0) // not setup yet
        {
            _mapLoaded = false;
            NavigationServer3D.MapChanged += OnMapChanged;
        }
        else
        {
            _mapLoaded = true;
            CallDeferred(MethodName.SetBuildingEntranceNavTarget);
        }
    }

    private void OnMapChanged(Rid map)
    {
        if (map == _aiNav.GetNavigationMap())
        {
            SetBuildingEntranceNavTarget();
        }
    }

    public override void Exit()
	{
		base.Exit();
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
	private void SetBuildingEntranceNavTarget()
	{
        _setNav = false;
        foreach (var building in _residingCity.Buildings)
		{
			if (!building.Enterable) { continue; }
			var entranceLoc = new Vector3(
				building.GlobalPosition.X + building.DoorEntranceOffset.X,
				building.GlobalPosition.Y,
				building.GlobalPosition.Z + building.DoorEntranceOffset.Y);
			GD.Print($"Attempting to set building entrance nav target to: " +
				$"{entranceLoc}!");
			if (_aiNav.SetTarget(entranceLoc, true))
			{
				_setNav = true;
			}
			else
			{
				continue;
			}
		}

        if (_setNav)
        {
            GD.Print($"Set building entrance target Sucessfully!!");
            Status = TaskStatus.SUCCESS;
        }
        else
        {
            GD.Print($"Couldn't set building entrance target...");
            Status = TaskStatus.FAILURE;
        }
    }
	public override string[] _GetConfigurationWarnings()
	{
		var warnings = new List<string>();

		//

		return warnings.Concat(base._GetConfigurationWarnings()).ToArray();
	}
	#endregion
}

