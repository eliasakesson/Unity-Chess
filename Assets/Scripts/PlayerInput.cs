using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    GameManager gameManager;
    BoardUI boardUI;
    Board board;
    MovesHandler movesHandler;
    UIManager uiManager;

    Coord selectedCoord = null;

    bool mouseDown = false;

    private void Start() {
        gameManager = GetComponent<GameManager>();
        board = GetComponent<Board>();
        movesHandler = GetComponent<MovesHandler>();
        uiManager = GetComponent<UIManager>();
        boardUI = FindObjectOfType<BoardUI>();
    }

    private void Update() {
        if (gameManager.gameMode != GameManager.GameMode.Local && gameManager.isWhitesTurn != gameManager.startPlayerIsWhite){
            return;
        }
        
        if (Input.GetMouseButtonDown(0)){
            OnMouseDown();
        }

        if (Input.GetMouseButtonUp(0)){
            OnMouseUp();
        }

        if (mouseDown){
            OnMouseMove();
        }
    }

    private void OnMouseDown(){
        mouseDown = true;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (TryGetSquare(mousePos, out Coord coord)){
            int pieceFromCoord = board.GetPieceFromCoord(coord);
                
            // We have a piece selected, so we want to move it
            if (selectedCoord != null){
                // Try to move to coord
                if (TryMoveToCoord(coord)) return;
                
                // If clicked on own piece and it isn't same, select it
                if (pieceFromCoord != Piece.Empty && Piece.PieceColor(pieceFromCoord) == (gameManager.isWhitesTurn ? Piece.White : Piece.Black) && (coord.rank != selectedCoord.rank || coord.file != selectedCoord.file)){
                    SelectCoord(coord);
                }
                // Deselct coord
                else DeselectCoord();
            }
            // We don't have a piece selected, so we want to select it if its the right players turn
            else if (Piece.PieceColor(pieceFromCoord) == (gameManager.isWhitesTurn ? Piece.White : Piece.Black)){
                SelectCoord(coord);
            }
        } 
        // Clicked outside board, so deselct
        else DeselectCoord();
    }

    private void OnMouseMove(){
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (selectedCoord == null) return;

        SpriteRenderer pieceRenderer = boardUI.pieceRenderers[selectedCoord.rank, selectedCoord.file];
        pieceRenderer.transform.position = new Vector3(mousePos.x, mousePos.y, -pieceRenderer.transform.forward.z * 2);
    }

    private void OnMouseUp(){
        mouseDown = false;

        if (selectedCoord == null) return;

        SpriteRenderer pieceRenderer = boardUI.pieceRenderers[selectedCoord.rank, selectedCoord.file];
        pieceRenderer.transform.localPosition = -pieceRenderer.transform.forward;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (TryGetSquare(mousePos, out Coord coord)){
            if (coord.rank != selectedCoord.rank || coord.file != selectedCoord.file){
                TryMoveToCoord(coord);
            }
        }
    }

    private void SelectCoord(Coord coord){
        selectedCoord = coord;
        boardUI.ResetAllSquareColors();
        boardUI.SelectSquare(selectedCoord);
        
        List<Move> moves = movesHandler.GetLegalMoves(board, coord, gameManager.isWhitesTurn);
        if (moves != null){
            boardUI.HighlightLegalSquares(moves);
        }
    }

    private void DeselectCoord(){
        boardUI.ResetAllSquareColors();
        selectedCoord = null;
    }

    private bool TryMoveToCoord(Coord coord){
        List<Move> moves = movesHandler.GetLegalMoves(board, selectedCoord, gameManager.isWhitesTurn);
        // If move is legal
        if (moves != null){
            foreach (Move move in moves){
                if (move.to.rank == coord.rank && move.to.file == coord.file){
                    board.MovePiece(move);
                
                    if (board.CanPromote(coord)){
                        uiManager.OpenPromotionMenu(coord);
                        return true;
                    }
                    
                    selectedCoord = null;
                    gameManager.MoveMade();

                    return true;
                }
            } 
        } 

        return false;
    }

    public bool TryGetSquare(Vector2 pos, out Coord coord){
        int rank = Mathf.RoundToInt(pos.x + 3.5f);
        int file = Mathf.RoundToInt(pos.y + 3.5f);

        if (gameManager.boardFlipped){
            rank = 7 - rank;
            file = 7 - file;
        }

        coord = new Coord(rank, file);

        if(rank < 0 || rank > 7 || file < 0 || file > 7){
            return false;
        }

        return true;
    }

    public void PromoteAfterInput(Coord coord, int piece){
        board.Promote(coord, piece);
        
        selectedCoord = null;

        gameManager.MoveMade();
    }
}
