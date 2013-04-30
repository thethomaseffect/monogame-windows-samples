namespace Tetris
{
    public sealed class Record
    {
        public Record(string player)
        {
            Player = player;
            //InitializeComponent ();
        }

        internal string Player { get; private set; }
    }
}