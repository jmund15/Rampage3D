using Godot;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class ClimbSkid : BehaviorAction
{
    #region TASK_VARIABLES
    [Export]
    private string _skidAnimName;

    private CharacterBody3D _body;
    private ClimbableComponent _climbComp;
    private AnimationPlayer _animPlayer;

    private bool _lockingOn = true;
    #endregion
    #region TASK_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        base.Init(agent, bb);
        _body = Agent as CharacterBody3D;
        _animPlayer = BB.GetVar<AnimationPlayer>(BBDataSig.Anim);
    }
    public override void Enter()
    {
        base.Enter();
        _climbComp = BB.GetVar<ClimbableComponent>(BBDataSig.CurrClimbComp);
        LockOntoBuildingPosition();
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
        if (!_lockingOn && _body.Velocity.Y >= 0) // if we've stopped skidding and locked on
        {
            Status = TaskStatus.SUCCESS;
            return;
        }

        var velocity = _body.Velocity;
        if (velocity.Y < 0)
        {
            velocity.Y = Mathf.MoveToward(velocity.Y, 0, Monster.SkidFriction);
            GD.Print("CLIMB INIT SKIDDING VEL: ", velocity.Y);
        }
        else if (velocity.Y > 0)
        {
            velocity += _body.GetGravity() * delta;
            velocity.Y = Mathf.Max(velocity.Y, 0);
        }
        _body.Velocity = velocity;
        _body.MoveAndSlide();
    }
    #endregion
    #region TASK_HELPER
    private void LockOntoBuildingPosition()
    {
        //BB.GetVar<Sprite3D>(BBDataSig.Sprite).FlipH = IMovementComponent.GetDesiredFlipH(_inputDir);
        _lockingOn = true;

        var closestDist = float.MaxValue;
        Vector2 clampPos = Vector2.Zero;
        OrthogDirection clampDir = OrthogDirection.DownRight;
        var xzBodyPos = new Vector2(_body.GlobalPosition.X, _body.GlobalPosition.Z);
        foreach (var clampPair in _climbComp.XZPositionMap)
        {
            var pos = clampPair.Value;
            var dist = xzBodyPos.DistanceTo(pos);
            if (dist <= closestDist)
            {
                closestDist = dist;
                clampPos = pos;
                clampDir = clampPair.Key;
            }
        }

        PlayAnim.AnimWithOrthog(BB, _skidAnimName, clampDir);

        var lockTween = GetTree().CreateTween();
        var finalX = clampPos.X;
        var finalZ = clampPos.Y;
        lockTween.TweenProperty(_body, "position:x", finalX, 0.05);
        lockTween.Parallel().TweenProperty(_body, "position:z", finalZ, 0.05);
        lockTween.TweenProperty(this, PropertyName._lockingOn.ToString(), false, 0);
    }
    public override string[] _GetConfigurationWarnings()
	{
		var warnings = new List<string>();

		//

		return warnings.Concat(base._GetConfigurationWarnings()).ToArray();
	}
	#endregion
}

