using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BDCOM.OLT.Manager.Config;

namespace BDCOM.OLT.Manager.Core
{
    public class Logger
    {
        private readonly RichTextBox _logBox;
        private readonly string _logFile;

        public Logger(RichTextBox logBox)
        {
            _logBox = logBox ?? throw new ArgumentNullException(nameof(logBox));
            _logFile = AppConfig.LogFile;
            Directory.CreateDirectory(Path.GetDirectoryName(_logFile)!);
        }

        public void Info(string msg) => Log("INFO", msg);
        public void Warning(string msg) => Log("WARNING", msg);
        public void Error(string msg) => Log("ERROR", msg);

        private void Log(string level, string msg)
        {
            string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {msg}";
            try
            {
                File.AppendAllText(_logFile, line + Environment.NewLine);
            }
            catch { }

            if (_logBox.InvokeRequired)
            {
                _logBox.Invoke(() => AppendLog(line, level));
            }
            else
            {
                AppendLog(line, level);
            }
        }

        private void AppendLog(string line, string level)
        {
            _logBox.SelectionColor = level switch
            {
                "ERROR" => Color.Red,
                "WARNING" => Color.Orange,
                _ => Color.Black
            };
            _logBox.AppendText(line + Environment.NewLine);
            _logBox.ScrollToCaret();
        }
    }
}