using SeaBattle.Client.Controllers;
using SeaBattle.Client.Networking;
using SeaBattle.Common.Networking;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SeaBattle.Client.Views
{
    public partial class GameView : UserControl
    {
        private readonly GameController _controller;
        private readonly GameClient _client;

        private readonly Button[,] _myField = new Button[10, 10];
        private readonly Button[,] _enemyField = new Button[10, 10];

        private enum ViewCellState
        {
            Unknown,
            Miss,
            Hit,
            Sunk
        }
        private ViewCellState[,] _myView = new ViewCellState[10, 10];
        private ViewCellState[,] _enemyView = new ViewCellState[10, 10];

        private bool _myTurn;
        public bool IsPlayer1 { get; set; }
        private bool _roleReceived;
        private Label _turnLabel;

        public event Action<NetworkMessage> ServerMessageForwarded;

        public event Action GameFinished;

        public GameView(GameController controller, GameClient client)
        {
            _controller = controller;
            _client = client;

            BuildFields();

            // Подписка на серверные сообщения
            _client.MessageReceived += OnServerMessage;
        }

        private void BuildFields()
        {
            const int size = 30;
            const int board = 10;
            const int topOffset = 20;
            const int enemyOffset = 340;

            this.Width = enemyOffset + size * board + 20;
            this.Height = topOffset + size * board + 20;


            // Мое поле
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    var btn = new Button
                    {
                        Width = size,
                        Height = size,
                        Left = x * size,
                        Top = y * size + topOffset,
                        BackColor = Color.White,
                        Tag = (x, y)
                    };
                    _myField[x, y] = btn;
                    Controls.Add(btn);
                }
            }

            // Вражеское поле
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    var btn = new Button
                    {
                        Width = size,
                        Height = size,
                        Left = x * size + enemyOffset,
                        Top = y * size + topOffset,
                        BackColor = Color.White,
                        Tag = (x, y)
                    };
                    btn.Click += EnemyCell_Click;
                    _enemyField[x, y] = btn;
                    Controls.Add(btn);
                }
            }
            _turnLabel = new Label
            {
                AutoSize = true,
                Left = 20,
                Top = 0,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Text = "Ожидание начала игры..."
            };

            Controls.Add(_turnLabel);


        }

        private void EnemyCell_Click(object sender, EventArgs e)
        {
            if (!_myTurn)
                return;

            var btn = (Button)sender;
            var (x, y) = ((int, int))btn.Tag;

            // ❗ если уже стреляли — ничего не делаем
            if (_enemyView[x, y] != ViewCellState.Unknown)
                return;

            // Отправляем сообщение на сервер
            var msg = new NetworkMessage(NetworkCommand.Shoot, $"{x},{y}");
            _client.Send(msg);

            //временно
            Console.WriteLine($"Click. MyTurn={_myTurn}");

        }

        private void OnServerMessage(NetworkMessage msg)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => HandleServerMessage(msg)));
            }
            else
            {
                HandleServerMessage(msg);
            }
        }

        private void HandleServerMessage(NetworkMessage msg)
        {
            switch (msg.Command)
            {
                case NetworkCommand.PlayerRole:
                    {
                        IsPlayer1 = msg.Payload == "player1";
                        _roleReceived = true;
                        break;
                    }

                case NetworkCommand.YourTurn:
                    {
                        _myTurn = msg.Payload == "true";

                        _turnLabel.Text = _myTurn ? "👉 Ваш ход" : "⏳ Ход противника";

                        _turnLabel.ForeColor = _myTurn ? Color.Green : Color.Gray;

                        //временно
                        Console.WriteLine("MY TURN = " + _myTurn);

                        break;
                    }


                case NetworkCommand.GameStart:
                    {
                        /*if (!_roleReceived) //чтобы не было рассинхрона с gamestart до получения роли
                            return;

                        string first = msg.Payload;

                        _myTurn =
                            (IsPlayer1 && first == "player1") ||
                            (!IsPlayer1 && first == "player2");*/

                        break;
                    }


                //апдейт клетки по результату выстрела
                case NetworkCommand.ShotResult:
                    {
                        var parts = msg.Payload.Split(',');

                        int x = int.Parse(parts[0]);
                        int y = int.Parse(parts[1]);
                        string result = parts[2].ToLower();

                        if (result == "miss")
                        {
                            UpdateEnemyCell(x, y, ViewCellState.Miss);
                            //_myTurn = false;
                            break;
                        }

                        if (result == "hit")
                        {
                            UpdateEnemyCell(x, y, ViewCellState.Hit);
                            break;
                        }

                        if (result == "sunk")
                        {
                            // parts[3] = "x1:y1|x2:y2|x3:y3"
                            var shipCells = parts[3].Split('|');

                            foreach (var cell in shipCells)
                            {
                                var xy = cell.Split(':');
                                int sx = int.Parse(xy[0]);
                                int sy = int.Parse(xy[1]);

                                UpdateEnemyCell(sx, sy, ViewCellState.Sunk);
                            }

                            break;
                        }

                        break;
                    }

                case NetworkCommand.EnemyShot:
                    {
                        var parts = msg.Payload.Split(',');
                        int x = int.Parse(parts[0]);
                        int y = int.Parse(parts[1]);
                        string result = parts[2];

                        _myField[x, y].BackColor =
                            result == "hit" || result == "sunk"
                                ? Color.Red
                                : Color.Blue;
                        break;
                    }

                case NetworkCommand.GameOver:
                    {
                        /*string winner = msg.Payload; // "player1" / "player2"

                        bool iWon =
                            (IsPlayer1 && winner == "player1") ||
                            (!IsPlayer1 && winner == "player2");

                        MessageBox.Show(
                            iWon ? "🎉 Вы победили!" : "😢 Вы проиграли",
                            "Игра окончена"
                        );

                        GameFinished?.Invoke();*/

                        // ТОЛЬКО передаем сообщение дальше, не обрабатываем сами
                        ServerMessageForwarded?.Invoke(msg);
                        break;
                    }


                default:
                    // остальные команды пока игнорируем
                    break;
            }
        }


        //
        public void DrawMyShips((int x, int y)[] shipCoords)
        {
            foreach (var (x, y) in shipCoords)
            {
                _myField[x, y].BackColor = Color.Gray; // отображение своих кораблей
            }
        }

        // Обработка ответа на СВОЙ выстрел
        public void HandleShotResult(string payload)
        {
            var parts = payload.Split(',');
            int x = int.Parse(parts[0]);
            int y = int.Parse(parts[1]);
            string result = parts[2];

            // Красим клетку на вражеском поле
            _enemyField[x, y].BackColor =
                result.ToLower() == "hit" ? Color.Red : Color.Blue;
        }

        // Обработка выстрела ПРОТИВ ТЕБЯ
        public void HandleEnemyShot(string payload)
        {
            var parts = payload.Split(',');
            int x = int.Parse(parts[0]);
            int y = int.Parse(parts[1]);
            string result = parts[2];

            // Красим клетку на своём поле
            _myField[x, y].BackColor =
                result.ToLower() == "hit" ? Color.Red : Color.Blue;
        }
        //метод цвета
        private static Color GetColor(ViewCellState state)
        {
            switch (state)
            {
                case ViewCellState.Miss:
                    return Color.Blue;
                case ViewCellState.Hit:
                    return Color.Red;
                case ViewCellState.Sunk:
                    return Color.Black;
                default:
                    return Color.White;
            }
        }

        //обновление клетки
        private void UpdateEnemyCell(int x, int y, ViewCellState newState)
        {
            var old = _enemyView[x, y];

            if (old == ViewCellState.Sunk)
                return;

            if (old == ViewCellState.Hit && newState == ViewCellState.Miss)
                return;

            _enemyView[x, y] = newState;
            _enemyField[x, y].BackColor = GetColor(newState);
        }

    }
}
