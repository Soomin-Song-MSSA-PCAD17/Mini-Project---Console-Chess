﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mini_Project___Console_Chess
{
    public class Coordinate
    {
        private Dictionary<int, char> Files = new Dictionary<int, char>
        {
            {0,'a'},{1,'b'},{2,'c'},{3,'d'},{4,'e'},{5,'f'},{6,'g'},{7,'h'}
        };
        private Dictionary<int, char> Ranks = new Dictionary<int, char>
        {
            {0,'1'},{1,'2'},{2,'3'},{3,'4'},{4,'5'},{5,'6'},{6,'7'},{7,'8'}
        };
        /// <summary>
        /// horizontal lines, y value, represented by numbers
        /// </summary>
        public int Rank { get; set; }
        /// <summary>
        /// vertical lines, x value, represented by alphabet
        /// </summary>
        public int File { get; set; }
        public bool IsValidSpace { get => Files.ContainsKey(File) && Ranks.ContainsKey(Rank); }
    
        public Coordinate(int rank, int file)
        {
            Rank = rank;
            File = file;
        }

        public Coordinate(string algebraicNotation)
        {
            char file = algebraicNotation[0];
            char rank = algebraicNotation[1];
            File = Files.FirstOrDefault(x => x.Value == file).Key;
            Rank = Ranks.FirstOrDefault(x => x.Value == rank).Key;
        }

        public Coordinate(Coordinate original)
        {
            Rank = original.Rank;
            File = original.File;
        }

        public string ToAlgebraicNotation()
        {
            if(IsValidSpace) { return $"{Files[File]}{Ranks[Rank]}"; }
            else { return "--"; }
        }

        public static int[] FromAlgebraicNotation(string algebraicNotation)
        {
            int[] output = [-1, -1];
            output[0] = algebraicNotation[0] - 'a';
            output[1] = algebraicNotation[1] - '1';
            return output;
        }

        public static char NumToFile(int fileIndex)
        {
            var coord = new Coordinate(0,0);
            return coord.Files[fileIndex];
        }

        public static char NumToRank(int rankIndex)
        {
            var coord = new Coordinate(0, 0);
            return coord.Ranks[rankIndex];
        }

        public override string ToString()
        {
            return ToAlgebraicNotation();
        }
    }
}
