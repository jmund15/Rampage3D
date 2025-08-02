using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public partial class TargetVehicleEmbarkableCondition : BTCondition
{
    #region TASK_VARIABLES
    private VehicleOccupantsComponent _targetVehicleOccComp;

    public TargetVehicleEmbarkableCondition()
    {
        _targetVehicleOccComp = null;
    }
    #endregion
    #region TASK_UPDATES
    public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
        
    }
    public override void Enter()
	{
		base.Enter();
        _targetVehicleOccComp = BB.GetVar<VehicleSeat>(BBDataSig.TargetOrOccupiedVehicleSeat).VOccupantComp;
        if (_targetVehicleOccComp == null)
        {
            //GD.PushError($"TargetVehicleEmbarkableCondition: Target vehicle is null for agent {Agent.Name}.");
            OnExitTask();
            return;
        }
        if (!_targetVehicleOccComp.IsEmbarkable)
        {
            OnExitTask();
            return;
        }

        _targetVehicleOccComp.EmbarkableStatusChanged += OnEmbarkStatusChanged;

    }
    public override void Exit()
	{
		base.Exit();
        _targetVehicleOccComp.EmbarkableStatusChanged -= OnEmbarkStatusChanged;
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
    private void OnEmbarkStatusChanged(object sender, bool canEmbark)
    {
        if (SucceedTaskOnExit && canEmbark) // task succeeds when vehicle becomes embarkable
        { 
            OnExitTask();
        }
        else if (!canEmbark) // task fails when vehicle is no longer embarkable
        {
            OnExitTask();
        }
    }
    #endregion
}

