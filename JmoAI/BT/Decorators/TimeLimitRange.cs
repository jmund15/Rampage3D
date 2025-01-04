using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

[GlobalClass, Tool]
public partial class TimeLimitRange : BTCondition
{
    #region TASK_VARIABLES
    private float _min;
    private float _max;

    [Export(PropertyHint.Range, "0.0, 10.0, 0.1, or_greater")]
    public float LimitMin
    {
        get => _min;
        set
        {
            if (value == _min) { return; }
            _min = value;
            LimitMax = Mathf.Max(_max, _min);
        }
    }

    [Export(PropertyHint.Range, "0.0, 10.0, 0.1, or_greater")]
    public float LimitMax
    {
        get => _max;
        set
        {
            if (value == _max) { return; }
            _max = value;
            LimitMin = Mathf.Clamp(_min, 0.0f, _max);
        }
    }

    private float _limit;
    private float _elasped;
	#endregion
	#region TASK_UPDATES
	public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
        ConditionName = $"*TimeLimitRange:{LimitMin}-{LimitMax}*";
    }
    public override void Enter()
	{
		base.Enter();
        _elasped = 0f;
        _limit = Global.GetRndInRange(LimitMin, LimitMax);
    }
	public override void Exit()
	{
		base.Exit();
    }
    public override void ProcessFrame(float delta)
	{
        base.ProcessFrame(delta);
		_elasped += delta;
        //GD.Print("TIMELIMITRANGE CURRENT ELAPSED = ", _elasped);
		if (_elasped >= _limit)
		{
            //GD.Print("TIME LIMIT RANGE CONDITION FULL ELASPED");
			OnExitTask();
		}
    }
	public override void ProcessPhysics(float delta)
	{
		base.ProcessPhysics(delta);
    }
	#endregion
	#region TASK_HELPER
    #endregion
}

