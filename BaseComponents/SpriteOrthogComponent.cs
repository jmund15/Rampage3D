using Godot;
using System;

[GlobalClass, Tool]
public partial class SpriteOrthogComponent : Sprite3D
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
}
