using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mini_Project___Console_Chess
{
    public enum PieceColor { White, Black }
    public enum PieceType { King, Queen, Bishop, Knight, Rook, Pawn};
    public class Piece
    {
        public PieceColor Color { get; set; }
        public PieceType Type { get; set; }
        public Coordinate Position { get; set; }
        public Piece(PieceColor color, PieceType type, Coordinate position)
        {
            Color = color;
            Type = type;
            Position = position;
        }

        public ValidMove[] GetMoves()
        {
        //TODO: implement this function
            return new ValidMove[0];
        }
    }
}
