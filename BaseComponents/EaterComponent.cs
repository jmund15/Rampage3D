using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

[GlobalClass, Tool]
public partial class EaterComponent : Node
{
    public enum EatStatus
    {
        Restricted,
        Idle,
        Grabbing,
        EatAnticipation,
        Eating
    }
    [Export]
    public Node3D Body { get; private set; }
    [Export]
    private string _grabAnimName = "grab";
    [Export]
    private string _eatAnimName = "eat";

    private IBlackboard _bb;

	private HitboxComponent3D _hitboxComp;
    private SpriteOrthogComponent _orthogSprite;
    private AnimationPlayer _animPlayer;

    private EatStatus _status = EatStatus.Restricted;
    public EatStatus Status
    {
        get => _status; 
        private set
        {
            if (_status == value) { return; }
            switch (value)
            {
                case EatStatus.Restricted:
                    break;
                case EatStatus.Idle:
                    break;
                case EatStatus.Grabbing:
                    break;
                case EatStatus.EatAnticipation:
                    break;
                case EatStatus.Eating:
                    break;
                default:
                    break;
            }
            _status = value;
        }
    }
    public EatableComponent MostRecentEatableDetected { get; private set; }
    public Queue<EatableComponent> EatQueue { get; private set; } = new Queue<EatableComponent>();
    public List<EatableComponent> GettingEaten { get; private set; } = new List<EatableComponent>();
    private bool _allowEating = false;
    public bool AllowEating { 
        get => _allowEating; 
        set
        {
            if (_allowEating == value) { return; }
            _allowEating = value;

            if (_allowEating) { 
                Status = EatStatus.Idle; 
                //if (EatQueue.Count > 0)
                //{
                //    StartEatCycle();
                //}
                //else
                //{
                //    //notify nothing to eat?
                //}
            }
            else { Status = EatStatus.Restricted; }
        } 
    }

    private float _currThrowTime = 0f;
    //TODO: BASE ON MONSTER MASS/FORM ATTRIBUTES
    private Vector2 _throwTimeRange = new Vector2(1.0f, 1.25f);

    //TODO: ADD FUNCTIONALITY FOR EAT INTERUPTION (npcs land safely instead of getting eaten)

    public event EventHandler<EatableComponent> EatableHit;

    public event EventHandler<EatableComponent> GrabbedEatable;
    public event EventHandler<EatableComponent> EatingEatable;
    public event EventHandler<EatableComponent> AteEatable;

    public event EventHandler StartedEatingCycle;
    public event EventHandler FinishedEatingCycle;

    // 1 = max height; 0.25 = 25% of max height, etc.
    private List<float> _yCurvePercentage = 
        new List<float>() { 0.4f, 1f, 0f };

    public override void _Ready()
	{
		base._Ready();
        if (Engine.IsEditorHint()) { return; }
        _bb = Body.GetFirstChildOfType<Blackboard>();
        CallDeferred(MethodName.InitEaterVariables);
	}
    private void InitEaterVariables()
    {
        _hitboxComp = _bb.GetVar<HitboxComponent3D>(BBDataSig.HitboxComp);
        _orthogSprite = _bb.GetVar<SpriteOrthogComponent>(BBDataSig.Sprite);
        _animPlayer = _bb.GetVar<AnimationPlayer>(BBDataSig.Anim);

        _hitboxComp.HurtboxEntered += OnHurtboxEntered;
        _hitboxComp.AttackFinished += OnAttackFinished;
    }
    public override void _Process(double delta)
	{        
        if (Status == EatStatus.Idle && EatQueue.Count > 0)
        {
            StartEatCycle();
        }

        if (Status == EatStatus.Grabbing)
        {
            // don't need to subtract after grabbing since nothing can be grabbed
            // plus a scene tree timer is used for continuing eating anyway
            _currThrowTime -= (float)delta;

            while (EatQueue.Count > 0)
            {
                var eatable = EatQueue.Dequeue();

                ThrowEatable(eatable);
                GrabbedEatable?.Invoke(this, eatable);
                GD.Print($"Grabbed eatable '{eatable.Body.Name}'");
            }
        }
    }
    private void StartEatCycle()
    {
        Status = EatStatus.Grabbing;
        _orthogSprite = _bb.GetVar<SpriteOrthogComponent>(BBDataSig.Sprite);
        _animPlayer = _bb.GetVar<AnimationPlayer>(BBDataSig.Anim);
        _animPlayer.AnimationFinished += OnAnimationFinished;

        _animPlayer.Play(_grabAnimName + _animPlayer.GetAnimDirection());
        _hitboxComp.HitboxActivate();

        _currThrowTime = Global.GetRndInRange(_throwTimeRange.X, _throwTimeRange.Y);
        GetTree().CreateTimer(_currThrowTime).Timeout += OnThrowFinished;

        StartedEatingCycle?.Invoke(this, EventArgs.Empty);
    }
    private void ThrowEatable(EatableComponent eatable)
    {
        GettingEaten.Add(eatable);

        // Get the current position of the throw object and player
        Vector3 startPosition = eatable.Body.GlobalPosition;
        //startPosition.Y += _orthogSprite.SpriteHeight;
        Vector3 endPosition = Body.GlobalPosition;
        endPosition.X += Global.GetRndInRange(-0.1f, 0.1f);
        endPosition.Y += _orthogSprite.SpriteHeight / 2;
        endPosition.Z += Global.GetRndInRange(-0.1f, 0.1f);

        // Create a Tween node to animate the object
        var tween = CreateTween();

        // Height of the throw (in seconds)
        float throwHeight = Global.GetRndInRange(_currThrowTime * 4f, _currThrowTime * 5f); 

        var midXZ = GetTransitionPoint(startPosition, endPosition, .4f);
        var xOffsetRnd = Global.GetRndInRange(-0.25f, 0.25f);
        var zOffsetRnd = Global.GetRndInRange(-0.25f, 0.25f);
        Vector3 topThrowP = new Vector3(
            midXZ.X + xOffsetRnd,
            midXZ.Y + throwHeight,
            midXZ.Z + zOffsetRnd
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

        tween.TweenMethod(Callable.From<float>((weight) => CalcThrowCurve(eatable.Body, throwCurve, weight)), 0f, 1f, _currThrowTime).SetEase(Tween.EaseType.OutIn);//.SetTrans(Tween.TransitionType.Circ);
    }
    public Vector3 GetTransitionPoint(Vector3 pointA, Vector3 pointB, float t) // where t is that % between
    {
        // Ensure t is between 0 and 1 for valid interpolation.
        t = Mathf.Clamp(t, 0.0f, 1.0f);

        // Linear interpolation formula
        return pointA + (pointB - pointA) * t;
    }
    public virtual void CalcThrowCurve(Node3D thrownBody, List<Vector3> throwCurve, float weight)
    {
        thrownBody.GlobalPosition = 
            Global.QuadraticBezier3D(throwCurve[0], throwCurve[1], throwCurve[2], weight);
    }
    private void OnHurtboxEntered(HurtboxComponent3D hurtbox)
    {
        var eatableComp = hurtbox.GetFirstChildOfType<EatableComponent>();
        if (eatableComp == null)
        {
			return;
        }
        MostRecentEatableDetected = eatableComp;
        if (EatQueue.Contains(eatableComp) ||
            GettingEaten.Contains(eatableComp)) { return; } 
        EatQueue.Enqueue(eatableComp);
        EatableHit?.Invoke(this, MostRecentEatableDetected);
        //GD.Print("EATABLE HIT: ",  MostRecentEatableDetected.Body.Name);
    }
    private void OnAttackFinished()
    {
        //EatQueue.Clear();
    }
    private void OnThrowFinished()
    {
        if (Status == EatStatus.Grabbing)
        {
            _hitboxComp.HitboxDeactivate();
        }
        // eating was interupted? TODO: HANDLE INTERUPTION HERE AS WELL?
        else if (Status != EatStatus.EatAnticipation) { return; }

        GD.Print("finished throw for ", GettingEaten.Count, " eatables!");

        foreach (var chomp in GettingEaten)
        {
            EatingEatable?.Invoke(this, chomp);
        }
        _animPlayer.Play(_eatAnimName + _animPlayer.GetAnimDirection());
        Status = EatStatus.Eating;
    }
    private void OnAnimationFinished(StringName animName)
    {
        //TODO: HANDLE INTERUPTION HERE
        if (Status == EatStatus.Restricted) { return; }
        if (animName.ToString().Contains(_grabAnimName))
        {
            GD.Print("finished grab anim!");
            Status = EatStatus.EatAnticipation;
            _hitboxComp.HitboxDeactivate();
        }
        else if (animName.ToString().Contains(_eatAnimName))
        {
            foreach (var chomped in GettingEaten)
            {
                AteEatable?.Invoke(this, chomped);
            }
            Status = EatStatus.Idle;
            EatQueue.Clear(); GettingEaten.Clear();
            FinishedEatingCycle?.Invoke(this, EventArgs.Empty);

            _animPlayer.AnimationFinished -= OnAnimationFinished;
            GD.Print("finished eating cycle!");

        }
    }
}
