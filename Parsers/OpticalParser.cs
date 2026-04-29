using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BDCOM.OLT.Manager.Parsers
{
    public static class OpticalParser
    {
        public static Dictionary<string, double> ParseOnu(string output)
        {
            output = CleanOutput(output);

            var result = new Dictionary<string, double>();

            var patterns = new[]
            {
                (@"RxPower\(dBm\)\s*[:=]?\s*([-\d.]+)", "RxPower"),
                (@"TxPower\(dBm\)\s*[:=]?\s*([-\d.]+)", "TxPower"),
                (@"Temperature\(C\)\s*[:=]?\s*([-\d.]+)", "Temperature"),
                (@"Voltage\(V\)\s*[:=]?\s*([-\d.]+)", "Voltage"),
                (@"BiasCurrent\(mA\)\s*[:=]?\s*([-\d.]+)", "BiasCurrent"),
                (@"Rx Power\s*[:=]?\s*([-\d.]+)", "RxPower"),
                (@"Tx Power\s*[:=]?\s*([-\d.]+)", "TxPower")
            };

            foreach (var (regex, key) in patterns)
            {
                var match = Regex.Match(output, regex, RegexOptions.IgnoreCase);
                if (match.Success && double.TryParse(match.Groups[1].Value.Trim(), out double value))
                {
                    result[key] = value;
                }
            }

            return result;
        }

        public static List<(string OnuId, double RxPower)> ParsePort(string output)
        {
            output = CleanOutput(output);

            var results = new List<(string, double)>();

            // Основной паттерн для твоей прошивки
            var matches = Regex.Matches(output, @"epon\d*/\d*:?(\d+)\s+([-\d.]+)", RegexOptions.IgnoreCase);

            foreach (Match m in matches)
            {
                if (int.TryParse(m.Groups[1].Value, out int onuId) && 
                    double.TryParse(m.Groups[2].Value, out double power))
                {
                    results.Add((onuId.ToString(), power));
                }
            }

            return results.OrderBy(x => int.Parse(x.Item1)).ToList();
        }

        // Улучшенная очистка от мусора
        public static string CleanOutput(string output)
        {
            if (string.IsNullOrEmpty(output))
                return output;

            // Убираем только backspace
            output = Regex.Replace(output, @"[\b\x08]+", "");

            // Убираем длинные последовательности одинаковых символов (много пробелов или )
            output = Regex.Replace(output, @"(\s)\1{5,}", " ");
            output = Regex.Replace(output, @"()\1{3,}", "");

            // Убираем строки "Unknown command" и явные повторы команды
            output = Regex.Replace(output, @"Unknown command.*$", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            output = Regex.Replace(output, @"^.*optical-transceiver-diagnosis.*$", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            return output.Trim();
        }
    }
}