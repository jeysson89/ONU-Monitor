using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BDCOM.OLT.Manager.Parsers
{
    public static class MacParser
    {
        public static List<string> FindAll(string text)
        {
            var results = new HashSet<string>();
            var patterns = new[]
            {
                @"([0-9a-fA-F]{2}[:-]){5}[0-9a-fA-F]{2}",
                @"[0-9a-fA-F]{4}\.[0-9a-fA-F]{4}\.[0-9a-fA-F]{4}",
                @"[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}"
            };

            foreach (var pattern in patterns)
            {
                foreach (Match match in Regex.Matches(text, pattern))
                {
                    string normalized = Normalize(match.Value);
                    if (!string.IsNullOrEmpty(normalized))
                        results.Add(normalized);
                }
            }
            return results.OrderBy(x => x).ToList();
        }

        private static string Normalize(string mac)
        {
            string cleaned = Regex.Replace(mac, @"[^0-9a-fA-F]", "").ToLower();
            if (cleaned.Length != 12) return "";
            return $"{cleaned.Substring(0, 4)}.{cleaned.Substring(4, 4)}.{cleaned.Substring(8, 4)}";
        }
    }
}