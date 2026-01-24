using SeaBattle.Client.Controllers;
using SeaBattle.Client.Networking;
using SeaBattle.Common.Networking;
using SeaBattle.Client.Views;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SeaBattle.Client
{
    public partial class MainForm : Form
    {
        private GameClient _client;
        private GameController _controller;
        private GameView _gameView;

        private Button _connectButton;
        private TextBox _logBox;

        public MainForm()
        {
            InitializeComponent();
            Width = 1020;
            Height = 480;
            this.AutoScroll = true;
            Text = "SeaBattle Test Client";

            BuildUI();
        }

        private void BuildUI()
        {
            // =========================
            // Кнопка подключения
            // =========================
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

            // =========================
            // Лог сообщений от сервера
            // =========================
            _logBox = new TextBox
            {
                Multiline = true,
                Top = 50,
                Left = 10,
                Width = 360,
                Height = 350,
                ScrollBars = ScrollBars.Vertical
            };
            Controls.Add(_logBox);
        }

        // =========================
        // Подключение к серверу
        // =========================
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            _client = new GameClient();
            _controller = new GameController();

            _client.MessageReceived += OnServerMessage;

            try
            {
                _client.Connect("127.0.0.1", 5000);
                Log("Connected to server.");

                // Создаем GameView и добавляем на форму
                _gameView = new GameView(_controller, _client)
                {
                    Top = 10,
                    Left = 380,
                    Width = 400,
                    Height = 350
                };
                _gameView.GameFinished += OnGameFinished;

                Controls.Add(_gameView);

                // Для теста можно нарисовать свои корабли
                _gameView.DrawMyShips(new (int x, int y)[] { (0, 0), (0, 1), (0, 2) });

                // Отправляем Hello серверу
                var helloMsg = new NetworkMessage(NetworkCommand.Hello, "Привет сервер!");
                _client.Send(helloMsg);
            }
            catch (Exception ex)
            {
                Log("Connection error: " + ex.Message);
            }
        }

        // =========================
        // Логирование сообщений сервера
        // =========================
        private void OnServerMessage(NetworkMessage msg)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Log($"SERVER: {msg.Command} | {msg.Payload}")));
            }
            else
            {
                Log($"SERVER: {msg.Command} | {msg.Payload}");
            }
        }

        private void Log(string text)
        {
            _logBox.AppendText(text + Environment.NewLine);
        }

        // =========================
        // Событие окончания игры
        // =========================
        private void OnGameFinished()
        {
            MessageBox.Show("Игра окончена!");
        }
    }
}
