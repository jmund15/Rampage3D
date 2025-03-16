using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public abstract partial class PiecesBaseOnDestroy : BreakableOnDestroyStrategy
{
    [Export]
    public PackedScene PieceScene { get; private set; } = null;

    protected string[] _pieceTextures = Array.Empty<string>();
    [Export/*(PropertyHint.File, ".png")*/]
    public string[] PieceTextures 
    { 
        get => _pieceTextures;
        set 
        {
            _pieceTextures = value;
            GD.Print("PIECE TEXTURES SET TO Length: ", _pieceTextures.Length);
        }
    } 
    //public Godot.Collections.Array<string> PieceTextures { get; set; } = new Godot.Collections.Array<string>();
    
    [Export]
    public float PieceFadeTime { get; private set; } = 2.5f;
    [Export]
    public float PieceFadeDelay { get; private set; } = 2.5f;
    [Export]
    public Vector2 PieceForceRange { get; private set; } = new Vector2(5, 15);
    [Export]
    public Vector2 PieceForceRandRange { get; private set; } = new Vector2(0.5f, 0.75f);

    protected List<string> _pieceList = new List<string>();
    protected List<RigidBody3D> _pieces = new List<RigidBody3D>();
    public PiecesBaseOnDestroy() : base()
    {
    }
    public override void Destroy()
    {
        var hurtboxComp = BB.GetVar<HurtboxComponent3D>(BBDataSig.HurtboxComp);
        hurtboxComp.DeactivateHurtbox();
        var visuals = BB.GetVar<Node3D>(BBDataSig.Sprite);
        visuals.Hide();
        var healthComp = BB.GetVar<HealthComponent>(BBDataSig.HealthComp);
        var update = healthComp.LastHealthUpdate;
        if (update.Attack is null)
        {
            PiecesFlyOut(_pieces);
        }
        else
        {
            var hitDir = update.Attack.Direction;
            var hitForce = update.Attack.Force;
            PiecesFlyOut(_pieces, hitForce, hitDir);
        }
    }
    protected virtual void InitializePieces()
    {
        foreach (var piece in _pieceList)
        {
            var pieceText = ResourceLoader.Load<CompressedTexture2D>(piece);
            var pieceBody = PieceScene.Instantiate() as RigidBody3D;
            var pieceSprite = pieceBody.GetFirstChildOfType<Sprite3DComponent>();

            pieceSprite.Texture = pieceText;
            pieceBody.Position += Global.GetRndVector3ZeroY().Normalized() * 0.25f; // seperate broken pieces;
            pieceBody.Hide();
            pieceBody.SetDeferred(RigidBody3D.PropertyName.Freeze, true);
            Breakable.CallDeferred(Node.MethodName.AddChild, pieceBody);
            //CallDeferred(MethodName.SetPieceCollShape, pieceBody);
            _pieces.Add(pieceBody);
        }
    }
    protected virtual void PiecesFlyOut(List<RigidBody3D> pieces, float force = 0f, Vector3? hitDirection = null)
    {
        var pieceTween = Breakable.CreateTween();
        foreach (var piece in pieces)
        {
            piece.Show();
            var dropDir = hitDirection.HasValue ? 
                (Global.GetRndVector3PosY().Normalized() + hitDirection.Value).Normalized() : 
                Global.GetRndVector3PosY().Normalized();

            var dropForce = Mathf.Clamp(force, PieceForceRange.X, PieceForceRange.Y);
            var randForcemMult = Global.GetRndInRange(PieceForceRandRange.X, PieceForceRandRange.Y);
            var dropImpulse = dropForce * dropDir * randForcemMult;

            piece.SetDeferred(RigidBody3D.PropertyName.Freeze, false);
            piece.CallDeferred(RigidBody3D.MethodName.ApplyCentralImpulse, dropImpulse);
            pieceTween.Parallel().TweenProperty(
                piece.GetFirstChildOfType<Sprite3DComponent>(), "modulate:a", 0, PieceFadeTime)
                .SetEase(Tween.EaseType.In)
                .SetDelay(PieceFadeDelay);

            pieceTween.Parallel().TweenCallback(Callable.From(piece.QueueFree))
                .SetDelay(PieceFadeTime + PieceFadeDelay);

            //var collTween = CreateTween();
            //if (BreakableHealth > 0)
            //{
            //    piece.SetCollisionMaskValue(4, false);
            //}
            //collTween.TweenCallback(Callable.From(() => piece.SetCollisionMaskValue(4, true))).SetDelay(1.0f);
        }
        pieceTween.TweenCallback(Callable.From(Breakable.QueueFree));
    }

    protected virtual void SetPieceCollShape(RigidBody3D pieceBody)
    {
        var pieceSprite = pieceBody.GetFirstChildOfType<Sprite3DComponent>();
        GD.Print("sprite height: ", pieceSprite.SpriteHeight, "\nsprite widgth: ", pieceSprite.SpriteWidth);

        var pieceCollShape = pieceBody.GetFirstChildOfType<CollisionShape3D>().Shape as CapsuleShape3D;
        if (pieceSprite.SpriteHeight > pieceSprite.SpriteWidth)
        {
            pieceCollShape.Height = pieceSprite.SpriteHeight;
        }
        else
        {
            pieceCollShape.Height = pieceSprite.SpriteWidth;
            pieceBody.GetFirstChildOfType<CollisionShape3D>().RotationDegrees =
                new Vector3(45, 0, -90);
        }
    }
}
