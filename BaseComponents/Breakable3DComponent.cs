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
	public BreakableOnDamageStrategy OnDamageStrategy { get; set; }
	[Export]
	public BreakableOnDestroyStrategy OnDestroyStrategy { get; set; }
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

		OnDamageStrategy.BB = BB;
		OnDamageStrategy.Breakable = this;
		OnDestroyStrategy.BB = BB;
		OnDestroyStrategy.Breakable = this;

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
        //BB.SetVar(BBDataSig.)
        GD.Print("ON DAMAGED BREAKABLE");
        if (HealthComp.IsDead) { return; }
        OnDamageStrategy.Damage();
    }
    private void OnDestroyed(HealthUpdate destroyUpdate)
    {
        //BB.SetVar(BBDataSig.)
        HurtboxComp.SetDeferred(Area3D.PropertyName.Monitorable, false);
        HurtboxComp.SetDeferred(Area3D.PropertyName.Monitoring, false);
        OnDestroyStrategy.Destroy();
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
