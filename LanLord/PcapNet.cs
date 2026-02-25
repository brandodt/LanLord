using System;
using System.IO;
using System.Runtime.InteropServices;

// Pure-managed P/Invoke replacement for PcapNet.dll (C++/CLI mixed-mode, .NET Framework 2.0).
// .NET 8 cannot load mixed-mode assemblies compiled for .NET Framework; this file replicates
// the exact public API (CPcapNet, Driver, packet_headers) by calling wpcap.dll directly.
// Npcap in WinPcap-compatible mode exposes wpcap.dll in C:\Windows\SysWOW64 (x86 process).

namespace PcapNet
{
#pragma warning disable
    public class packet_headers
    {
        public uint caplen;
        public uint len;
    }

    public class CPcapNet : IDisposable
    {
        public IntPtr nicHandle;

        [DllImport("wpcap", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr pcap_open_live(string device, int snaplen, int promisc, int to_ms, System.Text.StringBuilder errbuf);

        [DllImport("wpcap", CallingConvention = CallingConvention.Cdecl)]
        private static extern void pcap_close(IntPtr p);

        [DllImport("wpcap", CallingConvention = CallingConvention.Cdecl)]
        private static extern int pcap_next_ex(IntPtr p, ref IntPtr pkt_header, ref IntPtr pkt_data);

        [DllImport("wpcap", CallingConvention = CallingConvention.Cdecl)]
        private static extern int pcap_sendpacket(IntPtr p, byte[] buf, int size);

        [DllImport("wpcap", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int pcap_compile(IntPtr p, ref bpf_program fp, string filter, int optimize, uint netmask);

        [DllImport("wpcap", CallingConvention = CallingConvention.Cdecl)]
        private static extern int pcap_setfilter(IntPtr p, ref bpf_program fp);

        [DllImport("wpcap", CallingConvention = CallingConvention.Cdecl)]
        private static extern void pcap_freecode(ref bpf_program fp);

        [StructLayout(LayoutKind.Sequential)]
        private struct bpf_program
        {
            public uint bf_len;
            public IntPtr bf_insns;
        }

        // pcap_pkthdr on 32-bit Windows: timeval (tv_sec 4 + tv_usec 4) + caplen 4 + len 4 = 16 bytes
        [StructLayout(LayoutKind.Sequential)]
        private struct pcap_pkthdr
        {
            public uint tv_sec;
            public uint tv_usec;
            public uint caplen;
            public uint len;
        }

        public CPcapNet()
        {
            nicHandle = IntPtr.Zero;
        }

        // adapterId is NetworkInterface.Id — a GUID string like "{XXXXXXXX-...}" on Windows.
        // WinPcap/Npcap device name format: \Device\NPF_{GUID}
        public bool pcapnet_openLive(string adapterId, int snaplen, int promisc, int to_ms, string errbuf)
        {
            if (nicHandle != IntPtr.Zero)
                return true;
            string device = @"\Device\NPF_" + adapterId;
            var sb = new System.Text.StringBuilder(256);
            nicHandle = pcap_open_live(device, snaplen, promisc, to_ms, sb);
            return nicHandle != IntPtr.Zero;
        }

        public void pcapnet_close()
        {
            if (nicHandle != IntPtr.Zero)
            {
                pcap_close(nicHandle);
                nicHandle = IntPtr.Zero;
            }
        }

        public int pcapnet_next_ex(out packet_headers pkt_hdr, out byte[] pkt_data)
        {
            pkt_hdr = null;
            pkt_data = null;
            IntPtr headerPtr = IntPtr.Zero;
            IntPtr dataPtr = IntPtr.Zero;
            int result = pcap_next_ex(nicHandle, ref headerPtr, ref dataPtr);
            if (result == 1 && headerPtr != IntPtr.Zero)
            {
                var hdr = Marshal.PtrToStructure<pcap_pkthdr>(headerPtr);
                pkt_hdr = new packet_headers { caplen = hdr.caplen, len = hdr.len };
                pkt_data = new byte[hdr.caplen];
                if (dataPtr != IntPtr.Zero)
                    Marshal.Copy(dataPtr, pkt_data, 0, (int)hdr.caplen);
            }
            return result;
        }

        public void pcapnet_sendpacket(byte[] data)
        {
            if (nicHandle != IntPtr.Zero && data != null)
                pcap_sendpacket(nicHandle, data, data.Length);
        }

        public int pcapnet_setFilter(string filter, uint netmask)
        {
            bpf_program fp = new bpf_program();
            if (pcap_compile(nicHandle, ref fp, filter, 1, netmask) != 0)
                return -1;
            int result = pcap_setfilter(nicHandle, ref fp);
            pcap_freecode(ref fp);
            return result;
        }

        public string[] getAllDevsID()
        {
            return new string[0];
        }

        public void Dispose()
        {
            pcapnet_close();
        }
    }

    public class Driver
    {
        public string driver_name = "npcap";

        // Returns true when Npcap (WinPcap-compatible) is installed and its wpcap.dll is present.
        // Covers both WinPcap-compat mode (SysWOW64\wpcap.dll) and Npcap-only mode (SysWOW64\Npcap\wpcap.dll).
        public bool create()
        {
            string sysX86 = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
            string sys    = Environment.GetFolderPath(Environment.SpecialFolder.System);
            return File.Exists(Path.Combine(sysX86, "wpcap.dll"))
                || File.Exists(Path.Combine(sysX86, "Npcap", "wpcap.dll"))
                || File.Exists(Path.Combine(sys, "wpcap.dll"))
                || File.Exists(Path.Combine(sys, "Npcap", "wpcap.dll"));
        }

        // Returns a non-zero pseudo-handle when Npcap is available (driver already running),
        // or IntPtr.Zero when the driver is not present (caller will prompt to install).
        public unsafe IntPtr openDeviceDriver(sbyte* driverName)
        {
            return create() ? new IntPtr(1) : IntPtr.Zero;
        }

        public void RegisterDriver() { }
        public void StartDriver() { }
        public void StopDriver() { }
        public void UnregisterDriver() { }
    }
#pragma warning restore
}
