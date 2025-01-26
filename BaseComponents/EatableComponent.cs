using Godot;
using System;

[GlobalClass, Tool]
public partial class EatableComponent : Node
{
    [Export]
    public Node3D Body;
    [Export]
    public float HungerSatiationValue { get; private set; } = 1f;
	[Export]
	private HurtboxComponent3D _hurtboxComp;
    public EaterComponent Eater { get; private set; }

	public event EventHandler<EaterComponent> Grabbed;
    public event EventHandler<EaterComponent> InMouth;
    public event EventHandler<EaterComponent> Eaten;
	public override void _Ready()
	{
		base._Ready();

		_hurtboxComp.HitboxEntered += OnHitboxEntered;
	}
    public override void _Process(double delta)
	{
	}
    private void OnHitboxEntered(HitboxComponent3D hitbox)
    {
        var eaterComp = hitbox.GetFirstChildOfType<EaterComponent>();
        if (eaterComp == null)
        {
            return;
        }
        Eater = eaterComp;
        Eater.GrabbedEatable += OnGrabbed;
        Eater.EatingEatable += OnInMouth;
        Eater.AteEatable += OnEaten;
    }

    private void OnGrabbed(object sender, EatableComponent e)
    {
        Grabbed?.Invoke(this, Eater);
    }
    private void OnInMouth(object sender, EatableComponent e)
    {
        InMouth?.Invoke(this, Eater);
    }
    private void OnEaten(object sender, EatableComponent e)
    {
        Eaten?.Invoke(this, Eater);
        Body.CallDeferred(MethodName.QueueFree);
    }
}
