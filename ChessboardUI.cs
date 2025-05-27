using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mini_Project___Console_Chess
{
    public class ChessboardUI
    {
        private ChessboardBackend Backend { get; set; }

        #region constants
        public const int SquareWidth = 4;
        public const int SquareHeight = 1;
        public const int VMargin = 3;
        public const int HMargin = 12;
        public const ConsoleColor BACKGROUND = ConsoleColor.Black;
        public const ConsoleColor DARKSQUAREBG = ConsoleColor.DarkGray;
        public const ConsoleColor LIGHTSQUAREBG = ConsoleColor.DarkYellow;
        public const ConsoleColor DARKPIECEFG = ConsoleColor.DarkMagenta;
        public const ConsoleColor LIGHTPIECEFG = ConsoleColor.Gray;

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
            // Console.SetWindowSize(SquareWidth * 8 + HMargin + 10, SquareHeight * 8 + VMargin + 10);
            Console.OutputEncoding = Encoding.UTF8;
            Console.BackgroundColor = BACKGROUND;
            Console.ForegroundColor = TEXTCOLOR;

            Backend.Initialize();
        }
        public void PrintBoard()
        {
            Backend.UpdateBoard();
            Console.Clear();
            Console.ForegroundColor = LABELCOLOR;

            // print squares from white's perspective

            #region print board labels
            // print file labels
            for (int f = 0; f < 8; f++)
            {
                Console.SetCursorPosition(f * SquareWidth + HMargin + 1, VMargin - 1);
                Console.Write(Coordinate.NumToFile(f));
                Console.SetCursorPosition(f * SquareWidth + HMargin + 1, 8 * SquareHeight + VMargin);
                Console.Write(Coordinate.NumToFile(f));
            }
            // print rank labels
            for (int r = 0; r < 8; r++)
            {
                Console.SetCursorPosition(HMargin - 2, (7 - r) * SquareHeight + VMargin);
                Console.Write(Coordinate.NumToRank(r));
                Console.SetCursorPosition(HMargin + 8 * SquareWidth + 1, (7 - r) * SquareHeight + VMargin);
                Console.Write(Coordinate.NumToRank(r));
            }
            #endregion print board labels

            Console.ForegroundColor = TEXTCOLOR;
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    PrintSquare(Backend.Board[rank, file]);
                }
            }
        }
        public void PrintSquare(Square square)
        {
            // figure out what goes inside the square
            string text;
            switch (square.Color)
            {
                case SquareColorTypes.dark:
                    Console.BackgroundColor = DARKSQUAREBG;
                    break;
                case SquareColorTypes.light:
                    Console.BackgroundColor = LIGHTSQUAREBG;
                    break;
            }
            if (square.Occupant == null) { text = $" {square.Position.ToAlgebraicNotation()} "; }
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
                switch (square.Occupant.Type)
                {
                    case PieceType.King:
                        text = " ♔  ";
                        break;
                    case PieceType.Queen:
                        text = " ♛  ";
                        break;
                    case PieceType.Bishop:
                        text = " ♝  ";
                        break;
                    case PieceType.Knight:
                        text = " ♞  ";
                        break;
                    case PieceType.Rook:
                        text = " ♜  ";
                        break;
                    case PieceType.Pawn:
                        text = " ♙  ";
                        break;
                }
            }

            // print the square
            Console.SetCursorPosition(
                (square.Position.File) * SquareWidth + HMargin,
                (7 - square.Position.Rank) * SquareHeight + VMargin
            );
            Console.Write(text);

            // reset colors
            Console.ForegroundColor = TEXTCOLOR;
            Console.BackgroundColor = BACKGROUND;
        }

        /// <summary>
        /// parse and handle commands here
        /// </summary>
        /// <returns>false: end the game.</returns>
        public bool CommandHandler()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(HMargin, 8 * SquareHeight + VMargin + 2);
            Console.Write($"{Backend.ActivePlayerString}'s turn.");
            Console.SetCursorPosition(HMargin, 8 * SquareHeight + VMargin + 3);
            Console.Write("Input command: ");

            string[] command = Console.ReadLine().Split(' ');
            if (command[0] == "quit")
            {
                Console.WriteLine(command[0]);
                return false;
            }

            if (command[0] == "debug")
            {
                switch (command[1].ToLower())
                {
                    case "trygetoccupant":
                    case "tgo":
                        Console.WriteLine(Backend.TryGetOccupant(new Coordinate(command[2]), out var piece));
                        break;
                    case "isattackedby":
                    case "iab":
                        int[] rankAndFile = Coordinate.FromAlgebraicNotation(command[2]);
                        bool attackedByWhite = Backend.Board[rankAndFile[0], rankAndFile[1]].IsAttackedBy(Player.White,Backend);
                        bool attackedByBlack = Backend.Board[rankAndFile[0], rankAndFile[1]].IsAttackedBy(Player.Black,Backend);
                        Console.WriteLine($"{command[2]} is attacked by:\n\tWhite: {attackedByWhite}\n\tBlack: {attackedByBlack}");
                        break;
                    case "checkforcheck":
                    case "c4c":
                        Coordinate newCoordinate = new Coordinate(command[3]);
                        if (Backend.TryGetOccupant(new Coordinate(command[2]), out Piece pieceToMove))
                        {
                            Move newMove = new Move(pieceToMove, newCoordinate);
                            Move.MoveDoesNotPutOwnKingInCheck(newMove, Backend);
                        }
                        break;
                }
            }
            else if (command.Length != 2)
            {
                Console.WriteLine("Invalid number of arguments.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return true;
            }

            string regexCoordinate = @"[a-h][1-8]"; //regex for algebraic notation
            Regex re = new Regex(regexCoordinate, RegexOptions.IgnoreCase);
            if (re.IsMatch(command[0]) && re.IsMatch(command[1]))
            {
                //check if input matches regex for validity of coordinate
                if (Backend.TryGetOccupant(new Coordinate(command[0]), out Piece? pieceToMove))
                { Backend.TryMove(new Move(pieceToMove, new Coordinate(command[1]))); }
                else
                { Console.WriteLine($"No piece found at {command[0]}."); }
            }
            else
            { Console.WriteLine($"{command[0]} or {command[1]} is not a valid coordinate."); }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            return true;
        }

        public void Run()
        {
            Console.Clear();
            bool continueGame = true;
            while (continueGame)
            {
                Console.Clear();
                PrintBoard(); // update display
                continueGame = CommandHandler();
            }
        }
    }
}
