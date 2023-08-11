using ChessChallenge.API;
using System;
using System.Linq;

namespace ChessChallenge.Example
{
    // MyBot3-3
    public class EvilBot : IChessBot
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

        // 8 bits per score
        /* ulong[] compressed = { 0, 18091310594175943473, 17729576757067842301, 17656368983588406777, 17583463653133123315, 18018903212831931891, 17659471727739404271, 0, 0, 6724510076046890585, 3037988131755930159, 578714746879347728, 277063994901510, 18158511498534388482, 18230852792426431494, 0, 14625668616749503405, 17871161878745705436, 1595470717666860521, 795204270349158396, 18161338403160326650, 17873669855958662133, 17796265298118698482, 17651561836709082572, 14907181883262102755, 16569855717665930228, 17003054520181519860, 17799357172263092472, 17798797507976232183, 17651294711605886709, 16858369780686190059, 16133854247110633202, 18159334980899504626, 16791984635788068851, 18379780590394479096, 18375550773540487678, 145529233262118397, 362835568813868800, 4514607629076226, 17720813485764968176, 17579796734441879033, 17941771364025433596, 144118482315770881, 72344596705051647, 18158237733175558397, 17941497615879503610, 17508020636922738425, 17869711558145539061, 1517436131058128144, 1588962699736059917, 584929225523136766, 17725060976879008500, 17654951682867065582, 17293259623309505514, 15923889354942314474, 17504932134664534519, 145247710940300550, 73184589245711877, 18374122438830260995, 72057598333288706, 18085326896522068481, 17869150811544485886, 18373274689578466557, 17726724477624058108, 1591202357256781810, 1949526440663706868, 2024117404048750586, 18446471394691117811, 18374965751245959932, 146086603817419001, 18446470312511208431, 16641068873990010624, 721993266920884988, 4235443680840184, 290782441826943734, 1304940102892129025, 798000315483360759, 145531376283087608, 17288743890399065333, 17002759842054861296, 432608227130608352, 17432587516366946318, 17657210019487222780, 17147723797407331832, 16640776334774304744, 17508008438829218041, 289347389234086656, 507765481550516729, 17870853946314518235, 363405120148801530, 438562246038849541, 75734416974678780, 18087594154091216375, 18159366965993733879, 17869722600641526515, 16930149142397054694 };

        int GetPieceSquareValue(int index)
        {
            return (sbyte)((compressed[index / 8] >> (8 * (index % 8))) & 255) * 2;
        } */
        ulong[] psts = { 657614902731556116, 420894446315227099, 384592972471695068, 312245244820264086, 364876803783607569, 366006824779723922, 366006826859316500, 786039115310605588, 421220596516513823, 366011295806342421,
                         366006826859316436, 366006896669578452, 162218943720801556, 440575073001255824, 657087419459913430, 402634039558223453, 347425219986941203, 365698755348489557, 311382605788951956, 147850316371514514,
                         329107007234708689, 402598430990222677, 402611905376114006, 329415149680141460, 257053881053295759, 291134268204721362, 492947507967247313, 367159395376767958, 384021229732455700, 384307098409076181,
                         402035762391246293, 328847661003244824, 365712019230110867, 366002427738801364, 384307168185238804, 347996828560606484, 329692156834174227, 365439338182165780, 386018218798040211, 456959123538409047,
                         347157285952386452, 365711880701965780, 365997890021704981, 221896035722130452, 384289231362147538, 384307167128540502, 366006826859320596, 366006826876093716, 366002360093332756, 366006824694793492,
                         347992428333053139, 457508666683233428, 329723156783776785, 329401687190893908, 366002356855326100, 366288301819245844, 329978030930875600, 420621693221156179, 422042614449657239, 384602117564867863,
                         419505151144195476, 366274972473194070, 329406075454444949, 275354286769374224, 366855645423297932, 329991151972070674, 311105941360174354, 256772197720318995, 365993560693875923, 258219435335676691,
                         383730812414424149, 384601907111998612, 401758895947998613, 420612834953622999, 402607438610388375, 329978099633296596, 67159620133902 };
        public int GetPieceSquareValue(int psq)
        {
            return (int)(((psts[psq / 10] >> (6 * (psq % 10))) & 63) - 20) * 8;
        }

        int[] gamephase = { 0, 0, 1, 1, 2, 4, 0 };
        int[] pieceEval = { 0, 100, 310, 320, 500, 1000, 10_000 };
        int Evaluate(Board board)
        {
            int mg = 0, eg = 0, phase = 0;
            foreach (bool whitePiece in new[] { true, false })
            {
                for (int piece = 1; piece <= 6; piece++)
                {
                    ulong bitBoard = board.GetPieceBitboard((PieceType)piece, whitePiece);
                    while (bitBoard != 0)
                    {
                        phase += gamephase[piece];
                        int index = BitboardHelper.ClearAndGetIndexOfLSB(ref bitBoard);
                        // punish bad pawnstructure
                        if (piece == 1)
                        {
                            ulong rankBitBoard = (ulong)0x0101010101010101 << index % 8;
                            ulong neigbourRanks = (index % 8) switch
                            {
                                0 => rankBitBoard << 1,
                                7 => rankBitBoard >> 1,
                                _ => (rankBitBoard << 1) + (rankBitBoard >> 1)
                            };
                            // against without (depth 4, 50 games)
                            // without: 20 10 20
                            // dubbled: 26 6 18             Best
                            // passed: 21 8 21 
                            // isolated: 25 7 18
                            // dubbled + passed: 21 11 18
                            // dubbled + isolated: 20 8 22
                            // all: 19 5 26                 Worst

                            // dubbled pawns
                            if ((bitBoard & (rankBitBoard)) != 0)
                            {
                                mg -= 50;
                                eg -= 50;
                            }

                            // isolated pawns
                            //if ((board.GetPieceBitboard(PieceType.Pawn, whitePiece) & neigbourRanks) == 0) mg -= 25;

                            // passed pawns
                            /* ulong pawnPath = rankBitBoard + neigbourRanks;
                            if (whitePiece) pawnPath <<= 8 * (index / 8 + 1);
                            else pawnPath >>= 64 - 8 * (index / 8);

                            if ((board.GetPieceBitboard(PieceType.Pawn, !whitePiece) & pawnPath) == 0) eg += 25;
                            */
                        }
                        int psi = 128 * (piece - 1) + index ^ (whitePiece ? 56 : 0);
                        mg += GetPieceSquareValue(psi) + pieceEval[piece];
                        eg += GetPieceSquareValue(psi + 64) + pieceEval[piece];
                    }
                }
                mg = -mg;
                eg = -eg;
            }
            return (mg * phase + eg * (24 - phase)) / 24 * (board.IsWhiteToMove ? 1 : -1);
        }

        int Search(Board board, Timer timer, int alpha, int beta, int depth, int ply)
        {
            ulong zobristkey = board.ZobristKey;
            bool capturesOnly = depth <= 0;
            bool isRoot = ply == 0;
            int bestEval = -100_000;

            if (!isRoot && board.IsRepeatedPosition()) return 0;

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
            //searchTime = 10;
            bestmoveRoot = Move.NullMove;

            for (int depth = 1; depth <= 25; depth++)
            {
                int _ = Search(board, timer, -100_000, 100_000, depth, 0);

                if (timer.MillisecondsElapsedThisTurn > searchTime)
                {
                    Console.WriteLine($"evil depth {depth}"); // #DEBUG
                    break;
                }
            }
            return bestmoveRoot.IsNull ? board.GetLegalMoves()[0] : bestmoveRoot;
        }
    }
}