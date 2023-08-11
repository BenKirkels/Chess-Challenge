using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

// MyBot2-4
public class MyBot : IChessBot
{
    int positions;
    int searchTime = 500;  // ms
    readonly Dictionary<ulong, int> evaluationTable = new();
    public Move Think(Board board, Timer timer)
    {
        if (timer.OpponentMillisecondsRemaining < 10_000) searchTime = 100;
        Move[] moves = board.GetLegalMoves();
        Move MoveToPlay = Move.NullMove;
        Move prevBest = Move.NullMove;
        positions = 0;

        for (int depth = 1; depth <= 1; depth++)
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
                Console.WriteLine($"{move} {eval}");
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
            if (timer.MillisecondsElapsedThisTurn > searchTime)
            {
                return int.MaxValue;
            }
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
227513023075534, 238551089684194, 249546205956834, 249546290168034, 249546290168034, 249546205956834, 238551089684194, 227513023075534, 363341275257509090, 723650196365382882, 723390690146385920, 17008412440276238336, 17008412440276238336, 723390690146385920, 723650196365382882, 363341275257509090, 363352270373844706, 18089276393794890998, 17728993921163717652, 2831285391584286, 2831285391584286, 17728993921163717652, 18089276393794890998, 363352270373844706, 5879049951636706, 5629499534271222, 5646035158688286, 1446803413475383336, 1446803413475383336, 5646035158688286, 5629499534271222, 5879049951636706, 368981769908380386, 368737738523990262, 729036703830235166, 1809906133432127016, 1809906133432127016, 729036703830235166, 368737738523990262, 368981769908380386, 734899239631905506, 734649689214867702, 1455236646185588756, 2175818105597840926, 2175818105597840926, 1455236646185588756, 734649689214867702, 734899239631905506, 3625636251206869730, 3625657184945232108, 3625397700201076982, 3625397700201074176, 3625397700201074176, 3625397700201076982, 3625657184945232108, 3625636251206869730, 227513023128270, 238551089731800, 249546206009570, 249546206334700, 249546206334700, 249546206009570, 238551089731800, 227513023128270
};
    private enum ScoreType { KingLate, King, Queen, Rook, Bishop, Knight, PawnLate, Pawn };
    int GetPieceSquareValue(ScoreType type, int index)
    {
        ulong CompressedValue = Compressed[index];
        ulong Mask = 0xFF;
        return (int)(sbyte)((CompressedValue >> (int)type * 8) & Mask);
    }

    int Evaluate(Board board)
    {
        positions++;


        int whiteEval = 0;
        int blackEval = 0;

        int whiteMaterial = CountMaterial(board, true);
        int blackMaterial = CountMaterial(board, false);

        int whiteMaterialWithoutPawns = whiteMaterial - board.GetPieceList(PieceType.Pawn, true).Count * 100;
        int blackMaterialWithoutPawns = blackMaterial - board.GetPieceList(PieceType.Pawn, false).Count * 100;
        float whiteEndgamePhaseWeight = 1 - Math.Min(1, whiteMaterialWithoutPawns / 1620f); // 1620 = 2*500 + 320 + 300
        float blackEndgamePhaseWeight = 1 - Math.Min(1, blackMaterialWithoutPawns / 1620f);

        whiteEval += whiteMaterial;
        blackEval += blackMaterial;
        whiteEval += MopUpEval(board, true, whiteMaterial, blackMaterial, blackEndgamePhaseWeight);
        blackEval += MopUpEval(board, false, blackMaterial, whiteMaterial, whiteEndgamePhaseWeight);

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
                int square = piece.Square.Index;
                //int square = piecelist.IsWhitePieceList ? piece.Square.Index : 63 - piece.Square.Index;

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


    static int CountMaterial(Board board, bool whiteMaterial)
    {
        int material = 10_000;
        material += board.GetPieceList(PieceType.Pawn, whiteMaterial).Count * 100;
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

            int friendlyKingSquare = board.GetKingSquare(friendlyColorWhite).Index;
            int opponentKingSquare = board.GetKingSquare(!friendlyColorWhite).Index;
            mopUpScore += ManhattanCenterDistance(opponentKingSquare) * 10;
            // use ortho dst to promote direct opposition
            mopUpScore += (14 - ManhattanDistance(friendlyKingSquare, opponentKingSquare)) * 4;

            return (int)(mopUpScore * endgameWeight);
        }
        return 0;
    }
    static int ManhattanCenterDistance(int sq)
    {
        int file, rank;
        file = sq & 7;
        rank = sq >> 3;
        file ^= (file - 4) >> 8;
        rank ^= (rank - 4) >> 8;
        return (file + rank) & 7;
    }
    static int ManhattanDistance(int sq1, int sq2)
    {
        int file1, file2, rank1, rank2;
        int rankDistance, fileDistance;
        file1 = sq1 & 7;
        file2 = sq2 & 7;
        rank1 = sq1 >> 3;
        rank2 = sq2 >> 3;
        rankDistance = Math.Abs(rank2 - rank1);
        fileDistance = Math.Abs(file2 - file1);
        return rankDistance + fileDistance;
    }

}