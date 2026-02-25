using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace LanLord
{
#pragma warning disable  // Falta el comentario XML para el tipo o miembro visible pblicamente
    public class PcList : IDisposable

    {
        private delegateOnNewPC delOnNewPC;

        private delegateOnNewPC delOnPCRemove;

        public List<PC> pclist;

        private readonly object pclistLock = new object();

        public PcList()
        {
            pclist = new List<PC>();
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public bool addPcToList(PC pc)
        {
            lock (pclistLock)
            {
                foreach (PC item in pclist)
                {
                    if (item.ip.ToString().CompareTo(pc.ip.ToString()) == 0)
                    {
                        item.timeSinceLastRarp = DateTime.Now;
                        return false;
                    }
                }
                pclist.Add(pc);
            }
            delOnNewPC.Invoke(pc);
            return true;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public bool removePcFromList(PC pc)
        {
            bool found = false;
            lock (pclistLock)
            {
                for (int i = 0; i < pclist.Count; i++)
                {
                    if (pclist[i].ip.ToString().CompareTo(pc.ip.ToString()) == 0)
                    {
                        pclist.RemoveAt(i);
                        found = true;
                        break;
                    }
                }
            }
            if (found)
            {
                delOnPCRemove.Invoke(pc);
                return true;
            }
            return false;
        }

        public PC getRouter()
        {
            lock (pclistLock)
            {
                foreach (PC item in pclist)
                {
                    if (item.isGateway)
                        return item;
                }
            }
            return null;
        }

        public PC getLocalPC()
        {
            lock (pclistLock)
            {
                foreach (PC item in pclist)
                {
                    if (item.isLocalPc)
                        return item;
                }
            }
            return null;
        }

        public PC getPCFromIP(byte[] ip)
        {
            lock (pclistLock)
            {
                foreach (PC item in pclist)
                {
                    if (tools.areValuesEqual(item.ip.GetAddressBytes(), ip))
                        return item;
                }
            }
            return null;
        }

        public PC getPCFromMac(byte[] Mac)
        {
            lock (pclistLock)
            {
                foreach (PC item in pclist)
                {
                    if (tools.areValuesEqual(item.mac.GetAddressBytes(), Mac))
                        return item;
                }
            }
            return null;
        }

        public void ResetAllPacketsCount()
        {
            lock (pclistLock)
            {
                foreach (PC item in pclist)
                {
                    item.nbPacketReceivedSinceLastReset = 0;
                    item.nbPacketSentSinceLastReset = 0;
                }
            }
        }

        public void SetCallBackOnNewPC(delegateOnNewPC callback)
        {
            delOnNewPC = callback;
        }

        public void SetCallBackOnPCRemove(delegateOnNewPC callback)
        {
            delOnPCRemove = callback;
        }



        public void Dispose()
        {

            GC.SuppressFinalize(this);
        }
    }
#pragma warning restore  // Falta el comentario XML para el tipo o miembro visible p�blicamente
}
