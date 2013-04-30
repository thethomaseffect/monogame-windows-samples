using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Tetris
{
    internal sealed class Board : DrawableGameComponent
    {
        private const int BlocksCountInFigure = 4;
        private const int Height = 20;
        private const int Width = 10;
        private readonly int[,] _boardColor;
        private readonly FieldState[,] _boardFields;
        private readonly float _epsilon;
        private readonly Vector2[, ,] _figures;
        private readonly Queue<int> _nextFigures = new Queue<int>();
        private readonly Queue<int> _nextFiguresModification = new Queue<int>();
        private readonly Random _random = new Random();
        private readonly Rectangle[] _rectangles;
        private readonly SpriteBatch _sBatch;
        private readonly Vector2 _startPositionForNewFigure = new Vector2(3, 0);
        private readonly Texture2D _textures;
        private bool _blockLine;

        private Vector2[] _dynamicFigure = new Vector2[BlocksCountInFigure];

        private int _dynamicFigureColor;

        private int _dynamicFigureModificationNumber;

        private int _dynamicFigureNumber;

        private float _movement;

        private Vector2 _positionForDynamicFigure;

        private bool _showNewBlock;

        private float _speed;

        public Board(Game game, ref Texture2D textures, Rectangle[] rectangles, float epsilon)
            : base(game)
        {
            _sBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));

            // Load textures for blocks
            _textures = textures;

            // Rectangles to draw figures
            _rectangles = rectangles;
            _epsilon = epsilon;

            // Create tetris board
            _boardFields = new FieldState[Width, Height];
            _boardColor = new int[Width, Height];

            #region Creating figures

            // Figures[figure's number, figure's modification, figure's block number] = Vector2
            // At all figures is 7, every has 4 modifications (for cube all modifications the same)
            // and every figure consists from 4 blocks
            _figures = new Vector2[7, 4, 4];
            // O-figure
            for (var i = 0; i < 4; i++)
            {
                _figures[0, i, 0] = new Vector2(1, 0);
                _figures[0, i, 1] = new Vector2(2, 0);
                _figures[0, i, 2] = new Vector2(1, 1);
                _figures[0, i, 3] = new Vector2(2, 1);
            }
            // I-figures
            for (var i = 0; i < 4; i += 2)
            {
                _figures[1, i, 0] = new Vector2(0, 0);
                _figures[1, i, 1] = new Vector2(1, 0);
                _figures[1, i, 2] = new Vector2(2, 0);
                _figures[1, i, 3] = new Vector2(3, 0);
                _figures[1, i + 1, 0] = new Vector2(1, 0);
                _figures[1, i + 1, 1] = new Vector2(1, 1);
                _figures[1, i + 1, 2] = new Vector2(1, 2);
                _figures[1, i + 1, 3] = new Vector2(1, 3);
            }
            // J-figures
            _figures[2, 0, 0] = new Vector2(0, 0);
            _figures[2, 0, 1] = new Vector2(1, 0);
            _figures[2, 0, 2] = new Vector2(2, 0);
            _figures[2, 0, 3] = new Vector2(2, 1);
            _figures[2, 1, 0] = new Vector2(2, 0);
            _figures[2, 1, 1] = new Vector2(2, 1);
            _figures[2, 1, 2] = new Vector2(1, 2);
            _figures[2, 1, 3] = new Vector2(2, 2);
            _figures[2, 2, 0] = new Vector2(0, 0);
            _figures[2, 2, 1] = new Vector2(0, 1);
            _figures[2, 2, 2] = new Vector2(1, 1);
            _figures[2, 2, 3] = new Vector2(2, 1);
            _figures[2, 3, 0] = new Vector2(1, 0);
            _figures[2, 3, 1] = new Vector2(2, 0);
            _figures[2, 3, 2] = new Vector2(1, 1);
            _figures[2, 3, 3] = new Vector2(1, 2);
            // L-figures
            _figures[3, 0, 0] = new Vector2(0, 0);
            _figures[3, 0, 1] = new Vector2(1, 0);
            _figures[3, 0, 2] = new Vector2(2, 0);
            _figures[3, 0, 3] = new Vector2(0, 1);
            _figures[3, 1, 0] = new Vector2(2, 0);
            _figures[3, 1, 1] = new Vector2(2, 1);
            _figures[3, 1, 2] = new Vector2(1, 0);
            _figures[3, 1, 3] = new Vector2(2, 2);
            _figures[3, 2, 0] = new Vector2(0, 1);
            _figures[3, 2, 1] = new Vector2(1, 1);
            _figures[3, 2, 2] = new Vector2(2, 1);
            _figures[3, 2, 3] = new Vector2(2, 0);
            _figures[3, 3, 0] = new Vector2(1, 0);
            _figures[3, 3, 1] = new Vector2(2, 2);
            _figures[3, 3, 2] = new Vector2(1, 1);
            _figures[3, 3, 3] = new Vector2(1, 2);
            // S-figures
            for (int i = 0; i < 4; i += 2)
            {
                _figures[4, i, 0] = new Vector2(0, 1);
                _figures[4, i, 1] = new Vector2(1, 1);
                _figures[4, i, 2] = new Vector2(1, 0);
                _figures[4, i, 3] = new Vector2(2, 0);
                _figures[4, i + 1, 0] = new Vector2(1, 0);
                _figures[4, i + 1, 1] = new Vector2(1, 1);
                _figures[4, i + 1, 2] = new Vector2(2, 1);
                _figures[4, i + 1, 3] = new Vector2(2, 2);
            }
            // Z-figures
            for (int i = 0; i < 4; i += 2)
            {
                _figures[5, i, 0] = new Vector2(0, 0);
                _figures[5, i, 1] = new Vector2(1, 0);
                _figures[5, i, 2] = new Vector2(1, 1);
                _figures[5, i, 3] = new Vector2(2, 1);
                _figures[5, i + 1, 0] = new Vector2(2, 0);
                _figures[5, i + 1, 1] = new Vector2(1, 1);
                _figures[5, i + 1, 2] = new Vector2(2, 1);
                _figures[5, i + 1, 3] = new Vector2(1, 2);
            }
            // T-figures
            _figures[6, 0, 0] = new Vector2(0, 1);
            _figures[6, 0, 1] = new Vector2(1, 1);
            _figures[6, 0, 2] = new Vector2(2, 1);
            _figures[6, 0, 3] = new Vector2(1, 0);
            _figures[6, 1, 0] = new Vector2(1, 0);
            _figures[6, 1, 1] = new Vector2(1, 1);
            _figures[6, 1, 2] = new Vector2(1, 2);
            _figures[6, 1, 3] = new Vector2(2, 1);
            _figures[6, 2, 0] = new Vector2(0, 0);
            _figures[6, 2, 1] = new Vector2(1, 0);
            _figures[6, 2, 2] = new Vector2(2, 0);
            _figures[6, 2, 3] = new Vector2(1, 1);
            _figures[6, 3, 0] = new Vector2(2, 0);
            _figures[6, 3, 1] = new Vector2(2, 1);
            _figures[6, 3, 2] = new Vector2(2, 2);
            _figures[6, 3, 3] = new Vector2(1, 1);

            #endregion Creating figures

            _nextFigures.Enqueue(_random.Next(7));
            _nextFigures.Enqueue(_random.Next(7));
            _nextFigures.Enqueue(_random.Next(7));
            _nextFigures.Enqueue(_random.Next(7));

            _nextFiguresModification.Enqueue(_random.Next(4));
            _nextFiguresModification.Enqueue(_random.Next(4));
            _nextFiguresModification.Enqueue(_random.Next(4));
            _nextFiguresModification.Enqueue(_random.Next(4));
        }

        private enum FieldState
        {
            Free,
            Static,
            Dynamic
        };

        public float Movement
        {
            set { _movement = value; }
            get { return _movement; }
        }

        public float Speed
        {
            set { _speed = value; }
            get { return _speed; }
        }

        /// <summary>
        /// Create new shape in the game, if need it
        /// </summary>
        public bool CreateNewFigure()
        {
            if (_showNewBlock)
            {
                // Generate new figure's shape
                _dynamicFigureNumber = _nextFigures.Dequeue();
                _nextFigures.Enqueue(_random.Next(7));

                _dynamicFigureModificationNumber = _nextFiguresModification.Dequeue();
                _nextFiguresModification.Enqueue(_random.Next(4));

                _dynamicFigureColor = _dynamicFigureNumber;

                // Position and coordinates for new dynamic figure
                _positionForDynamicFigure = _startPositionForNewFigure;
                for (int i = 0; i < BlocksCountInFigure; i++)
                    _dynamicFigure[i] = _figures[_dynamicFigureNumber, _dynamicFigureModificationNumber, i] +
                    _positionForDynamicFigure;

                if (!DrawFigureOnBoard(_dynamicFigure, _dynamicFigureColor))
                    return false;

                _showNewBlock = false;
            }
            return true;
        }

        /// <summary>
        /// Find, destroy and save lines's count
        /// </summary>
        /// <returns>Number of destoyed lines</returns>
        public int DestroyLines()
        {
            // Find total lines
            int blockLineCount = 0;
            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                    if (_boardFields[i, j] == FieldState.Static)
                        _blockLine = true;
                    else
                    {
                        _blockLine = false;
                        break;
                    }
                //Destroy total lines
                if (_blockLine)
                {
                    // Save number of total lines
                    blockLineCount++;
                    for (var l = j; l > 0; l--)
                        for (var k = 0; k < Width; k++)
                        {
                            _boardFields[k, l] = _boardFields[k, l - 1];
                            _boardColor[k, l] = _boardColor[k, l - 1];
                        }
                    for (var l = 0; l < Width; l++)
                    {
                        _boardFields[l, 0] = FieldState.Free;
                        _boardColor[l, 0] = -1;
                    }
                }
            }
            return blockLineCount;
        }

        public override void Draw(GameTime gameTime)
        {
            Vector2 startPosition;
            // Draw the blocks
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    if (_boardFields[i, j] != FieldState.Free)
                    {
                        startPosition = new Vector2((10 + i) * _rectangles[0].Width,
                (2 + j) * _rectangles[0].Height);
                        _sBatch.Draw(_textures, startPosition, _rectangles[_boardColor[i, j]], Color.White);
                    }

            // Draw next figures
            Queue<int>.Enumerator figure = _nextFigures.GetEnumerator();
            Queue<int>.Enumerator modification = _nextFiguresModification.GetEnumerator();
            for (int i = 0; i < _nextFigures.Count; i++)
            {
                figure.MoveNext();
                modification.MoveNext();
                for (int j = 0; j < BlocksCountInFigure; j++)
                {
                    startPosition = _rectangles[0].Height * (new Vector2(24, 3 + 5 * i) +
            _figures[figure.Current, modification.Current, j]);
                    _sBatch.Draw(_textures, startPosition,
            _rectangles[figure.Current], Color.White);
                }
            }

            base.Draw(gameTime);
        }

        public void FindDynamicFigure()
        {
            var blockNumberInDynamicFigure = 0;
            for (var i = 0; i < Width; i++)
                for (var j = 0; j < Height; j++)
                    if (_boardFields[i, j] == FieldState.Dynamic)
                        _dynamicFigure[blockNumberInDynamicFigure++] = new Vector2(i, j);
        }

        public override void Initialize()
        {
            _showNewBlock = true;
            _movement = 0;
            _speed = 0.1f;

            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    ClearBoardField(i, j);

            base.Initialize();
        }

        public void MoveFigureDown()
        {
            // Sorting blocks fro dynamic figure to correct moving
            SortingVector2(ref _dynamicFigure, false, _dynamicFigure.GetLowerBound(0), _dynamicFigure.GetUpperBound(0));
            // Check colisions
            for (int i = 0; i < BlocksCountInFigure; i++)
            {
                if ((Math.Abs(_dynamicFigure[i].Y - (Height - 1)) < _epsilon))
                {
                    for (int k = 0; k < BlocksCountInFigure; k++)
                        _boardFields[(int)_dynamicFigure[k].X, (int)_dynamicFigure[k].Y] = FieldState.Static;
                    _showNewBlock = true;
                    return;
                }
                if (_boardFields[(int)_dynamicFigure[i].X, (int)_dynamicFigure[i].Y + 1] == FieldState.Static)
                {
                    for (int k = 0; k < BlocksCountInFigure; k++)
                        _boardFields[(int)_dynamicFigure[k].X, (int)_dynamicFigure[k].Y] = FieldState.Static;
                    _showNewBlock = true;
                    return;
                }
            }
            // Move figure on board
            for (int i = BlocksCountInFigure - 1; i >= 0; i--)
            {
                _boardFields[(int)_dynamicFigure[i].X, (int)_dynamicFigure[i].Y + 1] =
            _boardFields[(int)_dynamicFigure[i].X, (int)_dynamicFigure[i].Y];
                _boardColor[(int)_dynamicFigure[i].X, (int)_dynamicFigure[i].Y + 1] =
            _boardColor[(int)_dynamicFigure[i].X, (int)_dynamicFigure[i].Y];
                ClearBoardField((int)_dynamicFigure[i].X, (int)_dynamicFigure[i].Y);
                // Change position for blocks in DynamicFigure
                _dynamicFigure[i].Y = _dynamicFigure[i].Y + 1;
            }
            // Change position vector
            //if (PositionForDynamicFigure.Y < height - 1)
            _positionForDynamicFigure.Y++;
        }

        public void MoveFigureLeft()
        {
            // Sorting blocks fro dynamic figure to correct moving
            SortingVector2(ref _dynamicFigure, true, _dynamicFigure.GetLowerBound(0), _dynamicFigure.GetUpperBound(0));
            // Check colisions
            for (int i = 0; i < BlocksCountInFigure; i++)
            {
                if ((Math.Abs(_dynamicFigure[i].X - 0) < _epsilon))
                    return;
                if (_boardFields[(int)_dynamicFigure[i].X - 1, (int)_dynamicFigure[i].Y] == FieldState.Static)
                    return;
            }
            // Move figure on board
            for (int i = 0; i < BlocksCountInFigure; i++)
            {
                _boardFields[(int)_dynamicFigure[i].X - 1, (int)_dynamicFigure[i].Y] =
            _boardFields[(int)_dynamicFigure[i].X, (int)_dynamicFigure[i].Y];
                _boardColor[(int)_dynamicFigure[i].X - 1, (int)_dynamicFigure[i].Y] =
            _boardColor[(int)_dynamicFigure[i].X, (int)_dynamicFigure[i].Y];
                ClearBoardField((int)_dynamicFigure[i].X, (int)_dynamicFigure[i].Y);
                // Change position for blocks in DynamicFigure
                _dynamicFigure[i].X = _dynamicFigure[i].X - 1;
            }
            // Change position vector
            //if (PositionForDynamicFigure.X > 0)
            _positionForDynamicFigure.X--;
        }

        public void MoveFigureRight()
        {
            // Sorting blocks fro dynamic figure to correct moving
            SortingVector2(ref _dynamicFigure, true, _dynamicFigure.GetLowerBound(0), _dynamicFigure.GetUpperBound(0));
            // Check colisions
            for (int i = 0; i < BlocksCountInFigure; i++)
            {
                if ((Math.Abs(_dynamicFigure[i].X - (Width - 1)) < _epsilon))
                    return;
                if (_boardFields[(int)_dynamicFigure[i].X + 1, (int)_dynamicFigure[i].Y] == FieldState.Static)
                    return;
            }
            // Move figure on board
            for (int i = BlocksCountInFigure - 1; i >= 0; i--)
            {
                _boardFields[(int)_dynamicFigure[i].X + 1, (int)_dynamicFigure[i].Y] =
            _boardFields[(int)_dynamicFigure[i].X, (int)_dynamicFigure[i].Y];
                _boardColor[(int)_dynamicFigure[i].X + 1, (int)_dynamicFigure[i].Y] =
            _boardColor[(int)_dynamicFigure[i].X, (int)_dynamicFigure[i].Y];
                ClearBoardField((int)_dynamicFigure[i].X, (int)_dynamicFigure[i].Y);
                // Change position for blocks in DynamicFigure
                _dynamicFigure[i].X = _dynamicFigure[i].X + 1;
            }
            // Change position vector
            //if (PositionForDynamicFigure.X < width - 1)
            _positionForDynamicFigure.X++;
        }

        public void RotateFigure()
        {
            // Check colisions for next modification
            var testDynamicFigure = new Vector2[_dynamicFigure.GetUpperBound(0) + 1];
            for (int i = 0; i < BlocksCountInFigure; i++)
                testDynamicFigure[i] = _figures[_dynamicFigureNumber, (_dynamicFigureModificationNumber + 1) % 4, i] + _positionForDynamicFigure;

            // Make sure that figure can rotate if she stand near left and right borders
            SortingVector2(ref testDynamicFigure, true, testDynamicFigure.GetLowerBound(0), testDynamicFigure.GetUpperBound(0));
            int leftFigureBound;
            int rightFigureBound;
            if ((leftFigureBound = (int)testDynamicFigure[0].X) < 0)
            {
                //int leftFigureBound = (int)TestDynamicFigure[0].X;
                for (int i = 0; i < BlocksCountInFigure; i++)
                {
                    testDynamicFigure[i] += new Vector2(0 - leftFigureBound, 0);
                }
                if (TryPlaceFigureOnBoard(testDynamicFigure))
                    _positionForDynamicFigure +=
            new Vector2(0 - leftFigureBound, 0);
            }
            if ((rightFigureBound = (int)testDynamicFigure[BlocksCountInFigure - 1].X) >= Width)
            {
                //int rightFigureBound = (int)TestDynamicFigure[BlocksCountInFigure - 1].X;
                for (int i = 0; i < BlocksCountInFigure; i++)
                {
                    testDynamicFigure[i] -= new Vector2(rightFigureBound - Width + 1, 0);
                }
                if (TryPlaceFigureOnBoard(testDynamicFigure))
                    _positionForDynamicFigure -=
            new Vector2(rightFigureBound - Width + 1, 0);
            }

            if (TryPlaceFigureOnBoard(testDynamicFigure))
            {
                _dynamicFigureModificationNumber = (_dynamicFigureModificationNumber + 1) % 4;
                // Clear dynamic fields
                for (int i = 0; i <= _dynamicFigure.GetUpperBound(0); i++)
                    ClearBoardField((int)_dynamicFigure[i].X, (int)_dynamicFigure[i].Y);
                _dynamicFigure = testDynamicFigure;
                for (int i = 0; i <= _dynamicFigure.GetUpperBound(0); i++)
                {
                    _boardFields[(int)_dynamicFigure[i].X, (int)_dynamicFigure[i].Y] = FieldState.Dynamic;
                    _boardColor[(int)_dynamicFigure[i].X, (int)_dynamicFigure[i].Y] = _dynamicFigureColor;
                }
            }
        }

        private void ClearBoardField(int i, int j)
        {
            _boardFields[i, j] = FieldState.Free;
            _boardColor[i, j] = -1;
        }

        private bool DrawFigureOnBoard(Vector2[] vector, int color)
        {
            if (!TryPlaceFigureOnBoard(vector))
                return false;
            for (var i = 0; i <= vector.GetUpperBound(0); i++)
            {
                _boardFields[(int)vector[i].X, (int)vector[i].Y] = FieldState.Dynamic;
                _boardColor[(int)vector[i].X, (int)vector[i].Y] = color;
            }
            return true;
        }

        private void SortingVector2(ref Vector2[] vector, bool sortByX, int a, int b)
        {
            if (a >= b)
                return;
            int i = a;
            for (int j = a; j <= b; j++)
            {
                if (sortByX)
                {
                    if (vector[j].X <= vector[b].X)
                    {
                        Vector2 tempVector = vector[i];
                        vector[i] = vector[j];
                        vector[j] = tempVector;
                        i++;
                    }
                }
                else
                {
                    if (vector[j].Y <= vector[b].Y)
                    {
                        Vector2 tempVector = vector[i];
                        vector[i] = vector[j];
                        vector[j] = tempVector;
                        i++;
                    }
                }
            }
            int c = i - 1;
            SortingVector2(ref vector, sortByX, a, c - 1);
            SortingVector2(ref vector, sortByX, c + 1, b);
        }

        private bool TryPlaceFigureOnBoard(Vector2[] vector)
        {
            for (int i = 0; i <= vector.GetUpperBound(0); i++)
                if ((vector[i].X < 0) || (vector[i].X >= Width) ||
            (vector[i].Y >= Height))
                    return false;
            for (int i = 0; i <= vector.GetUpperBound(0); i++)
                if (_boardFields[(int)vector[i].X, (int)vector[i].Y] == FieldState.Static)
                    return false;
            return true;
        }
    }
}