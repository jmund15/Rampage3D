using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

[GlobalClass, Tool]
public partial class Breakable3DComponent : Node3D
{
	#region COMPONENT_VARIABLES
	[Export]
	public Node3D Visuals { get; private set; }
	[Export]
	public Blackboard BB { get; private set; }
	[Export]
	public HealthComponent HealthComp { get; set; }
	[Export]
	public HurtboxComponent3D HurtboxComp { get; set; }
	[Export]
	public Godot.Collections.Array<BreakableOnDamageStrategy> OnDamageStrategies { get; set; }
	[Export]
	public Godot.Collections.Array<BreakableOnDestroyStrategy> OnDestroyStrategies { get; set; }
	#endregion
	#region COMPONENT_UPDATES
	public override void _Ready()
	{
		base._Ready();
		if (Engine.IsEditorHint()) { return; }
        BB.SetVar(BBDataSig.Agent, this);
        BB.SetVar(BBDataSig.Sprite, Visuals);
		BB.SetVar(BBDataSig.HealthComp, HealthComp);
		BB.SetVar(BBDataSig.HurtboxComp, HurtboxComp);

		foreach (var damageStrat in OnDamageStrategies)
		{
            damageStrat.BB = BB;
            damageStrat.Breakable = this;
        }
        foreach (var destroyStrat in OnDestroyStrategies)
        {
            destroyStrat.BB = BB;
            destroyStrat.Breakable = this;
        }

		HealthComp.Damaged += OnDamaged;
        HealthComp.Destroyed += OnDestroyed;
	}
    public override void _Process(double delta)
	{
		base._Process(delta);
	}
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
	}
    #endregion
    #region COMPONENT_HELPER
    #endregion
    #region SIGNAL_LISTENERS
    private void OnDamaged(object sender, HealthUpdate e)
    {
        if (HealthComp.IsDead) { return; }
        foreach (var damageStrat in OnDamageStrategies)
        {
            damageStrat.Damage();
        }
    }
    private void OnDestroyed(HealthUpdate destroyUpdate)
    {
        //BB.SetVar(BBDataSig.)
        HurtboxComp.SetDeferred(Area3D.PropertyName.Monitorable, false);
        HurtboxComp.SetDeferred(Area3D.PropertyName.Monitoring, false);
        foreach (var destroyStrat in OnDestroyStrategies)
        {
            destroyStrat.Destroy();
        }
    }
    #endregion
    #region HELPER_CLASSES
    public enum BreakableDataSig
	{
		Breakable,
		HealthComp,
		HurtboxComp
	}
    #endregion
}
