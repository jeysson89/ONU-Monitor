using System;
using System.Drawing;
using System.Windows.Forms;
using BDCOM.OLT.Manager.Core;
using BDCOM.OLT.Manager.UI;

namespace BDCOM.OLT.Manager
{
    public partial class ExtraFunctionsDialog : Form
    {
        private readonly MainForm _mainForm;
        private readonly TelnetClient? _telnetClient;

        public ExtraFunctionsDialog(MainForm mainForm)
        {
            _mainForm = mainForm;
            _telnetClient = mainForm.GetTelnetClient();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Дополнительные функции";
            this.Size = new Size(460, 680);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int y = 30;

            AddButton("Save (write all)", Color.Teal, y, SaveConfig); y += 65;
            AddButton("Reboot OLT", Color.Crimson, y, RebootOLT); y += 65;

            AddLabel("Управление LAN портом ONU", y); y += 35;
            AddButton("Выкл LAN ONU", Color.Crimson, y, () => LanOnuCommand("off")); y += 65;
            AddButton("Вкл LAN ONU", Color.MediumSeaGreen, y, () => LanOnuCommand("on")); y += 65;

            AddLabel("Управление портом EPON", y); y += 35;
            AddButton("Выкл Порт EPON", Color.Crimson, y, () => EponPortCommand("off")); y += 65;
            AddButton("Вкл Порт EPON", Color.MediumSeaGreen, y, () => EponPortCommand("on")); y += 65;

            var btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(160, 580),
                Size = new Size(140, 45),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(btnClose);
        }

        private void AddButton(string text, Color color, int y, Action action)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(60, y),
                Size = new Size(340, 58),
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => action();
            this.Controls.Add(btn);
        }

        private void AddLabel(string text, int y)
        {
            var lbl = new Label
            {
                Text = text,
                Location = new Point(60, y),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.DimGray,
                AutoSize = true
            };
            this.Controls.Add(lbl);
        }

        // ==================== Правильные команды ====================

        private async void SaveConfig()
        {
            if (_telnetClient == null) return;
            await _telnetClient.ExecuteAsync("write all");
            MessageBox.Show("Конфигурация сохранена (write all)", "Успешно");
        }

        private async void RebootOLT()
        {
            if (_telnetClient == null) return;
            if (MessageBox.Show("Перезагрузить OLT?\nВсе абоненты будут отключены!", "ВНИМАНИЕ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            var confirm = new SecondConfirmDialog("OLT", "перезагрузку OLT");
            if (confirm.ShowDialog(this) != DialogResult.OK) return;

            await _telnetClient.ExecuteAsync("reload");
            MessageBox.Show("Команда reload отправлена.\nOLT перезагружается...", "Выполнено");
        }

        private async void LanOnuCommand(string state)
        {
            var p = _mainForm.GetCurrentParams();
            if (p == null) return;

            string cmd = state == "on"
                ? $"interface epon {p.Slot}/{p.Port}:{p.OnuId}\nno epon onu port 1 ctc shutdown\nexit"
                : $"interface epon {p.Slot}/{p.Port}:{p.OnuId}\nepon onu port 1 ctc shutdown\nexit";

            await _telnetClient.ExecuteAsync(cmd);
            MessageBox.Show($"LAN порт ONU {p.FullId} {state.ToUpper()}", "Выполнено");
        }

        private async void EponPortCommand(string state)
        {
            string port = _mainForm.GetCurrentPort();
            if (string.IsNullOrEmpty(port))
            {
                MessageBox.Show("Укажите порт в поле 'Порт'", "Ошибка");
                return;
            }

            string cmd = state == "on"
                ? $"interface epon 0/{port}\nno shutdown\nexit"
                : $"interface epon 0/{port}\nshutdown\nexit";

            await _telnetClient.ExecuteAsync(cmd);
            MessageBox.Show($"Порт EPON 0/{port} {state.ToUpper()}", "Выполнено");
        }
    }
}