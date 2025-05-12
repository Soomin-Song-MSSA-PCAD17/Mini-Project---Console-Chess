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
        
        public Move(Piece piece, Coordinate endPosition)
        {
            Piece = piece;
            EndPosition = endPosition;
        }

        /// changes the position of piece
        /// doesn't check if move is valid
        public void Execute() {
            Console.WriteLine($"{Piece.Position.ToAlgebraicNotation()} to {EndPosition.ToAlgebraicNotation()}");
            Piece.Position.File = EndPosition.File;
            Piece.Position.Rank = EndPosition.Rank;
        }

        /// check if move is valid before calling Execute();
        public static bool IsValidMove(Piece piece, Coordinate endPosition, ChessboardBackend boardState)
        {
            Move move = new Move(piece, endPosition);
            switch(piece.Type)
            {
                case PieceType.Pawn:
                    return IsValidPawnMove(move, boardState);
                default:
                    return false; // if it's an unknown piece type, return false
            }
        }
        public bool IsValidMove(ChessboardBackend boardState)
        {
            switch (this.Piece.Type)
            {
                case PieceType.Pawn:
                    return IsValidPawnMove(this, boardState);
                default:
                    return false; // if it's an unknown piece type, return false
            }
        }

        private static bool IsValidPawnMove(Move move, ChessboardBackend boardState)
        {
            int forward = move.Piece.Color==PieceColor.White ? 1 : -1;

            string moveType = "no pattern match";
            if (move.StartPosition.Rank + forward == move.EndPosition.Rank && move.StartPosition.File == move.EndPosition.File)
            { moveType = "single advance"; }
            else if (move.StartPosition.Rank + 2 * forward == move.EndPosition.Rank && move.StartPosition.File == move.EndPosition.File)
            { moveType = "double advance"; }
            else if(move.StartPosition.Rank+forward == move.EndPosition.Rank && Math.Abs(move.StartPosition.File-move.EndPosition.File)==1)
            { moveType = "diagonal capture"; }
            Console.WriteLine($"{move.Piece.Type} at {move.StartPosition} is attempting to move: {moveType}");

            Square endSquare = boardState.GetSquare(move.EndPosition);
            switch (moveType)
            {
                case "single advance":
                    // single advance: non capture, forward space must be open
                    if (endSquare.Occupant == null)
                    { return true; }
                    break;
                case "double advance":
                    // double advance: non capture, two spaces in front of it must be open, must be in starting rank
                    Coordinate pathCoord = new Coordinate(move.StartPosition.Rank + forward, move.StartPosition.File);
                    int startingRank = move.Piece.Color == PieceColor.White ? 1 : 6;

                    if (boardState.GetSquare(pathCoord).Occupant == null &&
                        move.StartPosition.Rank == startingRank &&
                        endSquare.Occupant == null)
                    { return true; }
                    break;
                case "diagonal capture":
                    // diagonal capture: capture, diagonal space must have different color piece
                    if(endSquare.Occupant.Color != move.Piece.Color)
                    {
                        endSquare.Occupant.Kill();
                        return true;
                    }
                    break;
            }



                // en passant: need to implement move history. capture, pawn must have double advanced and end up next to it

                // promotion: can result from single advance or diagonal capture. change type to something else


                return false; // if it doesn't match any patterns, return false
        }
    }
}
