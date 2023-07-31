using ChessChallenge.API;
using ChessChallenge.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;


public class MyBot : IChessBot
{
    int positions;
    int searchTime = 1000;  // ms
    readonly Dictionary<ulong, int> evaluationTable = new();
    public Move Think(Board board, Timer timer)
    {
        if (timer.MillisecondsRemaining < 10_000) searchTime = 100;
        Move[] moves = board.GetLegalMoves();
        Move MoveToPlay = Move.NullMove;
        Move prevBest = Move.NullMove;
        positions = 0;

        for (int depth = 1; depth <= int.MaxValue; depth++)
        {
            if (timer.MillisecondsElapsedThisTurn > searchTime)
            {
                Console.WriteLine($"MyBot: Depth {depth - 1} reached with {positions} positions in {timer.MillisecondsElapsedThisTurn}ms");
                break;
            };
            int BestEvalIter = -int.MaxValue;
            foreach (Move move in Order(board, moves, prevBest))
            {
                board.MakeMove(move);
                int eval = -Minimax(board, depth - 1, -int.MaxValue, -BestEvalIter, false, prevBest, timer);
                board.UndoMove(move);
                if (eval > BestEvalIter)
                {
                    BestEvalIter = eval;
                    MoveToPlay = move;
                }
            }
            prevBest = MoveToPlay;
        }
        return MoveToPlay;
    }

    int Minimax(Board board, int depth, int alpha, int beta, bool capturesOnly, Move prevBest, Timer timer)
    {
        if (timer.MillisecondsElapsedThisTurn > searchTime) return int.MaxValue;

        if (board.IsInCheckmate()) return -100000 - depth;
        if (board.IsDraw()) return 0;
        if (depth == 0) return Minimax(board, int.MaxValue, alpha, beta, true, prevBest, timer);
        if (capturesOnly)
        {
            //int eval = evaluationTable.ContainsKey(board.ZobristKey) ? evaluationTable[board.ZobristKey] : Evaluate(board);
            int eval = Evaluate(board);
            if (eval >= beta) return beta;
            if (eval > alpha) alpha = eval;
        }

        foreach (Move move in Order(board, board.GetLegalMoves(capturesOnly), prevBest))
        {
            if (timer.MillisecondsElapsedThisTurn > searchTime) return int.MaxValue;

            board.MakeMove(move);
            int eval = -Minimax(board, depth - 1, -beta, -alpha, capturesOnly, prevBest, timer);
            if (eval == -int.MaxValue)
            {
                board.UndoMove(move);
                return int.MaxValue;
            }
            evaluationTable[board.ZobristKey] = eval;
            board.UndoMove(move);

            if (eval >= beta) return beta;
            if (eval > alpha) alpha = eval;
        }
        return alpha;
    }
    Move[] Order(Board board, Move[] moves, Move prevBest)
    {
        return moves.OrderByDescending(move => Help(move, prevBest)).ToArray();
        /* Dictionary<Move, int> moveScores = new();
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            moveScores[move] = evaluationTable.GetValueOrDefault(board.ZobristKey, Convert.ToInt32(move.IsPromotion) + move.CapturePieceType - move.MovePieceType);
            board.UndoMove(move);
        }
        if (!prevBest.Equals(Move.NullMove)) moveScores[prevBest] = int.MaxValue;
        return moves.OrderByDescending(move => moveScores[move]).ToArray(); */
    }
    int Help(Move move, Move prevBest)
    {
        if (move.Equals(prevBest)) return int.MaxValue;
        return Convert.ToInt32(move.IsPromotion) + move.CapturePieceType - move.MovePieceType;
    }
    ulong[] Compressed = new ulong[]
    {
14849753360064446464, 16293730985874817024, 16288101486341259264, 16285292255607128064, 16285292255607128064, 16288101486341259264, 16293730985874817024, 14849753360064446464, 16290917314144503050, 16290645752205281802, 2570, 388106, 388106, 2570, 16290645752205281802, 16290917314144503050, 16354530658881766666, 17792596228002151178, 1507585472988902922, 2228161413368446986, 2228161413368446986, 1507585472988902922, 17792596228002151178, 16354530658881766666, 16351721406672797716, 17789781478066880532, 2225346663601340436, 2943107854213846036, 2943107854213846036, 2225346663601340436, 17789781478066880532, 16351721406672797716, 16348906656905692446, 17786966728383989022, 2222531913750350366, 2940293104446740766, 2940293104446740766, 2222531913750350366, 17786966728383989022, 16348906656905692446, 16348901159347554866, 17786966728299776562, 1501955973370745906, 2219717164067135026, 2219717164067135026, 1501955973370745906, 17786966728299776562, 16348901159347554866, 16348900102784954960, 17066390830885646928, 17786966771249459792, 57983888152080976, 57983888152080976, 17786966771249459792, 17066390830885646928, 16348900102784954960, 14907737205266841600, 15625509391163719680, 16346085331543654400, 17063852019713966080, 17063852019713966080, 16346085331543654400, 15625509391163719680, 14907737205266841600};
    private enum ScoreType { PawnLate, Pawn, Knight, Bishop, Rook, Queen, King, KingLate };
    int GetPieceSquareValue(ScoreType type, int index)
    {
        ulong CompressedValue = Compressed[index];
        ulong Mask = 0xFF;
        return (int)(sbyte)((CompressedValue >> (int)type * 8) & Mask);
    }

    int Evaluate(Board board)
    {
        float whiteEndgamePhaseWeight = 1 - Math.Min(1, CountMaterial(board, true, false) / 1620f); // 1620 = 2*500 + 320 + 300
        float blackEndgamePhaseWeight = 1 - Math.Min(1, CountMaterial(board, false, false) / 1620f);

        int whiteEval = CountMaterial(board, true);
        int blackEval = CountMaterial(board, false);
        whiteEval += MopUpEval(board, true, whiteEval, blackEval, blackEndgamePhaseWeight);
        blackEval += MopUpEval(board, false, blackEval, whiteEval, whiteEndgamePhaseWeight);

        int eval = whiteEval - blackEval;

        foreach (PieceList piecelist in board.GetAllPieceLists())
        {
            int sign = -1;
            float endgameWeight = blackEndgamePhaseWeight;
            if (piecelist.IsWhitePieceList)
            {
                sign = 1;
                endgameWeight = whiteEndgamePhaseWeight;
            }
            foreach (Piece piece in piecelist)
            {
                //int square = piece.Square.Index;
                int square = piecelist.IsWhitePieceList ? piece.Square.Index : 63 - piece.Square.Index;

                eval += piece.PieceType switch
                {
                    PieceType.Pawn => sign * (int)Math.Round(GetPieceSquareValue(ScoreType.Pawn, square) * (1 - endgameWeight) + GetPieceSquareValue(ScoreType.PawnLate, square) * endgameWeight),
                    PieceType.King => sign * (int)Math.Round(GetPieceSquareValue(ScoreType.King, square) * (1 - endgameWeight) + GetPieceSquareValue(ScoreType.KingLate, square) * endgameWeight),
                    _ => sign * GetPieceSquareValue((ScoreType)piece.PieceType, square),
                };
            }
        }


        return board.IsWhiteToMove ? eval : -eval;
    }

    static int CountMaterial(Board board, bool whiteMaterial, bool includePawns = true)
    {
        int material = 0;
        if (includePawns) material += board.GetPieceList(PieceType.Pawn, whiteMaterial).Count * 100;
        material += board.GetPieceList(PieceType.Knight, whiteMaterial).Count * 300;
        material += board.GetPieceList(PieceType.Bishop, whiteMaterial).Count * 320;
        material += board.GetPieceList(PieceType.Rook, whiteMaterial).Count * 500;
        material += board.GetPieceList(PieceType.Queen, whiteMaterial).Count * 900;
        return material;
    }
    static int MopUpEval(Board board, bool friendlyColorWhite, int myMaterial, int opponentMaterial, float endgameWeight)
    {
        int mopUpScore = 0;
        if (myMaterial > opponentMaterial + 100 * 2 && endgameWeight > 0)
        {
            Square opponentKingSquare = board.GetKingSquare(!friendlyColorWhite);
            mopUpScore += ManhattanCenterDistance(opponentKingSquare) * 10;
            // use ortho dst to promote direct opposition
            mopUpScore += (14 - ManhattanDistance(board.GetKingSquare(friendlyColorWhite), opponentKingSquare)) * 4;

            return (int)(mopUpScore * endgameWeight);
        }
        return 0;
    }
    static int ManhattanCenterDistance(Square sq)
    {
        int file = sq.File;
        int rank = sq.Rank;
        file ^= (file - 4) >> 8;
        rank ^= (rank - 4) >> 8;
        return (file + rank) & 7;
    }

    static int ManhattanDistance(Square sq1, Square sq2)
    {
        return Math.Abs(sq2.Rank - sq1.Rank) + Math.Abs(sq2.File - sq1.File);
    }

}