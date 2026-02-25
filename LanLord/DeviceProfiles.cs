using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Xml;

namespace LanLord
{
#pragma warning disable
    public class DeviceProfile
    {
        public string mac;
        public string name;
        public int capDown;
        public int capUp;
        public bool isBlocked;
    }

    public class DeviceProfiles
    {
        private const string FileName = "profiles.xml";
        private Dictionary<string, DeviceProfile> profiles = new Dictionary<string, DeviceProfile>();

        public DeviceProfiles()
        {
            load();
        }

        public void Save(PC pc)
        {
            string key = pc.mac.ToString();
            if (!profiles.ContainsKey(key))
                profiles[key] = new DeviceProfile { mac = key };
            profiles[key].name = pc.name;
            profiles[key].capDown = pc.capDown;
            profiles[key].capUp = pc.capUp;
            profiles[key].isBlocked = pc.isBlocked;
            persist();
        }

        public bool Apply(PC pc)
        {
            string key = pc.mac.ToString();
            if (!profiles.ContainsKey(key)) return false;
            DeviceProfile p = profiles[key];
            if (p.name != null && p.name != "") pc.name = p.name;
            pc.capDown = p.capDown;
            pc.capUp = p.capUp;
            pc.isBlocked = p.isBlocked;
            // When restoring a blocked device, disable forwarding immediately
            if (p.isBlocked) pc.redirect = false;
            return true;
        }

        public DeviceProfile Get(string mac)
        {
            DeviceProfile p;
            return profiles.TryGetValue(mac, out p) ? p : null;
        }

        private void load()
        {
            if (!File.Exists(FileName)) return;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(FileName);
                foreach (XmlNode node in doc.SelectNodes("/profiles/device"))
                {
                    DeviceProfile p = new DeviceProfile
                    {
                        mac = node.Attributes["mac"]?.Value ?? "",
                        name = node.Attributes["name"]?.Value ?? "",
                        capDown = int.Parse(node.Attributes["capDown"]?.Value ?? "0"),
                        capUp = int.Parse(node.Attributes["capUp"]?.Value ?? "0"),
                        isBlocked = bool.Parse(node.Attributes["isBlocked"]?.Value ?? "false")
                    };
                    if (p.mac != "") profiles[p.mac] = p;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DeviceProfiles load error: " + ex.Message);
            }
        }

        private void persist()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("profiles");
                doc.AppendChild(root);
                foreach (DeviceProfile p in profiles.Values)
                {
                    XmlElement e = doc.CreateElement("device");
                    e.SetAttribute("mac", p.mac);
                    e.SetAttribute("name", p.name ?? "");
                    e.SetAttribute("capDown", p.capDown.ToString());
                    e.SetAttribute("capUp", p.capUp.ToString());
                    e.SetAttribute("isBlocked", p.isBlocked.ToString());
                    root.AppendChild(e);
                }
                doc.Save(FileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("DeviceProfiles save error: " + ex.Message);
            }
        }
    }
#pragma warning restore
}
