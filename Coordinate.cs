using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mini_Project___Console_Chess
{
    public class Coordinate(int rank, int file)
    {
        public int Rank { get; set; } = rank;
        public int File { get; set; } = file;
    
        public string ToAlgebraicNotation()
        {
            Dictionary<int, char> Files = new Dictionary<int, char>
            {
                {0,'a'},{1,'b'},{2,'c'},{3,'d'},{4,'e'},{5,'f'},{6,'g'},{7,'h'}
            };
            Dictionary<int, char> Ranks = new Dictionary<int, char>
            {
                {0,'1'},{1,'2'},{2,'3'},{3,'4'},{4,'5'},{5,'6'},{6,'7'},{7,'8'}
            };
            return $"{Files[File]}{Ranks[Rank]}";
        }

        public static char NumToFile(int fileIndex)
        {
            Dictionary<int, char> Files = new Dictionary<int, char>
            {
                {0,'a'},{1,'b'},{2,'c'},{3,'d'},{4,'e'},{5,'f'},{6,'g'},{7,'h'}
            };
            return Files[fileIndex];
        }
        
        public static char NumToRank(int rankIndex)
        {
            Dictionary<int, char> Ranks = new Dictionary<int, char>
            {
                {0,'1'},{1,'2'},{2,'3'},{3,'4'},{4,'5'},{5,'6'},{6,'7'},{7,'8'}
            };
            return Ranks[rankIndex];
        }
    }
}
