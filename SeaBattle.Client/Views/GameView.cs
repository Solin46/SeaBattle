using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SeaBattle.Client.Controllers;
using SeaBattle.Common.Game;
using SeaBattle.Common.Enums;

namespace SeaBattle.Client.Views
{
    public partial class GameView : UserControl
    {
        private readonly GameController _controller;

        private Button[,] _myField = new Button[10, 10];
        private Button[,] _enemyField = new Button[10, 10];

        public event Action GameFinished;

        public GameView(GameController controller)
        {
            InitializeComponent();
            _controller = controller;
            BuildGrids();
            DrawMyBoard();
            SetupEnemyBoardForTest();
        }

        private void BuildGrids()
        {
            const int cellSize = 30;
            int offset = 350; // смещение для поля врага

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    // Мое поле
                    var myBtn = new Button
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Left = x * cellSize,
                        Top = y * cellSize,
                        Enabled = false
                    };
                    this.Controls.Add(myBtn);
                    _myField[x, y] = myBtn;

                    // Поле врага
                    var enemyBtn = new Button
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Left = offset + x * cellSize,
                        Top = y * cellSize,
                        Tag = (x, y)
                    };
                    enemyBtn.Click += EnemyCell_Click;
                    this.Controls.Add(enemyBtn);
                    _enemyField[x, y] = enemyBtn;
                }
            }
        }

        private void DrawMyBoard()
        {
            foreach (var ship in _controller.State.MyBoard.Ships)
            {
                foreach (var (x, y) in ship.Decks)
                {
                    _myField[x, y].BackColor = Color.Gray;
                }
            }
        }

        // ======================
        // Для теста: копируем свои корабли во врага
        // ======================
        private void SetupEnemyBoardForTest()
        {
            foreach (var ship in _controller.State.MyBoard.Ships)
            {
                var copyDecks = new List<(int X, int Y)>(ship.Decks);
                _controller.State.EnemyBoard.PlaceShip(new Ship(copyDecks));
            }
        }

        // ======================
        // ЛКМ по полю врага
        // ======================
        private void EnemyCell_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var (x, y) = ((int, int))btn.Tag;

            var result = _controller.ShootAtEnemy(x, y);

            switch (result)
            {
                case ShotResult.Miss:
                    btn.BackColor = Color.LightBlue;
                    break;
                case ShotResult.Hit:
                    btn.BackColor = Color.OrangeRed;
                    break;
                case ShotResult.Sunk:
                    btn.BackColor = Color.Red;
                    break;
            }

            // Проверка победы
            if (_controller.State.Phase == GamePhase.Finished)
            {
                GameFinished?.Invoke();
            }
        }
    }
}
