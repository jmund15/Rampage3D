using Godot;
using System;

public partial class ClimberComponent : Node
{
	public ClimbableComponent ClimbableComp { get; private set; }
    [Export]
    private CharacterBody3D _body;
    private IMovementComponent _moveComp;

    private Vector3 _origSpritePos = new Vector3();

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
        get => ClimbingDir.GetOppositeDir();
    }
    private float _maxDistFromClimbable = 1.0f;

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
        _moveComp = _body as IMovementComponent;
        if (_moveComp == null)
        {
            throw new Exception("CLIMBER COMP ERROR || CLIMBER'S BODY MUST IMPLEMENT 'IMovementComponent'");
        }
    }
	public override void _Process(double delta)
	{
	}
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_body.IsOnWall() && !IsClimbing)
        {
            var coll = _body.GetLastSlideCollision();
            var wallCollider = coll.GetCollider() as Node3D;
            //GD.Print("wall collider: ", wallCollider.Name);
            var climbComp = wallCollider.GetFirstChildOfType<ClimbableComponent>();
            if (climbComp == null) 
            {
                AvailableClimbable = false;
                return; 
            }

            //var collPos = coll.GetPosition();
            //Make sure body is facing climable directly
            var velVec = _body.Velocity;
            OrthogDirection desDir;
            //if (desVec.IsZeroApprox() || desVec.GetOrthogDirection() != _moveComp.GetFaceDirection())
            //{
            //    AvailableClimbable = false;
            //    return;
            //}
            if (velVec.X == 0 && velVec.Z == 0)
            {
                desDir = _moveComp.GetFaceDirection();
            }
            else {
                desDir = velVec.GetOrthogDirection();
            }

            var xzBodyPos = new Vector2(_body.Position.X, _body.Position.Z);
            var nearestClimbOnPos = climbComp.GetClosestClimbablePosOfDir(xzBodyPos, desDir);
            var distToClimbOn = nearestClimbOnPos.DistanceTo(xzBodyPos);
            GD.Print("distance to climbon pos: ", distToClimbOn);
            if (distToClimbOn < _maxDistFromClimbable)
            {
                ClimbableComp = climbComp;
                AvailableClimbable = true;
            }
            //var collVec = new Vector3(desVec.X, 0f, desVec.Y);
            //var collAngle = coll.GetAngle(upDirection: collVec);

            //GD.Print("just collided @ normal: ", coll.GetNormal(), 
            //    " at direction: ", collVec,
            //    " and angle: ", collAngle);
            //if (collAngle > (3 * Mathf.Pi) / 4)
            //{
            //    ClimbableComp = climbComp;
            //    AvailableClimbable = true;
            //}
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
    public void StartClimb(OrthogDirection climbDir)
    {
        ClimbableComp.EjectClimbers += OnClimbableRequestEject;
        IsClimbing = true;
        AvailableClimbable = false;
        ClimbingDir = climbDir;

        //TODO: CHOOSE SPRITE CORRECTLY
        var sprite = _body.GetFirstChildOfType<Sprite3D>();
        _origSpritePos = sprite.GlobalPosition;

        //BB.GetVar<Sprite3D>(BBDataSig.Sprite).FlipH = IMovementComponent.GetDesiredFlipH(_inputDir);
        LockingOn = true;

        var xzBodyPos = new Vector2(_body.GlobalPosition.X, _body.GlobalPosition.Z);
        var attachPos = ClimbableComp.GetClosestClimbablePosOfDir(xzBodyPos, ClimbingDir);
        GD.Print("chosen climb pos: ", attachPos);

        var lockTween = GetTree().CreateTween();
        var finalX = attachPos.X;
        var spriteOffsetX = 0f;
        var spriteOffsetY = 0f;

        const float DOWNANIM_LOWERX = -2f;
        const float DOWNANIM_LOWERY = -4f;
        var finalZ = attachPos.Y;
        if (ClimbingDir.GetAnimDir() == AnimDirection.Down)
        {
            spriteOffsetX = DOWNANIM_LOWERX;
            spriteOffsetY = DOWNANIM_LOWERY;
        }

        lockTween.TweenProperty(_body, "global_position:x", finalX, 0.05);
        //lockTween.Parallel().TweenProperty(sprite, "offset:x", spriteOffsetX, 0.05);
        lockTween.Parallel().TweenProperty(sprite, "offset:y", spriteOffsetY, 0.05);
        lockTween.Parallel().TweenProperty(_body, "global_position:z", finalZ, 0.05);
        lockTween.TweenProperty(this, PropertyName.LockingOn.ToString(), false, 0);
        //lockTween.TweenProperty(this, PropertyName.IsClimbing.ToString(), true, 0);
    }

    public void StopClimb()
    {
        ClimbableComp.EjectClimbers -= OnClimbableRequestEject;
        IsClimbing = false;
        LockingOn = true;

        //TODO: CHOOSE SPRITE CORRECTLY
        var sprite = _body.GetFirstChildOfType<Sprite3D>();

        var lockTween = GetTree().CreateTween();

        //lockTween.TweenProperty(sprite, "global_position:x", _origSpritePos.X, 0.05);
        lockTween/*.Parallel()*/.TweenProperty(sprite, "offset:y", 0, 0.05);
        //lockTween.Parallel().TweenProperty(sprite, "global_position:z", _origSpritePos.Z, 0.05);
        lockTween.TweenProperty(this, PropertyName.LockingOn.ToString(), false, 0);
    }

    public void ClimbTick(float yVal)
    {

    }
    
}
