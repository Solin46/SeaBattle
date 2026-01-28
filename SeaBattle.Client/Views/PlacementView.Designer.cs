using System.Windows.Forms;

namespace SeaBattle.Client.Views
{
    partial class PlacementView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Наши элементы управления
        private System.Windows.Forms.Panel _gridPanel;
        private System.Windows.Forms.ComboBox _shipSizeCombo;
        private System.Windows.Forms.Button _orientationButton;
        private System.Windows.Forms.Button _doneButton;
        private System.Windows.Forms.Label _shipsCounterLabel; //счётчик расставленных кораблей


        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support — do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();

            // =================== Панель сетки ===================
            _gridPanel = new System.Windows.Forms.Panel
            {
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(300, 300)
            };
            this.Controls.Add(_gridPanel);

            // =================== ComboBox для размера корабля ===================
            _shipSizeCombo = new System.Windows.Forms.ComboBox
            {
                Location = new System.Drawing.Point(20, 20),
                Width = 80,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _shipSizeCombo.Items.AddRange(new object[] { 1, 2, 3, 4 });
            _shipSizeCombo.SelectedIndex = 0;
            this.Controls.Add(_shipSizeCombo);

            // =================== Кнопка ориентации ===================
            _orientationButton = new System.Windows.Forms.Button
            {
                Location = new System.Drawing.Point(120, 20),
                Text = "→"
            };
            _orientationButton.Click += (s, e) =>
            {
                if (_orientationButton.Text == "→") _orientationButton.Text = "↓";
                else _orientationButton.Text = "→";
            };
            this.Controls.Add(_orientationButton);

            // =================== Label для счётчика ===================
            _shipsCounterLabel = new System.Windows.Forms.Label
            {
                Location = new System.Drawing.Point(20, 370),
                Size = new System.Drawing.Size(300, 20),
                Text = "Корабли: 0 / 10"
            };
            this.Controls.Add(_shipsCounterLabel);


            // =================== Кнопка "Готово" ===================
            _doneButton = new System.Windows.Forms.Button
            {
                Location = new System.Drawing.Point(200, 20),
                Text = "Готово"
            };
            _doneButton.Click += DoneButton_Click;
            this.Controls.Add(_doneButton);

            // =================== Настройки UserControl ===================
            this.Name = "PlacementView";
            this.Size = new System.Drawing.Size(350, 400);

            this.ResumeLayout(false);
        }

        #endregion
    }
}
