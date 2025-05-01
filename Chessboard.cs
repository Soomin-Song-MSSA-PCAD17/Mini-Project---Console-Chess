using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mini_Project___Console_Chess
{
    public class Chessboard
    {
        private Square[,] board;
        public int SquareWidth = 3;
        public int SquareHeight = 1;
        public const int VMargin = 3;
        public const int HMargin = 32;
        public const ConsoleColor BACKGROUND = ConsoleColor.Black;
        public const ConsoleColor DARKSQUAREBG = ConsoleColor.DarkGray;
        public const ConsoleColor LIGHTSQUAREBG = ConsoleColor.White;
        public const ConsoleColor DARKPIECEFG = ConsoleColor.DarkMagenta;
        public const ConsoleColor LIGHTPIECEFG = ConsoleColor.Cyan;

        public const ConsoleColor TEXTCOLOR = ConsoleColor.Black;
        public const ConsoleColor LABELCOLOR = ConsoleColor.Yellow;

        public Chessboard()
        {
            Initialize();
        }
        public void Initialize()
        {
            Console.BackgroundColor = BACKGROUND;
            Console.ForegroundColor = TEXTCOLOR;
            board = new Square[8, 8];
            for (int r = 0; r < 8; r++)
            {
                for (int f = 0; f < 8; f++)
                {
                    board[r, f] = new Square(r, f);
                }
            }
            PrintBoard();
        }
        public void PrintBoard()
        {
            Console.Clear();
            Console.ForegroundColor = LABELCOLOR;

            for (int f = 0; f < 8; f++)
            {
                Console.SetCursorPosition(f*SquareWidth+HMargin+1, VMargin-1);
                Console.Write(Coordinate.NumToFile(f));
                Console.SetCursorPosition(f*SquareWidth+HMargin+1, 8*SquareHeight+VMargin);
                Console.Write(Coordinate.NumToFile(f));
            }

            for (int r = 0; r < 8; r++)
            {
                Console.SetCursorPosition(HMargin-1,(7-r)*SquareHeight+VMargin);
                Console.Write(Coordinate.NumToRank(r));
                Console.SetCursorPosition(HMargin+8*SquareWidth,(7-r)*SquareHeight+VMargin);
                Console.Write(Coordinate.NumToRank(r));
            }
            Console.ForegroundColor = TEXTCOLOR;

            for (int rank = 0; rank < 8; rank++)
            {
                for(int file =  0; file < 8; file++)
                {
                    board[rank, file].PrintSquare(this);
                }
            }
            Console.WriteLine();
        }

        private class Square
        {
            public enum SquareColorTypes { dark, light }

            public Coordinate Position { get; set; }
            public SquareColorTypes Color { get; set; }

            //public Piece? Occupant { get; set; };
            public Square(int rank, int file)
            {
                Position = new Coordinate(rank, file);
                if((rank+file)%2==0) { Color=SquareColorTypes.dark; }
                else { Color=SquareColorTypes.light; }
            }

            public void PrintSquare(Chessboard board)
            {
                Console.SetCursorPosition((Position.File) * board.SquareWidth + HMargin,
                    (7-Position.Rank) * board.SquareHeight + VMargin);
                if (Color == SquareColorTypes.dark)
                {
                    Console.BackgroundColor = DARKSQUAREBG;
                    Console.Write($" {Position.ToAlgebraicNotation()}");
                    Console.BackgroundColor = BACKGROUND;
                }
                if (Color == SquareColorTypes.light)
                {
                    Console.BackgroundColor = LIGHTSQUAREBG;
                    Console.Write($" {Position.ToAlgebraicNotation()}");
                    Console.BackgroundColor = BACKGROUND;
                }
                

            }
        }


    }
}
