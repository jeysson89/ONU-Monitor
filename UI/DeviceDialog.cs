using System;
using System.Windows.Forms;
using BDCOM.OLT.Manager.Models;

namespace BDCOM.OLT.Manager.UI
{
    public class DeviceDialog : Form
    {
        public Device? ResultDevice { get; private set; }

        private TextBox txtName, txtIp, txtPort, txtUser, txtPass, txtEnable;

        public DeviceDialog(Device? edit = null)
        {
            InitializeDialog(edit);
        }

        private void InitializeDialog(Device? edit)
        {
            this.Text = edit == null ? "Новое устройство" : "Редактирование устройства";
            this.Size = new Size(460, 420);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;

            int y = 25;
            txtName = AddField("Название:", edit?.Name ?? "OLT-01", y); y += 45;
            txtIp = AddField("IP адрес:", edit?.Ip ?? "192.168.1.1", y); y += 45;
            txtPort = AddField("Порт:", (edit?.Port ?? 23).ToString(), y); y += 45;
            txtUser = AddField("Логин:", edit?.Username ?? "admin", y); y += 45;
            txtPass = AddField("Пароль:", edit?.Password ?? "", y, true); y += 45;
            txtEnable = AddField("Enable пароль:", edit?.EnablePassword ?? "", y, true); y += 55;

            var btnSave = new Button { Text = "Сохранить", Location = new Point(250, y), Size = new Size(90, 35) };
            btnSave.Click += (s, e) => Save();
            this.Controls.Add(btnSave);

            var btnCancel = new Button { Text = "Отмена", Location = new Point(350, y), Size = new Size(80, 35) };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
        }

        private TextBox AddField(string label, string value, int y, bool isPassword = false)
        {
            this.Controls.Add(new Label { Text = label, Location = new Point(30, y + 5), AutoSize = true });
            var tb = new TextBox { Text = value, Location = new Point(160, y), Width = 260 };
            if (isPassword) tb.PasswordChar = '●';
            this.Controls.Add(tb);
            return tb;
        }

        private void Save()
        {
            int port = int.TryParse(txtPort.Text, out int p) ? p : 23;

            ResultDevice = new Device
            {
                Name = txtName.Text.Trim(),
                Ip = txtIp.Text.Trim(),
                Port = port,
                Username = txtUser.Text.Trim(),
                Password = txtPass.Text,
                EnablePassword = txtEnable.Text
            };
            this.DialogResult = DialogResult.OK;
        }
    }
}