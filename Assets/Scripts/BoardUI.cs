using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BoardUI : MonoBehaviour
{
    // GameManager reference
    GameManager gm;

    // Assigned in editor
    public Theme theme;
    public GameObject numberPrefab;

    // Mesh and sprite renderers
    MeshRenderer[,] squareRenderers;
    public SpriteRenderer[,] pieceRenderers;

    private void Start(){
        // Get GameManager
        gm = FindObjectOfType<GameManager>();
    }

    public void CreateBoardUI(){
        // Create empty mesh and sprite renderers
        squareRenderers = new MeshRenderer[8, 8];
        pieceRenderers = new SpriteRenderer[8, 8];
        
        // Loop through ranks and files
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                // Get coord
                Coord coord = new Coord(rank, file);
                
                // Create square
                Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                square.gameObject.name = $"Square {GetSquareName(coord)}";
                square.SetParent(transform.GetChild(0));
                square.localScale = new Vector3(1, 1, 1);
                square.position = GetPosFromCoord(coord);

                // Create mesh on square
                MeshRenderer mr = square.GetComponent<MeshRenderer>();
                mr.material = theme.unlit;
                mr.material.color = (rank + file) % 2 == 0 ? theme.lightColor : theme.darkColor;
                squareRenderers[rank, file] = mr;

                // Create sprite in square
                SpriteRenderer sr = new GameObject("Piece").AddComponent<SpriteRenderer>();
                sr.transform.SetParent(square);
                sr.transform.localScale = Vector3.one * 0.2f;
                sr.transform.localPosition = -sr.transform.forward;
                pieceRenderers[rank, file] = sr;

                // Add rank number to left side
                if (rank == 0){
                    TMP_Text fileText = Instantiate(numberPrefab).GetComponent<TMP_Text>();
                    fileText.transform.SetParent(square);
                    fileText.transform.localPosition = -fileText.transform.forward;
                    fileText.text = (file + 1).ToString();
                    fileText.transform.SetParent(transform.GetChild(1));
                    fileText.gameObject.name = "File " + (file + 1).ToString();
                }

                // Add rank character to bottom
                if (file == 0){
                    TMP_Text rankText = Instantiate(numberPrefab).GetComponent<TMP_Text>();
                    rankText.transform.SetParent(square);
                    rankText.transform.localPosition = -rankText.transform.forward;
                    rankText.text = ((char)(rank + 65)).ToString();
                    rankText.alignment = TextAlignmentOptions.BottomRight;
                    rankText.transform.SetParent(transform.GetChild(1));
                    rankText.gameObject.name = "Rank " + ((char)(rank + 65)).ToString();
                }
            }
        }
    }

    public void UpdateBoard(Board board, bool flipBoard){
        // Rotate board if flipped
        transform.GetChild(0).localRotation = Quaternion.Euler(0, 0, flipBoard ? 180 : 0);

        // Loop through board squares
        for (int i = 0; i < board.squares.Length; i++)
        {
            // Get coord
            Coord coord = new Coord(i % 8, i / 8);
            // Update piece on coord
            UpdatePiece(coord, board.GetPieceFromCoord(coord), flipBoard);
        }
    }

    public void UpdatePiece(Coord coord, int piece, bool flipped){
        // Get piece sprite from theme
        Sprite sprite = theme.GetPieceSprite(piece);
        // Get sprite renderer on coord
        SpriteRenderer renderer = pieceRenderers[coord.rank, coord.file];

        if (renderer != null){
            // Assign sprite
            renderer.sprite = sprite;
            // Rotate if board is flipped
            renderer.transform.localRotation = Quaternion.Euler(flipped ? 180 : 0, flipped ? 180 : 0, 0);
        }
    }

    public void SelectSquare(Coord coord){
        // Set square color to selected theme color
        squareRenderers[coord.rank, coord.file].material.color = (coord.rank + coord.file) % 2 == 0 ? theme.lightSelectedColor : theme.darkSelectedColor;
    }

    public void DeselectSquare(Coord coord){
        // Reset square color to theme color
        squareRenderers[coord.rank, coord.file].material.color = (coord.rank + coord.file) % 2 == 0 ? theme.lightColor : theme.darkColor;
    }

    public void HighlightLegalSquares(List<Move> moves){
        if (moves == null) return;
        
        // Loop through legal moves
        foreach (Move coord in moves)
        {
            // Check that it's inside array
            if (coord.to.rank < 0 || coord.to.rank > 7 || coord.to.file < 0 || coord.to.file > 7) continue;
            // Set square color to legal theme color
            squareRenderers[coord.to.rank, coord.to.file].material.color = (coord.to.rank + coord.to.file) % 2 == 0 ? theme.lightLegalColor : theme.darkLegalColor;
        }
    }

    public void ResetAllSquareColors(){
        // Loop through all squares
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                // Reset square color to theme color
                squareRenderers[rank, file].material.color = (rank + file) % 2 == 0 ? theme.lightColor : theme.darkColor;
            }
        }
    }

    private Vector3 GetPosFromCoord(Coord coord, int depth = 0){
        return new Vector3(coord.rank - 3.5f, coord.file - 3.5f, -depth);
    }

    public string GetSquareName(Coord coord){
        char fileLetter = (char)(coord.file + 65);
        return $"{fileLetter}{coord.rank + 1}";
    }

    public void VisualizeBoard(Board board){
        for (int rank = 7; rank >= 0; rank--)
        {
            string line = "";
            for (int file = 0; file < 8; file++)
            {
                line += Piece.PieceToFenChar(board.squares[rank * 8 + file]) + "\t";
            }
            Debug.Log(line);
        }
    }
}
