using Godot;
using System;
using System.Collections.Generic;

public enum FloorHealthState
{
    Stable,
    Cracked,
    Crumbling,
    Destroyed
}

[GlobalClass, Tool]
public partial class BuildingFloorComponent : MeshInstance3D
{
    #region COMPONENT_VARIABLES
    [Export]
    public FloorMaterial Material { get; private set; }
    [Export]
    public float FloorMaxHealth { get; private set; } = 0f;
    public HealthComponent HealthComp { get; private set; }
    private AnimatedSprite3D WallCrack;
    private List<AnimatedSprite3D> _wallCracks = new List<AnimatedSprite3D>();
    private float _crackOffset = 0.025f;
    public float YCenter { get; private set; }
    public List<Vector2> XFacePoses { get; private set; } = new List<Vector2>();
    public List<Vector2> ZFacePoses { get; private set; } = new List<Vector2>();
    public FloorHealthState HealthState { get; private set; }

    private float _healthStateChangeAmt;

    private Dictionary<FloorHealthState, float> _healthStateMap = new Dictionary<FloorHealthState, float>();
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
    {
        base._Ready();
        HealthComp = this.GetFirstChildOfType<HealthComponent>();
        WallCrack = GetNode<AnimatedSprite3D>("floorDestruction32x");

        if (FloorMaxHealth > 0f)
        {
            HealthComp.SetMaxHealth(FloorMaxHealth);
        }
        HealthComp.Damaged += OnDamaged;
        HealthState = FloorHealthState.Stable;
        _healthStateChangeAmt = HealthComp.MaxHealth / 3f;
        _healthStateMap.Add(FloorHealthState.Stable, HealthComp.MaxHealth);
        _healthStateMap.Add(FloorHealthState.Cracked, HealthComp.MaxHealth - _healthStateChangeAmt);
        _healthStateMap.Add(FloorHealthState.Crumbling, HealthComp.MaxHealth - _healthStateChangeAmt * 2);
        _healthStateMap.Add(FloorHealthState.Destroyed, 0f);


        //define ycenter
        YCenter = (Mesh.GetAabb().GetCenter().Y * Scale.Y) + GlobalPosition.Y;
        //GD.Print($"floor {Name} YCENTER: {YCenter}.");
        float baseSizeX = 1 * Scale.X;
        float baseSizeZ = 1 * Scale.Z;
        int xFaces = Mathf.FloorToInt(Mesh.GetAabb().Size.X * Scale.X / baseSizeX);
        int zFaces = Mathf.FloorToInt(Mesh.GetAabb().Size.Z * Scale.Z / baseSizeZ);

        for (int i = 0; i < xFaces; i++)
        {
            var xFacePos = new Vector2(
                (Mesh.GetAabb().Position.X * Scale.X) + GlobalPosition.X + (baseSizeX * i) + (baseSizeX / 2f),
                (Mesh.GetAabb().End.Z * Scale.Z) + GlobalPosition.Z
                );
            XFacePoses.Add(xFacePos);
            Vector3 wallCrackDLPosition = new Vector3(
                xFacePos.X,
                YCenter,
                xFacePos.Y + _crackOffset
            );
            var wallCrackDL = WallCrack.Duplicate() as AnimatedSprite3D;
            AddChild(wallCrackDL);
            wallCrackDL.GlobalPosition = wallCrackDLPosition;
            _wallCracks.Add(wallCrackDL);

            
            //GD.Print($"x face {i} is at position {wallCrackDLPosition}");
        }
        for (int i = 0; i < zFaces; i++)
        {
            var zFacePos = new Vector2(
                (Mesh.GetAabb().End.X * Scale.X) + GlobalPosition.X,
                (Mesh.GetAabb().Position.Z * Scale.Z) + GlobalPosition.Z + (baseSizeZ * i) + (baseSizeZ / 2f)
                );
            ZFacePoses.Add(zFacePos);
            Vector3 wallCrackDRPosition = new Vector3(
                zFacePos.X + _crackOffset,
                YCenter,
                zFacePos.Y
            );
            var wallCrackDR = WallCrack.Duplicate() as AnimatedSprite3D;
            AddChild(wallCrackDR);
            wallCrackDR.GlobalPosition = wallCrackDRPosition;
            wallCrackDR.RotateY(Mathf.Pi / 2f);
            _wallCracks.Add(wallCrackDR);
            //GD.Print($"z face {i} is at position {wallCrackDRPosition}");
        }
        foreach (var crack in _wallCracks)
        {
            crack.Play(FloorModelMappings.FloorMaterialAnimationMap[Material]);
            crack.Pause();
            crack.Hide();
        }
        //WallCrack.QueueFree();
        //GD.Print("number of wallCracks: ", this.GetChildrenOfType<AnimatedSprite3D>().Count);
        //GD.Print("Normal size x floor: ",  xFaces);
        
        
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
    #endregion
    #region SIGNAL_LISTENERS
    private void OnDamaged(object sender, HealthUpdate healthUpdate)
    {
        //GD.Print($"FLOOR DAMAGED TO HEALTH {healthUpdate.NewHealth} out of {healthUpdate.MaxHealth}");
        CheckHealthState();

        var hitDirection = Vector3.Zero;
        if (healthUpdate.Attack == null)
        {
            hitDirection = Global.GetRndVector3(); //TODO: MAKE Y 0?
        }
        else
        {
            hitDirection = healthUpdate.Attack.Direction; //TODO: MAKE Y 0?
        }

        var damage = healthUpdate.HealthChange;
        if (healthUpdate.NewHealth <= 0)
        {
            //var polyPlayback = _sfxComponent.GetStreamPlayback() as AudioStreamPlaybackPolyphonic;
            //_damageSfxStreamNum = polyPlayback.PlayStream(OnDamageSfx);
        }
        //var scaleMult = damage * 0.5f;
        var posMult = ((damage + 0.2f) / 2f) * 0.08f;

        //var scaleShift = scaleMult * hitDirection;
        var posShift = posMult * hitDirection;
        //GD.Print("POS SHIFTING: ", posShift);
        //WIGGLE
        var scaleTween = CreateTween();
        scaleTween.TweenProperty(this, "position", Position + posShift, 0.1f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
        scaleTween.TweenProperty(this, "position", Position - (posShift / 2), 0.1f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
        scaleTween.TweenProperty(this, "position", Position, 0.1f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);

    }
    #endregion
    #region COMPONENT_HELPER
    private void CheckHealthState()
    {
        switch (HealthComp.Health)
        {
            case float n when n > _healthStateMap[FloorHealthState.Cracked]:
                if (HealthState != FloorHealthState.Stable)
                {
                    HealthState = FloorHealthState.Stable;
                    foreach (var crack in _wallCracks)
                    {
                        crack.Hide();
                    }
                }
                break;
            case float n when n > _healthStateMap[FloorHealthState.Crumbling]:
                if (HealthState != FloorHealthState.Cracked)
                {
                    HealthState = FloorHealthState.Cracked;
                    foreach (var crack in _wallCracks)
                    {
                        crack.Show();
                        crack.Frame = 0;
                    }
                }
                break;
            case float n when n > _healthStateMap[FloorHealthState.Destroyed]:
                if (HealthState != FloorHealthState.Crumbling)
                {
                    HealthState = FloorHealthState.Crumbling;
                    foreach (var crack in _wallCracks)
                    {
                        crack.Show();
                        crack.Frame = 1;
                    }
                }
                break;
            default:
                if (HealthState != FloorHealthState.Destroyed)
                {
                    HealthState = FloorHealthState.Destroyed;
                    foreach (var crack in _wallCracks)
                    {
                        crack.Show();
                        crack.Frame = 2;
                    }
                }
                break;
        }
        //GD.Print($"current anim: {WallCracks[0].Animation}, \nshown: {WallCracks[0].Visible}, \nframe: {WallCracks[0].Frame}");
    }
    #endregion
}
