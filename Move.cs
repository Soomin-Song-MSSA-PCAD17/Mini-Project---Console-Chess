using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mini_Project___Console_Chess
{
    public class Move
    {
        public Piece Piece;
        public Coordinate StartPosition { get; set; }
        public Coordinate EndPosition { get; set; }
        public int DeltaRank { get => EndPosition.Rank - StartPosition.Rank; }
        public int DeltaFile { get => EndPosition.File - StartPosition.File; }
        private bool IsEnPassant;
        private bool IsCastling;

        public Move(Piece piece, Coordinate endPosition)
        {
            Piece = piece;
            EndPosition = endPosition;
            StartPosition = new Coordinate(Piece.Position.ToAlgebraicNotation());
            IsEnPassant = false;
            IsCastling = false;
        }

        /// use for deep copy
        public Move(Piece piece, Coordinate startPosition, Coordinate endPosition)
        {
            Piece = piece;
            StartPosition = startPosition;
            EndPosition = endPosition;
            IsEnPassant = false;
            IsCastling = false;
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
            if (IsEnPassant)
            {
                captureTarget = boardState.MoveHistory.Last().Piece;
                Console.WriteLine($"En passant capturing {captureTarget}");
                captureTarget.Kill();
            }
            if (IsCastling)
            {
                // TODO: implement moving rook
                if (DeltaFile == 2)
                {
                    //Kingside castle
                    Console.WriteLine("Executing Kingside castle");
                    if(boardState.TryGetOccupant(new(EndPosition.Rank, EndPosition.File + 1), out Piece? rook))
                    {
                        new Move(rook, new(EndPosition.Rank, EndPosition.File - 1)).Execute(boardState);
                    }
                }
                else
                {
                    //Queenside castle
                    Console.WriteLine("Executing Queenside castle");
                    if(boardState.TryGetOccupant(new(EndPosition.Rank, EndPosition.File - 1), out Piece? rook))
                    {
                        new Move(rook, new(EndPosition.Rank, EndPosition.File + 1)).Execute(boardState);
                    }
                }
                // Move for rook can be instantiated without affecting move history
                
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
            bool cantBeMoved = false;
            bool isValid = false;

            // TODO: also check if moving will open up your own king
            if (StartPosition.Rank == -1)
            {
                Console.WriteLine($"{Piece} has already been captured.");
                cantBeMoved = true;
            }
            if ((boardState.ActivePlayer == Player.White && Piece.Color == PieceColor.Black)
                || (boardState.ActivePlayer == Player.Black && Piece.Color == PieceColor.White))
            {
                Console.WriteLine("Only the active player's piece can be moved.");
                cantBeMoved = true;
            }
            if (StartPosition.Rank == EndPosition.Rank && StartPosition.File == EndPosition.File)
            {
                Console.WriteLine($"{Piece} did not move.");
                cantBeMoved = true;
            }

            if (!cantBeMoved)
            {
                switch (Piece.Type)
                {
                    case PieceType.Pawn:
                        isValid = IsValidPawnMove(this, boardState);
                        break;
                    case PieceType.Bishop:
                        isValid = IsValidBishopMove(this, boardState);
                        break;
                    case PieceType.Knight:
                        isValid = IsValidKnightMove(this, boardState);
                        break;
                    case PieceType.Rook:
                        isValid = IsValidRookMove(this, boardState);
                        break;
                    case PieceType.Queen:
                        isValid = IsValidQueenMove(this, boardState);
                        break;
                    case PieceType.King:
                        isValid = IsValidKingMove(this, boardState);
                        break;
                    default:
                        return false; // if it's an unknown piece type, return false
                }
            }

            if(!isValid) { return false; }

            // TODO: check 

            // is this a valid move and does this capture opponent's king? if so, no need to check if this opens up your own king
            if(boardState.TryGetOccupant(EndPosition, out Piece? occupant))
            {
                if(occupant.Color != Piece.Color && occupant.Type==PieceType.King) //occupant is opponent's king
                {
                    return true;
                } 
            }
            // if it's a valid move but doesn't capture opponent's king, does this open up your own king? if so, return false


            return true; // if it's a valid move but no restrictions, return true
        }

        private static bool IsValidPawnMove(Move move, ChessboardBackend boardState)
        {
            int forward = move.Piece.Color == PieceColor.White ? 1 : -1;
            bool validity = false;
            Move lastMove = null;
            if(boardState.MoveHistory.Count>0) { lastMove = boardState.MoveHistory.Last(); }

            string moveType = "no pattern match";
            if (move.DeltaRank == forward && move.DeltaFile == 0)
            { moveType = "single advance"; }
            else if (move.DeltaRank == 2 * forward && move.DeltaFile == 0)
            { moveType = "double advance"; }
            else if (lastMove != null &&
                lastMove.Piece.Type == PieceType.Pawn && Math.Abs(lastMove.DeltaRank) == 2 && // if opponent's pawn double advanced
                Math.Abs(lastMove.Piece.Position.File - move.Piece.Position.File) == 1 && // if opponent's pawn is next to this pawn
                move.DeltaRank == forward && Math.Abs(move.DeltaFile) == 1 // diagonal move to capture
            )
            { moveType = "en passant"; }
            else if (move.DeltaRank == forward && Math.Abs(move.DeltaFile) == 1)
            { moveType = "diagonal capture"; }
            Console.WriteLine($"{move.Piece.Type} at {move.StartPosition} is attempting to move: {moveType}");


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
                    if (boardState.TryGetOccupant(move.EndPosition, out Piece? occupyingPiece))
                    {
                        if ((occupyingPiece != null) && (occupyingPiece.Color != move.Piece.Color))
                        {
                            validity = true;
                        }
                    }
                    break;
                case "en passant":
                    if (lastMove.Piece.Position.File == move.EndPosition.File) // if moving behind opponent pawn
                    {
                        Console.WriteLine("This is a valid en passant move");
                        move.IsEnPassant = true;
                        validity = true;
                        return validity;
                    }
                    break;
                default:
                    Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}.");
                    return false;
                    break;
            }

            // promotion: check after completing movement. change piecetype
            #region pawn promotion
            int promotionRank = move.Piece.Color == PieceColor.White ? 7 : 0;
            if (move.EndPosition.Rank == promotionRank)
            {
                Console.WriteLine("\n\tKnight Bishop Rook Queen");
                Console.Write("Promote pawn to: ");
                while (move.Piece.Type == PieceType.Pawn)
                {
                    string? input = Console.ReadLine();
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
                for (int i = 1; i < magnitude; i++)
                {
                    // make sure in-between squares are empty
                    // trygetoccupant the next space
                    int newRank = move.StartPosition.Rank + rankDirection * i;
                    int newFile = move.StartPosition.File + fileDirection * i;

                    if (boardState.TryGetOccupant(new Coordinate(newRank, newFile), out Piece? blocker))
                    {
                        // if occupant value is found, block movement
                        Console.WriteLine($"Could not execute move. Blocked by {blocker}");
                        return false;
                    }
                }
                // check if last square is either empty or occupied by an opponent's piece
                boardState.TryGetOccupant(move.EndPosition, out Piece? occupant);
                if (occupant == null)
                {
                    // moving into empty square
                    //Console.WriteLine("Moving into empty square.");
                    return true;
                }
                else if (occupant.Color != move.Piece.Color)
                {
                    // capturing opponent's piece
                    Console.WriteLine($"Capturing {occupant}.");
                    return true;
                }
                else
                {
                    // blocked by piece of own color
                    Console.WriteLine($"Blocked by {occupant}");
                    return false;
                }
            }
            else
            {
                Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}.");
                return false;
            }
        }

        private static bool IsValidKnightMove(Move move, ChessboardBackend boardState)
        {
            int deltaRank = move.EndPosition.Rank - move.StartPosition.Rank;
            int deltaFile = move.EndPosition.File - move.StartPosition.File;
            int magnitude = Math.Abs(deltaRank) + Math.Abs(deltaFile);

            // if total magnitude is 3 but not straight line move, move is valid
            if (magnitude == 3 && deltaRank != 0 && deltaFile != 0)
            {
                Console.WriteLine("This is an L-shaped move.");
                boardState.TryGetOccupant(move.EndPosition, out Piece? occupant);
                if (occupant == null)
                {
                    // moving into empty square
                    //Console.WriteLine("Moving into empty square.");
                    return true;
                }
                else if (occupant.Color != move.Piece.Color)
                {
                    // capturing opponent's piece
                    Console.WriteLine($"Capturing {occupant}.");
                    return true;
                }
                else
                {
                    // blocked by piece of own color
                    Console.WriteLine($"Blocked by {occupant}");
                    return false;
                }
            }
            else
            {
                Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}.");
                return false;
            }
            Console.WriteLine($"Unknown error while attempting to move {move.Piece}");
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

                    if (boardState.TryGetOccupant(new Coordinate(newRank, newFile), out Piece? blocker))
                    {
                        // if occupant value is found, block movement
                        Console.WriteLine($"Could not execute move. Blocked by {blocker}");
                        return false;
                    }
                }
                // check last square to see if it's empty or capture
                boardState.TryGetOccupant(move.EndPosition, out Piece? occupant);
                if (occupant == null)
                {
                    // moving into empty square
                    //Console.WriteLine("Moving into empty square.");
                    return true;
                }
                else if (occupant.Color != move.Piece.Color)
                {
                    // capturing opponent's piece
                    Console.WriteLine($"Capturing {occupant}.");
                    return true;
                }
                else
                {
                    // blocked by piece of own color
                    Console.WriteLine($"Blocked by {occupant}");
                    return false;
                }
            }
            else
            {
                Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}.");
                return false;
            }
        }

        private static bool IsValidQueenMove(Move move, ChessboardBackend boardState)
        {
            return IsValidBishopMove(move, boardState) || IsValidRookMove(move, boardState);
        }

        private static bool IsValidKingMove(Move move, ChessboardBackend boardState)
        {
            // TODO: Need to implement a way to check if a square is attacked
            int deltaRank = move.EndPosition.Rank - move.StartPosition.Rank;
            int deltaFile = move.EndPosition.File - move.StartPosition.File;
            // normally, King can only move 1 space sideways or diagonally
            if (Math.Abs(deltaRank) <= 1 && Math.Abs(deltaFile) <= 1)
            {
                boardState.TryGetOccupant(move.EndPosition, out Piece? occupant);
                if (occupant == null)
                {
                    // moving into empty square
                    //Console.WriteLine("Moving into empty square.");
                    return true;
                }
                else if (occupant.Color != move.Piece.Color)
                {
                    // capturing opponent's piece
                    Console.WriteLine($"Capturing {occupant}.");
                    return true;
                }
                else
                {
                    // blocked by piece of own color
                    Console.WriteLine($"Blocked by {occupant}");
                    return false;
                }
            }
            else if (deltaFile == 2) // kingside castle
            {
                Console.WriteLine("Attempting to kingside castle.");
                // make sure king hasn't moved
                if(move.Piece.WasMoved(boardState)) { Console.WriteLine($"{move.Piece} has been moved already."); return false; }
                // make sure rook hasn't moved
                if(boardState.TryGetOccupant(new Coordinate(move.Piece.Position.Rank,move.Piece.Position.File+3),out Piece? rook))
                {
                    if(rook.WasMoved(boardState)) { Console.WriteLine($"{rook} has been moved already."); return false; }
                }
                // make sure all the spaces are open
                for(int i = 1; i <= 2; i++)
                {
                    if (boardState.TryGetOccupant(new Coordinate(move.Piece.Position.Rank, move.Piece.Position.File + i), out Piece? shouldBeEmpty))
                    {
                        if(shouldBeEmpty!=null) { Console.WriteLine($"{shouldBeEmpty} is blocking the path."); return false; }
                    }
                }
                // TODO: make sure all the spaces are not attacked
                move.IsCastling = true;
                return true;
            }
            else if (deltaFile == -3) // queenside castle
            {
                Console.WriteLine("Attempting to queenside castle.");
                // make sure king hasn't moved
                if (move.Piece.WasMoved(boardState)) { Console.WriteLine($"{move.Piece} has been moved already."); return false; }
                // make sure rook hasn't moved
                if (boardState.TryGetOccupant(new Coordinate(move.Piece.Position.Rank, move.Piece.Position.File-4), out Piece? rook))
                {
                    if (rook.WasMoved(boardState)) { Console.WriteLine($"{rook} has been moved already."); return false; }
                }
                // make sure all the spaces are open
                for (int i = -1; i >= -3; i--)
                {
                    if (boardState.TryGetOccupant(new Coordinate(move.Piece.Position.Rank, move.Piece.Position.File + i), out Piece? shouldBeEmpty))
                    {
                        if (shouldBeEmpty != null) { Console.WriteLine($"{shouldBeEmpty} is blocking the path."); return false; }
                    }
                }
                // TODO: make sure all the spaces are not attacked
                move.IsCastling = true;
                return true;
            }
            else
            {
                Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}.");
                return false;
            }
            Console.WriteLine($"Unknown error while attempting to move {move.Piece}");
            return false;
        }

        public static bool MoveDoesNotPutOwnKingInCheck(Move move, ChessboardBackend boardState)
        {
            Player color = move.Piece.Color == PieceColor.White ? Player.White : Player.Black;
            ChessboardBackend copy = new ChessboardBackend(boardState);
            // execute the move on the copy
            copy.TryGetOccupant(move.StartPosition, out Piece newPiece);
            Move newMove = new Move(newPiece, move.EndPosition);
            newMove.Execute(copy);

            // see if the move causes king to be in check
            Piece ownKing = copy.Pieces.Find(pc => (pc.Color == move.Piece.Color) && (pc.Type == PieceType.King));
            List<Piece> checking = new List<Piece> { };
            ChessboardBackend.ToggleActivePlayer(copy); // change active player in order to check what pieces can move into king's space
            foreach (Piece piece in copy.Pieces)
            {
                if (piece.Color != ownKing.Color) // easier on the messages
                {
                    Move attackKing = new Move(piece, ownKing.Position);
                    if (attackKing.IsValidMove(copy)) { checking.Add(piece); }
                }
            }

            foreach (Piece piece in checking)
            {
                Console.WriteLine($"{piece} is attacking {ownKing}");
            }
            if (checking.Count == 0)
            {
                Console.WriteLine($"No enemy pieces are attacking {ownKing}");
                return true;
            }
            else
            {
                Console.WriteLine($"Move cannot be executed because {ownKing} would be in check.");
                return false;
            }
        }

        public override string ToString() { return $"{Piece.Color} {Piece.Type} from {StartPosition} to {EndPosition}"; }
    }
}
