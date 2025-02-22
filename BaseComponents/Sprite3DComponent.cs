using Godot;
using System;

[GlobalClass, Tool]
public partial class Sprite3DComponent : Sprite3D, ISpriteComponent
{
	public float SpriteHeight { get; private set; }
	public float SpriteWidth { get; private set; }

	public override void _Ready()
	{
		if (Texture is AtlasTexture atlasText)
		{
			SpriteHeight = atlasText.Region.Size.Y * PixelSize * Scale.Y;
            SpriteWidth = atlasText.Region.Size.X * PixelSize * Scale.X;
        }
		else
		{
            SpriteHeight = RegionRect.Size.Y * PixelSize * Scale.Y;
            SpriteWidth = RegionRect.Size.X * PixelSize * Scale.X;
        }
		//GD.Print("SpriteOrthogComp Sprite Height: ", SpriteHeight);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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
