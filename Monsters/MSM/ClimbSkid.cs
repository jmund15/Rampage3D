using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class ClimbSkid : BehaviorAction
{
    #region TASK_VARIABLES
    [Export]
    private string _skidAnimName;
    [Export]
    private float _skidFriction = 1f;

    private CharacterBody3D _body;
    private IVelocityChar3DComponent _velComp;
    private ClimberComponent _climberComp;
    private IAnimComponent _animPlayer;
    #endregion
    #region TASK_UPDATES
    public override void Init(Node agent, IBlackboard bb)
    {
        base.Init(agent, bb);
        _body = Agent as CharacterBody3D;
        _velComp = BB.GetVar<IVelocityChar3DComponent>(BBDataSig.VelComp);
        _animPlayer = BB.GetVar<IAnimComponent>(BBDataSig.Anim);
    }
    public override void Enter()
    {
        base.Enter();
        _climberComp = BB.GetVar<ClimberComponent>(BBDataSig.ClimberComp);
        _climberComp.StartClimb(BB.GetVar<IMovementComponent>(BBDataSig.MoveComp).GetFaceDirection());
        //_climberComp.FinishedClimbableAttach += OnFinishAttach;
        PlayAnim.AnimWithOrthog(BB, _skidAnimName, _climberComp.ClimbingDir);
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
        if (!_climberComp.LockingOn && _body.Velocity.Y >= 0) // if we've stopped skidding and locked on
        {
            Status = TaskStatus.SUCCESS;
            return;
        }

        var velocity = _body.Velocity;
        if (velocity.Y < 0)
        {
            velocity.Y = Mathf.MoveToward(velocity.Y, 0, _skidFriction);
            GD.Print("CLIMB INIT SKIDDING VEL: ", velocity.Y);
        }
        else if (velocity.Y > 0)
        {
            velocity += _body.GetWeightedGravity() * delta;
            velocity.Y = Mathf.Max(velocity.Y, 0);
        }
        _body.Velocity = velocity;
        _body.MoveAndSlide();
    }
    #endregion
    #region TASK_HELPER   
    private void OnFinishAttach(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
    public override string[] _GetConfigurationWarnings()
	{
		var warnings = new List<string>();

		//

		return warnings.Concat(base._GetConfigurationWarnings()).ToArray();
	}
	#endregion
}

