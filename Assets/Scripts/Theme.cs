using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Theme : ScriptableObject
{
    public Color lightColor, darkColor;
    public Color lightSelectedColor, darkSelectedColor;
    public Color lightLegalColor, darkLegalColor;
    public Material unlit;

    public PieceSprites whitePieceSprites, blackPieceSprites;

    public Sprite GetPieceSprite(int piece){
        if(piece == Piece.Empty){
            return null;
        }

        bool isWhite = piece < Piece.Black;
        PieceSprites pieceSprites = isWhite ? whitePieceSprites : blackPieceSprites;
        piece = isWhite ? piece - Piece.White : piece - Piece.Black;

        return pieceSprites.GetSprite(piece);
    }
}

[System.Serializable]
public class PieceSprites {
    public Sprite pawn, knight, bishop, rook, queen, king;

    public Sprite GetSprite(int piece){
        switch(piece){
            case Piece.Pawn:
                return pawn;
            case Piece.Knight:
                return knight;
            case Piece.Bishop:
                return bishop;
            case Piece.Rook:
                return rook;
            case Piece.Queen:
                return queen;
            case Piece.King:
                return king;
            default:
                return null;
        }
    }
}
