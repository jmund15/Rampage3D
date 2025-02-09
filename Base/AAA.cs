//#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

[Tool]
public partial class AAA : Node
{
    public enum SpriteType
    {
        Monster,
        Critter
    }
    [Export]
    public Godot.Collections.Dictionary<SpriteType, int> SpriteTypePixelMap = new Godot.Collections.Dictionary<SpriteType, int>()
    {
        { SpriteType.Monster, 32},
        { SpriteType.Critter, 10}
    };
    public enum AAADirection
    {
        DOWN = 0,
        UP,
        LEFT,
        RIGHT,
        DOWNLEFT,
        DOWNRIGHT,
        UPLEFT,
        UPRIGHT
    }
    public static string GetFaceDirectionString(AAADirection direction)
    {
        switch (direction)
        {
            case AAADirection.DOWN:
                return "Down";
            case AAADirection.UP:
                return "Up";
            case AAADirection.LEFT:
                return "Left";
            case AAADirection.RIGHT:
                return "Right";
            case AAADirection.DOWNLEFT:
                return "DownLeft";
            case AAADirection.DOWNRIGHT:
                return "DownRight";
            case AAADirection.UPLEFT:
                return "UpLeft";
            case AAADirection.UPRIGHT:
                return "UpRight";
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

    [Export(PropertyHint.SaveFile, "*.tscn")]
    public string SavePath { get; private set; }
    [Export]
    private int _initOffset = 32;//10
    [Export]
    private int _frameHeight = 32;
        
  //  public static Dictionary<AAADirection, int> DirectionOffset = new Dictionary<AAADirection, int>()
  //  {
  //      //{ Direction.RIGHT, initOffset },
		////{ Direction.DOWNRIGHT, initOffset + frameHeight },
  //      { AAADirection.DOWN, _initOffset },
  //      //{ Direction.DOWNLEFT, initOffset + frameHeight * 3},
  //      //{ Direction.LEFT, initOffset + frameHeight * 4 },
  //      //{ Direction.UPLEFT, initOffset + frameHeight * 5 },
  //      { AAADirection.UP, _initOffset + _frameHeight },
  //      //{ Direction.UPRIGHT, initOffset + frameHeight * 7 },
  //  };

    [Export]
    private SpriteType _spriteType;
    [Export]
    public Godot.Collections.Dictionary<string, int> BodyParts { get; private set; } = new Godot.Collections.Dictionary<string, int>()
    {
        { "body", 2 },
        { "pants", 2 },
        { "shirt", 2 },
        { "hat", 2 },
    };
    [Export]
    public Godot.Collections.Array<AAADirection> AnimDirections { get; private set; } = new Godot.Collections.Array<AAADirection>()
    {
        AAADirection.UP,
        AAADirection.DOWN,
    };

    private Node _sprite;

    // Called when the script is executed (using File -> Run in Script Editor).
    public override void _Ready()
	{
        if (!Engine.IsEditorHint()) { return; }
        PortableCompressedTexture2D texture;
        AtlasTexture atlasTexture;
        _sprite = this.GetFirstChildOfType<Node>();
        if (_sprite is Sprite2D spritesheet)
		{
            texture = spritesheet.Texture as PortableCompressedTexture2D;
            atlasTexture = spritesheet.Texture as AtlasTexture;
        }
        else if (_sprite is Sprite3D spritesheet3D)
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


        AutoAnimateSprite3D();

        //GetTree().CreateTimer(5.0f).Timeout += () => SaveThisSceneAs(SavePath);

        //var scenePath = SaveDir + "//" + SavePath;
        //CallDeferred(MethodName.SaveThisSceneAs, SavePath);
        //SaveThisSceneAs(SavePath);

		//EditorInterface.Singleton.SaveScene();
        GD.Print($"Finished Animation Automation!");

    }

    private void AutoAnimateSprite2D()
    {

    }
    private void AutoAnimateSprite3D()
    {
        var animPlayer = _sprite.GetFirstChildOfType<AnimationPlayer>();
        var animLibrary = animPlayer.GetAnimationLibrary("");

        
        //var appendLibrary = ResourceLoader.Load<AnimationLibrary>();

        // TODO: MAKE NEW SPRITE FOR BODY PARTS HERE
        int partNum = 1;
        foreach (var bodyPartPair in BodyParts)
        {
            var part = bodyPartPair.Key;
            var typesOfPart = bodyPartPair.Value;
            Sprite3D partSprite;
            AnimationPlayer partPlayer;
            AnimationLibrary partLibrary;
            if (partNum == 1)
            {
                partSprite = _sprite as Sprite3D;
                partPlayer = animPlayer;
                partLibrary = animLibrary;
            }
            else
            {
                partSprite = _sprite.Duplicate() as Sprite3D;
                partPlayer = new AnimationPlayer();
                partSprite.Name = part;
                partPlayer.Name = part + "AnimationPlayer";
                AddChild(partSprite);
                partSprite.Owner = GetTree().EditedSceneRoot;
                partSprite.AddChild(partPlayer);
                partPlayer.Owner = partSprite;
                partLibrary = new AnimationLibrary();//partPlayer.GetAnimationLibrary(""); //animLibrary.Duplicate(true) as AnimationLibrary; //deep copy
                partPlayer.AddAnimationLibrary("", partLibrary);
                
            }
            foreach (var animName in animPlayer.GetAnimationList())
            {
                int currHeight = _initOffset;
                GD.Print($"Starting automating '{animName}'...");
                var anim = animPlayer.GetAnimation(animName);
                if (AnimLoopMap.ContainsKey(animName))
                {
                    anim.LoopMode = AnimLoopMap[animName];
                    //GD.Print($"Set loop mode to '{anim.LoopMode}'.");
                }
                int trackNum = 1;
                for (int typeNum = 1; typeNum <= typesOfPart; typeNum++)
                {
                    foreach (var dir in AnimDirections)
                    {
                        var dirAnim = anim.Duplicate(true) as Animation;

                        var numFrames = dirAnim.TrackGetKeyCount(trackNum);
                        for (int i = 0; i < numFrames; i++)
                        {
                            var currRect = (Rect2)dirAnim.TrackGetKeyValue(trackNum, i);
                            var dirRect = new Rect2(currRect.Position.X, currHeight,
                                currRect.Size.X, _frameHeight);

                            dirAnim.TrackSetKeyValue(trackNum, i, Variant.From(dirRect));
                        }
                        partLibrary.AddAnimation(animName + GetFaceDirectionString(dir) + typeNum.ToString(), dirAnim);
                        currHeight += _frameHeight;
                        if (partNum == 1)
                        {
                            partLibrary.RemoveAnimation(animName);
                        }
                    }
                }
            }
            partNum++;
        }
    }

    private void SaveThisSceneAs(string scenePath)
    {
        GD.Print("saving Scene: ", EditorInterface.Singleton.GetEditedSceneRoot().Name);
        EditorInterface.Singleton.SaveSceneAs(scenePath);
        EditorInterface.Singleton.OpenSceneFromPath(scenePath);
        //var topNode = EditorInterface.Singleton.GetEditedSceneRoot();
        //topNode.Name = "TESTED WOOO";
        //EditorInterface.Singleton.SaveScene();

        //var packedScene = new PackedScene();
        //packedScene.Pack(GetTree().CurrentScene);
        //GD.Print("current scene num children: ", GetTree().CurrentScene.GetChildCount());
        //ResourceSaver.Save(packedScene, scenePath);

        //GetTree().Quit();
    }
}
//#endif
