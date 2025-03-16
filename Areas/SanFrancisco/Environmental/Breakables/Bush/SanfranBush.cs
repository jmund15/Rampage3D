using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class SanfranBush : StaticBody3D
{
	private int _bushSize = 0;
	[Export(PropertyHint.Enum,"Small,Medium,Large")]
	public int BushSize 
	{
		get => _bushSize;
        private set
		{
			if (_bushSize == value) { return; }
			_bushSize = value;
			BushComp = new Vector2I(BushSize, BushColor);
		}
	}
	private int _bushColor;
    [Export(PropertyHint.Enum, "Color1,Color2,Color3")]
    public int BushColor
    {
        get => _bushColor;
        private set
        {
            if (_bushColor == value) { return; }
            _bushColor = value;
            BushComp = new Vector2I(BushSize, BushColor);
        }
    }

    private Dictionary<Vector2I, string> _bushTextureMap = new Dictionary<Vector2I, string>()
	{
		{new Vector2I(0,0), "res://Areas/SanFrancisco/Environmental/Breakables/Bush/01/breakable_foliage_bush_small01.png" },
        {new Vector2I(0,1), "res://Areas/SanFrancisco/Environmental/Breakables/Bush/02/breakable_foliage_bush_small02.png" },
        {new Vector2I(0,2), "res://Areas/SanFrancisco/Environmental/Breakables/Bush/03/breakable_foliage_bush_small03.png" },
        {new Vector2I(1,0), "res://Areas/SanFrancisco/Environmental/Breakables/Bush/01/breakable_foliage_bush_medium01.png" },
        {new Vector2I(1,1), "res://Areas/SanFrancisco/Environmental/Breakables/Bush/02/breakable_foliage_bush_medium02.png" },
        {new Vector2I(1,2), "res://Areas/SanFrancisco/Environmental/Breakables/Bush/03/breakable_foliage_bush_medium03.png" },
        {new Vector2I(2,0), "res://Areas/SanFrancisco/Environmental/Breakables/Bush/01/breakable_foliage_bush_big01.png" },
        {new Vector2I(2,1), "res://Areas/SanFrancisco/Environmental/Breakables/Bush/02/breakable_foliage_bush_big02.png" },
        {new Vector2I(2,2), "res://Areas/SanFrancisco/Environmental/Breakables/Bush/03/breakable_foliage_bush_big03.png" },
    };
	private Vector2I _bushComp = Vector2I.Zero;
	public Vector2I BushComp
	{
		get => _bushComp;
        set
		{
			if (_bushComp == value) { return; }
			_bushComp = value;
			SetTexture();
		}
	}

	private Dictionary<int, string[]> _bushPieceMap = new Dictionary<int, string[]>()
	{
        {0,  new string[]
			{ "res://Areas/SanFrancisco/Environmental/Breakables/Bush/01/breakable_foliage_bush_particles_onbreak01_01.png",
				"res://Areas/SanFrancisco/Environmental/Breakables/Bush/01/breakable_foliage_bush_particles_onbreak01_02.png",
				"res://Areas/SanFrancisco/Environmental/Breakables/Bush/01/breakable_foliage_bush_particles_onbreak01_03.png",
				"res://Areas/SanFrancisco/Environmental/Breakables/Bush/01/breakable_foliage_bush_particles_onbreak01_04.png" }
		},
        {1,  new string[]
            { "res://Areas/SanFrancisco/Environmental/Breakables/Bush/02/breakable_foliage_bush_particles_onbreak02_01.png",
                "res://Areas/SanFrancisco/Environmental/Breakables/Bush/02/breakable_foliage_bush_particles_onbreak02_02.png",
                "res://Areas/SanFrancisco/Environmental/Breakables/Bush/02/breakable_foliage_bush_particles_onbreak02_03.png",
                "res://Areas/SanFrancisco/Environmental/Breakables/Bush/02/breakable_foliage_bush_particles_onbreak02_04.png" }
        },
        {2,  new string[]
            { "res://Areas/SanFrancisco/Environmental/Breakables/Bush/03/breakable_foliage_bush_particles_onbreak03_01.png",
                "res://Areas/SanFrancisco/Environmental/Breakables/Bush/03/breakable_foliage_bush_particles_onbreak03_02.png",
                "res://Areas/SanFrancisco/Environmental/Breakables/Bush/03/breakable_foliage_bush_particles_onbreak03_03.png",
                "res://Areas/SanFrancisco/Environmental/Breakables/Bush/03/breakable_foliage_bush_particles_onbreak03_04.png" }
        },

    };


    private HealthComponent _healthComp;
	private Sprite3DComponent _sprite;
	private Breakable3DComponent _breakableComp;
    public override void _Ready()
	{
		_healthComp = this.GetFirstChildOfType<HealthComponent>();
        _sprite = this.GetFirstChildOfType<Sprite3DComponent>();
		_breakableComp = this.GetFirstChildOfType<Breakable3DComponent>();
		
        switch (BushSize)
		{
			case 0:
				_healthComp.SetMaxHealth(1); break;
			case 1:
				_healthComp.SetMaxHealth(1); break;
			case 2:
				_healthComp.SetMaxHealth(2); break;
			default:
				GD.PrintErr("SanFranBush size not in range!");
				return;
		}
		BushComp = new Vector2I(BushSize, BushColor);
		SetTexture();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetTexture()
	{
		if (_sprite == null) { return; }

		PiecesOnDamage poDamage = null;
        PiecesBaseOnDestroy poDestroy = null;
        foreach (var damageStrat in _breakableComp.OnDamageStrategies)
		{
			if (damageStrat is PiecesOnDamage podStrat)
			{
                poDamage = podStrat;
				break;
			}
		}
        foreach (var destroyStrat in _breakableComp.OnDestroyStrategies)
        {
            if (destroyStrat is PiecesBaseOnDestroy podStrat)
            {
                poDestroy = podStrat;
                break;
            }
        }

        _sprite.Texture.ResourceLocalToScene = true;
        var bushText = ResourceLoader.Load<CompressedTexture2D>(_bushTextureMap[BushComp]);
		_sprite.Texture = bushText;

		if (poDamage != null)
		{
			poDamage.PieceTextures = _bushPieceMap[BushColor];
		}
		if (poDestroy != null)
		{
            poDestroy.PieceTextures = _bushPieceMap[BushColor];
        }
    }
}
