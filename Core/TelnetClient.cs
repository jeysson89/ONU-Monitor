using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BDCOM.OLT.Manager.Config;
using BDCOM.OLT.Manager.Enums;
using BDCOM.OLT.Manager.Models;

namespace BDCOM.OLT.Manager.Core
{
    public class TelnetClient
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        private readonly Device _device;
        private readonly Logger _logger;
        private ConnectionState _state = ConnectionState.Disconnected;
        private readonly object _lock = new();
        private System.Threading.Timer? _reconnectTimer;   // Исправлено: полное имя
        private int _reconnectAttempt = 0;

        public event Action<ConnectionState>? OnStateChanged;

        public ConnectionState State => _state;

        public TelnetClient(Device device, Logger logger)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> ConnectAsync()
        {
            lock (_lock)
            {
                if (_state == ConnectionState.Connected) return true;
                _state = ConnectionState.Connecting;
                OnStateChanged?.Invoke(_state);
            }

            _logger.Info($"Подключение к {_device.Name} ({_device.Ip}:{_device.Port})...");

            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(_device.Ip, _device.Port);
                _stream = _client.GetStream();

                if (!await AuthenticateAsync())
                    throw new Exception("Authentication failed");

                lock (_lock)
                {
                    _state = ConnectionState.Connected;
                    _reconnectAttempt = 0;
                    OnStateChanged?.Invoke(_state);
                }

                _logger.Info($"Успешно подключено к {_device.Name}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка подключения к {_device.Name}: {ex.Message}");
                Cleanup();
                _state = ConnectionState.Error;
                OnStateChanged?.Invoke(_state);
                ScheduleReconnect();
                return false;
            }
        }

        private async Task<bool> AuthenticateAsync()
        {
            await ReadUntil("Username:");
            await WriteLine(_device.Username);

            await ReadUntil("Password:");
            await WriteLine(_device.Password);

            await Task.Delay(1000);
            string output = await ReadAvailable();

            if (output.Contains(">"))
            {
                await WriteLine("enable");
                await Task.Delay(1000);
                string enableOut = await ReadAvailable();

                string pwd = string.IsNullOrEmpty(_device.EnablePassword) ? _device.Password : _device.EnablePassword;
                if (enableOut.Contains("Password"))
                {
                    await WriteLine(pwd);
                    await Task.Delay(1000);
                    enableOut = await ReadAvailable();
                }
                return enableOut.Contains("#");
            }
            return output.Contains("#");
        }

        public async Task<(string output, bool success)> ExecuteAsync(string command, bool handleMore = true)
        {
            lock (_lock)
                if (_state != ConnectionState.Connected)
                    throw new InvalidOperationException("Not connected");

            var start = DateTime.Now;
            _logger.Info($"Выполнение команды: {command}");

            try
            {
                await WriteLine(command);
                string output = await ReadUntilPrompt(handleMore);
                var duration = (int)(DateTime.Now - start).TotalMilliseconds;
                _logger.Info($"Команда выполнена за {duration} мс");
                return (output, true);
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка выполнения команды '{command}': {ex.Message}");
                if (ex.Message.Contains("closed") || ex.Message.Contains("timeout"))
                    ScheduleReconnect();
                return (ex.Message, false);
            }
        }

        private async Task<string> ReadUntilPrompt(bool handleMore)
        {
            var sb = new StringBuilder();
            var buffer = new byte[16384];

            while (true)
            {
                if (_stream!.DataAvailable)
                {
                    int read = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    string text = Encoding.UTF8.GetString(buffer, 0, read);
                    sb.Append(text);

                    if (text.Contains("#")) break;
                    if (handleMore && text.Contains("--More--")) await Write(" ");
                    if (text.Contains("(y/n)")) await WriteLine("y");
                }
                else
                {
                    await Task.Delay(30);
                }
            }
            return sb.ToString();
        }

        private async Task ReadUntil(string marker)
        {
            var timeout = DateTime.Now.AddSeconds(TimeoutConfig.AuthTimeout);
            var sb = new StringBuilder();
            var buffer = new byte[4096];

            while (DateTime.Now < timeout)
            {
                if (_stream!.DataAvailable)
                {
                    int read = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, read));
                    if (sb.ToString().Contains(marker)) return;
                }
                await Task.Delay(50);
            }
            throw new TimeoutException($"Timeout waiting for '{marker}'");
        }

        private async Task<string> ReadAvailable()
        {
            if (_stream == null || !_stream.DataAvailable) return "";
            var buffer = new byte[8192];
            int read = await _stream.ReadAsync(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, read);
        }

        private async Task WriteLine(string text) => await Write(text + "\r\n");

        private async Task Write(string text)
        {
            if (_stream == null) return;
            var data = Encoding.UTF8.GetBytes(text);
            await _stream.WriteAsync(data, 0, data.Length);
            await _stream.FlushAsync();
        }

        private void ScheduleReconnect()
        {
            if (!AppConfig.AutoReconnect || _reconnectAttempt >= TimeoutConfig.ReconnectAttempts) return;

            _reconnectAttempt++;
            _logger.Info($"Попытка переподключения {_reconnectAttempt}/{TimeoutConfig.ReconnectAttempts}...");

            _reconnectTimer?.Dispose();
            _reconnectTimer = new System.Threading.Timer(async _ =>
            {
                try { await ConnectAsync(); }
                catch { }
            }, null, TimeoutConfig.ReconnectDelay * 1000, Timeout.Infinite);
        }

        public void Disconnect()
        {
            _reconnectTimer?.Dispose();
            Cleanup();
            _state = ConnectionState.Disconnected;
            OnStateChanged?.Invoke(_state);
        }

        private void Cleanup()
        {
            try
            {
                _stream?.Close();
                _client?.Close();
            }
            catch { }
            _stream = null;
            _client = null;
        }
    }
}