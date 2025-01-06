using Godot;
using System;
using System.Collections.Generic;

[GlobalClass, Tool]
public partial class EaterComponent : Node
{
    private Node3D _eaterBody;
	[Export]
	private HitboxComponent3D _hitboxComp;
    [Export]
    private SpriteOrthogComponent _orthogSprite;

	public EatableComponent CurrEatable { get; private set; }
    private Node3D _eatableBody;

	public event EventHandler<EatableComponent> EatableHit;
    public event EventHandler<EatableComponent> ThrowPathFinished;
    public event EventHandler<EatableComponent> StartedEating;
    public event EventHandler<EatableComponent> FinishedEating;

    private bool _isEating = false;

    // 1 = max height; 0.25 = 25% of max height, etc.
    protected List<float> YCurvePercentage = 
        new List<float>() { 0.4f, 1f, 0f };

    public override void _Ready()
	{
		base._Ready();
        _eaterBody = GetOwner<Node3D>();
        _hitboxComp.HurtboxEntered += OnHurtboxEntered;
	}

    public override void _Process(double delta)
	{
	}
    public void ThrowEatable()
    {
        if (!_isEating) { return; } //ERROR

        // Get the current position of the throw object and player
        Vector3 startPosition = _eatableBody.GlobalPosition;
        //startPosition.Y += _orthogSprite.SpriteHeight;
        Vector3 endPosition = _eaterBody.GlobalPosition;
        endPosition.Y += _orthogSprite.SpriteHeight;

        // Create a Tween node to animate the object
        var tween = CreateTween();

        // Duration of the throw (in seconds)
        float throwDuration = Global.GetRndInRange(1f, 2f); // TODO: MAKE BASED ON MONSTER FORM AND STRENGTH
        float throwHeight = throwDuration * 4f; // TODO: CALC HEIGHT BETTER


        var midXZ = GetTransitionPoint(startPosition, endPosition, .4f);
        Vector3 topThrowP = new Vector3(
            midXZ.X,
            midXZ.Y + throwHeight,
            midXZ.Z
            );
        //// Start position, end position, and the apex of the throw
        //Vector3 midPoint = new Vector3((startPosition.X + endPosition.X) / 2,
        //                               Mathf.Max(startPosition.Y, endPosition.Y) + 5, // Higher apex
        //                               (startPosition.Z + endPosition.Z) / 2);


        List<Vector3> throwCurve = new List<Vector3>
        {
            startPosition,
            topThrowP,
            endPosition
        };

        tween.TweenMethod(Callable.From<float>((weight) => CalcThrowCurve(throwCurve, weight)), 0f, 1f, throwDuration).SetEase(Tween.EaseType.OutIn);//.SetTrans(Tween.TransitionType.Circ);
        tween.TweenCallback(Callable.From(() => ThrowPathFinished?.Invoke(this, CurrEatable)));
        //ThrowPathFinished?.Invoke(this, CurrEatable);
    }
    public Vector3 GetTransitionPoint(Vector3 pointA, Vector3 pointB, float t) // where t is that % between
    {
        // Ensure t is between 0 and 1 for valid interpolation.
        t = Mathf.Clamp(t, 0.0f, 1.0f);

        // Linear interpolation formula
        return pointA + (pointB - pointA) * t;
    }
    public virtual void CalcThrowCurve(List<Vector3> throwCurve, float weight)
    {
        _eatableBody.GlobalPosition = 
            Global.QuadraticBezier3D(throwCurve[0], throwCurve[1], throwCurve[2], weight);
    }
    public void CommenceConsumption()
    {
        //TODO: more? control eatable?
        StartedEating?.Invoke(this, CurrEatable);
    }
    public void CompletedConsumption()
    {
        FinishedEating?.Invoke(this, CurrEatable);
        _isEating = false;
    }
    private void OnHurtboxEntered(HurtboxComponent3D hurtbox)
    {
        if (_isEating) { return; }
        var eatableComp = hurtbox.GetFirstChildOfType<EatableComponent>();
        if (eatableComp == null)
        {
			return;
        }
        CurrEatable = eatableComp;
        _eatableBody = CurrEatable.GetOwner<Node3D>();
        EatableHit?.Invoke(this, CurrEatable);
        _isEating = true;
    }

}
