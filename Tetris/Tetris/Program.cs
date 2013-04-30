using System;

namespace Tetris
{
    internal static class Program
    {
        private static Engine _game;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            _game = new Engine();
            _game.Run();
        }
    }
}