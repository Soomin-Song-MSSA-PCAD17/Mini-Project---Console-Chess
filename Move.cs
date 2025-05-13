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
            bool validity = false;

            string moveType = "no pattern match";
            if (move.StartPosition.Rank + forward == move.EndPosition.Rank && move.StartPosition.File == move.EndPosition.File)
            { moveType = "single advance"; }
            else if (move.StartPosition.Rank + 2 * forward == move.EndPosition.Rank && move.StartPosition.File == move.EndPosition.File)
            { moveType = "double advance"; }
            else if(move.StartPosition.Rank+forward == move.EndPosition.Rank && Math.Abs(move.StartPosition.File-move.EndPosition.File)==1)
            { moveType = "diagonal capture"; }
            Console.WriteLine($"{move.Piece.Type} at {move.StartPosition} is attempting to move: {moveType}");


            // TODO: en passant: need to implement move history. capture, pawn must have double advanced and end up next to it


            Square endSquare = boardState.GetSquare(move.EndPosition);
            switch (moveType)
            {
                case "single advance":
                    // single advance: non capture, forward space must be open
                    if (endSquare.Occupant == null)
                    { validity = true; }
                    break;
                case "double advance":
                    // double advance: non capture, two spaces in front of it must be open, must be in starting rank
                    Coordinate pathCoord = new Coordinate(move.StartPosition.Rank + forward, move.StartPosition.File);
                    int startingRank = move.Piece.Color == PieceColor.White ? 1 : 6;

                    if (boardState.GetSquare(pathCoord).Occupant == null &&
                        move.StartPosition.Rank == startingRank &&
                        endSquare.Occupant == null)
                    { validity = true; }
                    break;
                case "diagonal capture":
                    // diagonal capture: capture, diagonal space must have different color piece
                    if(endSquare.Occupant.Color != move.Piece.Color)
                    {
                        endSquare.Occupant.Kill();
                        validity = true;
                    }
                    break;
            }

            // promotion: check after completing movement. change piecetype
            int promotionRank = move.Piece.Color == PieceColor.White ? 7 : 0;
            if (move.EndPosition.Rank == promotionRank)
            {
                Console.WriteLine("\n\tKnight Bishop Rook Queen");
                Console.Write("Pawn promotion to: ");
                while(move.Piece.Type==PieceType.Pawn)
                {
                    string input = Console.ReadLine();
                    {
                        switch (input)
                        {
                            case "k":
                            case "n":
                            case "knight":
                                move.Piece.Type = PieceType.Knight;
                                break;
                            case "b":
                            case "bishop":
                                move.Piece.Type = PieceType.Bishop;
                                break;
                            case "r":
                            case "rook":
                                move.Piece.Type = PieceType.Rook;
                                break;
                            case "q":
                            case "queen":
                                move.Piece.Type = PieceType.Queen;
                                break;
                            default:
                                Console.WriteLine("Invalid selection.");
                                break;
                        }
                    }
                }
            }

            return validity; // if it doesn't match any patterns, return false
        }
    }
}
