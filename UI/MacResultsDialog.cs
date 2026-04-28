using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BDCOM.OLT.Manager.UI
{
    public class MacResultsDialog : Form
    {
        private readonly List<string> _macs;

        public MacResultsDialog(List<string> macs)
        {
            _macs = macs;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = $"MAC-адреса ({_macs.Count})";
            this.Size = new Size(620, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            var listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 11f),
                SelectionMode = SelectionMode.MultiExtended
            };
            listBox.Items.AddRange(_macs.ToArray());
            this.Controls.Add(listBox);

            var panelBottom = new Panel { Dock = DockStyle.Bottom, Height = 50 };

            var btnCopyAll = new Button
            {
                Text = "📋 Копировать все",
                Location = new Point(20, 10),
                Size = new Size(180, 35),
                BackColor = Color.Teal,
                ForeColor = Color.White
            };
            btnCopyAll.Click += (s, e) =>
            {
                Clipboard.SetText(string.Join(Environment.NewLine, _macs));
                MessageBox.Show($"Скопировано {_macs.Count} MAC-адресов", "Готово");
            };

            var btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(420, 10),
                Size = new Size(120, 35)
            };
            btnClose.Click += (s, e) => this.Close();

            panelBottom.Controls.Add(btnCopyAll);
            panelBottom.Controls.Add(btnClose);
            this.Controls.Add(panelBottom);
        }
    }
}