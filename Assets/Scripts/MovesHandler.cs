using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovesHandler : MonoBehaviour
{
    GameManager gm;
    Board board;

    private void Start(){
        gm = GetComponent<GameManager>();
    }

    public List<Move> GetLegalMoves(Board board, Coord coord, bool isWhite, bool onlyLegalMoves = true){
        List<Move> moves = new List<Move>();
        
        this.board = board;
        moves.Clear();

        int piece = board.GetPieceFromCoord(coord);

        piece = Piece.PieceType(piece);

        if(piece == Piece.Empty){
            return null;
        }

        switch (piece)
        {
            case Piece.Pawn:
                moves.AddRange(GetPawnMoves(coord, isWhite));
                break;
            case Piece.Knight:
                moves.AddRange(GetKnightMoves(coord, isWhite));
                break;
            case Piece.Bishop:
                moves.AddRange(GetBishopMoves(coord, isWhite));
                break;
            case Piece.Rook:
                moves.AddRange(GetRookMoves(coord, isWhite));
                break;
            case Piece.Queen:
                moves.AddRange(GetQueenMoves(coord, isWhite));
                break;
            case Piece.King:
                moves.AddRange(GetKingMoves(coord, isWhite));
                break;
            default:
                break;
        }

        if (!onlyLegalMoves){
            return moves;
        }

        List<Move> legalMoves = new List<Move>();
        foreach (Move move in moves)
        {
            if (!TryIfMoveResultsInKingDanger(move, isWhite)){
                move.priority = board.GetPieceFromCoord(move.to) - board.GetPieceFromCoord(move.from);
                legalMoves.Add(move);
            }
        }

        return legalMoves;
    }

    private bool TryIfMoveResultsInKingDanger(Move move, bool isWhite){
        int[] tempSquares = board.squares.Clone() as int[];
        board.TempMovePiece(move);

        for (int i = 0; i < board.squares.Length; i++)
        {
            List<Move> enemyMoves = GetLegalMoves(board, new Coord(i % 8, i / 8), !isWhite, false);

            if (enemyMoves == null){
                continue;
            }

            foreach (Move enemyMove in enemyMoves)
            {
                if (Piece.PieceColor(board.GetPieceFromCoord(enemyMove.from)) == (isWhite ? Piece.White : Piece.Black)) continue;
                
                int piece = board.GetPieceFromCoord(enemyMove.to);
                if (Piece.PieceType(piece) == Piece.King && Piece.PieceColor(piece) == (isWhite ? Piece.White : Piece.Black)){
                    board.squares = tempSquares;
                    return true;
                }
            }
        }

        board.squares = tempSquares;
        return false;
    }

    private List<Move> GetKingMoves(Coord coord, bool isWhite){
        Vector2Int[] directions = new Vector2Int[8]{
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(1, -1)
        };

        List<Move> moves = GetSlidingMoves(coord, isWhite, directions, 1);

        // Castling

        int file = isWhite ? 0 : 7;
        int color = isWhite ? Piece.White : Piece.Black;

        if (coord.rank == 4 && coord.file == file){
            // King side
            if (board.GetPieceFromCoord(new Coord(5, file)) == Piece.Empty && board.GetPieceFromCoord(new Coord(6, file)) == Piece.Empty){
                if (board.GetPieceFromCoord(new Coord(7, file)) == Piece.Rook + color){

                    moves.Add(new Move(coord, new Coord(6, file), new Coord(7, file), new Coord(5, file)));
                }
            }
            // Queen side
            if (board.GetPieceFromCoord(new Coord(1, file)) == Piece.Empty && board.GetPieceFromCoord(new Coord(2, file)) == Piece.Empty && board.GetPieceFromCoord(new Coord(3, file)) == Piece.Empty){
                if (board.GetPieceFromCoord(new Coord(0, file)) == Piece.Rook + color){
                    moves.Add(new Move(coord, new Coord(2, file), new Coord(0, file), new Coord(3, file)));
                }
            }
        }

        return moves;
    }

    private List<Move> GetQueenMoves(Coord coord, bool isWhite){
        Vector2Int[] directions = new Vector2Int[8]{
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(1, -1)
        };

        List<Move> moves = GetSlidingMoves(coord, isWhite, directions);
        return moves;
    }

    private List<Move> GetRookMoves(Coord coord, bool isWhite){
        Vector2Int[] directions = new Vector2Int[4]{
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, -1)
        };

        List<Move> moves = GetSlidingMoves(coord, isWhite, directions);
        return moves;
    }

    private List<Move> GetBishopMoves(Coord coord, bool isWhite){
        Vector2Int[] directions = new Vector2Int[4]{
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
        };

        List<Move> moves = GetSlidingMoves(coord, isWhite, directions);
        return moves;
    }

    private List<Move> GetKnightMoves(Coord coord, bool isWhite){ 
        Vector2Int[] directions = new Vector2Int[8]{
            new Vector2Int(1, 2),
            new Vector2Int(2, 1),
            new Vector2Int(2, -1),
            new Vector2Int(1, -2),
            new Vector2Int(-1, -2),
            new Vector2Int(-2, -1),
            new Vector2Int(-2, 1),
            new Vector2Int(-1, 2)
        };

        List<Move> moves = new List<Move>();

        for (int i = 0; i < directions.Length; i++)
        {
            Coord move = new Coord(coord.rank + directions[i].x, coord.file + directions[i].y);
            int movePiece = board.GetPieceFromCoord(move);
            if(movePiece != Piece.OutOfBounds && Piece.PieceColor(movePiece) != (isWhite ? Piece.White : Piece.Black)){
                moves.Add(new Move(coord, move));
            }
        }

        return moves;
    }

    private List<Move> GetPawnMoves(Coord coord, bool isWhite){
        List<Move> moves = new List<Move>();
        
        int direction = isWhite ? 1 : -1;

        // Forward
        Coord move = new Coord(coord.rank, coord.file + direction);
        if(board.GetPieceFromCoord(move) == Piece.Empty){
            moves.Add(new Move(coord, move));
        }

        // Double Forward
        if (direction == 1 && coord.file == 1 || direction == -1 && coord.file == 6)
        {
            move = new Coord(coord.rank, coord.file + direction * 2);
            if(board.GetPieceFromCoord(move) == Piece.Empty){
                moves.Add(new Move(coord, move));
            }
        }

        // Diagonal Left
        move = new Coord(coord.rank - 1, coord.file + direction);
        if(board.GetPieceFromCoord(move) != Piece.Empty && CoordIsInsideBoard(move) && board.GetPieceFromCoord(move) < Piece.Black != isWhite){
            moves.Add(new Move(coord, move));
        }

        // Diagonal Right
        move = new Coord(coord.rank + 1, coord.file + direction);
        if(board.GetPieceFromCoord(move) != Piece.Empty && CoordIsInsideBoard(move) && board.GetPieceFromCoord(move) < Piece.Black != isWhite){
            moves.Add(new Move(coord, move));
        }

        return moves;
    }

    private List<Move> GetSlidingMoves(Coord coord, bool isWhite, Vector2Int[] directions, int maxLength = 8){
        List<Move> moves = new List<Move>();
        
        for (int i = 0; i < directions.Length; i++)
        {
            int length = GetLengthOfStraight(coord, directions[i]) > maxLength ? maxLength : GetLengthOfStraight(coord, directions[i]);
            for (int y = 0; y < length; y++)
            {
                Coord move = new Coord(coord.rank + directions[i].x * (y + 1), coord.file + directions[i].y * (y + 1));
                int movePiece = board.GetPieceFromCoord(move);
                if (movePiece == Piece.OutOfBounds || Piece.PieceColor(movePiece) == (isWhite ? Piece.White : Piece.Black))
                {
                    break;
                }

                moves.Add(new Move(coord, move));

                if (Piece.PieceColor(movePiece) == (isWhite ? Piece.Black : Piece.White))
                {
                    break;
                }
            }
        }

        return moves;
    }

    private int GetLengthOfStraight(Coord coord, Vector2Int direction){
        int length = 1;
        Coord move = new Coord(coord.rank + direction.x, coord.file + direction.y);
        while(board.GetPieceFromCoord(move) != Piece.OutOfBounds){
            length++;
            move = new Coord(move.rank + direction.x, move.file + direction.y);
        }
        return length;
    }

    private bool CoordIsInsideBoard(Coord coord){
        return coord.rank >= 0 && coord.rank < 8 && coord.file >= 0 && coord.file < 8;
    }
}
