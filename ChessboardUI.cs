using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mini_Project___Console_Chess
{
    public class ChessboardUI
    {
        ChessboardBackend Backend { get; set; }

        #region constants
        public const int SquareWidth = 3;
        public const int SquareHeight = 1;
        public const int VMargin = 3;
        public const int HMargin = 32;
        public const ConsoleColor BACKGROUND = ConsoleColor.Black;
        public const ConsoleColor DARKSQUAREBG = ConsoleColor.DarkGray;
        public const ConsoleColor LIGHTSQUAREBG = ConsoleColor.Yellow;
        public const ConsoleColor DARKPIECEFG = ConsoleColor.DarkMagenta;
        public const ConsoleColor LIGHTPIECEFG = ConsoleColor.Cyan;

        public const ConsoleColor TEXTCOLOR = ConsoleColor.Black;
        public const ConsoleColor LABELCOLOR = ConsoleColor.Yellow;
        #endregion

        public ChessboardUI()
        {
            Backend = new ChessboardBackend();
            Initialize();
        }
        public void Initialize()
        {
            Console.BackgroundColor = BACKGROUND;
            Console.ForegroundColor = TEXTCOLOR;
            
            PrintBoard();
        }
        public void PrintBoard()
        {
            Console.Clear();

            Console.ForegroundColor = LABELCOLOR;
            // print file labels
            for (int f = 0; f < 8; f++)
            {
                Console.SetCursorPosition(f*SquareWidth+HMargin+1, VMargin-1);
                Console.Write(Coordinate.NumToFile(f));
                Console.SetCursorPosition(f*SquareWidth+HMargin+1, 8*SquareHeight+VMargin);
                Console.Write(Coordinate.NumToFile(f));
            }
            // print rank labels
            for (int r = 0; r < 8; r++)
            {
                Console.SetCursorPosition(HMargin-2,(7-r)*SquareHeight+VMargin);
                Console.Write(Coordinate.NumToRank(r));
                Console.SetCursorPosition(HMargin+8*SquareWidth+1,(7-r)*SquareHeight+VMargin);
                Console.Write(Coordinate.NumToRank(r));
            }

            // print squares
            Console.ForegroundColor = TEXTCOLOR;
            for (int rank = 0; rank < 8; rank++)
            {
                for(int file =  0; file < 8; file++)
                {
                    PrintSquare(Backend.Board[rank, file]);
                }
            }
        }
        public void PrintSquare(Square square)
        {
            //TODO: check if there's pieces on it
            string text;
            if (square.Occupant == null) { text = $" {square.Position.ToAlgebraicNotation()}"; }
            else
            {
                text = " X ";
                switch (square.Occupant.Color)
                {
                    case PieceColor.Black:
                        Console.ForegroundColor = DARKPIECEFG;
                        break;
                    case PieceColor.White:
                        Console.ForegroundColor = LIGHTPIECEFG;
                        break;
                }
                switch(square.Occupant.Type)
                {
                    case PieceType.King:
                        text = " K ";
                        break;
                    case PieceType.Queen:
                        text = " Q ";
                        break;
                    case PieceType.Bishop:
                        text = " B ";
                        break;
                    case PieceType.Knight:
                        text = " N ";
                        break;
                    case PieceType.Rook:
                        text = " R ";
                        break;
                    case PieceType.Pawn:
                        text = " p ";
                        break;
                }
            }

            Console.SetCursorPosition(
                (square.Position.File) * SquareWidth + HMargin,
                (7 - square.Position.Rank) * SquareHeight + VMargin
            );

            if (square.Color == SquareColorTypes.dark)
            {
                Console.BackgroundColor = DARKSQUAREBG;
                Console.Write(text);
            }
            if (square.Color == SquareColorTypes.light)
            {
                Console.BackgroundColor = LIGHTSQUAREBG;
                Console.Write(text);
            }

            // reset colors
            Console.ForegroundColor = TEXTCOLOR;
            Console.BackgroundColor = BACKGROUND;

        }
    }
}
