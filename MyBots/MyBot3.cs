using ChessChallenge.API;
using System;

// Checks one move and response ahead
// Uses capture and recapture evaluation
// Checks for checkmate and draw
// Previous version improvement: plays random move to prevent 3 move repetition
// Problem: random moves when no capture is available

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    int searchDepth = 6;
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        bool IAmWhite = board.IsWhiteToMove;
        Move MoveToPlay = moves[0];
        int BestEval = IAmWhite ? int.MinValue : int.MaxValue;

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int eval = Minimax(board, searchDepth, int.MinValue, int.MaxValue, !IAmWhite);
            board.UndoMove(move);
            if (IAmWhite)
            {
                if (eval > BestEval)
                {
                    BestEval = eval;
                    MoveToPlay = move;
                }
            }
            else
            {
                if (eval < BestEval)
                {
                    BestEval = eval;
                    MoveToPlay = move;
                }
            }
        }
        return MoveToPlay;
    }

    int Minimax(Board board, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        if (board.IsInCheckmate())
        {
            return maximizingPlayer ? int.MinValue : int.MaxValue;
        }
        if (board.IsDraw())
        {
            return 0;
        }
        if (depth == 0)
        {
            return Evaluate(board);
        }

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;
            foreach (Move move in board.GetLegalMoves())
            {
                board.MakeMove(move);
                int eval = Minimax(board, depth - 1, alpha, beta, false);
                board.UndoMove(move);
                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha)
                {
                    break;
                }
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (Move move in board.GetLegalMoves())
            {
                board.MakeMove(move);
                int eval = Minimax(board, depth - 1, alpha, beta, true);
                board.UndoMove(move);
                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);
                if (beta <= alpha)
                {
                    break;
                }
            }
            return minEval;
        }
    }

    int Evaluate(Board board)
    {
        int score = 0;
        foreach (PieceList piece in board.GetAllPieceLists())
        {
            score += piece.IsWhitePieceList ? pieceValues[(int)piece.TypeOfPieceInList] : -pieceValues[(int)piece.TypeOfPieceInList];
        }
        return score;
    }
}