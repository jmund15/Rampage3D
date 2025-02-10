//#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public enum SpriteType
{
    Monster,
    Critter
}
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

[Tool]
public partial class AAA : EditorScript
{
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

    private static string _aaaParamsPath = "res://Base/aaa_params.tres";
    private AAAParameters _aaaParams;
    private int _initOffset;// = 32;//10
    private int _frameHeight;// = 32;
    private Node _sprite;
    private PortableCompressedTexture2D _texture;
    private AtlasTexture _atlasTexture;

    private int _currHeight;

    // Called when the script is executed (using File -> Run in Script Editor).
    public override void _Run()
    {
        //GD.Print("resource type: ", ResourceLoader.Load(_aaaParamsPath).GetType().FullName);
        _aaaParams = ResourceLoader.Load<AAAParameters>(_aaaParamsPath);// as AAAParameters;

        //GD.Print("loaded resource! param list of dirs: ", _aaaParams.AnimDirections);

        SaveThisSceneAs(_aaaParams.SavePath);
        _initOffset = _aaaParams.SpriteTypePixelMap[_aaaParams.SpriteType];
        _frameHeight = _aaaParams.SpriteTypePixelMap[_aaaParams.SpriteType];

        _sprite = GetScene();
        if (_sprite is Sprite2D spritesheet)
		{
            _texture = spritesheet.Texture as PortableCompressedTexture2D;
            _atlasTexture = spritesheet.Texture as AtlasTexture;
        }
        else if (_sprite is Sprite3D spritesheet3D)
        {
            _texture = spritesheet3D.Texture as PortableCompressedTexture2D;
            _atlasTexture = spritesheet3D.Texture as AtlasTexture;
        }
        else
        {
            GD.PrintErr("AAA ERROR || Current scene is not a Sprite2D or Sprite3D!");
            return;
        }

        //FileDialogOptions options = new();
        //string? appendLibPath = await this.Extensibility.Shell().ShowOpenFileDialogAsync(options, cancellationToken);

        int textureHeight;
        if (_texture != null)
        {
            textureHeight = _texture.GetHeight();
        }
        else if (_atlasTexture != null)
        {
            textureHeight = _atlasTexture.GetHeight();
        }


        AutoAnimateSprite3D();

        //GetTree().CreateTimer(5.0f).Timeout += () => SaveThisSceneAs(SavePath);

        //var scenePath = SaveDir + "//" + SavePath;
        //CallDeferred(MethodName.SaveThisSceneAs, SavePath);


        //EditorInterface.Singleton.SaveScene();
        //CallDeferred(MethodName.OpenAndSaveScene, _aaaParams.SavePath);
        OpenAndSaveScene(_aaaParams.SavePath);
        //GD.Print($"Finished Animation Automation!");
    }

    private void AutoAnimateSprite2D()
    {

    }
    private void AutoAnimateSprite3D()
    {
        var animPlayer = _sprite.GetFirstChildOfType<AnimationPlayer>();
        var globalAnimLibrary = animPlayer.GetAnimationLibrary("");
        ResourceSaver.Save(globalAnimLibrary, "res://testLib.tres");
        animPlayer.RemoveAnimationLibrary(""); // remove globalAnimLibrary


        //var appendLibrary = ResourceLoader.Load<AnimationLibrary>();

        // TODO: MAKE NEW SPRITE FOR BODY PARTS HERE
        int partNum = 1;
        int partOffset = 0;
        foreach (var bodyPartPair in _aaaParams.BodyParts)
        {
            var part = bodyPartPair.Key;
            var typesOfPart = bodyPartPair.Value;
            partOffset = _initOffset + (partNum - 1) * _frameHeight * typesOfPart * _aaaParams.AnimDirections.Count;
            Sprite3D partSprite;
            AnimationPlayer partPlayer;
            AnimationLibrary partLibrary = new AnimationLibrary();//globalAnimLibrary.Duplicate(true) as AnimationLibrary;
            if (partNum == 1)
            {
                partSprite = _sprite as Sprite3D;
                partPlayer = animPlayer;
                partPlayer.AddAnimationLibrary("", partLibrary);
            }
            else
            {
                //continue;
                partSprite = new Sprite3D();//_sprite.Duplicate((int)Node.DuplicateFlags.UseInstantiation) as Sprite3D;
                _sprite.AddChild(partSprite);
                partSprite.Owner = _sprite;
                partSprite.Texture = _atlasTexture.Duplicate(true) as AtlasTexture;
                partSprite.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
                partPlayer = new AnimationPlayer();//partSprite.GetFirstChildOfType<AnimationPlayer>();//new AnimationPlayer();
                partPlayer.AddAnimationLibrary("", partLibrary);
                partSprite.AddChild(partPlayer);
                partPlayer.Owner = _sprite;
                //partPlayer.Reparent(partSprite);

                partSprite.Name = part;
                partPlayer.Name = part + "AnimationPlayer";
                //_sprite.AddChild(partSprite);
                //partSprite.AddChild(partPlayer);
                
            }

            GD.Print($"children count of {partSprite}'s node: {partSprite.GetChildCount()}");

            //GD.Print($"part anim library anims: {partLibrary.GetAnimationList()}");
            //GD.Print($"blobal anim library anims: {globalAnimLibrary.GetAnimationList()}");

            foreach (var animName in globalAnimLibrary.GetAnimationList()) 
            {
                _currHeight = partOffset;
                //GD.Print($"Starting automating '{animName}'...");
                var anim = globalAnimLibrary.GetAnimation(animName);
                if (_aaaParams.AnimLoopMap.ContainsKey(animName))
                {
                    anim.LoopMode = _aaaParams.AnimLoopMap[animName];
                    //GD.Print($"Set loop mode to '{anim.LoopMode}'.");
                }
                int trackNum = 1;
                for (int typeNum = 1; typeNum <= typesOfPart; typeNum++)
                {
                    foreach (var dir in _aaaParams.AnimDirections)
                    {

                        var dirAnim = anim.Duplicate(true) as Animation;

                        var numFrames = dirAnim.TrackGetKeyCount(trackNum);
                        for (int i = 0; i < numFrames; i++)
                        {
                            var currRect = (Rect2)dirAnim.TrackGetKeyValue(trackNum, i);
                            var dirRect = new Rect2(currRect.Position.X, _currHeight,
                                currRect.Size.X, _frameHeight);

                            dirAnim.TrackSetKeyValue(trackNum, i, Variant.From(dirRect));
                        }
                        var newAnimName = animName + GetFaceDirectionString(dir) + typeNum.ToString();
                        GD.Print($"For {partPlayer.Name}'s animation '{newAnimName}', height is: {_currHeight}");
                        partLibrary.AddAnimation(newAnimName, dirAnim);
                        _currHeight += _frameHeight;
                        //partLibrary.RemoveAnimation(animName);
                    }
                }
            }
            partNum++;
            partOffset += _currHeight;
        }
        //globalAnimLibrary.Dispose();
    }

    private void SaveThisSceneAs(string scenePath)
    {
        GD.Print("saving Scene: ", EditorInterface.Singleton.GetEditedSceneRoot().Name);
        EditorInterface.Singleton.SaveSceneAs(scenePath);
        //EditorInterface.Singleton.OpenSceneFromPath(scenePath);S
        //var topNode = EditorInterface.Singleton.GetEditedSceneRoot();
        //topNode.Name = "TESTED WOOO";
        //EditorInterface.Singleton.SaveScene();

        //var packedScene = new PackedScene();
        //packedScene.Pack(GetTree().CurrentScene);
        //GD.Print("current scene num children: ", GetTree().CurrentScene.GetChildCount());
        //ResourceSaver.Save(packedScene, scenePath);

        //GetTree().Quit();
    }
    private void OpenAndSaveScene(string scenePath)
    {
        EditorInterface.Singleton.OpenSceneFromPath(scenePath);
        EditorInterface.Singleton.SaveScene();

        GD.Print($"Finished Animation Automation!");
        //var packedScene = new PackedScene();
        //packedScene.Pack(GetTree().CurrentScene);
        //GD.Print("current scene num children: ", GetTree().CurrentScene.GetChildCount());
        //ResourceSaver.Save(packedScene, scenePath);

        //GetTree().Quit();
    }
}
//#endif
