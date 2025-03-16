using Godot;
using System.Collections.Generic;


[Tool]
public partial class SanfranStreetlight : StaticBody3D
{
    public record SLTextures(
        string Head,
        string Body,
        string[] OnBreak
    );
    #region COMPONENT_VARIABLES
    private int _type = 0;
    [Export(PropertyHint.Enum, "1L,1R,2L,2R,3")]
    public int Type
    {
        get => _type;
        private set
        {
            if (_type == value) { return; }
            _type = value;
            SetTexture();
        }
    }

    private Dictionary<int, SLTextures> _slTextureMap = new Dictionary<int, SLTextures>()
    {
        {0, new SLTextures(
            Body: "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight01/streetlight01_L/breakable_metro_streetlight01_L_body.png",
            Head: "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight01/streetlight01_L/breakable_metro_streetlight01_L_head.png",
            OnBreak: new string[] {
                "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight01/streetlight01_L/breakable_metro_streetlight01_L_body_onbreak.png",
                "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight01/streetlight01_L/breakable_metro_streetlight01_L_head_onbreak.png" }
            )},
        {1, new SLTextures(
            Body: "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight01/streetlight01_R/breakable_metro_streetlight01_R_body.png",
            Head: "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight01/streetlight01_R/breakable_metro_streetlight01_R_head.png",
            OnBreak: new string[] {
                "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight01/streetlight01_R/breakable_metro_streetlight01_R_body_onbreak.png",
                "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight01/streetlight01_R/breakable_metro_streetlight01_R_head_onbreak.png" }
            )},
        {2, new SLTextures(
            Body: "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight02/breakable_metro_streetlight02_body.png",
            Head: "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight02/breakable_metro_streetlight02_head.png",
            OnBreak: new string[] {
                "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight02/breakable_metro_streetlight02_body_onbreak.png",
                "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight02/breakable_metro_streetlight02_head_onbreak.png" }
            )},
        {3, new SLTextures(
            Body: "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight02/breakable_metro_streetlight02_body.png",
            Head: "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight02/breakable_metro_streetlight02_head.png",
            OnBreak: new string[] {
                "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight02/breakable_metro_streetlight02_body_onbreak.png",
                "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight02/breakable_metro_streetlight02_head_onbreak1.png",
                "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight02/breakable_metro_streetlight02_head_onbreak2.png"}
            )},
        {4, new SLTextures(
            Body: "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight03/breakable_metro_streetlight03_body.png",
            Head: "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight03/breakable_metro_streetlight03_head.png",
            OnBreak: new string[] {
                "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight03/breakable_metro_streetlight03_body_onbreak.png",
                "res://Areas/SanFrancisco/Environmental/Breakables/streetlights/streetlight03/breakable_metro_streetlight03_head_onbreak.png" }
            )}
    };

    private HealthComponent _healthComp;
    private Sprite3DComponent _bodySprite;
    private Sprite3DComponent _headSprite;
    private Breakable3DComponent _breakableComp;
    #endregion
    #region COMPONENT_UPDATES
    public override void _Ready()
	{
		base._Ready();
        _healthComp = this.GetFirstChildOfType<HealthComponent>();
        _bodySprite = GetNode<Sprite3DComponent>("Visuals/Body");
        _headSprite = GetNode<Sprite3DComponent>("Visuals/Head");
        _breakableComp = this.GetFirstChildOfType<Breakable3DComponent>();

        SetTexture();
    }
	public override void _Process(double delta)
	{
		base._Process(delta);
	}
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
	}
    #endregion
    #region COMPONENT_HELPER
    public void SetTexture()
    {
        if (_headSprite == null || _bodySprite == null) { return; }
        PiecesBaseOnDestroy poDestroy = null;
        foreach (var destroyStrat in _breakableComp.OnDestroyStrategies)
        {
            if (destroyStrat is PiecesBaseOnDestroy podStrat)
            {
                poDestroy = podStrat;
                break;
            }
        }

        _bodySprite.Texture.ResourceLocalToScene = true;
        _headSprite.Texture.ResourceLocalToScene = true;
        var bodyText = ResourceLoader.Load<CompressedTexture2D>(_slTextureMap[Type].Body);
        var headText = ResourceLoader.Load<CompressedTexture2D>(_slTextureMap[Type].Head);
        _bodySprite.Texture = bodyText;
        _headSprite.Texture = headText;

        if (Type == 3)
        {
            _headSprite.FlipH = true;
        }
        else
        {
            _headSprite.FlipH = false;
        }

        if (poDestroy != null)
        {
            poDestroy.PieceTextures = _slTextureMap[Type].OnBreak;
        }
    }
    #endregion
    #region SIGNAL_LISTENERS
    #endregion
}
