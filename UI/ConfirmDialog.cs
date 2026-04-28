using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BDCOM.OLT.Manager.Models;

namespace BDCOM.OLT.Manager.UI
{
    public class ConfirmDialog : Form
    {
        private readonly string _action;
        private readonly Dictionary<string, string> _context;
        public bool Confirmed { get; private set; } = false;

        public ConfirmDialog(string action, Dictionary<string, string> context)
        {
            _action = action;
            _context = context ?? new Dictionary<string, string>();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = GetTitle();
            this.Size = new Size(440, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            int y = 20;

            // Иконка
            var lblIcon = new Label
            {
                Text = GetIcon(),
                Font = new Font("Segoe UI", 48),
                Location = new Point(40, y),
                AutoSize = true
            };
            this.Controls.Add(lblIcon);

            // Заголовок
            var lblTitle = new Label
            {
                Text = GetTitle(),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(130, y + 15),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            y += 90;

            // Контекст
            int cy = 0;
            var panel = new Panel { Location = new Point(40, y), Size = new Size(360, 90) };
            foreach (var kv in _context)
            {
                panel.Controls.Add(new Label
                {
                    Text = kv.Key + ":",
                    Location = new Point(0, cy),
                    Width = 100,
                    TextAlign = ContentAlignment.MiddleRight
                });

                panel.Controls.Add(new Label
                {
                    Text = kv.Value,
                    Location = new Point(110, cy),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                });
                cy += 28;
            }
            this.Controls.Add(panel);

            y += 100;

            // Предупреждение
            var lblWarn = new Label
            {
                Text = GetWarning(),
                Location = new Point(40, y),
                Size = new Size(360, 80),
                ForeColor = Color.OrangeRed,
                Font = new Font("Segoe UI", 9.75f, FontStyle.Bold)
            };
            this.Controls.Add(lblWarn);

            y += 95;

            // Кнопки
            var btnConfirm = new Button
            {
                Text = GetConfirmText(),
                Location = new Point(180, y),
                Size = new Size(120, 40),
                BackColor = GetConfirmColor(),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            btnConfirm.Click += (s, e) => { Confirmed = true; this.DialogResult = DialogResult.OK; };
            this.Controls.Add(btnConfirm);

            var btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(310, y),
                Size = new Size(90, 40)
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
        }

        private string GetTitle()
        {
            return _action switch
            {
                "reboot" => "Перезагрузка ONU",
                "delete" => "Удаление ONU",
                "lan_off" => "Отключение LAN порта",
                "lan_on" => "Включение LAN порта",
                "olt_port_off" => "⚠️ ОТКЛЮЧЕНИЕ ПОРТА OLT",
                "olt_reboot" => "⚠️ ПЕРЕЗАГРУЗКА OLT",
                _ => "Подтверждение"
            };
        }

        private string GetIcon()
        {
            return _action switch
            {
                "reboot" or "olt_reboot" => "🔄",
                "delete" => "🗑",
                "lan_off" or "olt_port_off" => "🔴",
                "lan_on" => "🟢",
                _ => "⚠️"
            };
        }

        private string GetWarning()
        {
            return _action switch
            {
                "olt_port_off" => "ВНИМАНИЕ!\nВсе абоненты на этом порту потеряют интернет!",
                "olt_reboot" => "ВНИМАНИЕ!\nПерезагрузка OLT отключит ВСЕХ абонентов!",
                "lan_off" => "Абонент потеряет доступ в интернет.",
                "delete" => "Требуется повторная регистрация ONU.",
                _ => "Вы уверены в выполнении действия?"
            };
        }

        private string GetConfirmText()
        {
            return _action.Contains("off") || _action.Contains("delete") ? "ВЫПОЛНИТЬ" : "Подтвердить";
        }

        private Color GetConfirmColor()
        {
            return (_action.Contains("off") || _action.Contains("delete") || _action.Contains("reboot"))
                ? Color.FromArgb(198, 40, 40)
                : Color.FromArgb(46, 125, 50);
        }
    }
}