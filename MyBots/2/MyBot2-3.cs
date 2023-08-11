using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    readonly int searchDepth = 4;
    //int positionCount = 0;
    Dictionary<ulong, int> evaluationTable = new();
    public Move Think(Board board, Timer timer)
    {
        //Console.WriteLine("New turn");
        Move[] moves = board.GetLegalMoves();
        Move MoveToPlay = Move.NullMove;

        for (int depth = 1; depth <= searchDepth; depth++)
        {
            if (200 < timer.MillisecondsElapsedThisTurn)
            {
                //Console.WriteLine($"MyBot depth reached: {depth - 1}");
                break;
            }
            int BestEvalIter = -int.MaxValue;
            Move MoveToPlayIter = Move.NullMove;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int eval = -Minimax(board, depth - 1, -int.MaxValue, int.MaxValue, false);
                //Console.WriteLine($"First {move} {eval}");
                board.UndoMove(move);
                if (eval > BestEvalIter)
                {
                    BestEvalIter = eval;
                    MoveToPlayIter = move;
                }
            }
            MoveToPlay = MoveToPlayIter;
        }
        //Console.WriteLine($"MyBot position count: {positionCount}");
        return MoveToPlay;
    }

    int Minimax(Board board, int depth, int alpha, int beta, bool capturesOnly)
    {
        if (board.IsInCheckmate()) return -100000 * depth;
        if (board.IsDraw()) return 0;
        if (depth == 0) return Minimax(board, int.MaxValue, alpha, beta, true);
        if (capturesOnly)
        {
            int eval = Evaluate(board);
            if (eval >= beta) return beta;
            if (eval > alpha) alpha = eval;
        }

        foreach (Move move in Order(board, board.GetLegalMoves(capturesOnly)))
        {
            board.MakeMove(move);
            int eval = -Minimax(board, depth - 1, -beta, -alpha, capturesOnly);
            evaluationTable[board.ZobristKey] = eval;
            board.UndoMove(move);

            if (eval >= beta) return beta;
            if (eval > alpha) alpha = eval;
        }
        return alpha;
    }
    Move[] Order(Board board, Move[] moves)
    {
        int currentEval = evaluationTable.ContainsKey(board.ZobristKey) ? evaluationTable[board.ZobristKey] : 0;
        Dictionary<Move, int> moveEvaluations = new();
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            moveEvaluations[move] = evaluationTable.ContainsKey(board.ZobristKey) ? evaluationTable[board.ZobristKey] : (currentEval + move.CapturePieceType - move.MovePieceType);
            board.UndoMove(move);
        }
        return moves.OrderByDescending(move => moveEvaluations[move]).ToArray();
    }

    readonly int[] pawns = new int[]
    {
    100, 100, 100, 100, 100, 100, 100, 100,
    105, 110, 110,  80,  80, 110, 110, 105,
    105,  95,  90, 100, 100,  90,  95, 105,
    100, 100, 100, 120, 120, 100, 100, 100,
    105, 105, 110, 125, 125, 110, 105, 105,
    110, 110, 120, 130, 130, 120, 110, 110,
    150, 150, 150, 150, 150, 150, 150, 150,
    100, 100, 100, 100, 100, 100, 100, 100
    };

    readonly int[] knights = new int[]
    {
    250, 260, 270, 270, 270, 270, 260, 250,
    260, 280, 300, 305, 305, 300, 280, 260,
    270, 305, 310, 315, 315, 310, 305, 270,
    270, 300, 315, 320, 320, 315, 300, 270,
    270, 305, 315, 320, 320, 315, 305, 270,
    270, 300, 310, 315, 315, 310, 300, 270,
    260, 280, 300, 300, 300, 300, 280, 260,
    250, 260, 270, 270, 270, 270, 260, 250
    };
    readonly int[] bishops = new int[]
    {
    280, 290, 290, 290, 290, 290, 290, 280,
    290, 305, 300, 300, 300, 300, 305, 290,
    290, 310, 310, 310, 310, 310, 310, 290,
    290, 300, 310, 310, 310, 310, 300, 290,
    290, 305, 305, 310, 310, 305, 305, 290,
    290, 300, 305, 310, 310, 305, 300, 290,
    290, 300, 300, 300, 300, 300, 300, 290,
    280, 290, 290, 290, 290, 290, 290, 280
    };

    int Evaluate(Board board)
    {
        if (evaluationTable.ContainsKey(board.ZobristKey))
        {
            return evaluationTable[board.ZobristKey];
        }
        //positionCount++;
        int score = 0;
        foreach (PieceList pieceList in board.GetAllPieceLists())
        {
            foreach (Piece piece in pieceList)
            {
                bool isWhite = piece.IsWhite;
                int sign = isWhite ? 1 : -1;
                int index = isWhite ? piece.Square.Index : 63 - piece.Square.Index;
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
                        score += sign * 500;
                        int[] bestRanks = { 1, 6 };
                        if (bestRanks.Contains(piece.Square.Rank))
                        {
                            score += sign * 10;
                        }
                        break;
                    case PieceType.Queen:
                        score += sign * 900;
                        int[] border = { 0, 7 };
                        if (border.Contains(piece.Square.Rank) || border.Contains(piece.Square.File))
                        {
                            score -= sign * 15;
                        }
                        break;
                    case PieceType.King:
                        score += sign * 10000;

                        if (board.GetPieceList(PieceType.Queen, !isWhite).Count != 0)
                        {
                            // Early game
                            // Keep king at the back
                            int distance = isWhite ? piece.Square.Rank : 7 - piece.Square.Rank;
                            score -= sign * 10 * distance;
                        }
                        else
                        {
                            // Late game
                            // Move king to the center
                            int[] edge = { 0, 1, 6, 7 };
                            if (edge.Contains(piece.Square.Rank) || edge.Contains(piece.Square.File))
                            {
                                score -= sign * 50;
                            }
                        }
                        break;
                }
            }
        }
        return board.IsWhiteToMove ? score : -score;
    }
}