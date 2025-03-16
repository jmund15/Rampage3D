using Godot;
using System;

[GlobalClass, Tool]
public partial class Sprite3DComponent : Sprite3D, ISpriteComponent
{
	public float SpriteHeight { get; private set; }
	public float SpriteWidth { get; private set; }

	public override void _Ready()
	{
        TextureChanged += OnTextureChanged;
        TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        OnTextureChanged();
		//GD.Print("SpriteOrthogComp Sprite Height: ", SpriteHeight);
	}
    public override void _Process(double delta)
	{
	}
    private void OnTextureChanged()
    {
        if (Texture is AtlasTexture atlasText)
        {
            SpriteHeight = atlasText.Region.Size.Y * PixelSize * Scale.Y;
            SpriteWidth = atlasText.Region.Size.X * PixelSize * Scale.X;
        }
        else if (Texture is CompressedTexture2D compText)
        {
            SpriteHeight = compText.GetSize().Y * PixelSize * Scale.Y;
            SpriteWidth = compText.GetSize().X * PixelSize * Scale.X;
        }
        else
        {
            SpriteHeight = RegionRect.Size.Y * PixelSize * Scale.Y;
            SpriteWidth = RegionRect.Size.X * PixelSize * Scale.X;
        }
    }
    public float GetSpriteHeight()
    {
        return SpriteHeight;
    }

    public float GetSpriteWidth()
    {
        return SpriteWidth;
    }

    //public bool GetFlipH()
    //{
    //    return FlipH;
    //}

    //public bool GetFlipV()
    //{
    //    return FlipV;
    //}

    public Node GetInterfaceNode()
    {
        return this;
    }
}
