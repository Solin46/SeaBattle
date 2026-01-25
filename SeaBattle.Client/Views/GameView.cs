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

        }

        private void EnemyCell_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var (x, y) = ((int, int))btn.Tag;

            // Отправляем сообщение на сервер
            var msg = new NetworkMessage(NetworkCommand.Shoot, $"{x},{y}");
            _client.Send(msg);

            // Для теста сразу закрашиваем клетку как промах (сервер пока фиктивный)
            btn.BackColor = Color.Blue;
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
                case NetworkCommand.ShotResult:
                    {
                        // payload: "x,y,hit/miss"
                        var parts = msg.Payload.Split(',');
                        int x = int.Parse(parts[0]);
                        int y = int.Parse(parts[1]);
                        string result = parts[2];

                        // Обновляем ВРАЖЕСКОЕ поле
                        _enemyField[x, y].BackColor =
                            result.ToLower() == "hit"
                                ? Color.Red
                                : Color.Blue;

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
                    GameFinished?.Invoke();
                    break;

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

    }
}
