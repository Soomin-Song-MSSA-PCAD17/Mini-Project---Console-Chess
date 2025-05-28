using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mini_Project___Console_Chess
{
    public enum PieceColor { White = 'W', Black = 'B' }
    public enum PieceType { King, Queen, Bishop, Knight, Rook, Pawn };
    public class Piece
    {
        public PieceColor Color { get; set; }
        public PieceType Type { get; set; }
        public Coordinate Position { get; set; }
        public bool IsCaptured { get => !Position.IsValidSpace; }
        public Piece(PieceColor color, PieceType type, Coordinate position)
        {
            Color = color;
            Type = type;
            Position = position;
        }

        public void Kill()
        {
            Position.Rank = -1;
            Position.File = -1;
        }

        public bool WasMoved(ChessboardBackend boardState)
        {
            bool wasMoved = false;
            boardState.MoveHistory.ForEach(delegate (Move x)
            {
                if (x.Piece == this) { wasMoved = true; }
            });
            return wasMoved;
        }

        public override string ToString()
        {
            return $"{Color.ToString()} {Type.ToString()} at {Position.ToAlgebraicNotation()}";
        }
    }
}
