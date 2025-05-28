using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mini_Project___Console_Chess
{
    public enum Player { White = 'W', Black = 'B' }
    public class ChessboardBackend
    {
        public Square[,] Board;
        public List<Piece> Pieces;
        public Player ActivePlayer;
        public string ActivePlayerString { get => ActivePlayer == Player.White ? "White" : "Black"; }
        public List<Move> MoveHistory;
        public ChessboardBackend()
        {
            Initialize();
            UpdateBoard();
        }
        
        /// deep copy the original board
        public ChessboardBackend(ChessboardBackend original)
        {
            // copy active player
            ActivePlayer = original.ActivePlayer;

            // make new pieces
            Dictionary<Piece,Piece> originalToCopyDict = new Dictionary<Piece, Piece>{ };
            Pieces = new List<Piece>{ };
            foreach (Piece piece in original.Pieces)
            {
                Piece newPiece = new Piece(piece.Color, piece.Type, new Coordinate(piece.Position));
                Pieces.Add(newPiece);
                originalToCopyDict[piece] = newPiece;
            }

            // make new board and populate it with pieces
            Board = InitializeBoard();
            UpdateBoard();
            // copy move history
            MoveHistory = [];
            foreach (Move move in original.MoveHistory)
            {
                MoveHistory.Add(new Move(originalToCopyDict[move.Piece], move.StartPosition, move.EndPosition));
            }

        }

        public void Initialize()
        {
            Board = InitializeBoard();
            Pieces= InitializePieces();
            MoveHistory = [];
            ActivePlayer = Player.White;
        }

        private static Square[,] InitializeBoard()
        {
            var board = new Square[8, 8];
            for (int r = 0; r < 8; r++)
            {
                for (int f = 0; f < 8; f++)
                {
                    board[r, f] = new Square(r, f);
                }
            }
            return board;
        }
        private static List<Piece> InitializePieces()
        {
            // generate all 32 pieces
            List<Piece> pieces =
            [
                new(PieceColor.White,PieceType.Rook,new Coordinate("a1")),
                new(PieceColor.White,PieceType.Knight,new Coordinate("b1")),
                new(PieceColor.White,PieceType.Bishop,new Coordinate("c1")),
                new(PieceColor.White,PieceType.Queen,new Coordinate("d1")),
                new(PieceColor.White,PieceType.King,new Coordinate("e1")),
                new(PieceColor.White,PieceType.Bishop,new Coordinate("f1")),
                new(PieceColor.White,PieceType.Knight,new Coordinate("g1")),
                new(PieceColor.White,PieceType.Rook,new Coordinate("h1")),
                new(PieceColor.White,PieceType.Pawn,new Coordinate("a2")),
                new(PieceColor.White,PieceType.Pawn,new Coordinate("b2")),
                new(PieceColor.White,PieceType.Pawn,new Coordinate("c2")),
                new(PieceColor.White,PieceType.Pawn,new Coordinate("d2")),
                new(PieceColor.White,PieceType.Pawn,new Coordinate("e2")),
                new(PieceColor.White,PieceType.Pawn,new Coordinate("f2")),
                new(PieceColor.White,PieceType.Pawn,new Coordinate("g2")),
                new(PieceColor.White,PieceType.Pawn,new Coordinate("h2")),

                new(PieceColor.Black,PieceType.Pawn,new Coordinate("a7")),
                new(PieceColor.Black,PieceType.Pawn,new Coordinate("b7")),
                new(PieceColor.Black,PieceType.Pawn,new Coordinate("c7")),
                new(PieceColor.Black,PieceType.Pawn,new Coordinate("d7")),
                new(PieceColor.Black,PieceType.Pawn,new Coordinate("e7")),
                new(PieceColor.Black,PieceType.Pawn,new Coordinate("f7")),
                new(PieceColor.Black,PieceType.Pawn,new Coordinate("g7")),
                new(PieceColor.Black,PieceType.Pawn,new Coordinate("h7")),
                new(PieceColor.Black,PieceType.Rook,new Coordinate("a8")),
                new(PieceColor.Black,PieceType.Knight,new Coordinate("b8")),
                new(PieceColor.Black,PieceType.Bishop,new Coordinate("c8")),
                new(PieceColor.Black,PieceType.Queen,new Coordinate("d8")),
                new(PieceColor.Black,PieceType.King,new Coordinate("e8")),
                new(PieceColor.Black,PieceType.Bishop,new Coordinate("f8")),
                new(PieceColor.Black,PieceType.Knight,new Coordinate("g8")),
                new(PieceColor.Black,PieceType.Rook,new Coordinate("h8")),
            ];
            // place all pieces in proper positions
            return pieces;
        }
        /// <summary>
        /// Update Board[,].Occupant based on Pieces[].Position
        /// </summary>
        public void UpdateBoard()
        {
            //reset each square
            for (int r = 0; r < 8; r++)
            {
                for (int f = 0; f < 8; f++)
                {
                    Board[r, f].Occupant = null;
                }
            }
            //update each square
            foreach (Piece piece in Pieces)
            {
                if (piece.IsCaptured) { }
                else
                {
                    Board[piece.Position.Rank, piece.Position.File].Occupant = piece;
                }
            }
        }
        public static void ToggleActivePlayer(ChessboardBackend backend)
        {
            if(backend.ActivePlayer==Player.White) { backend.ActivePlayer = Player.Black; }
            else { backend.ActivePlayer = Player.White; }
        }
        public Square GetSquare(Coordinate coordinate)
        {
            if(coordinate.Rank<0 || coordinate.File<0 || coordinate.Rank>7 || coordinate.File > 7)
            {
                return null;
            }
            return Board[coordinate.Rank, coordinate.File];
        }

        /// return true if there's a piece in the square
        /// return false if it's an invalid square or there's no occupant
        public bool TryGetOccupant(Coordinate coordinate, out Piece? piece)
        {
            if (coordinate.Rank < 0 || coordinate.Rank > 7 || coordinate.File < 0 || coordinate.File > 7)
            {
                // invalid square
                piece = null;
                return false;
            }
            Square square = Board[coordinate.Rank, coordinate.File];

            if (square.Occupant == null)
            {
                // valid square but no piece
                piece = null;
                return false;
            }
            else
            {
                // piece found
                piece = square.Occupant;
                return true;
            }
        }
        public bool TryMove(Move move)
        {
            if (!TryGetOccupant(move.StartPosition, out Piece? occupant)) { return false; } // moving fails if there's no piece to move at start position
            if (move.IsValidMove(this))
            {
                move.Execute(this);
                MoveHistory.Add(move);
                ActivePlayer = ActivePlayer == Player.White ? Player.Black : Player.White; // change active player
                return true;
            }
            return false;
        }
        
    }
}