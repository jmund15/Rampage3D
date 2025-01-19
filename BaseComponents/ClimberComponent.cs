using Godot;
using System;

public partial class ClimberComponent : Node
{
	public ClimbableComponent ClimbableComp { get; private set; }
    [Export]
    private CharacterBody3D _body;

    private bool _availableClimbable = false;
    public bool AvailableClimbable
    {
        get => _availableClimbable;
        private set
        {
            if (_availableClimbable == value) { return; }
            _availableClimbable = value;
            if (_availableClimbable)
            {
                FoundClimbable?.Invoke(this, ClimbableComp);
            }
            else
            {
                //LostClimbable?.Invoke(this, ClimbableComp);
            }
        }
    }

    private bool _isClimbing = false;
    public bool IsClimbing
    {
        get => _isClimbing;
        private set
        {
            if (_isClimbing == value) { return; }
            _isClimbing = value;
            if (_isClimbing)
            {
                StartedClimb?.Invoke(this, ClimbableComp);
            }
            else
            {
                ConcludedClimb?.Invoke(this, ClimbableComp);
            }
        }
    }

    private bool _lockingOn = false;
    public bool LockingOn {
        get => _lockingOn;
        private set
        {
            if (_lockingOn == value) { return; }
            _lockingOn = value;
            if (_lockingOn)
            {
                StartedClimbableAttach?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                FinishedClimbableAttach?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    private bool _climbableNeedEject = false;
    public bool EjectRequested
    {
        get => _climbableNeedEject;
        set
        {
            if (_climbableNeedEject == value) { return; }
            _climbableNeedEject = value;
            if (_climbableNeedEject)
            {
                EjectRequestSent?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                EjectRequestResponded?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    public OrthogDirection ClimbingDir { get; private set; }
    public OrthogDirection EjectDir
    {
        get => IMovementComponent.GetOppositeDirection(ClimbingDir);
    }

    public event EventHandler<ClimbableComponent> StartedClimb;
    public event EventHandler<ClimbableComponent> ConcludedClimb;

	public event EventHandler<ClimbableComponent> FoundClimbable;
    //public event EventHandler<ClimbableComponent> LostClimbable; // Not needed?

    public event EventHandler EjectRequestSent;
    public event EventHandler EjectRequestResponded;


    public event EventHandler StartedClimbableAttach;
    public event EventHandler FinishedClimbableAttach;

    public override void _Ready()
	{
	}
	public override void _Process(double delta)
	{
	}
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_body.IsOnWall() && !IsClimbing)
        {
            var wallCollider = _body.GetLastSlideCollision().GetCollider() as Node3D;
            //GD.Print("wall collider: ", wallCollider.Name);
            var climbComp = wallCollider.GetFirstChildOfType<ClimbableComponent>();
            if (climbComp == null) { return; }
            ClimbableComp = climbComp;
            AvailableClimbable = true;
        }
        else if (!_body.IsOnWall())
        {
            AvailableClimbable = false;
        }
    }
    private void OnClimbableRequestEject(object sender, EventArgs e)
    {
        EjectRequested = true;
    }
    public void StartClimb()
    {
        ClimbableComp.EjectClimbers += OnClimbableRequestEject;
        IsClimbing = true;
        AvailableClimbable = false;

        //BB.GetVar<Sprite3D>(BBDataSig.Sprite).FlipH = IMovementComponent.GetDesiredFlipH(_inputDir);
        LockingOn = true;

        var closestDist = float.MaxValue;
        Vector2 clampPos = Vector2.Zero;
        var xzBodyPos = new Vector2(_body.GlobalPosition.X, _body.GlobalPosition.Z);
        foreach (var clampPair in ClimbableComp.XZPositionMap)
        {
            var pos = clampPair.Value;
            var dist = xzBodyPos.DistanceTo(pos);
            if (dist <= closestDist)
            {
                closestDist = dist;
                clampPos = pos;
                ClimbingDir = clampPair.Key;
            }
        }


        var lockTween = GetTree().CreateTween();
        var finalX = clampPos.X;
        var finalZ = clampPos.Y;
        lockTween.TweenProperty(_body, "position:x", finalX, 0.05);
        lockTween.Parallel().TweenProperty(_body, "position:z", finalZ, 0.05);
        lockTween.TweenProperty(this, PropertyName.LockingOn.ToString(), false, 0);
        //lockTween.TweenProperty(this, PropertyName.IsClimbing.ToString(), true, 0);
    }

    public void StopClimb()
    {
        ClimbableComp.EjectClimbers -= OnClimbableRequestEject;
        IsClimbing = false;
    }

    public void ClimbTick(float yVal)
    {

    }
    
}
