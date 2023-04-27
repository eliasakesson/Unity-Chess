using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Script References
    Board board;
    BoardUI boardUI;
    MovesHandler movesHandler;
    AIPlayer aiPlayer;
    UIManager uiManager;

    // Game Modes
    public enum GameMode {
        Local,
        Computer,
        Stockfish
    }

    public GameMode gameMode;

    // Start Positions
    const string FEN_START = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
    const string FEN_CASTLING = "r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R";
    const string FEN_CHECK_TEST = "2K5/q5QQ/4k3/8/8/8/8/8 b - - 0 2";

    // Game running
    public bool gameRunning = false;
    public bool canStartGame = false;

    // Whose turn
    public bool startPlayerIsWhite = true;
    public bool isWhitesTurn = true;

    public bool boardFlipped = false;

    // Match duration
    float startTime = 300f;
    float whitesTimeLeft, blacksTimeLeft;
    
    // Stockfish search depth
    int stockfishDepth = 5;

    private void Awake() {
        // Get script references
        boardUI = FindObjectOfType<BoardUI>();
        board = GetComponent<Board>();
        movesHandler = GetComponent<MovesHandler>();
        aiPlayer = GetComponent<AIPlayer>();
        uiManager = GetComponent<UIManager>();

        // Setup game
        boardUI.CreateBoardUI();
        SetupGame();
    }

    private void Update(){
        if (gameRunning){
            UpdateTimer();
        }
    }

    public void SetupGame(){
        // Set start position
        board.PositionFromFen(FEN_START);
        // Update board pieces
        boardUI.UpdateBoard(board, boardFlipped);
        // Reset board colors
        boardUI.ResetAllSquareColors();
        // Set ui flipped
        uiManager.FlipBoard(boardFlipped);

        // Set match duration
        whitesTimeLeft = startTime;
        blacksTimeLeft = startTime;

        // Update timers
        uiManager.UpdateTimer(true, whitesTimeLeft);
        uiManager.UpdateTimer(false, blacksTimeLeft);
    }

    public void StartButtonPressed(){
        canStartGame = true;
        isWhitesTurn = true;

        // Make starting move if AI is white
        if (gameMode != GameMode.Local && isWhitesTurn != startPlayerIsWhite){
            if (gameMode == GameMode.Computer){
                aiPlayer.MakeAIMove();
            } else{
                aiPlayer.MakeStockfishMove(stockfishDepth);
            }
        }
    }

    public void StartGame(){
        gameRunning = true;
        canStartGame = false;

        whitesTimeLeft = startTime;
        blacksTimeLeft = startTime;

        board.moveIndex = 0;
        board.previousSquares.Clear();
        aiPlayer.SetRandomOpeningSequence();
    }

    public void MoveMade(){
        // Start game if not started
        if (!gameRunning){
            StartGame();
        }

        // Loop through legal moves to check for checkmate
        if (board.GetAllLegalMoves(movesHandler, !isWhitesTurn).Count == 0){
            // If checkmate, stop game
            gameRunning = false;
            isWhitesTurn = true;
            canStartGame = true;

            // Open win menu UI with message based on who won
            if (isWhitesTurn){
                switch (gameMode){
                    case GameMode.Local:
                        uiManager.OpenWinMenu("White won!", "White won by checkmate");
                        break;
                    case GameMode.Computer:
                        uiManager.OpenWinMenu("You won!", "You won by checkmate");
                        break;
                    case GameMode.Stockfish:
                        uiManager.OpenWinMenu("You won!", "You won by checkmate");
                        break;
                    default:
                        break;
                }
            } else{
                switch (gameMode){
                    case GameMode.Local:
                        uiManager.OpenWinMenu("Black won!", "Black won by checkmate");
                        break;
                    case GameMode.Computer:
                        uiManager.OpenWinMenu("The computer won!", "It won by checkmate");
                        break;
                    case GameMode.Stockfish:
                        uiManager.OpenWinMenu("Stockfish won!", "It won by checkmate");
                        break;
                    default:
                        break;
                }
            }
        }

        // Change turn
        isWhitesTurn = !isWhitesTurn;

        // If AI's turn, make AI move
        if (gameMode != GameMode.Local && isWhitesTurn != startPlayerIsWhite){
            if (gameMode == GameMode.Computer){
                aiPlayer.MakeAIMove();
            } else{
                aiPlayer.MakeStockfishMove(stockfishDepth);
            }
        }

        // Update board pieces and colors
        boardUI.UpdateBoard(board, boardFlipped);
        boardUI.ResetAllSquareColors();
    }

    private void UpdateTimer(){
        // Reduce current player's time
        if (isWhitesTurn){
            whitesTimeLeft -= Time.deltaTime;

            // If time is out, stop game and open win UI
            if (whitesTimeLeft <= 0){
                gameRunning = false;

                switch (gameMode){
                    case GameMode.Local:
                        uiManager.OpenWinMenu("Black won!", "Black won by time");
                        break;
                    case GameMode.Computer:
                        uiManager.OpenWinMenu("The computer won!", "It won by time");
                        break;
                    case GameMode.Stockfish:
                        uiManager.OpenWinMenu("Stockfish won!", "It won by time");
                        break;
                    default:
                        break;
                }
            }
        } else{
            blacksTimeLeft -= Time.deltaTime;

            // If time is out, stop game and open win UI
            if (blacksTimeLeft <= 0){
                gameRunning = false;

                switch (gameMode){
                    case GameMode.Local:
                        uiManager.OpenWinMenu("White won!", "White won by time");
                        break;
                    case GameMode.Computer:
                        uiManager.OpenWinMenu("You won!", "You won by time");
                        break;
                    case GameMode.Stockfish:
                        uiManager.OpenWinMenu("You won!", "You won by time");
                        break;
                    default:
                        break;
                }
            }
        }

        // Update timers
        uiManager.UpdateTimer(true, whitesTimeLeft);
        uiManager.UpdateTimer(false, blacksTimeLeft);
    }

    public void SetGameMode(string gamemode){
        // Set gamemode enum from string
        switch (gamemode){
            case "Local":
                gameMode = GameMode.Local;
                uiManager.SetNames("Player White", "Player Black");
                break;
            case "Computer":
                gameMode = GameMode.Computer;
                uiManager.SetNames("Player", "AI");
                break;
            case "Stockfish":
                gameMode = GameMode.Stockfish;
                uiManager.SetNames("Player", "Stockfish");
                break;
            default:
                return;
        }
    }

    public void SetStartColor(string color){
        // Set local color
        if (color == "white"){
            startPlayerIsWhite = true;
        } else if (color == "black"){
            startPlayerIsWhite = false;
        } else {
            startPlayerIsWhite = Random.Range(0, 2) == 0;
        }

        boardFlipped = !startPlayerIsWhite;
        // Update board pieces
        boardUI.UpdateBoard(board, boardFlipped);
        uiManager.FlipBoard(boardFlipped);
    }

    public void SetStartTime(float time){
        startTime = time;
    }

    public void SetStockfishDepth(float rating){
        stockfishDepth = (int)(rating / 200);
    }

    public void FlipBoard(){
        boardFlipped = !boardFlipped;
        boardUI.UpdateBoard(board, boardFlipped);

        uiManager.FlipBoard(boardFlipped);
    }

    public void Resign(){
        gameRunning = false;

        switch (gameMode){
            case GameMode.Local:
                uiManager.OpenWinMenu(isWhitesTurn ? "Black" : "White" + " won!", isWhitesTurn ? "Black" : "White" + " won by resignation");
                break;
            case GameMode.Computer:
                uiManager.OpenWinMenu("The computer won!", "It won by resignation");
                break;
            case GameMode.Stockfish:
                uiManager.OpenWinMenu("Stockfish won!", "It won by resignation");
                break;
            default:
                break;
        }
    }
}
