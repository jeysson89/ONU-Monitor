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
            this.Size = new Size(1300, 860);
            this.MinimumSize = new Size(1150, 720);
            this.BackColor = Theme.BG_PRIMARY;
            this.Font = new Font("Segoe UI", 10f);
            this.StartPosition = FormStartPosition.CenterScreen;

            // ==================== Устройства ====================
            var gbDevices = new GroupBox
            {
                Text = "Устройства",
                Location = new Point(12, 12),
                Size = new Size(1270, 180),           // Увеличили высоту
                BackColor = Theme.BG_SECONDARY,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _devicesPanel = new FlowLayoutPanel
            {
                Location = new Point(15, 30),
                Size = new Size(1240, 75),
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            gbDevices.Controls.Add(_devicesPanel);

            // Кнопки управления устройствами
            btnAddDevice = CreateButton("+ Добавить", Color.LimeGreen, new Point(15, 115), 115, AddDevice);
            btnEditDevice = CreateButton("✎ Изменить", Color.FromArgb(0, 122, 204), new Point(140, 115), 115, EditDevice);
            btnDeleteDevice = CreateButton("🗑 Удалить", Color.IndianRed, new Point(265, 115), 115, DeleteDevice);

            // Переносим кнопки подключения после "Удалить"
            btnConnect = CreateButton("Подключиться", Color.LimeGreen, new Point(390, 115), 130, async () => await Connect());
            btnDisconnect = CreateButton("Отключиться", Color.Crimson, new Point(530, 115), 130, Disconnect);
            btnExtraFunctions = CreateButton("⚡ Доп. функции", Color.MediumPurple, new Point(670, 115), 150, OpenExtraFunctions);

            gbDevices.Controls.Add(btnAddDevice);
            gbDevices.Controls.Add(btnEditDevice);
            gbDevices.Controls.Add(btnDeleteDevice);
            gbDevices.Controls.Add(btnConnect);
            gbDevices.Controls.Add(btnDisconnect);
            gbDevices.Controls.Add(btnExtraFunctions);

            this.Controls.Add(gbDevices);

            // ==================== Параметры ONU ====================
            var gbParams = new GroupBox
            {
                Text = "Параметры ONU",
                Location = new Point(12, 205),
                Size = new Size(1270, 90),
                BackColor = Theme.BG_SECONDARY,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            int x = 20;
            gbParams.Controls.Add(new Label { Text = "Слот:", Location = new Point(x, 35), AutoSize = true });
            txtSlot = new TextBox { Text = "0", Location = new Point(x + 50, 32), Width = 60, ReadOnly = true };
            x += 130;

            gbParams.Controls.Add(new Label { Text = "Порт:", Location = new Point(x, 35), AutoSize = true });
            txtPort = new TextBox { Location = new Point(x + 50, 32), Width = 80 };
            x += 150;

            gbParams.Controls.Add(new Label { Text = "ONU:", Location = new Point(x, 35), AutoSize = true });
            txtOnu = new TextBox { Location = new Point(x + 50, 32), Width = 80 };
            x += 150;

            gbParams.Controls.Add(new Label { Text = "MAC:", Location = new Point(x, 35), AutoSize = true });
            txtMac = new TextBox { Location = new Point(x + 50, 32), Width = 200 };
            x += 270;

            var btnFindMac = CreateButton("Найти по MAC", Color.Teal, new Point(x, 30), 150, SearchByMac);

            gbParams.Controls.Add(txtSlot);
            gbParams.Controls.Add(txtPort);
            gbParams.Controls.Add(txtOnu);
            gbParams.Controls.Add(txtMac);
            gbParams.Controls.Add(btnFindMac);
            this.Controls.Add(gbParams);

            // ==================== Операции ====================
            var gbOps = new GroupBox
            {
                Text = "Операции",
                Location = new Point(12, 310),
                Size = new Size(1270, 155),
                BackColor = Theme.BG_SECONDARY,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            int opX = 20;
            btnGetMac = CreateOperationButton("MAC-адреса", Color.DodgerBlue, opX, 35, GetMac); opX += 160;
            btnGetStatus = CreateOperationButton("Статус LAN", Color.MediumSeaGreen, opX, 35, GetStatus); opX += 160;
            btnGetOptical = CreateOperationButton("Оптика ONU", Color.Teal, opX, 35, GetOptical); opX += 160;
            btnGetPortOptical = CreateOperationButton("Сигналы EPON", Color.Teal, opX, 35, GetPortOptical); opX += 160;

            opX = 20;
            btnSetSpeed = CreateOperationButton("1 Гбит/с", Color.Orange, opX, 90, SetSpeed); opX += 160;
            btnRebootOnu = CreateOperationButton("Перезагрузить ONU", Color.Crimson, opX, 90, RebootOnu); opX += 160;
            btnDeleteOnu = CreateOperationButton("Удалить ONU", Color.Crimson, opX, 90, DeleteOnu);

            gbOps.Controls.Add(btnGetMac);
            gbOps.Controls.Add(btnGetStatus);
            gbOps.Controls.Add(btnGetOptical);
            gbOps.Controls.Add(btnGetPortOptical);
            gbOps.Controls.Add(btnSetSpeed);
            gbOps.Controls.Add(btnRebootOnu);
            gbOps.Controls.Add(btnDeleteOnu);

            this.Controls.Add(gbOps);

            // ==================== Журнал ====================
            var gbLog = new GroupBox
            {
                Text = "Журнал операций",
                Location = new Point(12, 480),
                Size = new Size(1270, 350),
                BackColor = Theme.BG_SECONDARY,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            _logBox = new RichTextBox
            {
                Location = new Point(15, 25),
                Size = new Size(1240, 310),
                ReadOnly = true,
                Font = new Font("Consolas", 9.75f),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
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

        // ===================== Основная логика =====================
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

        public void ExecuteCommand(string command, string successMessage)
        {
            if (_telnetClient == null) return;

            Task.Run(async () =>
            {
                var (output, success) = await _telnetClient.ExecuteAsync(command);
                if (success)
                    _logger.Info(successMessage);
                else
                    _logger.Error($"Ошибка: {output}");
            });
        }

        public ONUParams? GetCurrentParams()
        {
            string port = txtPort.Text.Trim();
            string onu = txtOnu.Text.Trim();
            if (string.IsNullOrEmpty(port) || string.IsNullOrEmpty(onu))
            {
                MessageBox.Show("Укажите порт и ONU ID", "Ошибка");
                return null;
            }
            return new ONUParams(txtSlot.Text, port, onu);
        }

        public string GetCurrentPort() => txtPort.Text.Trim();

        public TelnetClient? GetTelnetClient() => _telnetClient;

        // ===================== Реальные команды =====================
        private async void GetMac()
        {
            var p = GetCurrentParams(); if (p == null) return;
            var (output, _) = await _telnetClient!.ExecuteAsync($"show mac address-table interface ePON {p.Slot}/{p.Port}:{p.OnuId}");
            var macs = MacParser.FindAll(output);
            if (macs.Count > 0)
                new MacResultsDialog(macs).ShowDialog(this);
            else
                MessageBox.Show("MAC-адреса не найдены", "Результат");
        }

        private async void GetStatus()
        {
            var p = GetCurrentParams(); if (p == null) return;
            var (output, _) = await _telnetClient!.ExecuteAsync($"show epon interface epon {p.Slot}/{p.Port}:{p.OnuId} onu port 1 state");
            new ResultsDialog("Статус LAN", output).ShowDialog(this);
        }

        private async void GetOptical()
        {
            var p = GetCurrentParams();
            if (p == null) return;

            string[] cmds = {
                $"optical-transceiver-diagnosis interface epon {p.Slot}/{p.Port}:{p.OnuId}",
                $"show epon optical-transceiver-diagnosis interface epon {p.Slot}/{p.Port}:{p.OnuId}"
            };

            string rawOutput = "";
            bool success = false;

            foreach (var cmd in cmds)
            {
                _logger.Info($"Запрос оптических параметров ONU: {cmd}");
                (rawOutput, success) = await _telnetClient!.ExecuteAsync(cmd);

                if (success && (rawOutput.Contains("RxPower") || rawOutput.Contains("epon0/")))
                    break;
            }

            if (success && !string.IsNullOrWhiteSpace(rawOutput))
            {
                // Мягкая очистка — только убираем самый явный мусор
                string cleaned = OpticalParser.CleanOutput(rawOutput);

                var dict = OpticalParser.ParseOnu(rawOutput);

                string displayText;

                if (dict.Count > 0)
                {
                    displayText = string.Join("\n", dict.Select(kv => $"{kv.Key}: {kv.Value}"));
                }
                else
                {
                    // Если парсер ничего не нашёл — показываем очищенный raw вывод
                    displayText = cleaned;
                }

                new ResultsDialog("Оптические параметры ONU", displayText).ShowDialog(this);
            }
            else
            {
                MessageBox.Show("Не удалось получить оптические параметры или ответ пустой", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void GetPortOptical()
        {
            string port = GetCurrentPort();
            if (string.IsNullOrEmpty(port)) return;

            string cmd1 = $"show epon optical-transceiver-diagnosis interface epon 0/{port}";
            string cmd2 = $"show epon optical-transceiver-diagnosis interface epon 0/{port}";

            _logger.Info($"Запрос оптики порта 0/{port}");

            var (output, success) = await _telnetClient!.ExecuteAsync(cmd1, true);

            if (!success || string.IsNullOrWhiteSpace(output))
            {
                _logger.Info($"Повторная попытка: {cmd2}");
                (output, success) = await _telnetClient.ExecuteAsync(cmd2, true);
            }

            if (success)
            {
                var list = OpticalParser.ParsePort(output);
                if (list.Count > 0)
                {
                    string text = string.Join("\n", list.Select(x => $"ONU {x.OnuId}: {x.RxPower} dBm"));
                    new ResultsDialog($"Оптика порта 0/{port}", text).ShowDialog(this);
                }
                else
                {
                    new ResultsDialog($"Оптика порта 0/{port}", output).ShowDialog(this);
                }
            }
            else
            {
                MessageBox.Show("Не удалось получить данные по оптике порта", "Ошибка");
            }
        }

        private async void SetSpeed()
        {
            var p = GetCurrentParams();
            if (p == null) return;

            if (MessageBox.Show($"Установить скорость 1 Гбит/с (1000 Mbps) для ONU {p.FullId}?\n\n" +
                                "Это изменит upstream и downstream PIR.", 
                                "Подтверждение", 
                                MessageBoxButtons.YesNo, 
                                MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _logger.Info($"Начинаем установку 1 Гбит/с для ONU {p.FullId}");

            var commands = new[]
            {
                "enable",
                "config",
                $"interface epon {p.Slot}/{p.Port}:{p.OnuId}",
                "epon sla upstream pir 1000000 cir 10000",
                "epon sla downstream pir 1000000 cir 10000",
                "exit",
                "write all"
            };

            bool allSuccess = true;

            foreach (var cmd in commands)
            {
                _logger.Info($"Выполнение: {cmd}");
                var (output, success) = await _telnetClient!.ExecuteAsync(cmd);

                if (!success)
                {
                    _logger.Error($"Команда не выполнена: {cmd}");
                    allSuccess = false;
                }

                // Небольшая задержка между командами
                await Task.Delay((int)(TimeoutConfig.CommandDelay * 1000));
            }

            if (allSuccess)
                _logger.Info($"Скорость 1 Гбит/с успешно установлена для ONU {p.FullId}");
            else
                _logger.Error($"Установка 1 Гбит/с завершена с ошибками для ONU {p.FullId}");
        }

                private async void RebootOnu()
        {
            var p = GetCurrentParams();
            if (p == null) return;

            // Первое подтверждение
            var confirmDlg = new ConfirmDialog("reboot", new Dictionary<string, string>
            {
                { "Порт", p.Port },
                { "ONU", p.OnuId },
                { "ONU ID", p.FullId }
            });

            if (confirmDlg.ShowDialog(this) != DialogResult.OK)
            {
                _logger.Info("Перезагрузка ONU отменена на первом подтверждении");
                return;
            }

            // Второе подтверждение — ввод слова "АДЕКВАТНЫЙ"
            var secondConfirmDlg = new SecondConfirmDialog("reboot", p.FullId);
            if (secondConfirmDlg.ShowDialog(this) != DialogResult.OK)
            {
                _logger.Info("Перезагрузка ONU отменена — не введено слово подтверждения");
                return;
            }

            // Выполняем команду
            _logger.Info($"Отправка команды перезагрузки ONU {p.FullId}...");

            string command = $"epon reboot onu interface epon {p.Slot}/{p.Port}:{p.OnuId}";

            var (output, success) = await _telnetClient!.ExecuteAsync(command);

            if (success)
            {
                _logger.Info($"Команда перезагрузки ONU {p.FullId} успешно отправлена");
                MessageBox.Show($"ONU {p.FullId} отправлена на перезагрузку.\n\nОбычно перезагрузка занимает 30–90 секунд.", 
                                "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                _logger.Error($"Не удалось отправить команду перезагрузки ONU {p.FullId}");
                MessageBox.Show("Не удалось отправить команду перезагрузки.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void DeleteOnu()
        {
            var p = GetCurrentParams(); if (p == null) return;
            var dlg = new ConfirmDialog("delete", new Dictionary<string, string> { { "Порт", p.Port }, { "ONU", p.OnuId } });
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var cmds = new[] { "config", $"interface epon {p.Slot}/{p.Port}", $"no epon bind-onu seq {p.OnuId}", "exit", "write all" };
            foreach (var cmd in cmds)
            {
                await _telnetClient!.ExecuteAsync(cmd);
                await Task.Delay((int)(TimeoutConfig.CommandDelay * 1000));
            }
            _logger.Info($"ONU {p.FullId} удалена");
        }

                private async void SearchByMac()
        {
            string macInput = txtMac.Text.Trim();

            if (string.IsNullOrWhiteSpace(macInput))
            {
                MessageBox.Show("Введите MAC-адрес для поиска", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_telnetClient == null || _telnetClient.State != ConnectionState.Connected)
            {
                MessageBox.Show("Сначала подключитесь к OLT", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string normalizedMac = MacParser.Normalize(macInput);
                if (string.IsNullOrEmpty(normalizedMac))
                {
                    MessageBox.Show("Неверный формат MAC-адреса.\nПример: 1c3b.f38e.3e65 или 1C:3B:F3:8E:3E:65", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _logger.Info($"Поиск по MAC: {normalizedMac}");

                // Основная команда для поиска по MAC (как в большинстве BDCOM)
                string command = $"show mac address-table {normalizedMac}";

                var (output, success) = await _telnetClient.ExecuteAsync(command);

                if (!success)
                {
                    MessageBox.Show("Не удалось выполнить команду поиска по MAC", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Парсим результат
                var foundMacs = MacParser.FindAll(output);

                if (foundMacs.Count > 0)
                {
                    // Если нашли — показываем подробный результат
                    string resultText = "Найденные записи:\n\n" + 
                                      string.Join("\n", output.Split('\n')
                                        .Where(line => line.Contains(normalizedMac, StringComparison.OrdinalIgnoreCase) 
                                                    || line.Contains("ePON", StringComparison.OrdinalIgnoreCase))
                                        .Select(line => line.Trim()));

                    new ResultsDialog($"Результат поиска по MAC: {normalizedMac}", resultText).ShowDialog(this);
                }
                else
                {
                    // Если ничего не нашли
                    MessageBox.Show($"MAC-адрес {normalizedMac} не найден в таблице коммутации OLT.", 
                                    "Результат поиска", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка при поиске по MAC: {ex.Message}");
                MessageBox.Show($"Произошла ошибка при поиске:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenExtraFunctions()
        {
            if (_telnetClient == null || _telnetClient.State != ConnectionState.Connected)
            {
                MessageBox.Show("Нет активного подключения", "Ошибка");
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _telnetClient?.Disconnect();
            base.OnFormClosing(e);
        }
    }
}