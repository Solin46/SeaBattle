using System;
using System.Windows.Forms;

namespace SeaBattle.Client.Views
{
    public partial class GameOverView : UserControl
    {
        public event Action RestartRequested;

        public GameOverView()
        {
            InitializeComponent();

            // Создаём кнопку переиграть
            var restartBtn = new Button
            {
                Text = "Переиграть",
                Width = 120,
                Height = 40,
                Top = 50,
                Left = 50
            };
            restartBtn.Click += (s, e) => RestartRequested?.Invoke();
            this.Controls.Add(restartBtn);

            // Можно добавить метку "Победа / Поражение"
            var lbl = new Label
            {
                Text = "Игра окончена!",
                Top = 10,
                Left = 50,
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold)
            };
            this.Controls.Add(lbl);
        }
    }
}
