namespace Mini_Project___Console_Chess
{
    public enum SquareColorTypes { dark, light }
    public class Square
    {
        public SquareColorTypes Color { get; set; }
        public Coordinate Position { get; set; }

        public Piece? Occupant { get; set; }
        public Square(int rank, int file)
        {
            Position = new Coordinate(rank, file);
            if ((rank + file) % 2 == 0) { Color = SquareColorTypes.dark; }
            else { Color = SquareColorTypes.light; }
        }
        public bool IsAttackedBy(Player player, ChessboardBackend boardState)
        {
            // TODO: implement this for checks
            // check for enemy pawns along the two diagonals
            // check for enemy bishops and queens along the four diagonals
            // check for enemy rooks and queens along the four orthogonals
            // check for enemy knights from 8 possible squares
            // check for enemy kings from adjacent spaces
            return false;
        }
    }
}