using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public partial class SetBBValue : BTCondition
{
	#region TASK_VARIABLES
	[Export]
	private BBDataSig _valueToSet;
	[Export]
	//private Variant _value;
	private bool _value; //TODO: WHEN GODOT SUPPORT VARIANT EXPORT ADD BACK
	public SetBBValue()
	{
		_valueToSet = BBDataSig.Agent;
		_value = default;
    }
	#endregion
	#region TASK_UPDATES
	public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
        ConditionName = $"_Set{_valueToSet}To{_value}";
    }
    public override void Enter()
	{
		base.Enter();
		BB.SetPrimVar(_valueToSet, _value);
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
    #endregion
}

