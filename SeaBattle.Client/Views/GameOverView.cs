using System;
using System.Windows.Forms;

namespace SeaBattle.Client.Views
{
    public partial class GameOverView : UserControl
    {
        public event Action RestartRequested;

        public GameOverView(bool isWinner)
        {
            InitializeComponent();
            BuildUi(isWinner);
        }

        private void BuildUi(bool isWinner)
        {
            var lbl = new Label
            {
                Text = isWinner ? "Вы победили!" : "Вы проиграли",
                Top = 20,
                Left = 40,
                AutoSize = true,
                Font = new System.Drawing.Font(
                    "Arial", 14, System.Drawing.FontStyle.Bold)
            };
            Controls.Add(lbl);

            var restartBtn = new Button
            {
                Text = "Переиграть",
                Width = 140,
                Height = 40,
                Top = 70,
                Left = 40,
                Tag = false //нажата ли кнопка
            };
            restartBtn.Click += (s, e) =>
            {
                var button = (Button)s;

                // Проверяем, не нажата ли уже кнопка
                if ((bool)button.Tag == true)
                    return;

                // Помечаем как нажатую
                button.Tag = true;

                // Отключаем кнопку
                button.Enabled = false;
                button.Text = "Перезапуск...";
                button.BackColor = System.Drawing.Color.Gray;

                // Немного ждем для визуальной обратной связи
                Application.DoEvents();
                System.Threading.Thread.Sleep(100);

                RestartRequested?.Invoke();
            };
            Controls.Add(restartBtn);
        }
    }

}
