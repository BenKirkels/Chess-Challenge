using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

// Compressor
public class MyBot : IChessBot
{
    readonly sbyte[] pawnsEarly = new sbyte[]
    {
     0, 0,  0,  0,  0,  0, 0, 0,
     5,10, 10,-20,-20, 10,10, 5,
     5,-5,-10,  0,  0,-10,-5, 5,
     0, 0,  0, 20, 20,  0, 0, 0,
     5, 5, 10, 25, 25, 10, 5, 5,
    10,10, 20, 30, 30, 20,10,10,
    50,50, 50, 50, 50, 50,50,50,
     0, 0,  0,  0,  0,  0, 0, 0
    };
    readonly sbyte[] pawnLate = new sbyte[]
    {
     0, 0, 0, 0, 0, 0, 0, 0,
    10,10,10,10,10,10,10,10,
    10,10,10,10,10,10,10,10,
    20,20,20,20,20,20,20,20,
    30,30,30,30,30,30,30,30,
    50,50,50,50,50,50,50,50,
    80,80,80,80,80,80,80,80,
     0, 0, 0, 0, 0, 0, 0, 0
    };
    readonly sbyte[] knights = new sbyte[]
    {
    -50,-40,-30,-30,-30,-30,-40,-50,
    -40,-20,  0,  5,  5,  0,-20,-40,
    -30,  5, 10, 15, 15, 10,  5,-30,
    -30,  0, 15, 20, 20, 15,  0,-30,
    -30,  5, 15, 20, 20, 15,  5,-30,
    -30,  0, 10, 15, 15, 10,  0,-30,
    -40,-20,  0,  0,  0,  0,-20,-40,
    -50,-40,-30,-30,-30,-30,-40,-50
    };
    readonly sbyte[] bishops = new sbyte[]
    {
    -20,-10,-10,-10,-10,-10,-10,-20,
    -10,  5,  0,  0,  0,  0,  5,-10,
    -10, 10, 10, 10, 10, 10, 10,-10,
    -10,  0, 10, 10, 10, 10,  0,-10,
    -10,  5,  5, 10, 10,  5,  5,-10,
    -10,  0,  5, 10, 10,  5,  0,-10,
    -10,  0,  0,  0,  0,  0,  0,-10,
    -20,-10,-10,-10,-10,-10,-10,-20
    };
    readonly sbyte[] rooks = new sbyte[]
    {
     0,  0,  0,  5,  5,  0,  0,  0,
    -5,  0,  0,  0,  0,  0,  0, -5,
    -5,  0,  0,  0,  0,  0,  0, -5,
    -5,  0,  0,  0,  0,  0,  0, -5,
    -5,  0,  0,  0,  0,  0,  0, -5,
    -5,  0,  0,  0,  0,  0,  0, -5,
     5, 10, 10, 10, 10, 10, 10,  5,
     0,  0,  0,  0,  0,  0,  0,  0
    };
    readonly sbyte[] queens = new sbyte[]
    {
    -20,-10,-10, -5, -5,-10,-10,-20,
    -10,  0,  0,  0,  0,  0,  0,-10,
    -10,  0,  5,  5,  5,  5,  0,-10,
     -5,  0,  5,  5,  5,  5,  0, -5,
     -5,  0,  5,  5,  5,  5,  0, -5,
    -10,  0,  5,  5,  5,  5,  0,-10,
    -10,  0,  0,  0,  0,  0,  0,-10,
    -20,-10,-10, -5, -5,-10,-10,-20
    };
    readonly sbyte[] kingsEarly = new sbyte[]
    {
     20, 30, 10,  0,  0, 10, 30, 20,
     20, 20,  0,  0,  0,  0, 20, 20,
    -10,-20,-20,-20,-20,-20,-20,-10,
    -20,-30,-30,-40,-40,-30,-30,-20,
    -30,-40,-40,-50,-50,-40,-40,-30,
    -30,-40,-40,-50,-50,-40,-40,-30,
    -30,-40,-40,-50,-50,-40,-40,-30,
    -30,-40,-40,-50,-50,-40,-40,-30
    };
    readonly sbyte[] kingsLate = new sbyte[]
    {
    -50,-30,-30,-30,-30,-30,-30,-50,
    -30,-30,  0,  0,  0,  0,-30,-30,
    -30,-10, 20, 30, 30, 20,-10,-30,
    -30,-10, 30, 40, 40, 30,-10,-30,
    -30,-10, 30, 40, 40, 30,-10,-30,
    -30,-10, 20, 30, 30, 20,-10,-30,
    -30,-20,-10,  0,  0,-10,-20,-30,
    -50,-40,-30,-20,-20,-30,-40,-50
    };
    void Compress()
    {
        List<sbyte[]> evaluationBoards = new();
        evaluationBoards.Add(kingsLate);
        evaluationBoards.Add(kingsEarly);
        evaluationBoards.Add(queens);
        evaluationBoards.Add(rooks);
        evaluationBoards.Add(bishops);
        evaluationBoards.Add(knights);
        evaluationBoards.Add(pawnsEarly);
        evaluationBoards.Add(pawnLate);

        ulong[] compressedEvaluationBoard = new ulong[64];
        ulong CompressedValue;
        foreach (sbyte[] evaluationBoard in evaluationBoards)
        {
            for (int i = 0; i < evaluationBoard.Length; i++)
            {
                CompressedValue = compressedEvaluationBoard[i];
                CompressedValue = CompressedValue << 8 | (ulong)(byte)evaluationBoard[i];
                compressedEvaluationBoard[i] = CompressedValue;
            }
        }
        Console.WriteLine("Compressed evaluation board: " + string.Join(", ", compressedEvaluationBoard));
    }
    ulong[] Compressed = new ulong[]
    {
14849753360064446464, 16293730985874817024, 16288101486341259264, 16285292255607128064, 16285292255607128064, 16288101486341259264, 16293730985874817024, 14849753360064446464, 16290917314144503050, 16290645752205281802, 2570, 388106, 388106, 2570, 16290645752205281802, 16290917314144503050, 16354530658881766666, 17792596228002151178, 1507585472988902922, 2228161413368446986, 2228161413368446986, 1507585472988902922, 17792596228002151178, 16354530658881766666, 16351721406672797716, 17789781478066880532, 2225346663601340436, 2943107854213846036, 2943107854213846036, 2225346663601340436, 17789781478066880532, 16351721406672797716, 16348906656905692446, 17786966728383989022, 2222531913750350366, 2940293104446740766, 2940293104446740766, 2222531913750350366, 17786966728383989022, 16348906656905692446, 16348901159347554866, 17786966728299776562, 1501955973370745906, 2219717164067135026, 2219717164067135026, 1501955973370745906, 17786966728299776562, 16348901159347554866, 16348900102784954960, 17066390830885646928, 17786966771249459792, 57983888152080976, 57983888152080976, 17786966771249459792, 17066390830885646928, 16348900102784954960, 14907737205266841600, 15625509391163719680, 16346085331543654400, 17063852019713966080, 17063852019713966080, 16346085331543654400, 15625509391163719680, 14907737205266841600};
    void Rewrite(ulong[] array)
    {
        string[] result = new string[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            result[i] = "0x" + array[i].ToString("X");
        }
        Console.WriteLine("Compressed evaluation board: " + string.Join(", ", result));
    }
    private enum ScoreType { PawnLate, Pawn, Knight, Bishop, Rook, Queen, King, KingLate };
    int GetValue(ScoreType type, int index)
    {
        ulong CompressedValue = Compressed[index];
        ulong Mask = 0xFF;
        return (int)(sbyte)((CompressedValue >> (int)type * 8) & Mask);
    }
    public Move Think(Board board, Timer timer)
    {
        //Compress();

        //Rewrite(Compressed);

        List<sbyte[]> evaluationBoards = new();
        evaluationBoards.Add(kingsLate);
        evaluationBoards.Add(kingsEarly);
        evaluationBoards.Add(queens);
        evaluationBoards.Add(rooks);
        evaluationBoards.Add(bishops);
        evaluationBoards.Add(knights);
        evaluationBoards.Add(pawnsEarly);
        evaluationBoards.Add(pawnLate);
        for (int i = 0; i <= 7; i++)
        {
            sbyte[] evaluationBoard = evaluationBoards[i];
            for (int j = 0; j < evaluationBoard.Length; j++)
            {
                if (evaluationBoard[j] != GetValue((ScoreType)(7 - i), j))
                    Console.WriteLine($"ScoreType: {7 - i}, Index: {j}, Expected: {evaluationBoard[j]}, Actual: {GetValue((ScoreType)(7 - i), j)} !!!");
                else
                    Console.WriteLine($"ScoreType: {7 - i}, Index: {j}, Expected: {evaluationBoard[j]}, Actual: {GetValue((ScoreType)(7 - i), j)}");
            }
        }
        return board.GetLegalMoves()[0];
    }
}