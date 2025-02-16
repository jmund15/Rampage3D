using Godot;
using Godot.Collections;

[Tool]
public partial class ClimbState : Base3DState
{
    #region STATE_VARIABLES
    [Export]
    private string _animName = "climb";
	private CharacterBody3D _body;
    private float _bodyHeight;

    [Export(PropertyHint.NodeType, "State")]
    private State _climbIdleState;
    [Export(PropertyHint.NodeType, "State")]
	private State _jumpState;
    [Export(PropertyHint.NodeType, "State")]
    private State _descendState;

    private SpriteOrthogComponent _bodySprite;
    private float _topBodyDistFromPos;

	private ClimberComponent _climberComp;
    private Vector2 _inputDir;
    private AnimDirection _climbAnimDir;
	#endregion
	#region STATE_UPDATES
	public override void Init(Node agent, IBlackboard bb)
	{
		base.Init(agent, bb);
        _body = Agent as CharacterBody3D;
        _bodySprite = BB.GetVar<SpriteOrthogComponent>(BBDataSig.Sprite);
    }
	public override void Enter(Dictionary<State, bool> parallelStates)
	{
		base.Enter(parallelStates);
		_climberComp = BB.GetVar<ClimberComponent>(BBDataSig.ClimberComp);
        _climbAnimDir = _climberComp.ClimbingDir.GetAnimDir();
        //GD.Print("on climb enter anim: ", AnimPlayer.CurrentAnimation);
        //GD.Print("on climb enter anim direction: ", _climbDir);
        //GD.Print("climbable max climb height: ", _climbComp.MaxClimbHeight);

        BB.GetVar<IAnimComponent>(BBDataSig.Anim).StartAnim(_animName + _climbAnimDir.GetAnimationString());

        _body.Velocity = Vector3.Zero;

        _topBodyDistFromPos = _bodySprite.SpriteHeight;
        //GD.Print("_topBodyDistFromPos: ", _topBodyDistFromPos);
    }
	public override void Exit()
	{
        base.Exit();
        //_body.Velocity = Vector3.Zero;
        //_body.MoveAndSlide();
    }
    public override void ProcessFrame(float delta)
	{
		base.ProcessFrame(delta);
        
    }
	public override void ProcessPhysics(float delta)
	{
		base.ProcessPhysics(delta);
        _inputDir = MoveComp.GetDesiredDirection();

        if (_inputDir.IsZeroApprox())
        {
            EmitSignal(SignalName.TransitionState, this, _climbIdleState);
            return;
        }
        else if (MoveComp.WantsJump())
        {
            _climberComp.EjectRequested = true;
        }

        var desiredOrthogDir = _inputDir.GetOrthogDirection();
        if (_climberComp.ClimbingDir.GetOppositeDir() == desiredOrthogDir && _inputDir.Y != 0)
        {
            EmitSignal(SignalName.TransitionState, this, _descendState);
            return;
        }

        CheckIfOnRoof();

        HandleClimbVelocity();
    }
	public override void HandleInput(InputEvent @event)
	{
		base.HandleInput(@event);
	}
    #endregion
    #region STATE_HELPER
    private void HandleClimbVelocity()
    {
        Vector3 velocity = _body.Velocity;
        velocity.X = 0; velocity.Z = 0;

        if (_inputDir.GetOrthogDirection() != _climberComp.ClimbingDir)
        {
            return;
        }
        var climbInput = _inputDir.Length();//Mathf.Abs(_inputDir.Y);
        //GD.Print("climb input: ", climbInput);

        if (_body.Position.Y + (_topBodyDistFromPos / 4) < _climberComp.ClimbableComp.MaxClimbHeight) 
        {
            velocity.Y = climbInput * Monster.ClimbSpeed;
        }
        else { 
            GD.Print("climb max height: ",  _climberComp.ClimbableComp.MaxClimbHeight, 
                "\nbody curr height: ", _body.Position.Y);
            velocity.Y = 0; }

        _body.Velocity = velocity;
        _body.MoveAndSlide();
    }

    private void CheckIfOnRoof()
    {
        //GD.Print("body pos: ", _body.Position.Y, "\ncurr top of body: ", _body.Position.Y + _topBodyDistFromPos,
        //    "\nTop of building height:", _climbComp.MaxClimbHeight);
        float maxClimbHeight = _climberComp.ClimbableComp.MaxClimbHeight +  _climberComp.ClimbableComp.RoofComp.RoofRelHeightMap[_climberComp.ClimbingDir];
        
        if (_body.Position.Y + (_topBodyDistFromPos / 4) >= maxClimbHeight && _climberComp.ClimbableComp.CanClimbOnTop)
        {
            //float climbOnPush = 0.5f;
            //Vector2 pushDir = IMovementComponent.GetVectorFromDirection(MoveComp.GetFaceDirection())
            //    * climbOnPush;
            ////_body.Position = new Vector3
            ////    (_body.Position.X + pushDir.X,
            ////    _climbComp.MaxClimbHeight,
            ////    _body.Position.Z + pushDir.Y);
            //GD.Print("CURRENT BODY POS: ", _body.Position);
            //var climbPos = _climberComp.ClimbableComp.ClimbOnPosMap[MoveComp.GetFaceDirection()];
            //_body.Position = new Vector3(
            //    climbPos.X,
            //    /*_body.Position.Y + _topBodyDistFromPos,//*/climbPos.Y - _topBodyDistFromPos / 2,
            //    climbPos.Z
            //    );

            //GD.Print("SETTING BODY POS: ", _body.Position);
            _climberComp.StopClimb();
            EmitSignal(SignalName.TransitionState, this, _jumpState);
            return;
        }
    }
	
	#endregion
}
