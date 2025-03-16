using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public partial class PiecesRndOnDestroy : PiecesBaseOnDestroy
{
    [Export]
    public Vector2I PieceSpawnRange { get; private set; } = new Vector2I(2, 4);

    public PiecesRndOnDestroy() : base()
    {
        if (Engine.IsEditorHint())
        {
            return;
        }
        var numPieces = Global.Rnd.Next(PieceSpawnRange.X, PieceSpawnRange.Y);

        CallDeferred(MethodName.GetRndPieceList, numPieces);
        CallDeferred(MethodName.InitializePieces);
    }
    public override void Destroy()
    {
        base.Destroy();
    }
    public void GetRndPieceList(int numPieces)
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
}
