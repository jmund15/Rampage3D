using Godot;
using Godot.Collections;
using System;

[GlobalClass, Tool]
public partial class Breakable3DComponent : Node3D
{
	#region COMPONENT_VARIABLES
	[Export]
	public Node3D BreakableParent { get; private set; }
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
		BB.SetVar(BBDataSig.Agent, BreakableParent);
		BB.SetVar(BBDataSig.HealthComp, HealthComp);
		BB.SetVar(BBDataSig.HurtboxComp, HurtboxComp);

		OnDamageStrategy.BB = BB;
		OnDamageStrategy.Breakable = BreakableParent;
		OnDestroyStrategy.BB = BB;
		OnDestroyStrategy.Breakable = BreakableParent;

		HealthComp.Damaged += OnDamaged;
        HealthComp.Destroyed += OnDestroyed;
	}


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
        OnDestroyStrategy.Destroy();
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
