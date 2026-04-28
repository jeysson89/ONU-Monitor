using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BDCOM.OLT.Manager.Parsers
{
    public static class OpticalParser
    {
        public static Dictionary<string, double> ParseOnu(string output)
        {
            var result = new Dictionary<string, double>();
            var patterns = new (string regex, string key)[]
            {
                (@"RxPower\(dBm\)\s*:\s*([-\d.]+)", "rx_power"),
                (@"TxPower\(dBm\)\s*:\s*([-\d.]+)", "tx_power"),
                (@"Temperature\(C\)\s*:\s*([-\d.]+)", "temperature"),
                (@"Voltage\(V\)\s*:\s*([-\d.]+)", "voltage"),
                (@"BiasCurrent\(mA\)\s*:\s*([-\d.]+)", "bias_current"),
                (@"Rx Power\s*:\s*([-\d.]+)", "rx_power"),
                (@"Tx Power\s*:\s*([-\d.]+)", "tx_power"),
            };

            foreach (var (regex, key) in patterns)
            {
                var match = Regex.Match(output, regex, RegexOptions.IgnoreCase);
                if (match.Success && double.TryParse(match.Groups[1].Value, out double value))
                    result[key] = value;
            }
            return result;
        }

        public static List<(string OnuId, double RxPower)> ParsePort(string output)
        {
            var results = new List<(string, double)>();
            var matches = Regex.Matches(output, @"(\d+)\s+([-\d.]+)\s*dBm", RegexOptions.IgnoreCase);

            foreach (Match m in matches)
            {
                if (int.TryParse(m.Groups[1].Value, out int id) && double.TryParse(m.Groups[2].Value, out double power))
                    results.Add((id.ToString(), power));
            }
            return results.OrderBy(x => int.Parse(x.Item1)).ToList();
        }
    }
}