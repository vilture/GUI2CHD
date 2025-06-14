using System;
using System.Windows.Forms;

namespace GUI2CHD
{
    public class ExtractProgressForm : Form
    {
        public ProgressBar ProgressBar { get; private set; }
        public Label StatusLabel { get; private set; }

        public ExtractProgressForm(string archiveName)
        {
            this.Text = "Распаковка архива";
            this.Size = new System.Drawing.Size(400, 120);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.TopMost = true;

            StatusLabel = new Label
            {
                Text = $"Распаковка: {archiveName}",
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(370, 20)
            };

            ProgressBar = new ProgressBar
            {
                Location = new System.Drawing.Point(10, 40),
                Size = new System.Drawing.Size(370, 23),
                Minimum = 0,
                Maximum = 100
            };

            this.Controls.Add(StatusLabel);
            this.Controls.Add(ProgressBar);
        }
    }
} 