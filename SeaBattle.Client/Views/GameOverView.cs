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
                Text = isWinner ? "🎉 Вы победили!" : "😢 Вы проиграли",
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
                Left = 40
            };
            restartBtn.Click += (s, e) => RestartRequested?.Invoke();
            Controls.Add(restartBtn);
        }
    }

}
