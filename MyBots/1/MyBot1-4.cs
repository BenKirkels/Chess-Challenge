using ChessChallenge.API;
using System;
using System.Collections.Generic;

// Uses minimax algorithm with alpha-beta pruning to find the best move
// Uses a simple evaluation function using piece-square tables (Simplified Evaluation Function)
// https://www.chessprogramming.org/Simplified_Evaluation_Function
// Basic implementation of iterative deepening
// Plays better than chess.com level 1200 but cant beat it
// Problem: shuffles king and rook in the corner
// Problem: doesnt castle
public class MyBot : IChessBot
{
    readonly int maxSearchDepth = 10;
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        bool IAmWhite = board.IsWhiteToMove;
        Move MoveToPlay = Move.NullMove;

        for (int searchDepth = 1; searchDepth <= maxSearchDepth; searchDepth++)
        {
            if (0.005 * timer.MillisecondsRemaining < timer.MillisecondsElapsedThisTurn)
            {
                break;
            }
            int BestEvalIter = IAmWhite ? -100000 : 100000;
            Move MoveToPlayIter = Move.NullMove;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int eval = Minimax(board, searchDepth - 1, -100000, 100000, !IAmWhite);
                board.UndoMove(move);
                if (IAmWhite)
                {
                    if (eval > BestEvalIter)
                    {
                        BestEvalIter = eval;
                        MoveToPlayIter = move;
                    }
                }
                else
                {
                    if (eval < BestEvalIter)
                    {
                        BestEvalIter = eval;
                        MoveToPlayIter = move;
                    }
                }
            }
            MoveToPlay = MoveToPlayIter;
        }
        return MoveToPlay;
    }

    int Minimax(Board board, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        if (board.IsInCheckmate())
        {
            return maximizingPlayer ? -100000 : 100000;
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
            int maxEval = -100000;
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
            int minEval = 100000;
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
                        score += sign * kingsEarly[index];
                        break;
                }
            }
        }
        return score;
    }
}