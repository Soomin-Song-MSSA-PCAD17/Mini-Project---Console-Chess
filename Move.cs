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
        public void Execute(ChessboardBackend boardState, bool verbose=true)
        {
            if (verbose) { Console.WriteLine($"\nExecuting move: {Piece} to {EndPosition.ToAlgebraicNotation()}"); }
            // if end position has a piece
            if (boardState.TryGetOccupant(EndPosition, out Piece? captureTarget))
            {
                // if end position's piece is opponent's
                if (captureTarget != null && captureTarget.Color != Piece.Color)
                {
                    if (verbose) { Console.WriteLine($"{Piece} moving to {EndPosition} to capture {captureTarget}"); }
                    captureTarget.Kill();
                }   
            }
            else if (IsEnPassant)
            {
                captureTarget = boardState.MoveHistory.Last().Piece;
                if (verbose) { Console.WriteLine($"{Piece} moving to {EndPosition} to en passant capture {captureTarget}"); }
                captureTarget.Kill();
            }
            else if (IsCastling)
            {
                // Move for rook can be instantiated without affecting move history
                if (DeltaFile == 2)
                {
                    //Kingside castle
                    if (verbose) { Console.WriteLine($"{Piece} moving to {EndPosition} to execute kingside castle"); }
                    if (boardState.TryGetOccupant(new(EndPosition.Rank, EndPosition.File + 1), out Piece? rook))
                    { new Move(rook, new(EndPosition.Rank, EndPosition.File - 1)).Execute(boardState); }
                }
                else
                {
                    //Queenside castle
                    if (verbose) { Console.WriteLine($"{Piece} moving to {EndPosition} to execute queenside castle"); }
                    if (boardState.TryGetOccupant(new(EndPosition.Rank, EndPosition.File - 2), out Piece? rook))
                    { new Move(rook, new(EndPosition.Rank, EndPosition.File + 1)).Execute(boardState); }
                }
            }
            else
            {
                // if open square, just execute movement
                if (verbose) { Console.WriteLine($"{Piece} moving to {EndPosition}, an empty square"); }
            }

            int endRank = Piece.Color == PieceColor.White ? 7 : 0;
            if(verbose && Piece.Type==PieceType.Pawn && EndPosition.Rank == endRank)
            {
                // pawn promotion
                Console.WriteLine("\n\tKnight Bishop Rook Queen");
                Console.Write("Promote pawn to: ");
                while (Piece.Type == PieceType.Pawn)
                {
                    string? input = Console.ReadLine();
                    {
                        switch (input)
                        {
                            case "k":
                            case "n":
                            case "knight":
                                Piece.Type = PieceType.Knight;
                                break;
                            case "b":
                            case "bishop":
                                Piece.Type = PieceType.Bishop;
                                break;
                            case "r":
                            case "rook":
                                Piece.Type = PieceType.Rook;
                                break;
                            case "q":
                            case "queen":
                                Piece.Type = PieceType.Queen;
                                break;
                            default:
                                Console.WriteLine("Please select a valid piece type.");
                                break;
                        }
                    }
                }
            }

            // finally, change position of the piece
            Piece.Position.File = EndPosition.File;
            Piece.Position.Rank = EndPosition.Rank;
            if (verbose) { Console.WriteLine($"Movement successfully executed\n"); }
        }

        // TODO: after move history, have a way to display summarized move history
        public string ToAlgebraicNotation()
        {
            return "";
        }

        public bool IsValidMove(ChessboardBackend boardState, bool verbose = true)
        {
            bool cantBeMoved = false;
            bool isValid = false;

            if (Piece.IsCaptured)
            {
                if (verbose) { Console.WriteLine($"{Piece} has already been captured."); }
                cantBeMoved = true;
            }
            if ((boardState.ActivePlayer == Player.White && Piece.Color == PieceColor.Black)
                || (boardState.ActivePlayer == Player.Black && Piece.Color == PieceColor.White))
            {
                if (verbose) { Console.WriteLine("Only the active player's piece can be moved."); }
                cantBeMoved = true;
            }
            if (StartPosition.Rank == EndPosition.Rank && StartPosition.File == EndPosition.File)
            {
                if (verbose) { Console.WriteLine($"{Piece} did not move."); }
                cantBeMoved = true;
            }
            if (cantBeMoved) { return false; }
            else
            {
                switch (Piece.Type)
                {
                    case PieceType.Pawn:
                        isValid = IsValidPawnMove(this, boardState, verbose);
                        break;
                    case PieceType.Bishop:
                        isValid = IsValidBishopMove(this, boardState, verbose);
                        break;
                    case PieceType.Knight:
                        isValid = IsValidKnightMove(this, boardState, verbose);
                        break;
                    case PieceType.Rook:
                        isValid = IsValidRookMove(this, boardState, verbose);
                        break;
                    case PieceType.Queen:
                        isValid = IsValidQueenMove(this, boardState, verbose);
                        break;
                    case PieceType.King:
                        isValid = IsValidKingMove(this, boardState, verbose);
                        break;
                    default:
                        return false; // if it's an unknown piece type, return false
                }
            }

            if(!isValid) { return false; }

            if (verbose) // prevents infinite loop
            {
                bool kingIsSafe = MoveDoesNotPutOwnKingInCheck(this, boardState);
                if (kingIsSafe) { return true; }
                else { return false; }
            }
            return true;
        }

        private static bool IsValidPawnMove(Move move, ChessboardBackend boardState, bool verbose=true)
        {
            int forward = move.Piece.Color == PieceColor.White ? 1 : -1;
            bool validity = false;
            Move lastMove = boardState.MoveHistory.Count > 0 ? boardState.MoveHistory.Last() : null;

            string moveType = "no pattern match";
            if (move.DeltaRank == forward && move.DeltaFile == 0)
            { moveType = "single advance"; }
            else if (move.DeltaRank == 2 * forward && move.DeltaFile == 0)
            { moveType = "double advance"; }
            else if (lastMove != null &&
                lastMove.Piece.Type == PieceType.Pawn && Math.Abs(lastMove.DeltaRank) == 2 && // if opponent's pawn double advanced
                Math.Abs(lastMove.Piece.Position.File - move.Piece.Position.File) == 1 && // if opponent's pawn is next to this pawn
                lastMove.Piece.Position.Rank == move.Piece.Position.Rank && // two pawns are at same rank
                move.DeltaRank == forward && Math.Abs(move.DeltaFile) == 1 && // diagonal move to capture
                move.EndPosition.File == lastMove.EndPosition.File // active pawn moves behind opposing pawn
            )
            { moveType = "en passant"; }
            else if (move.DeltaRank == forward && Math.Abs(move.DeltaFile) == 1)
            { moveType = "diagonal capture"; }
            if (verbose) { Console.WriteLine($"{move.Piece} is attempting to move: {moveType}"); }


            Square endSquare = boardState.GetSquare(move.EndPosition);
            switch (moveType)
            {
                case "single advance":
                    // single advance: non capture, forward space must be open
                    if (endSquare.Occupant == null)
                    { validity = true; }
                    else
                    {
                        if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: blocked by {endSquare.Occupant}"); }
                        validity = false;
                    }
                    break;

                case "double advance":
                    // double advance: non capture, two spaces in front of it must be open, must be in starting rank
                    Coordinate pathCoord = new Coordinate(move.StartPosition.Rank + forward, move.StartPosition.File);
                    int startingRank = move.Piece.Color == PieceColor.White ? 1 : 6;
                    bool firstSquareBlocked = boardState.TryGetOccupant(pathCoord, out Piece firstSquareBlocker);
                    bool secondSquareBlocked = boardState.TryGetOccupant(move.EndPosition, out Piece secondSquareBlocker);

                    if (firstSquareBlocked)
                    {
                        if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: blocked by {firstSquareBlocker}"); }
                        validity = false;
                    }
                    if (secondSquareBlocked)
                    {
                        if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: blocked by {secondSquareBlocker}"); }
                        validity = false;
                    }
                    if (!firstSquareBlocked && !secondSquareBlocked)
                    {
                        validity = true;
                    }
                    break;

                case "diagonal capture":
                    // diagonal capture: capture, diagonal space must have different color piece
                    bool canCapture = boardState.TryGetOccupant(move.EndPosition, out Piece? occupyingPiece);
                    if (canCapture)
                    {
                        if (occupyingPiece.Color == move.Piece.Color)
                        {
                            if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: cannot capture {occupyingPiece}"); }
                        }
                        else
                        {
                            if (verbose) { Console.WriteLine($"{move.Piece} can move to {move.EndPosition} and can capture {occupyingPiece}."); }
                            validity = true;
                        }
                    }
                    else
                    {
                        if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: nothing to capture"); }
                    }
                    break;

                case "en passant":
                    if (lastMove.Piece.Position.File == move.EndPosition.File) // if moving behind opponent pawn
                    {
                        if (verbose) { Console.WriteLine($"{move.Piece} can move to {move.EndPosition} and en passant capture {lastMove.Piece}"); }
                        move.IsEnPassant = true;
                        validity = true;
                        return validity; // skip unnecessary promotion block
                    }
                    break;

                default:
                    if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: invalid Pawn move pattern for {move.Piece.Type}"); }
                    return false;
            }
            return validity; // if it doesn't match any patterns, return false
        }

        private static bool IsValidBishopMove(Move move, ChessboardBackend boardState, bool verbose = true)
        {
            // if the move is diagonal, then the change in horizontal and vertical position are the same in magnitude
            if (Math.Abs(move.StartPosition.Rank - move.EndPosition.Rank) == Math.Abs(move.StartPosition.File - move.EndPosition.File))
            {
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
                        if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: blocked by {blocker}"); }
                        return false;
                    }
                }
                // check if last square is either empty or occupied by an opponent's piece
                boardState.TryGetOccupant(move.EndPosition, out Piece? occupant);
                if (occupant == null)
                {
                    // moving into empty square
                    if (verbose) { Console.WriteLine($"{move.Piece} can move to {move.EndPosition}, which is an empty square."); }
                    return true;
                }
                else if (occupant.Color != move.Piece.Color)
                {
                    // capturing opponent's piece
                    if (verbose) { Console.WriteLine($"{move.Piece} can move to {move.EndPosition} and can capture {occupant}."); }
                    return true;
                }
                else
                {
                    // blocked by piece of own color
                    if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: blocked by {occupant}"); }
                    return false;
                }
            }
            else
            {
                if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: invalid Bishop move pattern for {move.Piece.Type}"); }
                return false;
            }
        }

        private static bool IsValidKnightMove(Move move, ChessboardBackend boardState, bool verbose = true)
        {
            int deltaRank = move.EndPosition.Rank - move.StartPosition.Rank;
            int deltaFile = move.EndPosition.File - move.StartPosition.File;
            int magnitude = Math.Abs(deltaRank) + Math.Abs(deltaFile);

            // if total magnitude is 3 but not straight line move, move is valid
            if (magnitude == 3 && deltaRank != 0 && deltaFile != 0)
            {
                //Console.WriteLine("This is an L-shaped move.");
                boardState.TryGetOccupant(move.EndPosition, out Piece? occupant);
                if (occupant == null)
                {
                    // moving into empty square
                    if (verbose) { Console.WriteLine($"{move.Piece} can move to {move.EndPosition}, which is an empty square."); }
                    return true;
                }
                else if (occupant.Color != move.Piece.Color)
                {
                    // capturing opponent's piece
                    if (verbose) { Console.WriteLine($"{move.Piece} can move to {move.EndPosition} and can capture {occupant}."); }
                    return true;
                }
                else
                {
                    // blocked by piece of own color
                    if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: blocked by {occupant}"); }
                    return false;
                }
            }
            else
            {
                if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: invalid Knight move pattern for {move.Piece.Type}"); }
                return false;
            }
            if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: unknown issue"); }
            return false;
        }

        private static bool IsValidRookMove(Move move, ChessboardBackend boardState, bool verbose = true)
        {
            // if the move is straight, it's a valid rook move
            if ((move.StartPosition.Rank == move.EndPosition.Rank) || (move.StartPosition.File == move.EndPosition.File))
            {
                //Console.WriteLine("This is a straight line move.");
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
                        if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: blocked by {blocker}"); }
                        return false;
                    }
                }
                // check last square to see if it's empty or capture
                boardState.TryGetOccupant(move.EndPosition, out Piece? occupant);
                if (occupant == null)
                {
                    // moving into empty square
                    if (verbose) { Console.WriteLine($"{move.Piece} can move to {move.EndPosition}, which is an empty square."); }
                    return true;
                }
                else if (occupant.Color != move.Piece.Color)
                {
                    // capturing opponent's piece
                    if (verbose) { Console.WriteLine($"{move.Piece} can move to {move.EndPosition} and can capture {occupant}."); }
                    return true;
                }
                else
                {
                    // blocked by piece of own color
                    if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: blocked by {occupant}"); }
                    return false;
                }
            }
            else
            {
                if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: invalid Rook move pattern for {move.Piece.Type}"); }
                return false;
            }
        }

        private static bool IsValidQueenMove(Move move, ChessboardBackend boardState, bool verbose = true)
        {
            if (move.DeltaRank == 0 || move.DeltaFile == 0)
            {
                return IsValidRookMove(move, boardState, verbose);
            }
            else if (Math.Abs(move.DeltaRank) == Math.Abs(move.DeltaFile))
            {
                return IsValidBishopMove(move, boardState, verbose);
            }
            else
            {
                if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: invalid Queen move pattern for {move.Piece.Type}"); }
                return false;
            }
        }

        private static bool IsValidKingMove(Move move, ChessboardBackend boardState, bool verbose = true)
        {
            int deltaRank = move.EndPosition.Rank - move.StartPosition.Rank;
            int deltaFile = move.EndPosition.File - move.StartPosition.File;
            // normally, King can only move 1 space sideways or diagonally
            if (Math.Abs(deltaRank) <= 1 && Math.Abs(deltaFile) <= 1)
            {
                boardState.TryGetOccupant(move.EndPosition, out Piece? occupant);
                if (occupant == null)
                {
                    // moving into empty square
                    if (verbose) { Console.WriteLine($"{move.Piece} can move to {move.EndPosition}, which is an empty square."); }
                    return true;
                }
                else if (occupant.Color != move.Piece.Color)
                {
                    // capturing opponent's piece
                    if (verbose) { Console.WriteLine($"{move.Piece} can move to {move.EndPosition} and can capture {occupant}."); }
                    return true;
                }
                else
                {
                    // blocked by piece of own color
                    if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: blocked by {occupant}"); }
                    return false;
                }
            }

            else if (deltaFile == 2) // kingside castle
            {
                if (verbose) { Console.WriteLine($"{move.Piece} is attempting to move: kingside castle"); }
                // make sure king hasn't moved
                if (move.Piece.WasMoved(boardState))
                {
                    if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: {move.Piece} has already moved this game."); }
                    return false;
                }
                // make sure rook hasn't moved
                if(boardState.TryGetOccupant(new Coordinate(move.Piece.Position.Rank,move.Piece.Position.File+3),out Piece? rook))
                {
                    if(rook.WasMoved(boardState))
                    {
                        if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: {rook} has already moved this game."); }
                        return false;
                    }
                }
                // make sure all the spaces are open
                for(int i = 1; i <= 2; i++)
                {
                    Coordinate castlingPath = new Coordinate(move.Piece.Position.Rank, move.Piece.Position.File + i);
                    if (boardState.TryGetOccupant(castlingPath, out Piece? shouldBeEmpty))
                    {
                        if(shouldBeEmpty!=null)
                        {
                            if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: blocked by {shouldBeEmpty}"); }
                            return false;
                        }
                    } 
                }
                // make sure all the spaces are unattacked
                for(int i = 0; i <= 2; i++)
                {
                    Coordinate castlingPath = new Coordinate(move.Piece.Position.Rank, move.Piece.Position.File + i);
                    if (!MoveDoesNotPutOwnKingInCheck(new Move(move.Piece, castlingPath), boardState, verbose:false))
                    {
                        if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: {castlingPath} is under attack"); }
                        return false;
                    }
                }

                move.IsCastling = true;
                return true;
            }
            else if (deltaFile == -2) // queenside castle
            {
                if (verbose) { Console.WriteLine($"{move.Piece} is attempting to move: queenside castle"); }
                // make sure king hasn't moved
                if (move.Piece.WasMoved(boardState))
                {
                    if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: {move.Piece} has already moved this game."); }
                    return false;
                }
                // make sure rook hasn't moved
                if (boardState.TryGetOccupant(new Coordinate(move.Piece.Position.Rank, move.Piece.Position.File-4), out Piece? rook))
                {
                    if (rook.WasMoved(boardState))
                    {
                        if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: {rook} has already moved this game."); }
                        return false;
                    }
                }
                // make sure all the spaces are open
                for (int i = -1; i >= -3; i--)
                {
                    Coordinate castlingPath = new Coordinate(move.Piece.Position.Rank, move.Piece.Position.File + i);
                    if (boardState.TryGetOccupant(castlingPath, out Piece? shouldBeEmpty))
                    {
                        if (shouldBeEmpty != null)
                        {
                            if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: blocked by {shouldBeEmpty}"); }
                            return false;
                        }
                    }
                }
                // make sure all the spaces are unattacked
                for (int i = 0; i >= -2; i--)
                {
                    Coordinate castlingPath = new Coordinate(move.Piece.Position.Rank, move.Piece.Position.File + i);
                    if (!MoveDoesNotPutOwnKingInCheck(new Move(move.Piece, castlingPath), boardState, verbose: false))
                    {
                        if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: {castlingPath} is under attack"); }
                        return false;
                    }
                }
                move.IsCastling = true;
                return true;
            }
            else
            {
                if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: invalid King move pattern for {move.Piece.Type}"); }
                return false;
            }
            if (verbose) { Console.WriteLine($"{move.Piece} cannot move to {move.EndPosition}: unknown issue"); }
            return false;
        }

        public static bool MoveDoesNotPutOwnKingInCheck(Move move, ChessboardBackend boardState, bool verbose=true)
        {
            Player color = move.Piece.Color == PieceColor.White ? Player.White : Player.Black;
            ChessboardBackend copy = new ChessboardBackend(boardState);
            Piece ownKing = copy.Pieces.Find(pc => (pc.Color == move.Piece.Color) && (pc.Type == PieceType.King));
            if (verbose) { Console.WriteLine($"\nCreating a temporary board to look for checks on {ownKing}..."); }
            
            // execute the move on the copy
            copy.TryGetOccupant(move.StartPosition, out Piece newPiece);
            Move newMove = new Move(newPiece, move.EndPosition);
            newMove.Execute(copy, verbose: false);
            copy.UpdateBoard();

            // see if the move causes king to be in check
            List<Piece> checking = new List<Piece> { };
            ChessboardBackend.ToggleActivePlayer(copy); // change active player in order to check what pieces can move into king's space
            foreach (Piece piece in copy.Pieces)
            {
                if (piece.Color != ownKing.Color) // easier on the messages
                {
                    Move attackKing = new Move(piece, ownKing.Position);
                    if (attackKing.IsValidMove(copy, verbose: false)) { checking.Add(piece); }
                }
            }

            foreach (Piece piece in checking)
            {
                if (verbose) { Console.WriteLine($"{piece} is attacking {ownKing}"); }
            }
            if (checking.Count == 0)
            {
                if (verbose) { Console.WriteLine($"{ownKing} is safe from attacks."); }
                return true;
            }
            else
            {
                if (verbose) { Console.WriteLine($"Move cannot be executed because {ownKing} would be in check."); }
                return false;
            }
        }

        public override string ToString() { return $"{Piece.Color} {Piece.Type} from {StartPosition} to {EndPosition}"; }
    }
}
