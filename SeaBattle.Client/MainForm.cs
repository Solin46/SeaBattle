using SeaBattle.Client.Controllers;
using SeaBattle.Client.Networking;
using SeaBattle.Client.Views;
using SeaBattle.Common.Networking;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SeaBattle.Client
{
    public partial class MainForm : Form
    {
        private GameClient _client;
        private GameController _controller;
        private GameView _gameView;
        private PlacementView _placementView;
        private GameOverView _gameOverView;

        private Button _connectButton;
        private TextBox _logBox;

        // Добавляем для хранения роли игрока
        private bool _isPlayer1;
        private bool _roleReceived = false;
        //для лога
        private Panel _logPanel;
        private Button _toggleLogButton;
        private bool _logExpanded = true;


        public MainForm()
        {
            InitializeComponent();
            Width = 1200;
            Height = 600;
            this.AutoScroll = true;
            Text = "SeaBattle Test Client";

            BuildUI();
        }

        private void BuildUI()
        {
            _connectButton = new Button
            {
                Text = "Connect to Server",
                Width = 150,
                Height = 30,
                Top = 10,
                Left = 10
            };
            _connectButton.Click += ConnectButton_Click;
            Controls.Add(_connectButton);

            /*_logBox = new TextBox
            {
                Multiline = true,
                Top = 50,
                Left = 10,
                Width = 360,
                Height = 350,
                ScrollBars = ScrollBars.Vertical
            };
            Controls.Add(_logBox);*/

            // Панель для лога с заголовком
            _logPanel = new Panel
            {
                Top = 50,
                Left = 10,
                Width = 360,
                Height = 400,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Заголовок панели
            var logHeader = new Panel
            {
                Height = 30,
                Dock = DockStyle.Top,
                BackColor = Color.LightGray
            };

            var logLabel = new Label
            {
                Text = "Network Log",
                Left = 10,
                Top = 8,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            _toggleLogButton = new Button
            {
                Text = "▲",
                Width = 30,
                Height = 20,
                Left = 320,
                Top = 5
            };
            _toggleLogButton.Click += ToggleLogButton_Click;

            logHeader.Controls.Add(logLabel);
            logHeader.Controls.Add(_toggleLogButton);

            // Сам лог
            _logBox = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical
            };

            _logPanel.Controls.Add(_logBox);
            _logPanel.Controls.Add(logHeader);

            Controls.Add(_logPanel);
        }

        private void ToggleLogButton_Click(object sender, EventArgs e)
        {
            _logExpanded = !_logExpanded;

            if (_logExpanded)
            {
                _logPanel.Height = 400;
                _toggleLogButton.Text = "▲";
                _logBox.Visible = true;
            }
            else
            {
                _logPanel.Height = 30; // Только заголовок
                _toggleLogButton.Text = "▼";
                _logBox.Visible = false;
            }
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            _client = new GameClient();
            _controller = new GameController();

            _client.MessageReceived += OnServerMessage;

            try
            {
                _client.Connect("127.0.0.1", 5000);
                Log("Connected to server.");

                // Создаем PlacementView для начала
                CreatePlacementView();

                var helloMsg = new NetworkMessage(NetworkCommand.Hello, "Привет сервер!");
                _client.Send(helloMsg);

                Log("Ожидание игроков...");

            }
            catch (Exception ex)
            {
                Log("Connection error: " + ex.Message);
            }
        }

        private void CreatePlacementView()
        {
            // Удаляем существующие вьюхи
            RemoveGameViews();

            _placementView = new PlacementView(_controller)
            {
                Top = 10,
                Left = 380,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            _placementView.PlacementFinished += OnPlacementFinished;
            Controls.Add(_placementView);
        }

        private void OnPlacementFinished()
        {
            var ships = _controller.State.MyBoard.Ships;

            string payload = string.Join(";", ships.Select(
                s => string.Join("|", s.Decks.Select(d => $"{d.X},{d.Y}"))
            ));

            _client.Send(new NetworkMessage(NetworkCommand.PlaceShips, payload));

            Log("Расстановка отправлена, ждём второго игрока...");
        }

        private void OnServerMessage(NetworkMessage msg)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnServerMessage(msg)));
                return;
            }

            // обработка склеенных сообщений
            if (msg.Payload != null &&
                (msg.Payload.StartsWith("falseGameOver") ||
                 msg.Payload.StartsWith("trueGameOver")))
            {
                Console.WriteLine($"ОБРАБОТКА СКЛЕЕННОГО СООБЩЕНИЯ: {msg.Command}|{msg.Payload}");

                // Извлекаем победителя
                string winner = msg.Payload.Contains("player1") ? "player1" : "player2";

                // Определяем, выиграли ли мы
                bool isWinner = false;
                if (_roleReceived)
                {
                    isWinner = (_isPlayer1 && winner == "player1") ||
                              (!_isPlayer1 && winner == "player2");
                }

                Console.WriteLine($"Победитель: {winner}, Мы выиграли: {isWinner}");

                // Показываем GameOverView
                ShowGameOver(isWinner);
                return;
            }

            Log($"SERVER: {msg.Command} | {msg.Payload}");

            switch (msg.Command)
            {
                case NetworkCommand.PlayerRole:
                    _isPlayer1 = msg.Payload == "player1";
                    _roleReceived = true;
                    Log($"Вы играете за: {msg.Payload}");
                    break;

                case NetworkCommand.GameStart:
                    StartGame();
                    break;

                case NetworkCommand.GameOver:
                    bool isWinnerNormal = (_roleReceived && msg.Payload == "player1" && _isPlayer1) ||
                                         (_roleReceived && msg.Payload == "player2" && !_isPlayer1);
                    ShowGameOver(isWinnerNormal);
                    break;
            }
        }

        private void StartGame()
        {
            if (_placementView != null)
            {
                Controls.Remove(_placementView);
                _placementView.Dispose();
                _placementView = null;
            }

            if (_gameOverView != null)
            {
                Controls.Remove(_gameOverView);
                _gameOverView.Dispose();
                _gameOverView = null;
            }

            _gameView = new GameView(_controller, _client)
            {
                Top = 10,
                Left = 380,
            };

            // Передаем сохраненную роль в GameView
            _gameView.IsPlayer1 = _isPlayer1;

            // Подписываемся на пересылку сообщений от GameView
            _gameView.ServerMessageForwarded += OnServerMessageFromGameView;

            Controls.Add(_gameView);

            // Рисуем свои корабли
            _gameView.DrawMyShips(
                _controller.State.MyBoard.Ships
                    .SelectMany(s => s.Decks)
                    .Select(d => (d.X, d.Y))
                    .ToArray()
            );

            Log("Игра началась!");
        }

        private void OnServerMessageFromGameView(NetworkMessage msg)
        {
            // Обрабатываем сообщения, которые GameView передал нам
            OnServerMessage(msg);
        }

        private void ShowGameOver(bool isWinner)
        {
            Log($"Показать GameOverView. Победитель: {(isWinner ? "Вы" : "Противник")}");

            // Удаляем GameView
            if (_gameView != null)
            {
                Controls.Remove(_gameView);
                _gameView.Dispose();
                _gameView = null;
            }

            // Показываем GameOverView
            _gameOverView = new GameOverView(isWinner)
            {
                Top = 200,
                Left = 500,
                Width = 220,
                Height = 150
            };

            _gameOverView.RestartRequested += OnRestartRequested;
            Controls.Add(_gameOverView);

            Log(isWinner ? "Вы победили!" : "Вы проиграли");
        }

        private void OnRestartRequested()
        {
            Log("Перезапуск игры...");

            // Полностью очищаем состояние
            _controller.RestartGame();
            _roleReceived = false;
            _isPlayer1 = false;

            // Отключаемся от сервера
            _client?.Disconnect();

            // Удаляем GameOverView
            if (_gameOverView != null)
            {
                Controls.Remove(_gameOverView);
                _gameOverView.Dispose();
                _gameOverView = null;
            }

            // Создаем новое подключение
            ConnectButton_Click(null, EventArgs.Empty);
        }

        private void RemoveGameViews()
        {
            if (_placementView != null)
            {
                Controls.Remove(_placementView);
                _placementView.Dispose();
                _placementView = null;
            }

            if (_gameView != null)
            {
                Controls.Remove(_gameView);
                _gameView.Dispose();
                _gameView = null;
            }

            if (_gameOverView != null)
            {
                Controls.Remove(_gameOverView);
                _gameOverView.Dispose();
                _gameOverView = null;
            }
        }

        private void Log(string text)
        {
            _logBox.AppendText(DateTime.Now.ToString("HH:mm:ss") + " - " + text + Environment.NewLine);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _client?.Disconnect();
            base.OnFormClosing(e);
        }
    }
}