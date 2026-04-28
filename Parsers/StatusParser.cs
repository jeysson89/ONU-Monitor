using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BDCOM.OLT.Manager.Parsers
{
    public static class StatusParser
    {
        public static Dictionary<string, string> Parse(string output)
        {
            var result = new Dictionary<string, string>();
            var patterns = new (string regex, string key)[]
            {
                (@"Hardware state is (\S+)", "hardware_state"),
                (@"Admin state is (\S+)", "admin_state"),
                (@"Flow-Control is (\S+)", "flow_control"),
                (@"Duplex is (\S+)", "duplex"),
                (@"Speed is (\S+)", "speed"),
                (@"Storm-Control is (\S+)", "storm_control"),
                (@"State\s*:\s*(\w+)", "state"),
                (@"Link\s*:\s*(\w+)", "link"),
            };

            foreach (var (regex, key) in patterns)
            {
                var match = Regex.Match(output, regex, RegexOptions.IgnoreCase);
                if (match.Success)
                    result[key] = match.Groups[1].Value.Trim();
            }
            return result;
        }
    }
}