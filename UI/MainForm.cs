using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BDCOM.OLT.Manager.Config;
using BDCOM.OLT.Manager.Core;
using BDCOM.OLT.Manager.Enums;
using BDCOM.OLT.Manager.Models;
using BDCOM.OLT.Manager.Parsers;
using BDCOM.OLT.Manager.UI;

namespace BDCOM.OLT.Manager
{
    public partial class MainForm : Form
    {
        private List<Device> _devices = new();
        private Device? _currentDevice;
        private TelnetClient? _telnetClient;
        private Logger _logger;

        private FlowLayoutPanel _devicesPanel = null!;
        private RichTextBox _logBox = null!;

        private TextBox txtSlot = null!, txtPort = null!, txtOnu = null!, txtMac = null!;

        private Button btnAddDevice = null!, btnEditDevice = null!, btnDeleteDevice = null!;
        private Button btnConnect = null!, btnDisconnect = null!, btnExtraFunctions = null!;
        private Button btnGetMac = null!, btnGetStatus = null!, btnGetOptical = null!, btnGetPortOptical = null!;
        private Button btnSetSpeed = null!, btnRebootOnu = null!, btnDeleteOnu = null!;

        public Device? CurrentDevice => _currentDevice;

        public MainForm()
        {
            AppConfig.EnsureDirs();
            LoadDevices();
            InitializeComponent();
            _logger = new Logger(_logBox);
            _logger.Info("Приложение запущено");
            RefreshDeviceButtons();
            UpdateButtonsState();
        }

        private void InitializeComponent()
        {
            this.Text = $"{AppConfig.AppName} v{AppConfig.Version}";
            this.Size = new Size(1250, 880);
            this.MinimumSize = new Size(1050, 720);
            this.BackColor = Theme.BG_PRIMARY;
            this.Font = new Font("Segoe UI", 10f);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Устройства
            var gbDevices = new GroupBox { Text = "Устройства", Location = new Point(12, 12), Size = new Size(1210, 145), BackColor = Theme.BG_SECONDARY };
            _devicesPanel = new FlowLayoutPanel { Location = new Point(15, 30), Size = new Size(1180, 70), AutoScroll = true };
            gbDevices.Controls.Add(_devicesPanel);

            btnAddDevice = CreateButton("+ Добавить", Color.LimeGreen, new Point(15, 110), 110, AddDevice);
            btnEditDevice = CreateButton("✎ Изменить", Color.FromArgb(0, 122, 204), new Point(135, 110), 110, EditDevice);
            btnDeleteDevice = CreateButton("🗑 Удалить", Color.IndianRed, new Point(255, 110), 110, DeleteDevice);

            gbDevices.Controls.Add(btnAddDevice);
            gbDevices.Controls.Add(btnEditDevice);
            gbDevices.Controls.Add(btnDeleteDevice);
            this.Controls.Add(gbDevices);

            // Параметры ONU
            var gbParams = new GroupBox { Text = "Параметры ONU", Location = new Point(12, 170), Size = new Size(1210, 85), BackColor = Theme.BG_SECONDARY };

            gbParams.Controls.Add(new Label { Text = "Слот:", Location = new Point(20, 35) });
            txtSlot = new TextBox { Text = "0", Location = new Point(70, 32), Width = 50, ReadOnly = true };

            gbParams.Controls.Add(new Label { Text = "Порт:", Location = new Point(140, 35) });
            txtPort = new TextBox { Location = new Point(190, 32), Width = 70 };

            gbParams.Controls.Add(new Label { Text = "ONU:", Location = new Point(280, 35) });
            txtOnu = new TextBox { Location = new Point(330, 32), Width = 70 };

            gbParams.Controls.Add(new Label { Text = "MAC:", Location = new Point(420, 35) });
            txtMac = new TextBox { Location = new Point(470, 32), Width = 180 };

            var btnFind = CreateButton("Найти по MAC", Color.Teal, new Point(670, 30), 130, SearchByMac);

            gbParams.Controls.Add(txtSlot); gbParams.Controls.Add(txtPort); gbParams.Controls.Add(txtOnu); gbParams.Controls.Add(txtMac); gbParams.Controls.Add(btnFind);
            this.Controls.Add(gbParams);

            // Операции
            var gbOps = new GroupBox { Text = "Операции", Location = new Point(12, 270), Size = new Size(1210, 135), BackColor = Theme.BG_SECONDARY };

            btnGetMac = CreateOperationButton("MAC-адреса", Color.DodgerBlue, 20, 30, GetMac);
            btnGetStatus = CreateOperationButton("Статус LAN", Color.MediumSeaGreen, 180, 30, GetStatus);
            btnGetOptical = CreateOperationButton("Оптика ONU", Color.Teal, 340, 30, GetOptical);
            btnGetPortOptical = CreateOperationButton("Сигналы EPON", Color.Teal, 500, 30, GetPortOptical);

            btnSetSpeed = CreateOperationButton("1 Гбит/с", Color.Orange, 20, 80, SetSpeed);
            btnRebootOnu = CreateOperationButton("Перезагрузить ONU", Color.Crimson, 180, 80, RebootOnu);
            btnDeleteOnu = CreateOperationButton("Удалить ONU", Color.Crimson, 340, 80, DeleteOnu);

            btnConnect = CreateOperationButton("Подключиться", Color.LimeGreen, 720, 30, async () => await Connect());
            btnDisconnect = CreateOperationButton("Отключиться", Color.Crimson, 860, 30, Disconnect);
            btnExtraFunctions = CreateOperationButton("⚡ Доп. функции", Color.MediumPurple, 1000, 30, OpenExtraFunctions);

            gbOps.Controls.Add(btnGetMac); gbOps.Controls.Add(btnGetStatus); gbOps.Controls.Add(btnGetOptical); gbOps.Controls.Add(btnGetPortOptical);
            gbOps.Controls.Add(btnSetSpeed); gbOps.Controls.Add(btnRebootOnu); gbOps.Controls.Add(btnDeleteOnu);
            gbOps.Controls.Add(btnConnect); gbOps.Controls.Add(btnDisconnect); gbOps.Controls.Add(btnExtraFunctions);
            this.Controls.Add(gbOps);

            // Лог
            var gbLog = new GroupBox { Text = "Журнал операций", Location = new Point(12, 420), Size = new Size(1210, 420), BackColor = Theme.BG_SECONDARY };
            _logBox = new RichTextBox { Location = new Point(15, 25), Size = new Size(1180, 380), ReadOnly = true, Font = new Font("Consolas", 9.75f), BackColor = Color.White };
            gbLog.Controls.Add(_logBox);
            this.Controls.Add(gbLog);
        }

        private Button CreateButton(string text, Color color, Point loc, int width, Action action)
        {
            var btn = new Button
            {
                Text = text,
                Location = loc,
                Size = new Size(width, 32),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.75f, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => action();
            return btn;
        }

        private Button CreateOperationButton(string text, Color color, int x, int y, Action action)
        {
            return CreateButton(text, color, new Point(x, y), 145, action);
        }

        // ===================== Основной функционал =====================
        private void RefreshDeviceButtons()
        {
            _devicesPanel.Controls.Clear();
            foreach (var dev in _devices)
            {
                var btn = new Button
                {
                    Text = dev.Name,
                    Width = 135,
                    Height = 38,
                    Margin = new Padding(6),
                    Tag = dev,
                    BackColor = dev == _currentDevice ? Color.DodgerBlue : Color.LightGray,
                    ForeColor = dev == _currentDevice ? Color.White : Color.Black
                };
                btn.Click += (s, e) => DeviceButtonClicked(dev);
                _devicesPanel.Controls.Add(btn);
            }
        }

        private async void DeviceButtonClicked(Device dev)
        {
            _currentDevice = dev;
            RefreshDeviceButtons();

            if (AppConfig.AutoConnect)
                await Connect();
        }

        public async Task Connect()
        {
            if (_currentDevice == null) return;

            _telnetClient = new TelnetClient(_currentDevice, _logger);
            _telnetClient.OnStateChanged += s => Invoke(() => UpdateButtonsState());

            await _telnetClient.ConnectAsync();
        }

        public void Disconnect()
        {
            _telnetClient?.Disconnect();
            _telnetClient = null;
            UpdateButtonsState();
        }

        private void UpdateButtonsState()
        {
            bool connected = _telnetClient?.State == ConnectionState.Connected;
            btnDisconnect.Enabled = connected;
            btnExtraFunctions.Enabled = connected;
            btnGetMac.Enabled = connected;
            btnGetStatus.Enabled = connected;
            btnGetOptical.Enabled = connected;
            btnGetPortOptical.Enabled = connected;
            btnSetSpeed.Enabled = connected;
            btnRebootOnu.Enabled = connected;
            btnDeleteOnu.Enabled = connected;
        }

        public void ExecuteCommand(string command, string successMsg)
        {
            if (_telnetClient == null) return;

            Task.Run(async () =>
            {
                var (outp, ok) = await _telnetClient.ExecuteAsync(command);
                if (ok)
                    _logger.Info(successMsg);
                else
                    _logger.Error($"Ошибка: {outp}");
            });
        }

        // ===================== Реальный функционал операций =====================
        private async void GetMac()
        {
            var p = GetCurrentParams(); if (p == null) return;
            var (output, _) = await _telnetClient!.ExecuteAsync($"show mac address-table interface ePON {p.Slot}/{p.Port}:{p.OnuId}");
            var macs = MacParser.FindAll(output);
            if (macs.Count > 0)
                new MacResultsDialog(macs).ShowDialog(this);
            else
                MessageBox.Show("MAC-адреса не найдены");
        }

        private async void GetStatus()
        {
            var p = GetCurrentParams(); if (p == null) return;
            var (output, _) = await _telnetClient!.ExecuteAsync($"show epon interface epon {p.Slot}/{p.Port}:{p.OnuId} onu port 1 state");
            new ResultsDialog("Статус LAN", output).ShowDialog(this);
        }

        private async void GetOptical()
        {
            var p = GetCurrentParams(); if (p == null) return;
            var (output, _) = await _telnetClient!.ExecuteAsync($"show epon optical-transceiver-diagnosis interface epon {p.Slot}/{p.Port}:{p.OnuId}");
            var dict = OpticalParser.ParseOnu(output);
            string text = string.Join("\n", dict.Select(x => $"{x.Key}: {x.Value}"));
            new ResultsDialog("Оптические параметры", text).ShowDialog(this);
        }

        private async void GetPortOptical()
        {
            string port = GetCurrentPort();
            if (string.IsNullOrEmpty(port)) return;
            var (output, _) = await _telnetClient!.ExecuteAsync($"show epon optical-transceiver-diagnosis interface epon 0/{port}", true);
            var list = OpticalParser.ParsePort(output);
            string text = string.Join("\n", list.Select(x => $"ONU {x.OnuId}: {x.RxPower} dBm"));
            new ResultsDialog($"Оптика порта 0/{port}", text).ShowDialog(this);
        }

        private async void SetSpeed()
        {
            var p = GetCurrentParams(); if (p == null) return;
            if (MessageBox.Show($"Установить 1 Гбит/с для {p.FullId}?", "Подтверждение", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            string[] cmds = { "config", $"interface EPON{p.Slot}/{p.Port}:{p.OnuId}", "epon sla upstream pir 1000000 cir 10000", "epon sla downstream pir 1000000 cir 10000", "exit" };
            foreach (var cmd in cmds)
            {
                await _telnetClient!.ExecuteAsync(cmd);
                await Task.Delay((int)(TimeoutConfig.CommandDelay * 1000));
            }
            _logger.Info($"Скорость 1 Гбит/с установлена для {p.FullId}");
        }

        private async void RebootOnu()
        {
            var p = GetCurrentParams(); if (p == null) return;
            var dlg = new ConfirmDialog("reboot", new Dictionary<string, string> { { "Порт", p.Port }, { "ONU", p.OnuId } });
            if (dlg.ShowDialog() != DialogResult.OK) return;

            await _telnetClient!.ExecuteAsync($"epon reboot onu interface epon {p.Slot}/{p.Port}:{p.OnuId}");
            _logger.Info($"ONU {p.FullId} отправлена на перезагрузку");
        }

        private async void DeleteOnu()
        {
            var p = GetCurrentParams(); if (p == null) return;
            var dlg = new ConfirmDialog("delete", new Dictionary<string, string> { { "Порт", p.Port }, { "ONU", p.OnuId } });
            if (dlg.ShowDialog() != DialogResult.OK) return;

            string[] cmds = { "config", $"interface epon {p.Slot}/{p.Port}", $"no epon bind-onu seq {p.OnuId}", "exit", "write all" };
            foreach (var cmd in cmds)
            {
                await _telnetClient!.ExecuteAsync(cmd);
                await Task.Delay((int)(TimeoutConfig.CommandDelay * 1000));
            }
            _logger.Info($"ONU {p.FullId} удалена");
        }

        private void SearchByMac()
        {
            string mac = txtMac.Text.Trim();
            if (string.IsNullOrEmpty(mac)) return;
            _logger.Info($"Поиск по MAC: {mac}");
            MessageBox.Show("Полноценный поиск по MAC будет добавлен позже");
        }

        private void OpenExtraFunctions()
        {
            if (_telnetClient == null || _telnetClient.State != ConnectionState.Connected)
            {
                MessageBox.Show("Нет подключения", "Ошибка");
                return;
            }
            new ExtraFunctionsDialog(this).ShowDialog();
        }

        private void AddDevice()
        {
            using var dlg = new DeviceDialog();
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.ResultDevice != null)
            {
                _devices.Add(dlg.ResultDevice);
                SaveDevices();
                RefreshDeviceButtons();
            }
        }

        private void EditDevice()
        {
            if (_currentDevice == null) return;
            using var dlg = new DeviceDialog(_currentDevice);
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.ResultDevice != null)
            {
                int idx = _devices.FindIndex(d => d.Id == _currentDevice!.Id);
                if (idx >= 0) _devices[idx] = dlg.ResultDevice;
                _currentDevice = dlg.ResultDevice;
                SaveDevices();
                RefreshDeviceButtons();
            }
        }

        private void DeleteDevice()
        {
            if (_currentDevice == null || MessageBox.Show($"Удалить {_currentDevice.Name}?", "Подтверждение", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            _devices.Remove(_currentDevice);
            _currentDevice = null;
            SaveDevices();
            RefreshDeviceButtons();
        }

        private void LoadDevices()
        {
            try
            {
                if (System.IO.File.Exists(AppConfig.DevicesFile))
                {
                    string json = System.IO.File.ReadAllText(AppConfig.DevicesFile);
                    _devices = System.Text.Json.JsonSerializer.Deserialize<List<Device>>(json) ?? new();
                }
            }
            catch { }
        }

        private void SaveDevices()
        {
            try
            {
                string json = System.Text.Json.JsonSerializer.Serialize(_devices, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(AppConfig.DevicesFile, json);
            }
            catch (Exception ex) { _logger.Error($"Ошибка сохранения: {ex.Message}"); }
        }

        public ONUParams? GetCurrentParams()
        {
            string port = txtPort.Text.Trim();
            string onu = txtOnu.Text.Trim();
            if (string.IsNullOrEmpty(port) || string.IsNullOrEmpty(onu))
            {
                MessageBox.Show("Укажите порт и ONU ID");
                return null;
            }
            return new ONUParams(txtSlot.Text, port, onu);
        }

        public string GetCurrentPort() => txtPort.Text.Trim();

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _telnetClient?.Disconnect();
            base.OnFormClosing(e);
        }
    }
}