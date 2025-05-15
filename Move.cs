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
        public void Execute(ChessboardBackend boardState)
        {
            Console.WriteLine($"{Piece.Position.ToAlgebraicNotation()} to {EndPosition.ToAlgebraicNotation()}");
            // if end position has a piece
            if (boardState.TryGetOccupant(EndPosition, out Piece? captureTarget))
            {
                // if end position's piece is opponent's
                if (captureTarget != null && captureTarget.Color != Piece.Color)
                {
                    Console.WriteLine($"Capturing {captureTarget}");
                    captureTarget.Kill();
                }
            }
            // if open square, just execute movement
            Piece.Position.File = EndPosition.File;
            Piece.Position.Rank = EndPosition.Rank;
        }

        // TODO: after move history, have a way to display summarized move history
        public string ToAlgebraicNotation()
        {
            return "";
        }

        public bool IsValidMove(ChessboardBackend boardState)
        {
            if (StartPosition == EndPosition)
            {
                Console.WriteLine("The piece did not move.");
                return false;
            }
            if ((boardState.ActivePlayer == Player.White && Piece.Color==PieceColor.Black)
                || (boardState.ActivePlayer == Player.Black && Piece.Color == PieceColor.White))
            {
                Console.WriteLine("Only the active player's piece can be moved.");
                return false;
            }
            switch (Piece.Type)
            {
                case PieceType.Pawn:
                    return IsValidPawnMove(this, boardState);
                case PieceType.Bishop:
                    return IsValidBishopMove(this, boardState);
                case PieceType.Knight:
                    return IsValidKnightMove(this, boardState);
                case PieceType.Rook:
                    return IsValidRookMove(this, boardState);
                case PieceType.Queen:
                    return IsValidQueenMove(this, boardState);
                case PieceType.King:
                    return IsValidKingMove(this, boardState);
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
                    if(boardState.TryGetOccupant(move.EndPosition,out var occupyingPiece))
                    {
                        if (occupyingPiece.Color != move.Piece.Color)
                        {
                            validity = true;
                        }
                    }
                    break;
                default:
                    Console.WriteLine($"No pattern match while attempting to move {move.Piece}");
                    break;
            }

            // promotion: check after completing movement. change piecetype
            #region pawn promotion
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
            #endregion
            return validity; // if it doesn't match any patterns, return false
        }

        private static bool IsValidBishopMove(Move move, ChessboardBackend boardState)
        {
            // if the move is diagonal, then the change in horizontal and vertical position are the same in magnitude
            if (Math.Abs(move.StartPosition.Rank - move.EndPosition.Rank) == Math.Abs(move.StartPosition.File - move.EndPosition.File))
            {
                Console.WriteLine("This is a diagonal move.");
                int magnitude = Math.Abs(move.StartPosition.Rank - move.EndPosition.Rank);
                int rankDirection = move.EndPosition.Rank > move.StartPosition.Rank ? 1 : -1;
                int fileDirection = move.EndPosition.File > move.StartPosition.File ? 1 : -1;
                // check if each square along the way is valid
                // i is the offset from start position (starts at 1)
                // end before reaching magnitude, because the last space may be occupied so it'll get checked separately after the loop
                for(int i = 1;i<magnitude;i++)
                {
                    // make sure in-between squares are empty
                    // trygetoccupant the next space
                    int newRank = move.StartPosition.Rank + rankDirection * i;
                    int newFile = move.StartPosition.File + fileDirection * i;

                    if (boardState.TryGetOccupant(new Coordinate(newRank, newFile), out Piece shouldBeEmpty))
                    {
                        // if occupant value is found, block movement
                        Console.WriteLine($"Could not execute move. Blocked by {shouldBeEmpty.ToString()}");
                        return false;
                    }
                }
                // check if last square is either empty or occupied by an opponent's piece
                boardState.TryGetOccupant(move.EndPosition, out Piece occupant);
                if(occupant == null)
                {
                    // moving into empty square
                    Console.WriteLine("Moving into empty square.");
                    return true;
                }
                else if(occupant.Color != move.Piece.Color)
                {
                    // capturing opponent's piece
                    Console.WriteLine($"Capturing {occupant.ToString()}.");
                    return true;
                }
                else
                {
                    // blocked by piece of own color
                    Console.WriteLine($"Blocked by {occupant.ToString()}");
                    return false;
                }
            }
            else
            {
                Console.WriteLine("This is not a diagonal move.");
                return false;
            }
            Console.WriteLine($"Unknown error while attempting to move {move.Piece.ToString()}");
            return false;
        }

        private static bool IsValidKnightMove(Move move, ChessboardBackend boardState)
        {
            int deltaRank = move.EndPosition.Rank - move.StartPosition.Rank;
            int deltaFile = move.EndPosition.File - move.StartPosition.File;
            int magnitude = Math.Abs(deltaRank)+Math.Abs(deltaFile);

            // if total magnitude is 3 but not straight line move, move is valid
            if(magnitude==3 && deltaRank!=0 && deltaFile != 0)
            {
                Console.WriteLine("This is an L-shaped move.");
                boardState.TryGetOccupant(move.EndPosition, out Piece occupant);
                if (occupant == null)
                {
                    // moving into empty square
                    Console.WriteLine("Moving into empty square.");
                    return true;
                }
                else if (occupant.Color != move.Piece.Color)
                {
                    // capturing opponent's piece
                    Console.WriteLine($"Capturing {occupant.ToString()}.");
                    return true;
                }
                else
                {
                    // blocked by piece of own color
                    Console.WriteLine($"Blocked by {occupant.ToString()}");
                    return false;
                }
            }
            Console.WriteLine($"Unknown error while attempting to move {move.Piece.ToString()}");
            return false;
        }

        private static bool IsValidRookMove(Move move, ChessboardBackend boardState)
        {
            // if the move is straight, it's a valid rook move
            if ((move.StartPosition.Rank == move.EndPosition.Rank) || (move.StartPosition.File == move.EndPosition.File))
            {
                Console.WriteLine("This is a straight line move.");
                // check all squares between start and end
                int magnitude = Math.Abs((move.EndPosition.Rank - move.StartPosition.Rank) + (move.EndPosition.File - move.StartPosition.File));
                int rankDirection = (move.EndPosition.Rank - move.StartPosition.Rank) / magnitude;
                int fileDirection = (move.EndPosition.File - move.StartPosition.File) / magnitude;

                for (int i = 1; i < magnitude; i++)
                {
                    // make sure in-between squares are empty
                    // trygetoccupant the next space
                    int newRank = move.StartPosition.Rank + rankDirection * i;
                    int newFile = move.StartPosition.File + fileDirection * i;

                    if (boardState.TryGetOccupant(new Coordinate(newRank, newFile), out Piece shouldBeEmpty))
                    {
                        // if occupant value is found, block movement
                        Console.WriteLine($"Could not execute move. Blocked by {shouldBeEmpty.ToString()}");
                        return false;
                    }
                }
                // check last square to see if it's empty or capture
                boardState.TryGetOccupant(move.EndPosition, out Piece occupant);
                if (occupant == null)
                {
                    // moving into empty square
                    Console.WriteLine("Moving into empty square.");
                    return true;
                }
                else if (occupant.Color != move.Piece.Color)
                {
                    // capturing opponent's piece
                    Console.WriteLine($"Capturing {occupant.ToString()}.");
                    return true;
                }
                else
                {
                    // blocked by piece of own color
                    Console.WriteLine($"Blocked by {occupant.ToString()}");
                    return false;
                }
            }
            else
            {
                Console.WriteLine("This is not a straight line move.");
                return false;
            }
            Console.WriteLine($"Unknown error while attempting to move {move.Piece.ToString()}");
            return false;
        }

        private static bool IsValidQueenMove(Move move, ChessboardBackend boardState)
        {
            return IsValidBishopMove(move, boardState) || IsValidRookMove(move, boardState);
        }

        private static bool IsValidKingMove(Move move, ChessboardBackend boardState)
        {
            int deltaRank = move.EndPosition.Rank - move.StartPosition.Rank;
            int deltaFile = move.EndPosition.File - move.StartPosition.File;
            // King can only move 1 space sideways or diagonally
            if(Math.Abs(deltaRank) <= 1 && Math.Abs(deltaFile) <= 1)
            {
                boardState.TryGetOccupant(move.EndPosition, out Piece occupant);
                if (occupant == null)
                {
                    // moving into empty square
                    Console.WriteLine("Moving into empty square.");
                    return true;
                }
                else if (occupant.Color != move.Piece.Color)
                {
                    // capturing opponent's piece
                    Console.WriteLine($"Capturing {occupant.ToString()}.");
                    return true;
                }
                else
                {
                    // blocked by piece of own color
                    Console.WriteLine($"Blocked by {occupant.ToString()}");
                    return false;
                }
            }
            else if (false) // TODO: castling. Need move history
            {

            }
                Console.WriteLine($"Unknown error while attempting to move {move.Piece.ToString()}");
            return false;
        }
    }
}
