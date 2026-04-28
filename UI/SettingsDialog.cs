using System;
using System.Windows.Forms;
using BDCOM.OLT.Manager.Config;

namespace BDCOM.OLT.Manager.UI
{
    public class SettingsDialog : Form
    {
        public bool AutoConnect { get; private set; }
        public bool AutoReconnect { get; private set; }
        public int TelnetTimeout { get; private set; }
        public int CommandTimeout { get; private set; }
        public int AuthTimeout { get; private set; }
        public double CommandDelay { get; private set; }
        public int ReconnectAttempts { get; private set; }
        public int ReconnectDelay { get; private set; }

        private CheckBox chkAutoConnect, chkAutoReconnect;
        private TextBox txtTelnet, txtCommand, txtAuth, txtDelay, txtAttempts, txtReconnectDelay;

        public SettingsDialog()
        {
            InitializeComponent();
            LoadCurrentValues();
        }

        private void InitializeComponent()
        {
            this.Text = "Настройки";
            this.Size = new Size(520, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            int y = 20;

            chkAutoConnect = new CheckBox { Text = "Автоподключение при выборе устройства", Location = new Point(30, y), AutoSize = true };
            this.Controls.Add(chkAutoConnect);
            y += 40;

            chkAutoReconnect = new CheckBox { Text = "Автоматическое переподключение при обрыве", Location = new Point(30, y), AutoSize = true };
            this.Controls.Add(chkAutoReconnect);
            y += 50;

            AddLabel("Telnet таймаут (сек):", 30, y);
            txtTelnet = AddNumericTextBox(TimeoutConfig.TelnetTimeout, 280, y); y += 40;

            AddLabel("Command таймаут (сек):", 30, y);
            txtCommand = AddNumericTextBox(TimeoutConfig.CommandTimeout, 280, y); y += 40;

            AddLabel("Auth таймаут (сек):", 30, y);
            txtAuth = AddNumericTextBox(TimeoutConfig.AuthTimeout, 280, y); y += 40;

            AddLabel("Задержка между командами (сек):", 30, y);
            txtDelay = AddNumericTextBox(TimeoutConfig.CommandDelay, 280, y); y += 40;

            AddLabel("Попыток переподключения:", 30, y);
            txtAttempts = AddNumericTextBox(TimeoutConfig.ReconnectAttempts, 280, y); y += 40;

            AddLabel("Задержка перед переподключением (сек):", 30, y);
            txtReconnectDelay = AddNumericTextBox(TimeoutConfig.ReconnectDelay, 280, y); y += 60;

            var btnSave = new Button { Text = "Сохранить", Location = new Point(280, y), Size = new Size(100, 40) };
            btnSave.Click += (s, e) => SaveSettings();
            this.Controls.Add(btnSave);

            var btnCancel = new Button { Text = "Отмена", Location = new Point(390, y), Size = new Size(90, 40) };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
        }

        private void AddLabel(string text, int x, int y)
        {
            this.Controls.Add(new Label { Text = text, Location = new Point(x, y + 5), AutoSize = true });
        }

        private TextBox AddNumericTextBox(object defaultValue, int x, int y)
        {
            var tb = new TextBox { Text = defaultValue.ToString(), Location = new Point(x, y), Width = 80 };
            this.Controls.Add(tb);
            return tb;
        }

        private void LoadCurrentValues()
        {
            chkAutoConnect.Checked = AppConfig.AutoConnect;
            chkAutoReconnect.Checked = AppConfig.AutoReconnect;
            txtTelnet.Text = TimeoutConfig.TelnetTimeout.ToString();
            txtCommand.Text = TimeoutConfig.CommandTimeout.ToString();
            txtAuth.Text = TimeoutConfig.AuthTimeout.ToString();
            txtDelay.Text = TimeoutConfig.CommandDelay.ToString();
            txtAttempts.Text = TimeoutConfig.ReconnectAttempts.ToString();
            txtReconnectDelay.Text = TimeoutConfig.ReconnectDelay.ToString();
        }

        private void SaveSettings()
        {
            AppConfig.AutoConnect = chkAutoConnect.Checked;
            AppConfig.AutoReconnect = chkAutoReconnect.Checked;

            TimeoutConfig.TelnetTimeout = int.Parse(txtTelnet.Text);
            TimeoutConfig.CommandTimeout = int.Parse(txtCommand.Text);
            TimeoutConfig.AuthTimeout = int.Parse(txtAuth.Text);
            TimeoutConfig.CommandDelay = double.Parse(txtDelay.Text);
            TimeoutConfig.ReconnectAttempts = int.Parse(txtAttempts.Text);
            TimeoutConfig.ReconnectDelay = int.Parse(txtReconnectDelay.Text);

            this.DialogResult = DialogResult.OK;
        }
    }
}