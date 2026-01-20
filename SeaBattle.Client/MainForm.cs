using SeaBattle.Client.Controllers;
using SeaBattle.Client.Networking;
using System;
using System.Windows.Forms;

namespace SeaBattle.Client
{
    public partial class MainForm : Form
    {
        private GameController _controller;
        private UserControl _currentView;

        /*public MainForm()
        {
            InitializeComponent();
            StartNewGame();
        }


        // =========================
        // Запуск новой игры
        // =========================
        private void StartNewGame()
        {
            _controller = new GameController();
            ShowPlacementView();
        }

        // =========================
        // Показать экран расстановки
        // =========================
        private void ShowPlacementView()
        {
            ClearCurrentView();
            var placement = new Views.PlacementView(_controller);
            placement.PlacementFinished += OnPlacementFinished;
            _currentView = placement;
            this.Controls.Add(_currentView);
            _currentView.Dock = DockStyle.Fill;
        }

        private void OnPlacementFinished()
        {
            _controller.FinishPlacement();

            ShowGameView();
        }

        // =========================
        // Показать игровой экран
        // =========================
        private void ShowGameView()
        {
            ClearCurrentView();
            var gameView = new Views.GameView(_controller);
            gameView.GameFinished += OnGameFinished;
            _currentView = gameView;
            this.Controls.Add(_currentView);
            _currentView.Dock = DockStyle.Fill;
        }

        // =========================
        // Показать экран конца игры
        // =========================
        private void OnGameFinished()
        {
            ClearCurrentView();
            var gameOver = new Views.GameOverView();
            gameOver.RestartRequested += OnRestartRequested;
            _currentView = gameOver;
            this.Controls.Add(_currentView);
            _currentView.Dock = DockStyle.Fill;
        }

        // =========================
        // Перезапуск игры
        // =========================
        private void OnRestartRequested()
        {
            StartNewGame();
        }

        // =========================
        // Очистка текущего UserControl
        // =========================
        private void ClearCurrentView()
        {
            if (_currentView != null)
            {
                this.Controls.Remove(_currentView);
                _currentView.Dispose();
                _currentView = null;
            }
        }*/

        //локальное подключение к серверу на одном пк

        private GameClient _networkClient;

        // Элементы интерфейса
        private Button _connectButton;
        private TextBox _logBox;

        public MainForm()
        {
            InitializeComponent();
            BuildUI();
        }

        // =========================
        // Создание UI для теста TCP
        // =========================
        private void BuildUI()
        {
            this.Width = 400;
            this.Height = 300;
            this.Text = "TCP Test Client";

            _connectButton = new Button
            {
                Text = "Connect to Server",
                Width = 150,
                Height = 30,
                Top = 10,
                Left = 10
            };
            _connectButton.Click += ConnectButton_Click;
            this.Controls.Add(_connectButton);

            _logBox = new TextBox
            {
                Multiline = true,
                Top = 50,
                Left = 10,
                Width = 360,
                Height = 200,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(_logBox);
        }

        // =========================
        // Подключение к серверу
        // =========================
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            _networkClient = new GameClient();
            _networkClient.MessageReceived += OnMessageReceived;

            try
            {
                _networkClient.Connect("127.0.0.1", 5000); // Локальный сервер
                Log("Connected to server.");

                // Отправляем тестовое сообщение
                _networkClient.Send("Hello from client!");
            }
            catch (Exception ex)
            {
                Log("Connection error: " + ex.Message);
            }
        }

        // =========================
        // Обработка сообщений от сервера
        // =========================
        private void OnMessageReceived(string msg)
        {
            // Проверяем, нужен ли Invoke
            if (_logBox.InvokeRequired)
            {
                // Оборачиваем лямбду в Action, чтобы соответствовать Delegate
                _logBox.Invoke(new Action(() => Log(msg)));
            }
            else
            {
                Log(msg);
            }
        }

        // Метод добавляет сообщение в текстовое поле или лог
        private void Log(string msg)
        {
            _logBox.AppendText(msg + Environment.NewLine); // если _logBox — TextBox
        }

    }
}
