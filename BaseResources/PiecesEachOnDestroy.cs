using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass, Tool]
public partial class PiecesEachOnDestroy : PiecesBaseOnDestroy
{

    public PiecesEachOnDestroy() : base()
    {
        if (Engine.IsEditorHint())
        {
            return;
        }
        CallDeferred(MethodName.GetPieceList);
        CallDeferred(MethodName.InitializePieces);
    }
    public override void Destroy()
    {
        base.Destroy();
    }
    public void GetPieceList()
    {
        //GD.Print("PackedScene path: ", PieceScene.ResourcePath);
        //GD.Print("NUMBER OF PIECE TEXTS AVAILABLE: ", PieceTextures.Length);

        _pieceList = new List<string>();
        foreach (var piece in PieceTextures)
        {
            _pieceList.Add(piece);
        }
    }
}
