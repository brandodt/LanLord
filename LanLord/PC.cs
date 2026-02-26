using System;
using System.Net;
using System.Net.NetworkInformation;

namespace LanLord
{
#pragma warning disable  // Falta el comentario XML para el tipo o miembro visible p�blicamente
    public class PC

    {
        public IPAddress ip;

        public PhysicalAddress mac;

        public string name;

        public bool isGateway;

        public bool isLocalPc;

        public int capDown;

        public int capUp;

        public bool redirect;

        public int totalPacketSent;

        public int totalPacketReceived;

        public int nbPacketSentSinceLastReset;

        public int nbPacketReceivedSinceLastReset;

        public DateTime timeSinceLastRarp;

        public bool isBlocked;

        public string lastDnsQuery;

        public string osGuess;

        public bool isSuspectedScanner;

        // 0 = no packet loss injection; 1-100 = percentage of packets to randomly drop
        public int packetLossPct;

        // null = DNS spoofing disabled; otherwise all A-record queries from this device
        // are answered with this IP (as a string, e.g. "192.168.1.100")
        public string dnsSpoofIP;
    }
#pragma warning restore  // Falta el comentario XML para el tipo o miembro visible p�blicamente
}
