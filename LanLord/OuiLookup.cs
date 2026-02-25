using System;
using System.Collections.Generic;
using System.IO;

namespace LanLord
{
#pragma warning disable
    public static class OuiLookup
    {
        private static Dictionary<string, string> _table;
        private static readonly object _lock = new object();
        private const string OuiFileName = "standards-oui.ieee.org.txt";

        /// <summary>
        /// Returns the vendor name for a MAC address, or empty string if not found.
        /// Returns "[Randomized MAC]" for locally-administered (privacy/random) addresses.
        /// </summary>
        public static string Lookup(string mac)
        {
            if (_table == null) load();
            // Normalise: strip separators, take first 6 hex chars (OUI prefix)
            string prefix = mac.Replace("-", "").Replace(":", "").Replace(" ", "").ToUpperInvariant();
            if (prefix.Length < 6) return "";
            // Detect locally-administered bit (bit 1 of first byte) → randomized MAC
            try
            {
                byte firstByte = Convert.ToByte(prefix.Substring(0, 2), 16);
                if ((firstByte & 0x02) != 0)
                    return "[Randomized MAC]";
            }
            catch { }
            prefix = prefix.Substring(0, 6);
            string vendor;
            return _table.TryGetValue(prefix, out vendor) ? vendor : "";
        }

        private static void load()
        {
            lock (_lock)
            {
                if (_table != null) return;
                var table = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OuiFileName);
                if (!File.Exists(path)) { _table = table; return; }
                try
                {
                    // Parse lines like: "286FB9     (base 16)		Nokia Shanghai Bell Co., Ltd."
                    const string marker = "(base 16)";
                    foreach (string line in File.ReadLines(path))
                    {
                        int idx = line.IndexOf(marker, StringComparison.Ordinal);
                        if (idx < 0) continue;
                        string prefix = line.Substring(0, idx).Trim();
                        if (prefix.Length != 6) continue;
                        string vendor = line.Substring(idx + marker.Length).Trim();
                        if (vendor.Length > 0)
                            table[prefix] = vendor;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("OuiLookup load error: " + ex.Message);
                }
                _table = table;
            }
        }
    }
#pragma warning restore
}
