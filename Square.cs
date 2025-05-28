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

        public override string ToString()
        {
            return Position.ToString();
        }
    }
}