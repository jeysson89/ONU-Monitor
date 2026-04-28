using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using BDCOM.OLT.Manager.Core;
using BDCOM.OLT.Manager.Enums;
using BDCOM.OLT.Manager.Models;

namespace BDCOM.OLT.Manager.UI
{
    public class ExtraFunctionsDialog : Form
    {
        private readonly MainForm _mainForm;

        public ExtraFunctionsDialog(MainForm mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Дополнительные функции OLT";
            this.Size = new Size(520, 680);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            int y = 20;

            AddButton("💾 Save (write all)", Color.Teal, y, SaveConfig); y += 55;
            AddButton("🔄 Reboot OLT", Color.Red, y, RebootOLT); y += 55;

            var sep1 = new Label { Text = "────────────────────────────", Location = new Point(50, y), AutoSize = true };
            Controls.Add(sep1); y += 40;

            AddButton("🔴 Выкл LAN ONU", Color.Red, y, () => LanPortCommand(true)); y += 55;
            AddButton("🟢 Вкл LAN ONU", Color.Green, y, () => LanPortCommand(false)); y += 55;

            var sep2 = new Label { Text = "────────────────────────────", Location = new Point(50, y), AutoSize = true };
            Controls.Add(sep2); y += 40;

            AddButton("🔴⚠️ Выкл Порт EPON", Color.Red, y, OltPortOff); y += 55;
            AddButton("🟢 Вкл Порт EPON", Color.Green, y, OltPortOn); y += 55;

            var lblWarn = new Label
            {
                Text = "⚠️ Внимание: Reboot OLT, управление портами и отключение LAN\nтребуют двойного подтверждения!",
                Location = new Point(40, y),
                Size = new Size(440, 60),
                ForeColor = Color.OrangeRed,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            Controls.Add(lblWarn);

            y += 80;
            var btnClose = new Button { Text = "Закрыть", Location = new Point(200, y), Size = new Size(120, 40) };
            btnClose.Click += (s, e) => this.Close();
            Controls.Add(btnClose);
        }

        private void AddButton(string text, Color color, int y, Action action)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(80, y),
                Size = new Size(360, 42),
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold)
            };
            btn.Click += (s, e) => action();
            Controls.Add(btn);
        }

        private void SaveConfig()
        {
            _mainForm.ExecuteCommand("write all", "Конфигурация сохранена");
        }

        private void RebootOLT()
        {
            if (_mainForm.CurrentDevice == null) return;

            var dlg1 = new ConfirmDialog("olt_reboot", new Dictionary<string, string> { { "device", _mainForm.CurrentDevice.Name } });
            if (dlg1.ShowDialog() != DialogResult.OK) return;

            var dlg2 = new SecondConfirmDialog("", "olt");
            if (dlg2.ShowDialog() != DialogResult.OK) return;

            _mainForm.ExecuteCommand("reload", "OLT перезагружается...");
        }

        private void LanPortCommand(bool shutdown)
        {
            var p = _mainForm.GetCurrentParams();
            if (p == null) return;

            var dlg = new ConfirmDialog(shutdown ? "lan_off" : "lan_on", 
                new Dictionary<string, string> { { "Порт", p.Port }, { "ONU", p.OnuId } });

            if (dlg.ShowDialog() != DialogResult.OK) return;

            string cmd = shutdown ? "epon onu port 1 ctc shutdown" : "no epon onu port 1 ctc shutdown";
            _mainForm.ExecuteCommand($"interface epon {p.Slot}/{p.Port}:{p.OnuId}\n{cmd}\nexit", 
                $"LAN порт {(shutdown ? "отключен" : "включен")}");
        }

        private void OltPortOff()
        {
            string port = _mainForm.GetCurrentPort();
            if (string.IsNullOrEmpty(port)) return;

            var dlg1 = new ConfirmDialog("olt_port_off", new Dictionary<string, string> { { "Порт", port } });
            if (dlg1.ShowDialog() != DialogResult.OK) return;

            var dlg2 = new SecondConfirmDialog(port, "port");
            if (dlg2.ShowDialog() != DialogResult.OK) return;

            _mainForm.ExecuteCommand($"interface epon 0/{port}\nshutdown\nexit\nwrite all", $"Порт EPON 0/{port} ОТКЛЮЧЕН");
        }

        private void OltPortOn()
        {
            string port = _mainForm.GetCurrentPort();
            if (string.IsNullOrEmpty(port)) return;

            var dlg = new ConfirmDialog("olt_port_on", new Dictionary<string, string> { { "Порт", port } });
            if (dlg.ShowDialog() != DialogResult.OK) return;

            _mainForm.ExecuteCommand($"interface epon 0/{port}\nno shutdown\nexit\nwrite all", $"Порт EPON 0/{port} ВКЛЮЧЕН");
        }
    }
}