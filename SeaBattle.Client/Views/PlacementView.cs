using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SeaBattle.Client.Controllers;
using SeaBattle.Common.Game;

namespace SeaBattle.Client.Views
{
    public partial class PlacementView : UserControl
    {
        private readonly GameController _controller;
        private readonly Button[,] _cells = new Button[GameBoard.Size, GameBoard.Size];

        public event Action PlacementFinished;

        public PlacementView(GameController controller)
        {
            InitializeComponent();
            _controller = controller;

            BuildGrid();
            ConfigureShipsCounterLabel();
            UpdateShipsCounter();
        }

        // =========================
        // НАСТРОЙКА LABEL
        // =========================
        private void ConfigureShipsCounterLabel()
        {
            _shipsCounterLabel.AutoSize = false;
            _shipsCounterLabel.Size = new Size(150, 120); // подбираем по высоте
            _shipsCounterLabel.TextAlign = ContentAlignment.TopLeft;
            _shipsCounterLabel.Font = new Font("Segoe UI", 10);
            _shipsCounterLabel.BorderStyle = BorderStyle.FixedSingle;
        }

        // =========================
        // СОЗДАНИЕ СЕТКИ
        // =========================
        private void BuildGrid()
        {
            const int cellSize = 30;

            for (int y = 0; y < GameBoard.Size; y++)
            {
                for (int x = 0; x < GameBoard.Size; x++)
                {
                    var btn = new Button
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Left = x * cellSize,
                        Top = y * cellSize,
                        Tag = (x, y),
                        BackColor = Color.White
                    };

                    btn.Click += Cell_Click;
                    btn.MouseUp += Cell_RightClick;

                    _gridPanel.Controls.Add(btn);
                    _cells[x, y] = btn;
                }
            }
        }

        // =========================
        // ЛКМ — ПОПЫТКА ПОСТАВИТЬ КОРАБЛЬ
        // =========================
        private void Cell_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var (x, y) = ((int, int))btn.Tag;

            int size = (int)_shipSizeCombo.SelectedItem;
            bool horizontal = _orientationButton.Text == "→";

            var decks = new List<(int X, int Y)>();
            for (int i = 0; i < size; i++)
            {
                int nx = horizontal ? x + i : x;
                int ny = horizontal ? y : y + i;

                if (nx >= GameBoard.Size || ny >= GameBoard.Size)
                    return;

                decks.Add((nx, ny));
            }

            var ship = new Ship(decks);

            if (_controller.PlaceShip(ship))
            {
                DrawShip(ship);
                UpdateShipsCounter();
            }
            else
            {
                MessageBox.Show("Нельзя поставить корабль здесь или превышен лимит по размеру!");
            }
        }

        // =========================
        // ПКМ — УДАЛЕНИЕ КОРАБЛЯ
        // =========================
        private void Cell_RightClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            var btn = (Button)sender;
            var (x, y) = ((int, int))btn.Tag;

            _controller.RemoveShipAt(x, y);
            RedrawAllShips();
            UpdateShipsCounter();
        }

        // =========================
        // ОТРИСОВКА
        // =========================
        private void DrawShip(Ship ship)
        {
            foreach (var (x, y) in ship.Decks)
                _cells[x, y].BackColor = Color.Gray;
        }

        private void ClearShip(Ship ship)
        {
            foreach (var (x, y) in ship.Decks)
                _cells[x, y].BackColor = Color.White;
        }

        // =========================
        // ПЕРЕРИСОВКА ВСЕХ КОРАБЛЕЙ
        // =========================
        private void RedrawAllShips()
        {
            for (int y = 0; y < GameBoard.Size; y++)
                for (int x = 0; x < GameBoard.Size; x++)
                    _cells[x, y].BackColor = Color.White;

            foreach (var ship in _controller.State.MyBoard.Ships)
                DrawShip(ship);
        }

        // =========================
        // СЧЁТЧИК КОРАБЛЕЙ ПО РАЗМЕРУ
        // =========================
        private void UpdateShipsCounter()
        {
            var counts = _controller.State.MyBoard.Ships
                .GroupBy(s => s.Decks.Count)
                .ToDictionary(g => g.Key, g => g.Count());

            string text = "Корабли:\n";
            foreach (var size in _controller.State.MyBoard.ShipLimits.Keys.OrderByDescending(k => k))
            {
                int placed = counts.ContainsKey(size) ? counts[size] : 0;
                int total = _controller.State.MyBoard.ShipLimits[size];
                text += $"{size}-палубные: {placed} / {total}\n";
            }

            _shipsCounterLabel.Text = text;
        }

        // =========================
        // КНОПКА "ГОТОВО"
        // =========================
        private void DoneButton_Click(object sender, EventArgs e)
        {
            if (_controller.AllShipsPlaced())
            {
                PlacementFinished?.Invoke();
            }
            else
            {
                MessageBox.Show("Расставьте все корабли по правилам.");
            }
        }
    }
}
