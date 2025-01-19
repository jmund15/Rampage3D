#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

[Tool]
public partial class AAA : EditorScript
{
  
    public enum Direction
    {
        DOWN = 0,
        UP,
        //LEFT,
        //RIGHT,
        //DOWNLEFT,
        //DOWNRIGHT,
        //UPLEFT,
        //UPRIGHT
    }
    public static string GetFaceDirectionString(Direction direction)
    {
        switch (direction)
        {
            case Direction.DOWN:
                return "Down";
            case Direction.UP:
                return "Up";
            //case Direction.LEFT:
            //    return "Left";
            //case Direction.RIGHT:
            //    return "Right";
            //case Direction.DOWNLEFT:
            //    return "DownLeft";
            //case Direction.DOWNRIGHT:
            //    return "DownRight";
            //case Direction.UPLEFT:
            //    return "UpLeft";
            //case Direction.UPRIGHT:
            //    return "UpRight";
            default:
                GD.PrintErr("not any face direction?? facedir = " + direction.ToString());
                return "Null";
        }
    }
    
    public static Dictionary<string, Animation.LoopModeEnum> AnimLoopMap = new Dictionary<string, Animation.LoopModeEnum>()
	{
		{ "idle", Animation.LoopModeEnum.Linear },
		{ "walk", Animation.LoopModeEnum.Linear },
		{ "run", Animation.LoopModeEnum.Linear },
        { "land", Animation.LoopModeEnum.None },
        { "land2", Animation.LoopModeEnum.None },
        { "jump", Animation.LoopModeEnum.None },
        { "punchStartup", Animation.LoopModeEnum.None },
        { "punch1", Animation.LoopModeEnum.None },
        { "punch2", Animation.LoopModeEnum.None },
        { "wallPunch", Animation.LoopModeEnum.None },
        { "wallKick", Animation.LoopModeEnum.None },
        { "lift", Animation.LoopModeEnum.None },
        { "grab", Animation.LoopModeEnum.None },
        { "eat", Animation.LoopModeEnum.None },

    };

    private const int initOffset = 32;//10
    private const int frameHeight = 32;
        
    public static Dictionary<Direction, int> DirectionOffset = new Dictionary<Direction, int>()
    {
        //{ Direction.RIGHT, initOffset },
		//{ Direction.DOWNRIGHT, initOffset + frameHeight },
        { Direction.DOWN, initOffset },
        //{ Direction.DOWNLEFT, initOffset + frameHeight * 3},
        //{ Direction.LEFT, initOffset + frameHeight * 4 },
        //{ Direction.UPLEFT, initOffset + frameHeight * 5 },
        { Direction.UP, initOffset + frameHeight },
        //{ Direction.UPRIGHT, initOffset + frameHeight * 7 },
    };

    // Called when the script is executed (using File -> Run in Script Editor).
    public override void _Run()
	{
        PortableCompressedTexture2D texture;
        AtlasTexture atlasTexture;
        Node Scene = GetScene();
        if (Scene is Sprite2D spritesheet)
		{
            texture = spritesheet.Texture as PortableCompressedTexture2D;
            atlasTexture = spritesheet.Texture as AtlasTexture;
        }
        else if (Scene is Sprite3D spritesheet3D)
        {
            texture = spritesheet3D.Texture as PortableCompressedTexture2D;
            atlasTexture = spritesheet3D.Texture as AtlasTexture;
        }
        else
        {
            GD.PrintErr("AAA ERROR || Current scene is not a Sprite2D or Sprite3D!");
            return;
        }

        //FileDialogOptions options = new();
        //string? appendLibPath = await this.Extensibility.Shell().ShowOpenFileDialogAsync(options, cancellationToken);

        int textureHeight;
        if (texture != null)
        {
            textureHeight = texture.GetHeight();
        }
        else if (atlasTexture != null)
        {
            textureHeight = atlasTexture.GetHeight();
        }

		var animPlayer = Scene.GetFirstChildOfType<AnimationPlayer>();
        var animLibrary = animPlayer.GetAnimationLibrary("");

        //var appendLibrary = ResourceLoader.Load<AnimationLibrary>();

		foreach (var animName in animPlayer.GetAnimationList())
		{
            GD.Print($"Starting automating '{animName}'...");
			var anim = animPlayer.GetAnimation(animName);
            if (AnimLoopMap.ContainsKey(animName))
            {
                anim.LoopMode = AnimLoopMap[animName];
                //GD.Print($"Set loop mode to '{anim.LoopMode}'.");
            }

            int trackNum = 1;

            foreach (var dir in Global.GetEnumValues<Direction>())
            {
                var dirAnim = anim.Duplicate(true) as Animation;
            
                var numFrames = dirAnim.TrackGetKeyCount(trackNum);
                for (int i = 0; i < numFrames; i++)
                {
                    var currRect = (Rect2)dirAnim.TrackGetKeyValue(trackNum, i);
                    var dirRect = new Rect2(currRect.Position.X, DirectionOffset[dir],
                        currRect.Size.X, frameHeight);

                    dirAnim.TrackSetKeyValue(trackNum, i, Variant.From(dirRect));
                }
                animLibrary.AddAnimation(animName + GetFaceDirectionString(dir), dirAnim);
            }

            animLibrary.RemoveAnimation(animName);
		}

		EditorInterface.Singleton.SaveScene();
        GD.Print($"Finished Animation Automation!");
    }
}
#endif
