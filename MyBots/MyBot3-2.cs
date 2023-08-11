using ChessChallenge.API;
using ChessChallenge.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

// MyBot3-1
public class MyBot : IChessBot
{
    Move bestmoveRoot;

    int searchTime = 2000;

    struct TTEntry
    {
        public ulong zobristHash;
        public Move move;
        public int depth, eval, flag; // 0 = exact, 1 = lower, 2 = upper

        public TTEntry(ulong _zobristHash, Move _move, int _depth, int _eval, int _flag)
        {
            zobristHash = _zobristHash;
            move = _move;
            depth = _depth;
            eval = _eval;
            flag = _flag;
        }
    }

    const int ttMask = 0x3FFFFF;
    TTEntry[] tt = new TTEntry[ttMask + 1];

    readonly ulong[] Compressed = new ulong[]
    {
14849753360064446464, 16293730985874817024, 16288101486341259264, 16285292255607128064, 16285292255607128064, 16288101486341259264, 16293730985874817024, 14849753360064446464, 16290917314144503050, 16290645752205281802, 2570, 388106, 388106, 2570, 16290645752205281802, 16290917314144503050, 16354530658881766666, 17792596228002151178, 1507585472988902922, 2228161413368446986, 2228161413368446986, 1507585472988902922, 17792596228002151178, 16354530658881766666, 16351721406672797716, 17789781478066880532, 2225346663601340436, 2943107854213846036, 2943107854213846036, 2225346663601340436, 17789781478066880532, 16351721406672797716, 16348906656905692446, 17786966728383989022, 2222531913750350366, 2940293104446740766, 2940293104446740766, 2222531913750350366, 17786966728383989022, 16348906656905692446, 16348901159347554866, 17786966728299776562, 1501955973370745906, 2219717164067135026, 2219717164067135026, 1501955973370745906, 17786966728299776562, 16348901159347554866, 16348900102784954960, 17066390830885646928, 17786966771249459792, 57983888152080976, 57983888152080976, 17786966771249459792, 17066390830885646928, 16348900102784954960, 14907737205266841600, 15625509391163719680, 16346085331543654400, 17063852019713966080, 17063852019713966080, 16346085331543654400, 15625509391163719680, 14907737205266841600
};
    /*
    type: 0 = pawnLate, 1 = pawnEarly, 2 = knight, 3 = bishop, 4 = rook, 5 = queen, 6 = kingEarly, 7 = kingLate
    */
    int GetPieceSquareValue(int type, int index)
    {
        return (int)(sbyte)((Compressed[index] >> type * 8) & 0xFF);
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
                    //enum ScoreType { PawnLate, Pawn, Knight, Bishop, Rook, Queen, King, KingLate };
                    PieceType.Pawn => sign * (int)Math.Round(GetPieceSquareValue(1, square) * (1 - endgameWeight) + GetPieceSquareValue(0, square) * endgameWeight),
                    PieceType.King => sign * (int)Math.Round(GetPieceSquareValue(6, square) * (1 - endgameWeight) + GetPieceSquareValue(7, square) * endgameWeight),
                    _ => sign * GetPieceSquareValue((int)piece.PieceType, square),
                };
            }
        }


        return board.IsWhiteToMove ? eval : -eval;
    }

    static int CountMaterial(Board board, bool whiteMaterial, bool includePawns = true)
    {
        /* int material = 0;
        if (includePawns) material += board.GetPieceList(PieceType.Pawn, whiteMaterial).Count * 100;
        material += board.GetPieceList(PieceType.Knight, whiteMaterial).Count * 300;
        material += board.GetPieceList(PieceType.Bishop, whiteMaterial).Count * 320;
        material += board.GetPieceList(PieceType.Rook, whiteMaterial).Count * 500;
        material += board.GetPieceList(PieceType.Queen, whiteMaterial).Count * 900;
        return material; */

        int[] materialValues = { 100, 300, 320, 500, 900 };

        int material = 0;
        PieceList[] pieceLists = board.GetAllPieceLists();
        for (int i = 0; i <= 4; i++)
        {
            if (i == 0 && !includePawns) continue;
            material += pieceLists[whiteMaterial ? i : i + 6].Count * materialValues[i];
        }
        return material;
    }
    static int MopUpEval(Board board, bool friendlyColorWhite, int myMaterial, int opponentMaterial, float endgameWeight)
    {
        int mopUpScore = 0;
        if (myMaterial > opponentMaterial + 100 * 2 && endgameWeight > 0)
        {
            Square opponentKingSquare = board.GetKingSquare(!friendlyColorWhite);
            mopUpScore += ManhattanCenterDistance(opponentKingSquare) * 10;
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

    int Search(Board board, Timer timer, int alpha, int beta, int depth, int ply)
    {
        ulong zobristkey = board.ZobristKey;
        bool capturesOnly = depth <= 0;
        bool isRoot = ply == 0;
        int bestEval = -100_000;

        if (!isRoot && board.IsRepeatedPosition()) return 0; // TODO: test improvement over IsDraw()

        TTEntry ttEntry = tt[zobristkey & ttMask];

        if (!isRoot && ttEntry.zobristHash == zobristkey && ttEntry.depth >= depth && (
            ttEntry.flag == 0 || // Exact
            ttEntry.flag == 1 && ttEntry.eval >= beta || // Lowerbound
            ttEntry.flag == 2 && ttEntry.eval <= alpha // Upperbound
            )) return ttEntry.eval;

        if (capturesOnly)
        {
            bestEval = Evaluate(board);
            if (bestEval >= beta) return bestEval;
            alpha = Math.Max(alpha, bestEval);
        }
        Move[] moves = board.GetLegalMoves(capturesOnly);

        Move bestMove = Move.NullMove;
        int origAlpha = alpha;

        foreach (Move move in moves.OrderByDescending(move => move.Equals(ttEntry.move) ? 100_000 : 100 * (int)move.CapturePieceType - (int)move.MovePieceType))
        {
            if (timer.MillisecondsElapsedThisTurn > searchTime) return 100_000;

            board.MakeMove(move);
            int eval = -Search(board, timer, -beta, -alpha, depth - 1, ply + 1);
            board.UndoMove(move);
            // New best move
            if (eval > bestEval)
            {
                bestEval = eval;
                bestMove = move;
                if (ply == 0) bestmoveRoot = move;

                // Improve alpha
                alpha = Math.Max(alpha, eval);

                // Fail-high
                if (alpha >= beta) break;

            }
        }

        if (!capturesOnly && moves.Length == 0) return board.IsInCheck() ? -100_000 + ply : 0;

        int flag = bestEval >= beta ? 1 : bestEval > origAlpha ? 0 : 2;

        tt[zobristkey & ttMask] = new TTEntry(zobristkey, bestMove, depth, bestEval, flag);

        return bestEval;

    }
    public Move Think(Board board, Timer timer)
    {
        searchTime = timer.MillisecondsRemaining / 30;
        bestmoveRoot = Move.NullMove;

        for (int depth = 1; depth <= 100; depth++)
        {
            int _ = Search(board, timer, -100_000, 100_000, depth, 0);

            if (timer.MillisecondsElapsedThisTurn > searchTime)
            {
                Console.WriteLine($"depth {depth}"); // #DEBUG
                break;
            }
        }
        return bestmoveRoot.IsNull ? board.GetLegalMoves()[0] : bestmoveRoot;
    }
}