using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mini_Project___Console_Chess
{
    public enum Player { White='W', Black='B' }
    public class ChessboardBackend
    {
        public Square[,] Board;
        public List<Piece> Pieces;
        public Player ActivePlayer;
        public string ActivePlayerString { get=>ActivePlayer==Player.White?"White":"Black"; }
        public ChessboardBackend()
        {
            InitializeBoard();
            InitializePieces();
            ActivePlayer = Player.White;
        }
        private void InitializeBoard()
        {
            var board = new Square[8, 8];
            for (int r = 0; r < 8; r++)
            {
                for (int f = 0; f < 8; f++)
                {
                    board[r, f] = new Square(r, f);
                }
            }
            Board = board;
        }
        private void InitializePieces()
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
            Pieces = pieces;
            UpdateBoard();
        }
        /// <summary>
        /// Update Board[,].Occupant based on Pieces[].Position
        /// </summary>
        private void UpdateBoard()
        {
            foreach(Piece piece in Pieces)
            {
                Board[piece.Position.Rank, piece.Position.File].Occupant = piece;
            }
        }
        public bool TryGetOccupant(Coordinate coordinate, out Piece? piece)
        {
            if(Board[coordinate.Rank, coordinate.File].Occupant==null)
            {
                piece = null;
                return false;
            }
            else
            {
                piece = Board[coordinate.Rank, coordinate.File].Occupant;
                return true;
            }
        }
        
        public void ExecuteMove(Move move)
        {
            // TODO: execute the piece movement
            UpdateBoard();
        }
    }
}