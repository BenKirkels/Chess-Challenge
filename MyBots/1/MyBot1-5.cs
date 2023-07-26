using ChessChallenge.API;
using ChessChallenge.Application;
using System;
using System.Collections.Generic;

// Uses minimax algorithm with alpha-beta pruning to find the best move
// Uses a simple evaluation function using piece-square tables (Simplified Evaluation Function)
// https://www.chessprogramming.org/Simplified_Evaluation_Function
// Basic implementation of iterative deepening
// Switches king evaluation function after 40 moves
// increased values for knights and bishops to 320 and 330
// Reaches depth 4-5
// Problem: Doesn't play well in the endgame, doesn't use pawns
// Problem: Doesn't castle when it should
public class MyBot : IChessBot
{
    readonly int maxSearchDepth = 10;
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        bool IAmWhite = board.IsWhiteToMove;
        Move MoveToPlay = moves[0];

        for (int searchDepth = 1; searchDepth <= maxSearchDepth; searchDepth++)
        {
            if (0.005 * timer.MillisecondsRemaining < timer.MillisecondsElapsedThisTurn)
            {
                Console.WriteLine($"Depth reached: {searchDepth - 1}");
                break;
            }
            int BestEvalIter = IAmWhite ? int.MinValue : int.MaxValue;
            Move MoveToPlayIter = moves[0];
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int eval = Minimax(board, searchDepth - 1, int.MinValue, int.MaxValue, !IAmWhite);
                board.UndoMove(move);
                if ((IAmWhite && eval > BestEvalIter) || (!IAmWhite && eval < BestEvalIter))
                {
                    BestEvalIter = eval;
                    MoveToPlayIter = move;
                }
            }
            MoveToPlay = MoveToPlayIter;
        }
        return MoveToPlay;
    }

    int Minimax(Board board, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        if (board.IsInCheckmate()) return maximizingPlayer ? -100000 * depth : 100000 * depth;
        if (board.IsDraw()) return 0;
        if (depth == 0) return Evaluate(board);

        int eval, bestEval;
        bestEval = maximizingPlayer ? int.MinValue : int.MaxValue;

        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            eval = Minimax(board, depth - 1, alpha, beta, !maximizingPlayer);
            board.UndoMove(move);

            if (maximizingPlayer)
            {
                bestEval = Math.Max(bestEval, eval);
                alpha = Math.Max(alpha, eval);
            }
            else
            {
                bestEval = Math.Min(bestEval, eval);
                beta = Math.Min(beta, eval);
            }
            if (beta <= alpha) break;
        }
        return bestEval;
    }

    readonly int[] pawns = new int[]
    {
    100, 100, 100, 100, 100, 100, 100, 100,
    150, 150, 150, 150, 150, 150, 150, 150,
    110, 110, 120, 130, 130, 120, 110, 110,
    105, 105, 110, 125, 125, 110, 105, 105,
    100, 100, 100, 120, 120, 100, 100, 100,
    105,  95,  90, 100, 100,  90,  95, 105,
    105, 110, 110,  80,  80, 110, 110, 105,
    100, 100, 100, 100, 100, 100, 100, 100
    };
    readonly int[] knights = new int[]
    {
    250, 260, 270, 270, 270, 270, 260, 250,
    260, 280, 300, 300, 300, 300, 280, 260,
    270, 300, 310, 315, 315, 310, 300, 270,
    270, 305, 315, 320, 320, 315, 305, 270,
    270, 300, 315, 320, 320, 315, 300, 270,
    270, 305, 310, 315, 315, 310, 305, 270,
    260, 280, 300, 305, 305, 300, 280, 260,
    250, 260, 270, 270, 270, 270, 260, 250
    };
    readonly int[] bishops = new int[]
    {
    280, 290, 290, 290, 290, 290, 290, 280,
    290, 300, 300, 300, 300, 300, 300, 290,
    290, 300, 305, 310, 310, 305, 300, 290,
    290, 305, 305, 310, 310, 305, 305, 290,
    290, 300, 310, 310, 310, 310, 300, 290,
    290, 310, 310, 310, 310, 310, 310, 290,
    290, 305, 300, 300, 300, 300, 305, 290,
    280, 290, 290, 290, 290, 290, 290, 280
    };

    readonly int[] rooks = new int[]
    {
     500, 500, 500, 500, 500, 500, 500, 500,
     505, 510, 510, 510, 510, 510, 510, 505,
     495, 500, 500, 500, 500, 500, 500, 495,
     495, 500, 500, 500, 500, 500, 500, 495,
     495, 500, 500, 500, 500, 500, 500, 495,
     495, 500, 500, 500, 500, 500, 500, 495,
     495, 500, 500, 500, 500, 500, 500, 495,
     500, 500, 500, 505, 505, 500, 500, 500
    };
    readonly int[] queens = new int[]
    {
    880, 890, 890, 895, 895, 890, 890, 880,
    890, 900, 900, 900, 900, 900, 900, 890,
    890, 900, 905, 905, 905, 905, 900, 890,
    895, 900, 905, 905, 905, 905, 900, 895,
    900, 900, 905, 905, 905, 905, 900, 895,
    890, 905, 905, 905, 905, 905, 900, 890,
    890, 900, 905, 900, 900, 900, 900, 890,
    880, 890, 890, 895, 895, 890, 890, 880
    };
    readonly int[] kingsEarly = new int[]
    {
    9970, 9960, 9960, 9950, 9950, 9960, 9960, 9970,
    9970, 9960, 9960, 9950, 9950, 9960, 9960, 9970,
    9970, 9960, 9960, 9950, 9950, 9960, 9960, 9970,
    9970, 9960, 9960, 9950, 9950, 9960, 9960, 9970,
    9980, 9970, 9970, 9960, 9960, 9970, 9970, 9980,
    9990, 9980, 9980, 9980, 9980, 9980, 9980, 9990,
    10020, 10020, 10000, 10000, 10000, 10000, 10020, 10020,
    10020, 10030, 10010, 10000, 10000, 10010, 10030, 10020
    };
    readonly int[] kingsLate = new int[]
{
    9950, 9960, 9970, 9980, 9980, 9970, 9960, 9950,
    9970, 9980, 9990, 10000, 10000, 9990, 9980, 9970,
    9970, 9990, 10020, 10030, 10030, 10020, 9990, 9970,
    9970, 9990, 10030, 10040, 10040, 10030, 9990, 9970,
    9970, 9990, 10030, 10040, 10040, 100300, 9990, 9970,
    9970, 9990, 10020, 10030, 10030, 10020, 9990, 9970,
    9970, 9970, 10000, 10000, 10000, 10000, 9970, 9970,
    9950, 9970, 9970, 9970, 9970, 9970, 9970, 9950
};


    int Evaluate(Board board)
    {
        int score = 0;
        foreach (PieceList pieceList in board.GetAllPieceLists())
        {
            int sign = pieceList.IsWhitePieceList ? 1 : -1;
            foreach (Piece piece in pieceList)
            {
                int index = piece.IsWhite ? piece.Square.Index : 63 - piece.Square.Index;
                switch (piece.PieceType)
                {
                    case PieceType.Pawn:
                        score += sign * pawns[index];
                        break;
                    case PieceType.Knight:
                        score += sign * knights[index];
                        break;
                    case PieceType.Bishop:
                        score += sign * bishops[index];
                        break;
                    case PieceType.Rook:
                        score += sign * rooks[index];
                        break;
                    case PieceType.Queen:
                        score += sign * queens[index];
                        break;
                    case PieceType.King:
                        if (board.PlyCount < 40)
                        {
                            score += sign * kingsEarly[index];
                            break;
                        }
                        score += sign * kingsLate[index];
                        break;
                }
            }
        }
        return score;
    }
}