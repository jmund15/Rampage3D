using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public partial class PiecesOnDamage : BreakableOnDamageStrategy
{
    [Export]
    public PackedScene PieceScene { get; private set; } = null;

    private string[] _pieceTextures = Array.Empty<string>();
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
    public Vector2I PieceSpawnRange { get; private set; } = new Vector2I(2, 4);
    [Export]
    public float PieceFadeTime { get; private set; } = 2.5f;
    [Export]
    public float PieceFadeDelay { get; private set; } = 2.5f;
    [Export]
    public Vector2 PieceForceRange { get; private set; } = new Vector2(5, 15);
    [Export]
    public Vector2 PieceForceRandRange { get; private set; } = new Vector2(0.5f, 0.75f);

    private List<string> _pieceList = new List<string>();
    private List<RigidBody3D> _pieces = new List<RigidBody3D>();
    public PiecesOnDamage() : base()
    {
        if (Engine.IsEditorHint())
        {
            return;
        }
        var numPieces = Global.Rnd.Next(PieceSpawnRange.X, PieceSpawnRange.Y);
        CallDeferred(MethodName.GetPieceList, numPieces);
        CallDeferred(MethodName.InitializePieces);
    }
    public override void Damage()
    {
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
    public void GetPieceList(int numPieces)
    {
        GD.Print("PackedScene path: ", PieceScene.ResourcePath);
        GD.Print("NUMBER OF PIECE TEXTS AVAILABLE: ", PieceTextures.Length);

        _pieceList = new List<string>();
        for (int i = 0; i < numPieces; i++)
        {
            int piece;
            piece = Global.Rnd.Next(0, PieceTextures.Length);
            _pieceList.Add(PieceTextures[piece]);
        }
        //return pieceList;
    }
    protected virtual void InitializePieces()
    {
        foreach (var piece in _pieceList)
        {
            var pieceText = ResourceLoader.Load<CompressedTexture2D>(piece);
            var pieceBody = PieceScene.Instantiate() as RigidBody3D;

            pieceBody.Position += Global.GetRndVector3ZeroY().Normalized() * 0.25f; // seperate broken pieces;
            pieceBody.GetFirstChildOfType<Sprite3DComponent>().Texture = pieceText;
            pieceBody.Hide();
            pieceBody.SetDeferred(RigidBody3D.PropertyName.Freeze, true);
            Breakable.CallDeferred(Node.MethodName.AddChild, pieceBody);
            _pieces.Add(pieceBody);
        }
    }

    protected virtual void PiecesFlyOut(List<RigidBody3D> pieces, float force = 0f, Vector3? hitDirection = null)
    {
        var pieceTween = Breakable.CreateTween();
        //var visuals = BB.GetVar<Node3D>(BBDataSig.Sprite);
        //visuals.Hide();
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
    }
}
