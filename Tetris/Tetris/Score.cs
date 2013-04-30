using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tetris
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public sealed class Score : DrawableGameComponent
    {
        private readonly SpriteFont _font;

        // Graphic
        private readonly SpriteBatch _sBatch;

        private int _level;

        private string _recordPlayer = "Player 1";

        private int _recordScore;

        // Counters
        private int _value;

        public Score(Game game, SpriteFont font)
            : base(game)
        {
            _sBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
            _font = font;
        }

        public int Level
        {
            set { _level = value; }
        }

        public string RecordPlayer
        {
            set { _recordPlayer = value; }
            get { return _recordPlayer; }
        }

        public int RecordScore
        {
            set { _recordScore = value; }
            get { return _recordScore; }
        }

        public int Value
        {
            set { _value = value; }
            get { return _value; }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            _sBatch.DrawString(_font, "Score:\n" + _value + "\nLevel: " + _level, new Vector2(1.5f * 24, 3 * 24), Color.Green);

            _sBatch.DrawString(_font, "Record:\n" + _recordPlayer + "\n" + _recordScore, new Vector2(1.5f * 24, 13 * 24), Color.Orange);

            base.Draw(gameTime);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            _value = 0;
            _level = 1;
            base.Initialize();
        }
    }
}