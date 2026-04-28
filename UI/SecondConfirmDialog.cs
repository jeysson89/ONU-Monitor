using System;
using System.Drawing;
using System.Windows.Forms;

namespace BDCOM.OLT.Manager.UI
{
    public class SecondConfirmDialog : Form
    {
        private readonly string _portOrDevice;
        private readonly string _type; // "port" или "olt"
        private TextBox txtConfirm;

        public bool Confirmed { get; private set; } = false;

        public SecondConfirmDialog(string portOrDevice, string type)
        {
            _portOrDevice = portOrDevice;
            _type = type;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "⚠️ ПОДТВЕРДИТЕ ДЕЙСТВИЕ ⚠️";
            this.Size = new Size(460, 380);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            int y = 20;

            var lblWarning = new Label
            {
                Text = "ВЫ УВЕРЕНЫ?",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.Red,
                Location = new Point(120, y),
                AutoSize = true
            };
            this.Controls.Add(lblWarning);

            y += 60;

            string text = _type == "port" 
                ? $"Порт EPON 0/{_portOrDevice} будет ОТКЛЮЧЕН!\nЭто действие НЕОБРАТИМО.\nВсе абоненты потеряют интернет.\n\nДля подтверждения введите слово:\nАДЕКВАТНЫЙ"
                : $"OLT БУДЕТ ПЕРЕЗАГРУЖЕН!\nЭто действие приведёт к отключению ВСЕХ абонентов!\n\nДля подтверждения введите слово:\nАДЕКВАТНЫЙ";

            var lblText = new Label
            {
                Text = text,
                Location = new Point(40, y),
                Size = new Size(380, 140),
                Font = new Font("Segoe UI", 10f)
            };
            this.Controls.Add(lblText);

            y += 150;

            txtConfirm = new TextBox
            {
                Location = new Point(100, y),
                Width = 260,
                Font = new Font("Segoe UI", 11f)
            };
            txtConfirm.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) CheckConfirm(); };
            this.Controls.Add(txtConfirm);

            y += 50;

            var btnConfirm = new Button
            {
                Text = "ПОДТВЕРДИТЬ",
                Location = new Point(180, y),
                Size = new Size(120, 40),
                BackColor = Color.Red,
                ForeColor = Color.White
            };
            btnConfirm.Click += (s, e) => CheckConfirm();
            this.Controls.Add(btnConfirm);

            var btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(310, y),
                Size = new Size(90, 40)
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnConfirm;
        }

        private void CheckConfirm()
        {
            if (txtConfirm.Text.Trim() == "АДЕКВАТНЫЙ")
            {
                Confirmed = true;
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Введено неверное слово!\nОперация отменена.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
            }
        }
    }
}