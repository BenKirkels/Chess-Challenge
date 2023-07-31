using ChessChallenge.API;
using ChessChallenge.Application;
using System;

namespace ChessChallenge.Example
{
    public class MyBot : IChessBot
    {
        public Move Think(Board board, Timer timer)
        {
            Console.WriteLine(CountMaterial1(board, true) + " ~ " + CountMaterial2(board, true));
            return board.GetLegalMoves()[0];  // 36 tokens without anything
        }

        static int CountMaterial1(Board board, bool whiteMaterial, bool includePawns = true)
        {
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
        static int CountMaterial2(Board board, bool whiteMaterial, bool includePawns = true)
        {
            int material = 0;
            if (includePawns) material += board.GetPieceList(PieceType.Pawn, whiteMaterial).Count * 100;
            material += board.GetPieceList(PieceType.Knight, whiteMaterial).Count * 300;
            material += board.GetPieceList(PieceType.Bishop, whiteMaterial).Count * 320;
            material += board.GetPieceList(PieceType.Rook, whiteMaterial).Count * 500;
            material += board.GetPieceList(PieceType.Queen, whiteMaterial).Count * 900;
            return material;
        }
    }
}