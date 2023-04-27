using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Diagnostics;

public class AIPlayer : MonoBehaviour
{
    // Create a instance of the AIPlayer
    public static AIPlayer Instance;

    // Script references
    GameManager gameManager;
    MovesHandler movesHandler;
    Board board;
    BoardUI boardUI;

    // Private variables
    Move moveToDo;
    Process process;

    // Bot opening moves
    public OpeningMoveSequence[] openingMoveSequences;  
    Move[] openingMoves;
    
    // Bot settings
    public float minWaitTime = 0.5f, maxWaitTime = 1.5f;
    float lastStockfishMoveTime = 0f;
    float stockfishCurrentDelay;

    // AI booleans
    bool shoudlMakeStockfishMove = false;

    private void Start() {
        // Create a instance of the AIPlayer
        Instance = this;

        // Get script references
        gameManager = GetComponent<GameManager>();
        movesHandler = GetComponent<MovesHandler>();
        board = GetComponent<Board>();
        boardUI = GetComponent<BoardUI>();

        SetRandomOpeningSequence();

        // Setup Stockfish
        SetupStockfish();
    }

    private void Update() {
        // If should make stockfish move,
        // and the time since last stockfish move is greater than the minimum wait time,
        if (shoudlMakeStockfishMove && Time.time - lastStockfishMoveTime > stockfishCurrentDelay){
            // Make the move
            MakeMove();
            
            // Should no longer make stockfish move
            shoudlMakeStockfishMove = false;
        }
    }

    public void SetRandomOpeningSequence(){
        openingMoves = openingMoveSequences[Random.Range(0, openingMoveSequences.Length)].moves;
    }

    public void MakeStockfishMove(int depth){
        // Set the time since last stockfish move to now
        lastStockfishMoveTime = Time.time;
        // Longer time for bot to play the longer into the game until maxwaittime
        stockfishCurrentDelay = Random.Range(minWaitTime, Mathf.Min(maxWaitTime, minWaitTime + board.moveIndex * 0.25f));

        // Get the stockfish move
        GetStockfishMove(board.BoardToFenString(), depth);
    }

    public void MakeAIMove(){
        // Get all legal moves
        List<Move> moves = board.GetAllLegalMoves(movesHandler, gameManager.isWhitesTurn);

        // Return if no moves found
        if (moves.Count == 0){
            UnityEngine.Debug.Log("No moves found for ai");
            return;
        }

        SetMovePriorities(ref moves);
        List<Move> bestMoves = GetBestMoves(moves);

        // Return if no best moves found
        if (bestMoves.Count == 0){
            UnityEngine.Debug.Log("No best moves found for ai");
            moveToDo = moves[Random.Range(0, moves.Count - 1)];

            Invoke("MakeMove", Random.Range(minWaitTime, maxWaitTime));
            return;
        }

        // Choose random move from best moves
        moveToDo = bestMoves[Random.Range(0, bestMoves.Count - 1)];
        // Write move to console
        UnityEngine.Debug.Log("AI is moving from " + moveToDo.ToString());

        // Make move after random time
        // Faster at beginning, slowing down based on count of moves but capping at maxWaitTime
        Invoke("MakeMove", Random.Range(minWaitTime, Mathf.Min(maxWaitTime, minWaitTime + board.moveIndex * 0.25f)));
    }

    private void SetMovePriorities(ref List<Move> moves){
        // Loop through all moves
        foreach(Move move in moves){
            // Clone all pieces on the board
            int[] tempSquares = board.squares.Clone() as int[];
            // Temporaraly make the move
            board.TempMovePiece(move);

            int bestPiece = 0;
            // Loop through all squares on board
            for (int i = 0; i < board.squares.Length; i++)
            {
                // Get moves enemy can make
                List<Move> enemyMoves = movesHandler.GetLegalMoves(board, new Coord(i % 8, i / 8), !gameManager.isWhitesTurn, false);

                if (enemyMoves == null){
                    continue;
                }

                // Loop through enemy moves
                foreach (Move enemyMove in enemyMoves)
                {
                    // Get value of piece
                    int piece = board.GetPieceFromCoord(enemyMove.to);
                    
                    // Set bestPiece to piece if worth more
                    bestPiece = Mathf.Max(piece, bestPiece);
                }
            }

            // Minimize priority based on how valueable enemy move is
            move.priority -= bestPiece;
            // Reset squares to before
            board.squares = tempSquares;
        }
    }

    private List<Move> GetBestMoves(List<Move> moves){
        // Choose best move
        int highestPriorities = -100;
        // Create empty list of best moves
        List<Move> bestMoves = new List<Move>();
        // Loop through all moves
        int index = gameManager.isWhitesTurn ? (board.moveIndex + 1) / 2 : board.moveIndex / 2;
        foreach (Move move in moves){
            // If opening move exists for move index
            if (openingMoves.Length > index){
                // Invert move based on color
                int openingFromRank = gameManager.isWhitesTurn ? openingMoves[index].from.rank : 7 - openingMoves[index].from.rank;
                int openingFromFile = gameManager.isWhitesTurn ? openingMoves[index].from.file : 7 - openingMoves[index].from.file;
                int openingToRank = gameManager.isWhitesTurn ? openingMoves[index].to.rank : 7 - openingMoves[index].to.rank;
                int openingToFile = gameManager.isWhitesTurn ? openingMoves[index].to.file : 7 - openingMoves[index].to.file;
                
                // If move is opening move, do it
                if (openingFromRank == move.from.rank && openingFromFile == move.from.file && openingToRank == move.to.rank && openingToFile == move.to.file){
                    UnityEngine.Debug.Log("Found opening move");
                    move.priority = 100;
                    bestMoves.Clear();
                    bestMoves.Add(new Move(new Coord(openingFromRank, openingFromFile), new Coord(openingToRank, openingToFile)));
                    break;
                }
            }
            // If move priority is higher than highest priority
            // Clear best moves list and add move to it
            if (move.priority > highestPriorities){
                highestPriorities = move.priority;
                bestMoves.Clear();
                bestMoves.Add(move);
            } 
            // If move priority is equal to highest priority
            // Add move to best moves list
            else if (move.priority == highestPriorities){
                bestMoves.Add(move);
            }
        }

        return bestMoves;
    }

    private void SetupStockfish(){
        // Create new process
        process = new Process();

        // Setup process
        ProcessStartInfo si = new ProcessStartInfo()
        {
            FileName = System.IO.Directory.GetCurrentDirectory() + "\\Assets\\stockfish\\stockfish.exe",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true
        };

        // Assign process info
        process.StartInfo = si;
        process.OutputDataReceived += new DataReceivedEventHandler(ProcessOutputDataReceived);
        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        // Send start commands to stockfish
        SendLine("uci");
        SendLine("isready");
    }

    private void GetStockfishMove(string forsythEdwardsNotationString, int depth){
        // Send game info to stockfish
        SendLine("ucinewgame");
        SendLine("position fen "+ forsythEdwardsNotationString);
        SendLine("go depth " + depth);
    }

    private void SendLine(string command)
    {
        // Write command in stockfish
        process.StandardInput.WriteLine(command);
        // Submit command
        process.StandardInput.Flush();
    }

    public static void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e){
        // Get data from event
        string data = e.Data;

        // If data contains best move
        if (data.Contains("bestmove")){
            // Get only best move notation
            string notation = e.Data.Substring(9, 4);
            // Send notation and make into Move object
            AIPlayer.Instance.NotationToMove(notation);
        }
    }

    public void NotationToMove(string notation){
        // Translate for example e3e5 to Move coords
        int fromRank = notation[0] - 96;
        int fromFile = int.Parse(notation[1].ToString());

        int toRank = notation[2] - 96;
        int toFile = int.Parse(notation[3].ToString());

        // Set move to do from coords
        moveToDo = new Move(new Coord(fromRank - 1, fromFile - 1), new Coord(toRank - 1, toFile - 1));
        shoudlMakeStockfishMove = true;
    }

    private void MakeMove(){
        // Return if game has ended
        if (!gameManager.gameRunning) return;
        
        // Make move on board
        board.MovePiece(moveToDo);

        // Promote to queen if it can
        if (board.CanPromote(moveToDo.to)){
            board.Promote(moveToDo.to, Piece.Queen);
        }

        // Tell gamemanager a move is made
        gameManager.MoveMade();
    }
}

[System.Serializable]
public class OpeningMoveSequence
{
    public string openingName;
    public Move[] moves;
}