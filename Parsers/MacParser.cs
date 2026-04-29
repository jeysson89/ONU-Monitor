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

        public static string Normalize(string mac)
        {
            if (string.IsNullOrEmpty(mac))
                return "";

            // Убираем все разделители и приводим к нижнему регистру
            string cleaned = Regex.Replace(mac, @"[^0-9a-fA-F]", "").ToLowerInvariant();

            if (cleaned.Length != 12)
                return "";

            // Возвращаем в формате xxxx.xxxx.xxxx
            return $"{cleaned.Substring(0, 4)}.{cleaned.Substring(4, 4)}.{cleaned.Substring(8, 4)}";
        }
    }
}