using System;
using System.Drawing;
using System.Windows.Forms;

namespace BDCOM.OLT.Manager.UI
{
    public class ResultsDialog : Form
    {
        public ResultsDialog(string title, string content)
        {
            this.Text = title;
            this.Size = new Size(820, 620);
            this.MinimumSize = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            var txtResult = new RichTextBox
            {
                Text = content,
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 10.25f),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Both
            };

            this.Controls.Add(txtResult);

            var btnClose = new Button
            {
                Text = "Закрыть",
                Dock = DockStyle.Bottom,
                Height = 45,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }
    }
}