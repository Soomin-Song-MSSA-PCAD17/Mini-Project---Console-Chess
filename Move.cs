using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mini_Project___Console_Chess
{
    public class Move
    {
        public Piece Piece;
        public Coordinate StartPosition { get => Piece.Position; }
        public Coordinate EndPosition;
        public bool IsCapture;
        
        public Move(Piece piece, Coordinate endPosition, bool isCapture)
        {
            Piece = piece;
            EndPosition = endPosition;
            IsCapture = isCapture;
        }

        public static bool IsValidMove(Piece piece, Coordinate endPosition, bool isCapture, ChessboardBackend boardState)
        {
            Move move = new Move(piece, endPosition, isCapture);
            switch(piece.Type)
            {
                case PieceType.Pawn:
                    return IsValidPawnMove(move, boardState);
                default:
                    return false; // if it's an unknown piece type, return false
            }
        }

        private static bool IsValidPawnMove(Move move, ChessboardBackend boardState)
        {
            int forward = move.Piece.Color==PieceColor.White ? 1 : -1;

            string moveType = "no pattern match";
            if (move.StartPosition.Rank + forward == move.EndPosition.Rank && move.StartPosition.File == move.EndPosition.File)
            {
                moveType = "single advance";
            }
            else if (move.StartPosition.Rank + 2 * forward == move.EndPosition.Rank && move.StartPosition.File == move.EndPosition.File)
            {
                moveType = "double advance";
            }
            else if(move.StartPosition.Rank+forward == move.EndPosition.Rank && Math.Abs(move.StartPosition.File-move.EndPosition.File)==1)
            {
                moveType = "diagonal capture";
            }

            switch (moveType)
            {
                case "single advance":
                    // single advance: non capture, forward space must be open
                    break;
                case "double advance":
                    // double advance: non capture, two spaces in front of it must be open
                    break;
                case "diagonal capture":
                    // diagonal capture: capture, diagonal space must have different color piece
                    break;
            }



                // en passant: need to implement move history. capture, pawn must have double advanced and end up next to it

                // promotion: can result from single advance or diagonal capture. change type to something else


                return false; // if it doesn't match any patterns, return false
        }
    }
}
