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
            //foreach (Piece piece in boardState.Pieces)
            //{
            //    bool colorMatches = (piece.Color == PieceColor.White && player == Player.White) || (piece.Color == PieceColor.Black && player == Player.Black);
            //    if (colorMatches)
            //    {
            //        Move move = new(piece, Position);
            //        if (move.IsValidMove(boardState,justChecking:true)) { return true; }
            //    }
            //}
            return false;
        }
    }
}