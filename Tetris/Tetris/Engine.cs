using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace Tetris
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public sealed class Engine : Game
    {
        private readonly Rectangle[] _blockRectangles = new Rectangle[7];

        // Graphics
        private readonly GraphicsDeviceManager _graphics;

        // Game
        private Board _board;

        private SpriteFont _gameFont;

        // Input
        private KeyboardState _oldKeyboardState = Keyboard.GetState();

        private bool _pause;
        private Score _score;
        private SpriteBatch _spriteBatch;
        private Texture2D _tetrisBackground, _tetrisTextures;

        public Engine()
        {
            _graphics = new GraphicsDeviceManager(this);
            //Content.RootDirectory = "Content";
            Content.RootDirectory = "Content";

            // Create sprite rectangles for each figure in texture file
            // O figure
            _blockRectangles[0] = new Rectangle(312, 0, 24, 24);
            // I figure
            _blockRectangles[1] = new Rectangle(0, 24, 24, 24);
            // J figure
            _blockRectangles[2] = new Rectangle(120, 0, 24, 24);
            // L figure
            _blockRectangles[3] = new Rectangle(216, 24, 24, 24);
            // S figure
            _blockRectangles[4] = new Rectangle(48, 96, 24, 24);
            // Z figure
            _blockRectangles[5] = new Rectangle(240, 72, 24, 24);
            // T figure
            _blockRectangles[6] = new Rectangle(144, 96, 24, 24);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(_tetrisBackground, Vector2.Zero, Color.White);

            base.Draw(gameTime);
            _spriteBatch.End();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Window.Title = "MonoGame XNA Tetris 2D";

            _graphics.PreferredBackBufferHeight = 600;
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.ApplyChanges();

            TargetElapsedTime = TimeSpan.FromSeconds(1.0f / 10.0f);

            // Try to open file if it exists, otherwise create it
            using (var fileStream = File.Open("record.dat", FileMode.OpenOrCreate))
            {
                fileStream.Close();
            }

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Add the SpriteBatch service
            Services.AddService(typeof(SpriteBatch), _spriteBatch);

            //Load 2D textures
            _tetrisBackground = Content.Load<Texture2D>("background");
            _tetrisTextures = Content.Load<Texture2D>("tetris");

            // Load game font
            //gameFont = Content.Load<SpriteFont> ("font");
            _gameFont = Content.Load<SpriteFont>("Arial");

            // Create game field
            _board = new Board(this, ref _tetrisTextures, _blockRectangles, 0.1f);
            _board.Initialize();
            Components.Add(_board);

            // Save player's score and game level
            _score = new Score(this, _gameFont);
            _score.Initialize();
            Components.Add(_score);

            // Load game record
            using (var streamReader = File.OpenText("record.dat"))
            {
                string player;
                if ((player = streamReader.ReadLine()) != null)
                    _score.RecordPlayer = player;
                int record;
                if ((record = Convert.ToInt32(streamReader.ReadLine())) != 0)
                    _score.RecordScore = record;
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            // Check pause
            bool pauseKey = (_oldKeyboardState.IsKeyDown(Keys.P) && (keyboardState.IsKeyUp(Keys.P)));

            _oldKeyboardState = keyboardState;

            if (pauseKey)
                _pause = !_pause;

            if (!_pause)
            {
                // Find dynamic figure position
                _board.FindDynamicFigure();

                // Increase player score
                int lines = _board.DestroyLines();
                if (lines > 0)
                {
                    _score.Value += (int)((5.0f / 2.0f) * lines * (lines + 3));
                    _board.Speed += 0.005f;
                }

                _score.Level = (int)(10 * _board.Speed);

                // Create new shape in game
                if (!_board.CreateNewFigure())
                    GameOver();
                else
                {
                    // If left key is pressed
                    if (keyboardState.IsKeyDown(Keys.Left))
                        _board.MoveFigureLeft();
                    // If right key is pressed
                    if (keyboardState.IsKeyDown(Keys.Right))
                        _board.MoveFigureRight();
                    // If down key is pressed
                    if (keyboardState.IsKeyDown(Keys.Down))
                        _board.MoveFigureDown();

                    // Rotate figure
                    if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.Space))
                        _board.RotateFigure();

                    // Moving figure
                    if (_board.Movement >= 1)
                    {
                        _board.Movement = 0;
                        _board.MoveFigureDown();
                    }
                    else
                        _board.Movement += _board.Speed;
                }
            }

            base.Update(gameTime);
        }

        private void GameOver()
        {
            if (_score.Value > _score.RecordScore)
            {
                _score.RecordScore = _score.Value;

                _pause = true;

                var record = new Record("Player Name");
                //record.ShowDialog ();

                _score.RecordPlayer = record.Player;

                using (var writer = File.CreateText("record.dat"))
                {
                    writer.WriteLine(_score.RecordPlayer);
                    writer.WriteLine(_score.RecordScore);
                }

                _pause = false;
            }
            _board.Initialize();
            _score.Initialize();
        }
    }
}