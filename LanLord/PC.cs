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

        // When true the redirector intercepts this device's HTTP (port 80) requests
        // and replies with an HTTP 302 redirect to the captive portal page instead of
        // forwarding the packet to the real server.
        public bool captivePortalEnabled;
    }
#pragma warning restore  // Falta el comentario XML para el tipo o miembro visible p�blicamente
}
