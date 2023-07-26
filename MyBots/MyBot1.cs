using ChessChallenge.API;
using System;


// Checks one move and response ahead
// Uses capture and recapture evaluation
// Problem: shuffels pieces around when no capture is available
public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        Random rng = new();
        Move moveToPlay = moves[rng.Next(moves.Length)];
        int max = -10000;

        foreach (Move move in moves)
        {
            PieceType capture = move.CapturePieceType;
            int captureEval = pieceValues[(int)capture];

            board.MakeMove(move);
            Move[] responseMoves = board.GetLegalMoves();
            int min = 10000;

            foreach (Move responseMove in responseMoves)
            {
                PieceType responseCapture = responseMove.CapturePieceType;
                int responseCaptureEval = pieceValues[(int)responseCapture];
            }
            if (min > max)
            {
                max = min;
                moveToPlay = move;
            }
            board.UndoMove(move);

        }
        return moveToPlay;
    }
}