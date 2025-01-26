using Godot;
using Godot.Collections;
using System;

[GlobalClass, Tool]
public partial class HungerBarComponent : Control
{
	#region COMPONENT_VARIABLES
	[Export]
	private Node3D _followTarget;
	private Vector3 _barOffset = new Vector3(0, -0.75f, 0);
	[Export]
	private EaterComponent _eaterComp;

	private ProgressBar _hungerBar;

	[Export]
	public Godot.Collections.Array<float> HungerSatiationIndex { get; private set; } = new Array<float>()
	{
		100f, // form 1 -> 2
		500f // form 2 -> 3
		//  any monsters with secret/more than 3 forms add here ! (so it's modular, yes i am a genius thank you for noticing)
	};
	public int TimesSatiated { get; private set; } = 0;
    public event EventHandler<int> SatiateToNewForm;
	#endregion
	#region COMPONENT_UPDATES
	public override void _Ready()
	{
		base._Ready();
        _hungerBar = this.GetFirstChildOfType<ProgressBar>();

        if (Engine.IsEditorHint() && (_followTarget == null || _eaterComp == null)) { return; }
		_eaterComp.AteEatable += OnFinishedEating;
		SatiateToNewForm += OnSatiateToNewForm;

        _hungerBar.MaxValue = HungerSatiationIndex[TimesSatiated];
        _hungerBar.MinValue = 0f;
        _hungerBar.SetValueNoSignal(0f);
		GD.Print("current hb value: ", _hungerBar.Value);
    }
    public override void _Process(double delta)
	{
		base._Process(delta);
		if (Engine.IsEditorHint()) { return; }
		var pos_3d = _followTarget.GlobalPosition + _barOffset;
		var cam = GetViewport().GetCamera3D();
		var pos_2d = cam.UnprojectPosition(pos_3d);
		GlobalPosition = pos_2d;
		Visible = !cam.IsPositionBehind(pos_3d);
    }
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
	}
    #endregion
    #region COMPONENT_HELPER
    #endregion
    #region SIGNAL_LISTENERS
    private void OnFinishedEating(object sender, EatableComponent e)
    {
        _hungerBar.Value += e.HungerSatiationValue;

		if (_hungerBar.Value >= _hungerBar.MaxValue)
		{
			SatiateToNewForm?.Invoke(this, TimesSatiated);
		}
    }
	private void OnSatiateToNewForm(object sender, int e)
	{
		var overflowSatiation = _hungerBar.Value - _hungerBar.MaxValue;
		TimesSatiated++;
		_hungerBar.	Value = _hungerBar.MinValue + overflowSatiation;
        _hungerBar.MaxValue = HungerSatiationIndex[TimesSatiated];
	}
    #endregion
}
