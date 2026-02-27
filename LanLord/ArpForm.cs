using AdvancedDataGridView;
using PcapNet;
using System;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LanLord
{
#pragma warning disable  // Falta el comentario XML para el tipo o miembro visible públicamente
    public delegate void delegateOnNewPC(PC pc);

    public delegate void DelUpdateName(PC pc, string str);
    public partial class ArpForm : Form
    {

        public int timerStatCount;
        private bool isSpoofingActive = false;
        public Driver driver;
        public PcList pcs;
        public CArp cArp;
        public CAdapter cAdapter;
        public byte[] routerIP;
        public object[] resolvState;
        private ContextMenuStrip rowContextMenu;
        private int _contextMenuRowIndex = -1;
        private DeviceProfiles deviceProfiles = new DeviceProfiles();

        // Captive portal: per-device HTTP redirect server (TcpListener on port 80)
        private System.Net.Sockets.TcpListener _captiveListener = null;
        private ToolStripMenuItem _captivePortalItem;   // kept for check-state updates
        private string _captivePortalHtml =
            "<!DOCTYPE html><html><head><meta charset='utf-8'><title>Network Notice</title>" +
            "<style>body{margin:0;display:flex;justify-content:center;align-items:center;" +
            "min-height:100vh;background:#0d1117;font-family:Segoe UI,Arial,sans-serif;}" +
            ".box{padding:40px 50px;background:#161b22;border-radius:14px;text-align:center;" +
            "max-width:500px;box-shadow:0 8px 32px #0008;}" +
            "h1{color:#f85149;margin-bottom:12px;}p{color:#8b949e;line-height:1.6;}" +
            ".badge{display:inline-block;margin-top:20px;padding:6px 16px;border-radius:20px;" +
            "background:#21262d;color:#58a6ff;font-size:.85rem;}</style></head>" +
            "<body><div class='box'><h1>&#x26A0; Network Access Restricted</h1>" +
            "<p>Your access to this network has been restricted by the network administrator.</p>" +
            "<div class='badge'>LanLord Captive Portal</div></div></body></html>";

        public NetworkInterface nicNet;
        public static ArpForm instance;
        public ArpForm()
        {
            InitializeComponent();
            ArpForm.instance = this;
            this.timerStatCount = 0;
            this.driver = new Driver();
            initRowContextMenu();
            this.treeGridView1.CellDoubleClick += new DataGridViewCellEventHandler(TreeGridView1_CellDoubleClick);
            this.treeGridView1.CellPainting += new DataGridViewCellPaintingEventHandler(TreeGridView1_CellPainting);
            // Apply dark theme to all context menus
            var darkRenderer = new DarkMenuRenderer();
            this.ContextMenuViews.Renderer = darkRenderer;
            this.ContextMenuViews.BackColor = UITheme.Surface1;
            this.LanLordTray.Renderer      = darkRenderer;
            this.LanLordTray.BackColor      = UITheme.Surface1;

            // Override the icon stored in the resx with the correct icon file
            try
            {
                string icoPath = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(Application.ExecutablePath),
                    "LanLord.ico");
                if (System.IO.File.Exists(icoPath))
                {
                    var icon = new System.Drawing.Icon(icoPath);
                    this.Icon = icon;
                    this.LanLordTrayIcon.Icon = icon;
                }
            }
            catch { }
        }
        public void licenseAccepted()
        {
            if (!this.driver.create())
            {
                int num = (int)MessageBox.Show("problem installing the drivers, do you have administrator privileges?");
                if (this == null)
                    return;
                this.Dispose();
            }
            else
            {
                CAdapter cadapter = new CAdapter();
                this.cAdapter = cadapter;
                if (!minimized) cadapter.Show((IWin32Window)this);
            }
        }
        bool first_start;
        bool minimized;

        public void NicIsSelected(NetworkInterface nic)
        {
            this.pcs = new PcList();
            this.pcs.SetCallBackOnNewPC(new delegateOnNewPC(this.callbackOnNewPC));
            this.pcs.SetCallBackOnPCRemove(new delegateOnNewPC(this.callbackOnPCRemove));
            this.nicNet = nic;
            this.lblStatus.Text = "  Adapter: " + nic.Name + " — click Scan to discover devices";
            CArp carp = new CArp(nic, this.pcs);
            this.cArp = carp;
            carp.startArpListener();
            this.cArp.findMacRouter();
            // DNS sniffer: update the IP cell tooltip of the device row with the last seen hostname
            carp.OnDnsQuery = (dnsPC, hostname) =>
            {
                if (!this.IsHandleCreated || this.IsDisposed) return;
                try
                {
                    string logLine = string.Format("[{0}]  {1,-17}  {2}{3}",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        dnsPC.ip.ToString(),
                        hostname,
                        Environment.NewLine);
                    System.IO.File.AppendAllText("dns_log.txt", logLine, System.Text.Encoding.UTF8);
                }
                catch { }
                this.Invoke((Action)(() =>
                {
                    for (int i = 0; i < this.treeGridView1.Nodes[0].Nodes.Count; i++)
                    {
                        var n = this.treeGridView1.Nodes[0].Nodes[i];
                        if (n.Cells[1].Value?.ToString() == dnsPC.ip.ToString())
                        {
                            string current = n.Cells[0].Value?.ToString() ?? string.Empty;
                            int arrow = current.IndexOf("  \u2192 ");
                            string baseName = arrow >= 0 ? current.Substring(0, arrow) : current;
                            n.Cells[0].Value = baseName + "  \u2192 " + hostname;
                            break;
                        }
                    }
                }));
            };

            // HTTP Host sniffer: log cleartext HTTP browsing destinations
            carp.OnHttpHost = (httpPC, host) =>
            {
                try
                {
                    string logLine = string.Format("[{0}]  {1,-17}  {2}{3}",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        httpPC.ip.ToString(),
                        host,
                        Environment.NewLine);
                    System.IO.File.AppendAllText("http_log.txt", logLine, System.Text.Encoding.UTF8);
                }
                catch { }
            };

            // Rogue ARP detector: alert when another machine impersonates the router
            carp.OnRogueArp = (rogueMac, claimedIp) =>
            {
                try
                {
                    string logLine = string.Format("[{0}]  ROGUE ARP — MAC {1} claiming to be {2}{3}",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        rogueMac, claimedIp,
                        Environment.NewLine);
                    System.IO.File.AppendAllText("security_alerts.txt", logLine, System.Text.Encoding.UTF8);
                }
                catch { }
                if (!this.IsHandleCreated || this.IsDisposed) return;
                this.Invoke((Action)(() =>
                {
                    this.LanLordTrayIcon.ShowBalloonTip(5000, "\u26A0 Rogue ARP Detected",
                        "MAC " + rogueMac + " is claiming to be the router (" + claimedIp + ")",
                        ToolTipIcon.Warning);
                }));
            };

            // Port scan detector: alert when a device probes many ports on a single host
            carp.OnPortScanDetected = (scanPC) =>
            {
                try
                {
                    string logLine = string.Format("[{0}]  PORT SCAN — {1} ({2}){3}",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        scanPC.ip.ToString(),
                        scanPC.mac.ToString(),
                        Environment.NewLine);
                    System.IO.File.AppendAllText("security_alerts.txt", logLine, System.Text.Encoding.UTF8);
                }
                catch { }
                if (!this.IsHandleCreated || this.IsDisposed) return;
                this.Invoke((Action)(() =>
                {
                    this.LanLordTrayIcon.ShowBalloonTip(5000, "\u26A0 Port Scan Detected",
                        scanPC.ip.ToString() + " is scanning a host on the network",
                        ToolTipIcon.Warning);
                    // Mark the device row name with [SCAN] indicator
                    for (int i = 0; i < this.treeGridView1.Nodes[0].Nodes.Count; i++)
                    {
                        var n = this.treeGridView1.Nodes[0].Nodes[i];
                        if (n.Cells[1].Value?.ToString() == scanPC.ip.ToString())
                        {
                            string current = n.Cells[0].Value?.ToString() ?? string.Empty;
                            if (!current.Contains("[SCAN]"))
                                n.Cells[0].Value = "[SCAN] " + current;
                            break;
                        }
                    }
                }));
            };
            PC pc = new PC();
            pc.ip = new IPAddress(this.cArp.localIP);
            pc.mac = new PhysicalAddress(this.cArp.localMAC);
            pc.capDown = 0;
            pc.capUp = 0;
            pc.isLocalPc = true;
            pc.name = string.Empty;
            pc.nbPacketReceivedSinceLastReset = 0;
            pc.nbPacketSentSinceLastReset = 0;
            pc.redirect = false;
            DateTime now = DateTime.Now;
            pc.timeSinceLastRarp = now;
            pc.totalPacketReceived = 0;
            pc.totalPacketSent = 0;
            pc.isGateway = false;
            this.pcs.addPcToList(pc);
            this.timer2.Interval = 5000;
            this.timer2.Start();
            // Captive portal: log interceptions to captive_log.txt
            carp.OnCaptivePortalHit = (cpPC, host) =>
            {
                try
                {
                    string logLine = string.Format("[{0}]  CAPTIVE  {1,-17}  {2}{3}",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        cpPC.ip.ToString(), host, Environment.NewLine);
                    System.IO.File.AppendAllText("captive_log.txt", logLine, System.Text.Encoding.UTF8);
                }
                catch { }
            };

            this.treeGridView1.Nodes[0].Expand();
            // Auto-start initial scan so the user sees devices immediately without pressing Scan
            triggerDiscovery();
        }

        [Obsolete]
        private void callbackOnNewPC(PC pc)
        {
            deviceProfiles.Apply(pc);
            object[] objArray = new object[1] { (object)pc };
            ArpForm arpForm = this;
            arpForm.Invoke((Delegate)new delegateOnNewPC(arpForm.AddPc), objArray);
            if (!pc.isLocalPc && !pc.isGateway)
                this.LanLordTrayIcon.ShowBalloonTip(2000, "New Device Found", pc.ip.ToString(), ToolTipIcon.Info);
            Dns.BeginResolve(pc.ip.ToString(), new AsyncCallback(this.EndResolvCallBack), pc);
        }

        [Obsolete]
        private void EndResolvCallBack(IAsyncResult re)
        {
            string str = (string)null;
            PC asyncState = (PC)re.AsyncState;
            try
            {
                str = Dns.EndResolve(re).HostName;
                if (str == (string)null)
                    str = "noname";
                object[] objArray = new object[2];
                this.resolvState = objArray;
                objArray[0] = (object)asyncState;
                this.resolvState[1] = (object)str;
                this.Invoke((Delegate)new DelUpdateName(this.updateTreeViewNameCallBack), this.resolvState);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private void updateTreeViewNameCallBack(PC pc, string str)
        {
            if (pc.isGateway)
            {
                this.treeGridView1.Nodes[0].Cells[0].Value = (object)str;
                this.treeGridView1.Nodes[0].ImageIndex = 1;
            }
            else
            {
                int index = 1;
                if (1 >= this.treeGridView1.Nodes[0].Nodes.Count)
                    return;
                while (this.treeGridView1.Nodes[0].Nodes[index].Cells[1].Value.ToString().CompareTo(pc.ip.ToString()) != 0)
                {
                    ++index;
                    if (index >= this.treeGridView1.Nodes[0].Nodes.Count)
                        return;
                }
                this.treeGridView1.Nodes[0].Nodes[index].Cells[0].Value = (object)str;
            }
        }

        private void callbackOnPCRemove(PC pc)
        {
            int index = 1;
            if (1 >= this.treeGridView1.Nodes[0].Nodes.Count)
                return;
            while (this.treeGridView1.Nodes[0].Nodes[index].Cells[1].Value.ToString().CompareTo(pc.ip.ToString()) != 0)
            {
                ++index;
                if (index >= this.treeGridView1.Nodes[0].Nodes.Count)
                    return;
            }
            this.treeGridView1.Nodes[0].Nodes.RemoveAt(index);
        }

        private void AddPc(PC pc)
        {
            if (pc.isGateway)
            {
                this.treeGridView1.Nodes[0].Cells[1].Value = (object)pc.ip.ToString();
                string gwMac = pc.mac.ToString();
                string gwVendor = OuiLookup.Lookup(gwMac);
                this.treeGridView1.Nodes[0].Cells[2].Value = gwVendor.Length > 0 ? gwMac + "  •  " + gwVendor : gwMac;
                this.treeGridView1.Nodes[0].Cells[5].ReadOnly = true;
                this.treeGridView1.Nodes[0].Cells[6].ReadOnly = true;
                this.treeGridView1.Nodes[0].Cells[7].ReadOnly = true;
                this.treeGridView1.Nodes[0].Cells[8].ReadOnly = true;
                this.treeGridView1.Nodes[0].Cells[5].Value = (object)0;
                this.treeGridView1.Nodes[0].Cells[6].Value = (object)0;
                this.treeGridView1.Nodes[0].Cells[7].ReadOnly = true;
                this.treeGridView1.Nodes[0].Cells[8].ReadOnly = true;
            }
            else if (pc.isLocalPc)
            {
                TreeGridNode treeGridNode = this.treeGridView1.Nodes[0].Nodes.Add((object)"Your PC", (object)pc.ip, (object)pc.mac.ToString());
                treeGridNode.ImageIndex = 0;
                treeGridNode.Cells[5].Value = (object)0;
                treeGridNode.Cells[6].Value = (object)0;
                treeGridNode.Cells[5].ReadOnly = true;
                treeGridNode.Cells[6].ReadOnly = true;
                treeGridNode.Cells[7].Value = (object)false;
                treeGridNode.Cells[8].Value = (object)false;
                treeGridNode.Cells[7].ReadOnly = true;
                treeGridNode.Cells[8].ReadOnly = true;
            }
            else
            {
                string macStr = pc.mac.ToString();
                string vendor = OuiLookup.Lookup(macStr);
                string macDisplay = vendor.Length > 0 ? macStr + "  \u2022  " + vendor : macStr;
                if (pc.osGuess != null) macDisplay += "  \u2022  " + pc.osGuess;
                TreeGridNode treeGridNode = this.treeGridView1.Nodes[0].Nodes.Add((object)(pc.name ?? string.Empty), (object)pc.ip, (object)macDisplay, (object)string.Empty, (object)string.Empty, (object)(pc.capDown / 1024), (object)(pc.capUp / 1024), (object)pc.isBlocked, (object)true);
                treeGridNode.ImageIndex = 0;
                treeGridNode.Cells[5].ReadOnly = false;
                treeGridNode.Cells[6].ReadOnly = false;
            }
        }
        private int timer2DiscoveryTicks = 0;
        private const int AutoRescanEveryTicks = 12; // 12 × 5 s = 60 s

        private void triggerDiscovery()
        {
            this.cArp.OnScanProgress = (scanned, total) =>
            {
                if (this.IsHandleCreated && !this.IsDisposed)
                    this.Invoke((Action)(() =>
                    {
                        if (scanned >= total)
                            this.lblStatus.Text = string.Format("  Scan complete \u2014 {0} device(s) found", this.treeGridView1.Nodes[0].Nodes.Count - 1);
                        else
                            this.lblStatus.Text = string.Format("  Scanning network... {0}/{1}", scanned, total);
                    }));
            };
            this.lblStatus.Text = "  Scanning network for devices...";
            this.cArp.startArpDiscovery();
        }

        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            timer2DiscoveryTicks = 0; // reset auto-rescan counter when user scans manually
            triggerDiscovery();
        }

        private void ToolStripButton2_Click(object sender, EventArgs e)
        {
            if (isSpoofingActive)
                return;
            this.cArp.startRedirector();
            this.lblStatus.Text = "  Spoofing active — intercepting network traffic";
            this.lblStatus.ForeColor = Color.White;
            this.statusStrip1.BackColor = UITheme.Danger;
            isSpoofingActive = true;
            this.btnStartSpoof.Enabled = false;
            this.btnStopSpoof.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Start();
            this.timerSpoof.Start();
            this.timerSpoof.Interval = 2000;
            this.timerDiscovery.Start();
            SetSpoofDependentColumnsEnabled(true);
        }

        private void ToolStripButton3_Click(object sender, EventArgs e)
        {
            if (!isSpoofingActive)
                return;
            this.cArp.stopRedirector();
            this.cArp.completeUnspoof();
            this.lblStatus.Text = "  Spoofing stopped";
            this.lblStatus.ForeColor = Color.FromArgb(100, 140, 220);
            this.statusStrip1.BackColor = UITheme.Surface1;
            this.timer1.Stop();
            this.timerSpoof.Stop();
            int index = 0;
            if (0 < this.treeGridView1.Nodes[0].Nodes.Count)
            {
                do
                {
                    this.treeGridView1.Nodes[0].Nodes[index].Cells[3].Value = (object)string.Empty;
                    this.treeGridView1.Nodes[0].Nodes[index].Cells[4].Value = (object)string.Empty;
                    ++index;
                }
                while (index < this.treeGridView1.Nodes[0].Nodes.Count);
            }
            isSpoofingActive = false;
            this.btnStartSpoof.Enabled = true;
            this.btnStopSpoof.Enabled = false;
            this.timerDiscovery.Stop();
            // Cancel any active cell edit before resetting values to avoid DataError
            this.treeGridView1.CancelEdit();
            for (int i = 0; i < this.treeGridView1.Nodes[0].Nodes.Count; i++)
                this.treeGridView1.Nodes[0].Nodes[i].Cells[7].Value = (object)false;
            SetSpoofDependentColumnsEnabled(false);
            // Disable captive portal on all devices and stop the local HTTP server
            if (this.pcs != null)
                foreach (var p in this.pcs.pclist)
                    p.captivePortalEnabled = false;
            stopCaptivePortalServer();
        }

        private void ToolStripButton4_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists("hlpindex.html"))
            {
                Process.Start("hlpindex.html");
            }
            else
            {
                int num = (int)MessageBox.Show("help file is missing !");
            }
        }

        private void TreeGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (this.treeGridView1.CurrentCell == null)
                return;
            int col = this.treeGridView1.CurrentCell.ColumnIndex;
            // Only auto-commit for checkbox columns (Block=7, Spoof=8)
            if (col != 7 && col != 8)
                return;
            // Do not commit Block changes when spoofing is off
            if (col == 7 && !isSpoofingActive)
            {
                this.treeGridView1.CancelEdit();
                return;
            }
            if (!this.treeGridView1.IsCurrentCellDirty)
                return;
            this.treeGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void TreeGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 5 && e.RowIndex >= 2)
                {
                    IPAddress ipAddress = tools.getIpAddress(this.treeGridView1.Rows[e.RowIndex].Cells[1].Value.ToString());
                    if (this.treeGridView1.Rows[e.RowIndex].Cells[0].Value.ToString().CompareTo(string.Empty) != 0)
                    {
                        PC pcFromIp = this.pcs.getPCFromIP(ipAddress.GetAddressBytes());
                        if (pcFromIp != null)
                        {
                            Monitor.Enter((object)pcFromIp);
                            pcFromIp.capDown = Convert.ToInt32(this.treeGridView1.Rows[e.RowIndex].Cells[5].Value) * 1024;
                            Monitor.Exit((object)pcFromIp);
                            deviceProfiles.Save(pcFromIp);
                        }
                    }
                }
                if (e.ColumnIndex == 6 && e.RowIndex >= 2)
                {
                    IPAddress ipAddress = tools.getIpAddress(this.treeGridView1.Rows[e.RowIndex].Cells[1].Value.ToString());
                    if (this.treeGridView1.Rows[e.RowIndex].Cells[0].Value.ToString().CompareTo(string.Empty) != 0)
                    {
                        PC pcFromIp = this.pcs.getPCFromIP(ipAddress.GetAddressBytes());
                        if (pcFromIp != null)
                        {
                            Monitor.Enter((object)pcFromIp);
                            pcFromIp.capUp = Convert.ToInt32(this.treeGridView1.Rows[e.RowIndex].Cells[6].Value) * 1024;
                            Monitor.Exit((object)pcFromIp);
                            deviceProfiles.Save(pcFromIp);
                        }
                    }
                }
                if (e.ColumnIndex == 7 && e.RowIndex >= 2)
                {
                    IPAddress ipAddress = tools.getIpAddress(this.treeGridView1.Rows[e.RowIndex].Cells[1].Value.ToString());
                    PC pcFromIp = this.pcs.getPCFromIP(ipAddress.GetAddressBytes());
                    if (pcFromIp != null)
                    {
                        bool isBlocked = Convert.ToBoolean(this.treeGridView1.Rows[e.RowIndex].Cells[7].Value);
                        this.cArp.SetBlocked(pcFromIp, isBlocked);
                        deviceProfiles.Save(pcFromIp);
                    }
                }
                if (e.ColumnIndex != 8 || e.RowIndex < 2 || this.treeGridView1.Rows[e.RowIndex].Cells[0].Value.ToString().CompareTo(string.Empty) == 0)
                    return;
                IPAddress ipAddress1 = tools.getIpAddress(this.treeGridView1.Rows[e.RowIndex].Cells[1].Value.ToString());
                if (this.treeGridView1.Rows[e.RowIndex].Cells[8].Value.ToString().CompareTo("False") != 0)
                    return;
                for (int index = 0; index < 35; ++index)
                    this.cArp.UnSpoof(ipAddress1, new IPAddress(this.cArp.routerIP));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private unsafe void ArpForm_Load(object sender, EventArgs e)
        {
            // Security/authenticity notice — shown on every launch.
            using (Form notice = new Form())
            {
                notice.Text = "Security Notice — LanLord";
                notice.StartPosition = FormStartPosition.CenterScreen;
                notice.FormBorderStyle = FormBorderStyle.FixedDialog;
                notice.MaximizeBox = false;
                notice.MinimizeBox = false;
                notice.Width = 520;
                notice.Height = 270;
                notice.BackColor = UITheme.Surface0;

                var lbl = new Label
                {
                    Text =
                        "⚠  IMPORTANT — Please verify your download source\r\n\r\n" +
                        "LanLord uses ARP spoofing to intercept network traffic.\r\n" +
                        "A malicious copy could silently compromise your network.\r\n\r\n" +
                        "Only install LanLord from the official source:\r\n" +
                        "  https://github.com/brandodt/LanLord\r\n\r\n" +
                        "If you did not download from that address, uninstall\r\n" +
                        "this copy immediately and obtain a clean release.",
                    ForeColor = System.Drawing.Color.White,
                    Font = new System.Drawing.Font("Segoe UI", 9.5f),
                    Location = new System.Drawing.Point(16, 12),
                    Size = new System.Drawing.Size(478, 180),
                    TextAlign = System.Drawing.ContentAlignment.TopLeft
                };

                var btnOk = new Button
                {
                    Text = "I Understand — Continue",
                    DialogResult = DialogResult.OK,
                    Width = 200,
                    Height = 32,
                    Left = notice.ClientSize.Width / 2 - 210,
                    Top = 200,
                    BackColor = UITheme.Accent,
                    ForeColor = System.Drawing.Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnQuit = new Button
                {
                    Text = "Quit",
                    DialogResult = DialogResult.Cancel,
                    Width = 90,
                    Height = 32,
                    Left = notice.ClientSize.Width / 2 + 10,
                    Top = 200,
                    BackColor = UITheme.Surface1,
                    ForeColor = System.Drawing.Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnQuit.FlatAppearance.BorderSize = 0;

                notice.Controls.Add(lbl);
                notice.Controls.Add(btnOk);
                notice.Controls.Add(btnQuit);
                notice.AcceptButton = btnOk;
                notice.CancelButton = btnQuit;

                if (notice.ShowDialog(this) != DialogResult.OK)
                {
                    this.Dispose();
                    return;
                }
            }

            if (args.Length > 1)
            {
                if (args[1] == "minimize")
                {
                    first_start = true;
                    minimized = true;
                }
            }


            this.Text = "LanLord v" + Application.ProductVersion.ToString();
            //this.Icon = LanLord.Properties.Resources.dwaynelifter_icon;
            //LanLordTrayIcon.Icon = LanLord.Properties.Resources.dwaynelifter_icon.ico;
            if ((IntPtr)this.driver.openDeviceDriver((sbyte*)(void*)Marshal.StringToHGlobalAnsi("npf")) == IntPtr.Zero)
            {
                this.licenseAccepted();
            }
            else
            {
                CAdapter cadapter = new CAdapter();
                this.cAdapter = cadapter;
                if (!minimized) cadapter.Show((IWin32Window)this);
            }
        }

        private string formatBandwidth(float kb)
        {
            if (kb >= 1024f)
                return string.Format("{0:F2} MB/s", kb / 1024f);
            if (kb < 1f && kb > 0f)
                return string.Format("{0:F0} B/s", kb * 1024f);
            return string.Format("{0:F1} KB/s", kb);
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            ++this.timerStatCount;
            for (int index = 0; index < this.treeGridView1.Nodes[0].Nodes.Count; ++index)
            {
                try
                {
                    TreeGridNode node = this.treeGridView1.Nodes[0].Nodes[index];
                    PC pcFromIp = this.pcs.getPCFromIP(tools.getIpAddress(node.Cells[1].Value.ToString()).GetAddressBytes());
                    float kb1 = (float)pcFromIp.nbPacketReceivedSinceLastReset * 0.0009765625f / (float)(this.timer1.Interval / 1000) / (float)this.timerStatCount;
                    float kb2 = (float)pcFromIp.nbPacketSentSinceLastReset * 0.0009765625f / (float)(this.timer1.Interval / 1000) / (float)this.timerStatCount;
                    node.Cells[3].Value = (object)formatBandwidth(kb1);
                    node.Cells[4].Value = (object)formatBandwidth(kb2);
                    // Live-update MAC cell with OS guess once resolved
                    if (pcFromIp.osGuess != null)
                    {
                        string macCell = node.Cells[2].Value?.ToString() ?? string.Empty;
                        if (!macCell.Contains(pcFromIp.osGuess))
                        {
                            // Strip any stale OS guess (between last • and end) and append fresh one
                            int lastBullet = macCell.LastIndexOf("  \u2022  ");
                            string macBase = lastBullet >= 0 && macCell.Substring(lastBullet + 3).Contains("TTL=")
                                ? macCell.Substring(0, lastBullet)
                                : macCell;
                            node.Cells[2].Value = macBase + "  \u2022  " + pcFromIp.osGuess;
                        }
                    }
                    // Row color coding: blocked=red tint, spoofed=blue tint, else default
                    bool isBlocked = Convert.ToBoolean(node.Cells[7].Value);
                    bool isSpoofed = Convert.ToBoolean(node.Cells[8].Value);
                    Color rowBg = isBlocked
                        ? Color.FromArgb(60, 20, 20)          // red  tint — blocked
                        : (isSpoofed ? Color.FromArgb(18, 38, 70) // blue tint — spoofed
                        : UITheme.Surface2);
                    node.DefaultCellStyle.BackColor = rowBg;
                    node.DefaultCellStyle.ForeColor = UITheme.TextPrimary;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            this.pcs.ResetAllPacketsCount();
            this.timerStatCount = 0;
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            // Periodic auto-rescan: re-discover new devices every 60 seconds
            timer2DiscoveryTicks++;
            if (timer2DiscoveryTicks >= AutoRescanEveryTicks)
            {
                timer2DiscoveryTicks = 0;
                triggerDiscovery();
            }
            int index1 = 0;
            if (0 < this.treeGridView1.Nodes[0].Nodes.Count)
            {
                do
                {
                    PC pcFromIp = this.pcs.getPCFromIP(tools.getIpAddress(this.treeGridView1.Nodes[0].Nodes[index1].Cells[1].Value.ToString()).GetAddressBytes());
                    if (pcFromIp != null && !pcFromIp.isGateway && !pcFromIp.isLocalPc && DateTime.Now.Ticks - ((DateTime)pcFromIp.timeSinceLastRarp).Ticks > 3500000000L)
                    {
                        this.pcs.removePcFromList(pcFromIp);
                        index1 = 0;
                    }
                    ++index1;
                }
                while (index1 < this.treeGridView1.Nodes[0].Nodes.Count);
            }
            int index2 = 0;
            if (0 >= this.treeGridView1.Nodes[0].Nodes.Count)
                return;
            do
            {
                PC pcFromIp = this.pcs.getPCFromIP(tools.getIpAddress(this.treeGridView1.Nodes[0].Nodes[index2].Cells[1].Value.ToString()).GetAddressBytes());
                if (pcFromIp != null && DateTime.Now.Ticks - ((DateTime)pcFromIp.timeSinceLastRarp).Ticks > 200000000L)
                    this.cArp.findMac(pcFromIp.ip.ToString());
                ++index2;
            }
            while (index2 < this.treeGridView1.Nodes[0].Nodes.Count);
        }

        private void TimerSpoof_Tick(object sender, EventArgs e)
        {
            this.timerSpoof.Interval = 5000;
            int index = 0;
            if (0 >= this.treeGridView1.Nodes[0].Nodes.Count)
                return;
            do
            {
                bool isSpoofed = this.treeGridView1.Nodes[0].Nodes[index].Cells[8].Value.ToString().CompareTo("True") == 0;
                bool isBlocked = this.treeGridView1.Nodes[0].Nodes[index].Cells[7].Value.ToString().CompareTo("True") == 0;
                // Re-poison ARP cache for spoofed OR blocked devices to maintain MITM position
                if (isSpoofed || isBlocked)
                {
                    PC pcFromIp = this.pcs.getPCFromIP(tools.getIpAddress(this.treeGridView1.Nodes[0].Nodes[index].Cells[1].Value.ToString()).GetAddressBytes());
                    if (!pcFromIp.isLocalPc)
                        this.cArp.Spoof(pcFromIp.ip, new IPAddress(this.cArp.routerIP));
                }
                ++index;
            }
            while (index < this.treeGridView1.Nodes[0].Nodes.Count);
        }




        private void ViewMenuIP_CheckStateChanged(object sender, EventArgs e)
        {
            this.ColPCIP.Visible = this.ViewMenuIP.Checked;
        }

        private void ViewMenuMAC_CheckStateChanged(object sender, EventArgs e)
        {
            this.ColPCMac.Visible = this.ViewMenuMAC.Checked;
        }

        private void ViewMenuDownload_CheckStateChanged(object sender, EventArgs e)
        {
            this.ColDownload.Visible = this.ViewMenuDownload.Checked;
        }

        private void ViewMenuUpload_CheckStateChanged(object sender, EventArgs e)
        {
            this.ColUpload.Visible = this.ViewMenuUpload.Checked;
        }

        private void TreeGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (isSpoofingActive || e.RowIndex < 0 || e.ColumnIndex < 0)
                return;
            if (e.ColumnIndex == 7 || e.ColumnIndex == 5 || e.ColumnIndex == 6)
                this.lblStatus.Text = "  \u26A0 Start Spoofing first to use Block and bandwidth cap controls";
        }

        private void TreeGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Suppress format errors caused by programmatic cell resets or ReadOnly conflicts
            e.ThrowException = false;
        }

        private void ViewMenuDownloadCap_CheckStateChanged(object sender, EventArgs e)
        {
            this.ColDownCap.Visible = this.ViewMenuDownloadCap.Checked;
        }

        private void ViewMenuUploadCap_CheckStateChanged(object sender, EventArgs e)
        {
            this.ColUploadCap.Visible = this.ViewMenuUploadCap.Checked;
        }

        private void TreeGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            NumericUpDown num = e.Control as NumericUpDown;
            if (num != null)
            {
                num.BackColor = UITheme.Surface2;
                num.ForeColor = UITheme.TextPrimary;
            }
        }

        private void TreeGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // Fix NumericUpDown columns (5=DownCap, 6=UpCap) rendering with black text in dark theme
            if (e.RowIndex < 0 || (e.ColumnIndex != 5 && e.ColumnIndex != 6)) return;
            Color bg = isSpoofingActive ? UITheme.Surface2  : UITheme.Surface0;
            Color fg = isSpoofingActive ? UITheme.TextPrimary : UITheme.TextMuted;
            // Check row-level override (blocked/spoofed tint)
            var rowStyle = this.treeGridView1.Rows[e.RowIndex].DefaultCellStyle;
            if (rowStyle.BackColor != Color.Empty) bg = rowStyle.BackColor;
            using (SolidBrush bgBrush = new SolidBrush(bg))
                e.Graphics.FillRectangle(bgBrush, e.CellBounds);
            string text = e.Value != null ? e.Value.ToString() : "0";
            TextRenderer.DrawText(e.Graphics, text, e.CellStyle.Font ?? this.Font,
                e.CellBounds, fg,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            e.Paint(e.CellBounds, DataGridViewPaintParts.Focus | DataGridViewPaintParts.Border);
            e.Handled = true;
        }

        private void SetSpoofDependentColumnsEnabled(bool enabled)
        {
            this.ColBlock.ReadOnly = !enabled;
            this.ColDownCap.ReadOnly = !enabled;
            this.ColUploadCap.ReadOnly = !enabled;

            var disabledStyle = new DataGridViewCellStyle
            {
                BackColor          = UITheme.Surface0,
                ForeColor          = UITheme.TextMuted,
                SelectionBackColor = UITheme.Surface0,
                SelectionForeColor = UITheme.TextMuted
            };
            var enabledStyle = new DataGridViewCellStyle
            {
                BackColor          = UITheme.Surface2,
                ForeColor          = UITheme.TextPrimary,
                SelectionBackColor = UITheme.GridSelection,
                SelectionForeColor = Color.White
            };

            this.ColBlock.DefaultCellStyle = enabled ? enabledStyle : disabledStyle;
            this.ColDownCap.DefaultCellStyle = enabled ? enabledStyle : disabledStyle;
            this.ColUploadCap.DefaultCellStyle = enabled ? enabledStyle : disabledStyle;
        }



        private void ViewMenuBlock_CheckStateChanged(object sender, EventArgs e)
        {
            this.ColBlock.Visible = this.ViewMenuBlock.Checked;
        }

        private void ViewMenuSpoof_CheckStateChanged(object sender, EventArgs e)
        {
            this.ColSpoof.Visible = this.ViewMenuSpoof.Checked;
        }

        private void LanLordTrayIcon_DoubleClick(object sender, EventArgs e)
        {

        }

        string[] args = Environment.GetCommandLineArgs();

        private void LanLordTrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void ArpForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
            else
            {

                if (first_start)
                {

                    CAdapter cadapter = new CAdapter();
                    this.cAdapter = cadapter;
                    cadapter.Show((IWin32Window)this);
                    first_start = false;
                }


            }
            //this.LanLordTrayIcon.ShowBalloonTip(2000);
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
            //var rs = MessageBox.Show(this, "Quit?", "Quit", MessageBoxButtons.YesNo);
            //if (rs == DialogResult.Yes) Environment.Exit(0);
        }

        private void ShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;

        }

        private void ToolStripButton7_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
        }


        private void ArpForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!systemShutdown)
            {

                System.Windows.Forms.DialogResult result = MessageBox.Show("Are you sure you want to close the App?", "Application Closing!", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                switch (result)
                {
                    case System.Windows.Forms.DialogResult.OK:
                        if (WindowState == FormWindowState.Minimized)
                        {
                            Show();
                        }
                        ToolStripButton3_Click(btnStopSpoof, new EventArgs());
                        LanLordTrayIcon.Dispose();
                        Environment.Exit(0);
                        break;
                }
                e.Cancel = true;
            }
            else
            {
                ToolStripButton3_Click(btnStopSpoof, new EventArgs());
                LanLordTrayIcon.Dispose();
                Environment.Exit(0);

            }

        }

        private void ArpForm_Shown(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && args[1] == "minimize")
            {
                minimized = true;
                this.WindowState = FormWindowState.Minimized;
            }
            Opacity = 1.0;
            SetSpoofDependentColumnsEnabled(false);
        }



        #region Feature: row context menu (quick-block + bandwidth graph)

        private void initRowContextMenu()
        {
            rowContextMenu = new ContextMenuStrip();
            rowContextMenu.BackColor = UITheme.Surface1;
            rowContextMenu.ForeColor = UITheme.TextPrimary;
            rowContextMenu.Font      = new System.Drawing.Font("Segoe UI", 9.5F);
            rowContextMenu.Renderer  = new DarkMenuRenderer();

            var renameItem   = new ToolStripMenuItem("\u270E  Rename");
            var cutOffItem   = new ToolStripMenuItem("\u2298  Cut Off  (1 KB/s cap)");
            var unblockItem  = new ToolStripMenuItem("\u21A9  Remove Caps");
            var graphItem    = new ToolStripMenuItem("\u25A4  Bandwidth Graph");
            var portsItem    = new ToolStripMenuItem("\uD83D\uDD0D  Check Ports");
            var dnsLogItem   = new ToolStripMenuItem("\uD83D\uDCCB  DNS Log");
            var httpLogItem  = new ToolStripMenuItem("\uD83C\uDF10  HTTP Log");
            _captivePortalItem = new ToolStripMenuItem("\uD83D\uDEAB  Captive Portal");

            renameItem.Click   += renameItem_Click;
            cutOffItem.Click   += cutOffItem_Click;
            unblockItem.Click  += unblockItem_Click;
            graphItem.Click    += graphItem_Click;
            portsItem.Click    += portsItem_Click;
            dnsLogItem.Click   += dnsLogItem_Click;
            httpLogItem.Click  += httpLogItem_Click;
            _captivePortalItem.Click += captivePortalItem_Click;

            rowContextMenu.Items.Add(renameItem);
            rowContextMenu.Items.Add(new ToolStripSeparator());
            rowContextMenu.Items.Add(cutOffItem);
            rowContextMenu.Items.Add(unblockItem);
            rowContextMenu.Items.Add(new ToolStripSeparator());
            rowContextMenu.Items.Add(graphItem);
            rowContextMenu.Items.Add(new ToolStripSeparator());
            rowContextMenu.Items.Add(portsItem);
            rowContextMenu.Items.Add(dnsLogItem);
            rowContextMenu.Items.Add(httpLogItem);
            rowContextMenu.Items.Add(new ToolStripSeparator());
            rowContextMenu.Items.Add(_captivePortalItem);

            // Update captive portal check-mark when the menu opens
            rowContextMenu.Opening += (s, ev) =>
            {
                if (_contextMenuRowIndex < 2 || this.pcs == null) return;
                string rowIp = this.treeGridView1.Rows[_contextMenuRowIndex].Cells[1].Value?.ToString();
                if (string.IsNullOrEmpty(rowIp)) return;
                PC rowPc = this.pcs.getPCFromIP(tools.getIpAddress(rowIp).GetAddressBytes());
                bool active = rowPc != null && rowPc.captivePortalEnabled;
                _captivePortalItem.Text    = active ? "\uD83D\uDEAB  Captive Portal  \u2714 ON" : "\uD83D\uDEAB  Captive Portal";
                _captivePortalItem.Checked = active;
            };

            // Remove the default context menu from the grid and route manually
            this.treeGridView1.ContextMenuStrip = null;
            this.treeGridView1.MouseClick += treeGridView1_MouseClickContextMenu;
        }

        private void treeGridView1_MouseClickContextMenu(object sender, MouseEventArgs ev)
        {
            if (ev.Button != MouseButtons.Right) return;
            var hit = this.treeGridView1.HitTest(ev.X, ev.Y);
            if (hit.RowIndex >= 2)
            {
                _contextMenuRowIndex = hit.RowIndex;
                rowContextMenu.Show(this.treeGridView1, ev.Location);
            }
            else
            {
                ContextMenuViews.Show(this.treeGridView1, ev.Location);
            }
        }

        private void renameItem_Click(object sender, EventArgs e)
        {
            if (_contextMenuRowIndex < 2 || this.pcs == null) return;
            string ipStr = this.treeGridView1.Rows[_contextMenuRowIndex].Cells[1].Value?.ToString();
            if (string.IsNullOrEmpty(ipStr)) return;
            PC pc = this.pcs.getPCFromIP(tools.getIpAddress(ipStr).GetAddressBytes());
            if (pc == null || pc.isLocalPc || pc.isGateway) return;

            string currentName = this.treeGridView1.Rows[_contextMenuRowIndex].Cells[0].Value?.ToString() ?? string.Empty;
            // Strip any appended DNS suffix (" → hostname") before showing in input box
            int arrow = currentName.IndexOf("  \u2192 ");
            if (arrow >= 0) currentName = currentName.Substring(0, arrow);

            using (Form dlg = new Form())
            {
                dlg.Text = "Rename Device";
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.Width = 360;
                dlg.Height = 150;
                dlg.BackColor = UITheme.Surface0;

                var lbl = new Label
                {
                    Text = "Enter a name for " + ipStr + ":",
                    ForeColor = UITheme.TextSecondary,
                    Font = new System.Drawing.Font("Segoe UI", 9f),
                    Location = new System.Drawing.Point(12, 14),
                    Size = new System.Drawing.Size(320, 20)
                };
                var txt = new TextBox
                {
                    Text = currentName,
                    Location = new System.Drawing.Point(12, 38),
                    Size = new System.Drawing.Size(320, 24),
                    BackColor = UITheme.Surface1,
                    ForeColor = UITheme.TextPrimary,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new System.Drawing.Font("Segoe UI", 10f)
                };
                var btnOk = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Width = 80,
                    Height = 28,
                    Location = new System.Drawing.Point(164, 74),
                    BackColor = UITheme.Accent,
                    ForeColor = System.Drawing.Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnOk.FlatAppearance.BorderSize = 0;
                var btnCancel = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Width = 80,
                    Height = 28,
                    Location = new System.Drawing.Point(254, 74),
                    BackColor = UITheme.Surface1,
                    ForeColor = UITheme.TextPrimary,
                    FlatStyle = FlatStyle.Flat
                };
                btnCancel.FlatAppearance.BorderSize = 0;

                dlg.Controls.AddRange(new System.Windows.Forms.Control[] { lbl, txt, btnOk, btnCancel });
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;
                txt.SelectAll();

                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                string newName = txt.Text.Trim();
                pc.name = newName;
                // Preserve any DNS suffix that was already displayed
                string displayed = this.treeGridView1.Rows[_contextMenuRowIndex].Cells[0].Value?.ToString() ?? string.Empty;
                int arrowPos = displayed.IndexOf("  \u2192 ");
                this.treeGridView1.Rows[_contextMenuRowIndex].Cells[0].Value =
                    arrowPos >= 0 ? newName + displayed.Substring(arrowPos) : newName;
                deviceProfiles.Save(pc);
            }
        }

        private void cutOffItem_Click(object sender, EventArgs e)
        {
            if (!isSpoofingActive)
            {
                this.lblStatus.Text = "  \u26A0 Start Spoofing first to use Cut Off";
                return;
            }
            if (_contextMenuRowIndex < 2 || this.pcs == null) return;
            string ipStr = this.treeGridView1.Rows[_contextMenuRowIndex].Cells[1].Value?.ToString();
            if (string.IsNullOrEmpty(ipStr)) return;
            PC pc = this.pcs.getPCFromIP(tools.getIpAddress(ipStr).GetAddressBytes());
            if (pc == null) return;
            Monitor.Enter((object)pc);
            pc.capDown = 1024;
            pc.capUp = 1024;
            Monitor.Exit((object)pc);
            this.treeGridView1.CancelEdit();
            this.treeGridView1.Rows[_contextMenuRowIndex].Cells[5].Value = 1;
            this.treeGridView1.Rows[_contextMenuRowIndex].Cells[6].Value = 1;
        }

        private void unblockItem_Click(object sender, EventArgs e)
        {
            if (_contextMenuRowIndex < 2 || this.pcs == null) return;
            string ipStr = this.treeGridView1.Rows[_contextMenuRowIndex].Cells[1].Value?.ToString();
            if (string.IsNullOrEmpty(ipStr)) return;
            PC pc = this.pcs.getPCFromIP(tools.getIpAddress(ipStr).GetAddressBytes());
            if (pc == null) return;
            Monitor.Enter((object)pc);
            pc.capDown = 0;
            pc.capUp = 0;
            Monitor.Exit((object)pc);
            this.treeGridView1.CancelEdit();
            this.treeGridView1.Rows[_contextMenuRowIndex].Cells[5].Value = 0;
            this.treeGridView1.Rows[_contextMenuRowIndex].Cells[6].Value = 0;
        }

        private void graphItem_Click(object sender, EventArgs e)
        {
            if (_contextMenuRowIndex < 2 || this.pcs == null) return;
            string ipStr = this.treeGridView1.Rows[_contextMenuRowIndex].Cells[1].Value?.ToString();
            if (string.IsNullOrEmpty(ipStr)) return;
            PC pc = this.pcs.getPCFromIP(tools.getIpAddress(ipStr).GetAddressBytes());
            if (pc == null) return;
            new BandwidthGraphForm(pc).Show(this);
        }

        private void portsItem_Click(object sender, EventArgs e)
        {
            if (_contextMenuRowIndex < 2 || this.pcs == null) return;
            string ipStr = this.treeGridView1.Rows[_contextMenuRowIndex].Cells[1].Value?.ToString();
            if (string.IsNullOrEmpty(ipStr)) return;

            // Common TCP ports to probe
            int[] ports = {
                21, 22, 23, 25, 53, 80, 110, 135, 139, 143,
                443, 445, 554, 587, 1080, 1433, 1900, 3306, 3389,
                5900, 6881, 7070, 8080, 8443, 8888, 9100, 49152
            };

            // Build the scanner dialog
            Form dlg = new Form();
            dlg.Text = "Port Scan — " + ipStr;
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            dlg.Width  = 420;
            dlg.Height = 480;
            dlg.MinimumSize = new System.Drawing.Size(320, 320);
            dlg.BackColor = UITheme.Surface0;

            var lblTitle = new Label
            {
                Text = "Scanning " + ipStr + " (" + ports.Length + " ports)...",
                ForeColor = UITheme.TextSecondary,
                Font = new System.Drawing.Font("Segoe UI", 9f),
                Location = new System.Drawing.Point(10, 8),
                Size = new System.Drawing.Size(390, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var txtResults = new TextBox
            {
                Multiline   = true,
                ReadOnly    = true,
                ScrollBars  = ScrollBars.Vertical,
                Location    = new System.Drawing.Point(10, 32),
                Size        = new System.Drawing.Size(382, 380),
                BackColor   = UITheme.Surface1,
                ForeColor   = UITheme.TextPrimary,
                BorderStyle = BorderStyle.None,
                Font        = new System.Drawing.Font("Consolas", 9.5f),
                Anchor      = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            dlg.Controls.Add(lblTitle);
            dlg.Controls.Add(txtResults);
            dlg.Show(this);

            // Run scan on background thread; marshal results to UI thread
            System.Threading.Thread scanThread = new System.Threading.Thread(() =>
            {
                int open = 0;
                int connectTimeoutMs = 400;

                // Run all probes in parallel using Tasks so the overall scan is fast
                var tasks = new System.Collections.Generic.List<System.Threading.Tasks.Task>();
                var results = new System.Collections.Concurrent.ConcurrentBag<(int port, bool isOpen)>();

                foreach (int port in ports)
                {
                    int p = port;
                    tasks.Add(System.Threading.Tasks.Task.Run(async () =>
                    {
                        bool isOpen = false;
                        try
                        {
                            using (var tc = new System.Net.Sockets.TcpClient())
                            {
                                var cts = new System.Threading.CancellationTokenSource(connectTimeoutMs);
                                await tc.ConnectAsync(ipStr, p).WaitAsync(cts.Token);
                                isOpen = true;
                            }
                        }
                        catch { }
                        results.Add((p, isOpen));

                        // Append result line to the textbox live
                        string line = string.Format("{0,-6}  {1}\r\n",
                            p, isOpen ? "OPEN" : "closed");
                        if (!dlg.IsDisposed)
                        {
                            try
                            {
                                dlg.Invoke((Action)(() =>
                                {
                                    if (!txtResults.IsDisposed)
                                        txtResults.AppendText(line);
                                }));
                            }
                            catch { }
                        }
                    }));
                }

                System.Threading.Tasks.Task.WaitAll(tasks.ToArray());

                // Count open ports and update label
                foreach (var r in results)
                    if (r.isOpen) open++;

                if (!dlg.IsDisposed)
                {
                    try
                    {
                        dlg.Invoke((Action)(() =>
                        {
                            if (!lblTitle.IsDisposed)
                                lblTitle.Text = "Scan complete — " + open + " open port(s) found on " + ipStr;
                        }));
                    }
                    catch { }
                }
            });
            scanThread.IsBackground = true;
            scanThread.Start();
        }

        private void dnsLogItem_Click(object sender, EventArgs e)
        {
            if (_contextMenuRowIndex < 2 || this.pcs == null || this.cArp == null) return;
            string ipStr = this.treeGridView1.Rows[_contextMenuRowIndex].Cells[1].Value?.ToString();
            if (string.IsNullOrEmpty(ipStr)) return;
            PC pc = this.pcs.getPCFromIP(tools.getIpAddress(ipStr).GetAddressBytes());
            if (pc == null) return;

            Form dlg = new Form();
            dlg.Text = "DNS Log \u2014 " + ipStr;
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            dlg.Width  = 520;
            dlg.Height = 420;
            dlg.MinimumSize = new System.Drawing.Size(380, 260);
            dlg.BackColor = UITheme.Surface0;

            var lblTitle = new Label
            {
                Text      = isSpoofingActive
                            ? "Live DNS queries from " + ipStr + " (waiting...)"
                            : "\u26A0  Spoofing is not active \u2014 start spoofing to capture DNS",
                ForeColor = isSpoofingActive ? UITheme.TextSecondary : System.Drawing.Color.Orange,
                Font      = new System.Drawing.Font("Segoe UI", 9f),
                Location  = new System.Drawing.Point(10, 8),
                Size      = new System.Drawing.Size(490, 18),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var txtLog = new TextBox
            {
                Multiline   = true,
                ReadOnly    = true,
                ScrollBars  = ScrollBars.Vertical,
                Location    = new System.Drawing.Point(10, 32),
                Size        = new System.Drawing.Size(482, 340),
                BackColor   = UITheme.Surface1,
                ForeColor   = UITheme.TextPrimary,
                BorderStyle = BorderStyle.None,
                Font        = new System.Drawing.Font("Consolas", 9.5f),
                Anchor      = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            dlg.Controls.Add(lblTitle);
            dlg.Controls.Add(txtLog);
            dlg.Show(this);

            Action<PC, string> handler = (logPC, hostname) =>
            {
                if (logPC != pc) return;
                string line = string.Format("[{0}]  {1}\r\n",
                    DateTime.Now.ToString("HH:mm:ss"), hostname);
                if (!dlg.IsDisposed)
                {
                    try
                    {
                        dlg.Invoke((Action)(() =>
                        {
                            if (!txtLog.IsDisposed)
                            {
                                txtLog.AppendText(line);
                                if (isSpoofingActive)
                                    lblTitle.Text = "Live DNS queries from " + ipStr;
                            }
                        }));
                    }
                    catch { }
                }
            };

            this.cArp.OnDnsQuery += handler;
            dlg.FormClosed += (s2, e2) =>
            {
                if (this.cArp != null)
                    this.cArp.OnDnsQuery -= handler;
            };
        }

        private void httpLogItem_Click(object sender, EventArgs e)
        {
            if (_contextMenuRowIndex < 2 || this.pcs == null || this.cArp == null) return;
            string ipStr = this.treeGridView1.Rows[_contextMenuRowIndex].Cells[1].Value?.ToString();
            if (string.IsNullOrEmpty(ipStr)) return;
            PC pc = this.pcs.getPCFromIP(tools.getIpAddress(ipStr).GetAddressBytes());
            if (pc == null) return;

            Form dlg = new Form();
            dlg.Text = "HTTP Log \u2014 " + ipStr;
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            dlg.Width  = 520;
            dlg.Height = 420;
            dlg.MinimumSize = new System.Drawing.Size(380, 260);
            dlg.BackColor = UITheme.Surface0;

            var lblTitle = new Label
            {
                Text      = isSpoofingActive
                            ? "Live HTTP hosts from " + ipStr + " (waiting...)"
                            : "\u26A0  Spoofing is not active \u2014 start spoofing to capture HTTP",
                ForeColor = isSpoofingActive ? UITheme.TextSecondary : System.Drawing.Color.Orange,
                Font      = new System.Drawing.Font("Segoe UI", 9f),
                Location  = new System.Drawing.Point(10, 8),
                Size      = new System.Drawing.Size(490, 18),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var txtLog = new TextBox
            {
                Multiline   = true,
                ReadOnly    = true,
                ScrollBars  = ScrollBars.Vertical,
                Location    = new System.Drawing.Point(10, 32),
                Size        = new System.Drawing.Size(482, 340),
                BackColor   = UITheme.Surface1,
                ForeColor   = UITheme.TextPrimary,
                BorderStyle = BorderStyle.None,
                Font        = new System.Drawing.Font("Consolas", 9.5f),
                Anchor      = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            dlg.Controls.Add(lblTitle);
            dlg.Controls.Add(txtLog);
            dlg.Show(this);

            Action<PC, string> handler = (logPC, host) =>
            {
                if (logPC != pc) return;
                string line = string.Format("[{0}]  {1}\r\n",
                    DateTime.Now.ToString("HH:mm:ss"), host);
                if (!dlg.IsDisposed)
                {
                    try
                    {
                        dlg.Invoke((Action)(() =>
                        {
                            if (!txtLog.IsDisposed)
                            {
                                txtLog.AppendText(line);
                                if (isSpoofingActive)
                                    lblTitle.Text = "Live HTTP hosts from " + ipStr;
                            }
                        }));
                    }
                    catch { }
                }
            };

            this.cArp.OnHttpHost += handler;
            dlg.FormClosed += (s2, e2) =>
            {
                if (this.cArp != null)
                    this.cArp.OnHttpHost -= handler;
            };
        }

        private void captivePortalItem_Click(object sender, EventArgs e)
        {
            if (!isSpoofingActive)
            {
                this.lblStatus.Text = "  \u26A0 Start Spoofing first to use Captive Portal";
                return;
            }
            if (_contextMenuRowIndex < 2 || this.pcs == null) return;
            string ipStr = this.treeGridView1.Rows[_contextMenuRowIndex].Cells[1].Value?.ToString();
            if (string.IsNullOrEmpty(ipStr)) return;
            PC pc = this.pcs.getPCFromIP(tools.getIpAddress(ipStr).GetAddressBytes());
            if (pc == null || pc.isLocalPc || pc.isGateway) return;

            showCaptivePortalDialog(pc, ipStr);
        }

        private void showCaptivePortalDialog(PC pc, string ipStr)
        {
            Form dlg = new Form();
            dlg.Text = "Captive Portal \u2014 " + ipStr;
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.MaximizeBox = false; dlg.MinimizeBox = false;
            dlg.Width = 520; dlg.Height = 430;
            dlg.BackColor = UITheme.Surface0;

            var lblInfo = new Label
            {
                Text = pc.captivePortalEnabled
                     ? "\u2714  Captive portal is ACTIVE for " + ipStr
                     : "Enable captive portal for " + ipStr + ":",
                ForeColor = pc.captivePortalEnabled ? System.Drawing.Color.LightGreen : UITheme.TextSecondary,
                Font = new System.Drawing.Font("Segoe UI", 9f),
                Location = new System.Drawing.Point(12, 12),
                Size = new System.Drawing.Size(480, 18)
            };

            var lblHtml = new Label
            {
                Text = "Portal page HTML (edit to customise the message shown to the device):",
                ForeColor = UITheme.TextMuted,
                Font = new System.Drawing.Font("Segoe UI", 8.5f),
                Location = new System.Drawing.Point(12, 36),
                Size = new System.Drawing.Size(480, 18)
            };

            var txtHtml = new TextBox
            {
                Text = _captivePortalHtml,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Location = new System.Drawing.Point(12, 58),
                Size = new System.Drawing.Size(480, 270),
                BackColor = UITheme.Surface1,
                ForeColor = UITheme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new System.Drawing.Font("Consolas", 8.5f)
            };

            var btnToggle = new Button
            {
                Text = pc.captivePortalEnabled ? "Disable Portal" : "Enable Portal",
                DialogResult = DialogResult.OK,
                Width = 130,
                Height = 32,
                Location = new System.Drawing.Point(12, 342),
                BackColor = pc.captivePortalEnabled ? UITheme.Danger : UITheme.Accent,
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnToggle.FlatAppearance.BorderSize = 0;

            var btnCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Width = 90,
                Height = 32,
                Location = new System.Drawing.Point(154, 342),
                BackColor = UITheme.Surface1,
                ForeColor = UITheme.TextPrimary,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            dlg.Controls.AddRange(new System.Windows.Forms.Control[] { lblInfo, lblHtml, txtHtml, btnToggle, btnCancel });
            dlg.AcceptButton = btnToggle;
            dlg.CancelButton = btnCancel;

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            // Persist edited HTML
            _captivePortalHtml = txtHtml.Text;

            // Toggle the per-device flag
            pc.captivePortalEnabled = !pc.captivePortalEnabled;

            if (pc.captivePortalEnabled)
            {
                startCaptivePortalServer();
                this.lblStatus.Text = "  \uD83D\uDEAB Captive portal active for " + ipStr;
            }
            else
            {
                // Stop the server only if no other device still has the portal enabled
                bool anyActive = false;
                if (this.pcs != null)
                    foreach (var p in this.pcs.pclist)
                        if (p.captivePortalEnabled) { anyActive = true; break; }
                if (!anyActive)
                    stopCaptivePortalServer();
                this.lblStatus.Text = "  Captive portal disabled for " + ipStr;
            }
        }

        /// <summary>
        /// Starts a raw TcpListener on port 80 that serves the captive portal page.
        /// This avoids HttpListener's netsh URL-ACL requirement while still working
        /// with DNS-hijacked connections from iOS and Android devices.
        /// Does nothing if the server is already running.
        /// </summary>
        private void startCaptivePortalServer()
        {
            if (_captiveListener != null) return;
            // Add an inbound Windows Firewall rule so the device can reach us on port 80
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "advfirewall firewall add rule name=\"LanLord Captive Portal\" dir=in action=allow protocol=TCP localport=80 profile=any",
                    UseShellExecute = false,
                    CreateNoWindow  = true
                })?.WaitForExit(3000);
            }
            catch { }
            try
            {
                _captiveListener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, 80);
                _captiveListener.Start();
                if (this.cArp != null)
                    this.cArp.captivePortalUrl = "http://" + new System.Net.IPAddress(this.cArp.localIP) + "/";
                // Service loop runs on a background Task (Form-side async is acceptable)
                System.Threading.Tasks.Task.Run((Action)serveCaptiveRequests);
            }
            catch (Exception ex)
            {
                if (_captiveListener != null) { try { _captiveListener.Stop(); } catch { } }
                _captiveListener = null;
                MessageBox.Show(
                    "Could not start captive portal server on port 80:\n" + ex.Message +
                    "\n\nMake sure LanLord is running as Administrator and that nothing else is using port 80.",
                    "Captive Portal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Accepts TCP connections on port 80 and responds with the portal HTML page.
        /// Runs on a background Task until <see cref="stopCaptivePortalServer"/> is called.
        /// </summary>
        private void serveCaptiveRequests()
        {
            while (_captiveListener != null)
            {
                System.Net.Sockets.TcpClient client;
                try { client = _captiveListener.AcceptTcpClient(); }
                catch { break; }
                System.Threading.ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        using (client)
                        {
                            var ns = client.GetStream();
                            // Drain the HTTP request (we don't need to parse it)
                            byte[] reqBuf = new byte[4096];
                            ns.ReadTimeout = 2000;
                            try { ns.Read(reqBuf, 0, reqBuf.Length); } catch { }
                            // Send HTTP 200 with the portal page
                            byte[] body    = System.Text.Encoding.UTF8.GetBytes(_captivePortalHtml);
                            string hdr     = "HTTP/1.1 200 OK\r\n" +
                                             "Content-Type: text/html; charset=utf-8\r\n" +
                                             "Content-Length: " + body.Length + "\r\n" +
                                             "Connection: close\r\n\r\n";
                            byte[] hdrBytes = System.Text.Encoding.ASCII.GetBytes(hdr);
                            ns.Write(hdrBytes, 0, hdrBytes.Length);
                            ns.Write(body,     0, body.Length);
                        }
                    }
                    catch { }
                });
            }
        }

        /// <summary>Stops the captive portal HTTP server, clears the redirect URL, and removes the firewall rule.</summary>
        private void stopCaptivePortalServer()
        {
            if (_captiveListener == null) return;
            try { _captiveListener.Stop(); } catch { }
            _captiveListener = null;
            if (this.cArp != null) this.cArp.captivePortalUrl = null;
            // Remove the temporary firewall rule added at start
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "advfirewall firewall delete rule name=\"LanLord Captive Portal\"",
                    UseShellExecute = false,
                    CreateNoWindow  = true
                })?.WaitForExit(3000);
            }
            catch { }
        }

        private void TreeGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 2 || this.pcs == null) return;
            string ipStr = this.treeGridView1.Rows[e.RowIndex].Cells[1].Value?.ToString();
            if (string.IsNullOrEmpty(ipStr)) return;
            PC pc = this.pcs.getPCFromIP(tools.getIpAddress(ipStr).GetAddressBytes());
            if (pc == null || pc.isLocalPc || pc.isGateway) return;
            new BandwidthGraphForm(pc).Show(this);
        }

        #endregion

        #region v4: Block All / Unblock All / Export CSV

        private void btnBlockAll_Click(object sender, EventArgs e)
        {
            if (!isSpoofingActive)
            {
                this.lblStatus.Text = "  \u26A0 Start Spoofing first to use Block All";
                return;
            }
            this.treeGridView1.CancelEdit();
            for (int i = 0; i < this.treeGridView1.Nodes[0].Nodes.Count; i++)
            {
                TreeGridNode node = this.treeGridView1.Nodes[0].Nodes[i];
                string ipStr = node.Cells[1].Value?.ToString();
                if (string.IsNullOrEmpty(ipStr)) continue;
                PC pc = this.pcs.getPCFromIP(tools.getIpAddress(ipStr).GetAddressBytes());
                if (pc == null || pc.isLocalPc || pc.isGateway) continue;
                Monitor.Enter((object)pc);
                pc.capDown = 1024;
                pc.capUp = 1024;
                Monitor.Exit((object)pc);
                node.Cells[5].Value = 1;
                node.Cells[6].Value = 1;
                deviceProfiles.Save(pc);
            }
            this.lblStatus.Text = "  All devices capped to 1 KB/s";
        }

        private void btnUnblockAll_Click(object sender, EventArgs e)
        {
            if (!isSpoofingActive)
            {
                this.lblStatus.Text = "  \u26A0 Start Spoofing first to use Unblock All";
                return;
            }
            this.treeGridView1.CancelEdit();
            for (int i = 0; i < this.treeGridView1.Nodes[0].Nodes.Count; i++)
            {
                TreeGridNode node = this.treeGridView1.Nodes[0].Nodes[i];
                string ipStr = node.Cells[1].Value?.ToString();
                if (string.IsNullOrEmpty(ipStr)) continue;
                PC pc = this.pcs.getPCFromIP(tools.getIpAddress(ipStr).GetAddressBytes());
                if (pc == null || pc.isLocalPc || pc.isGateway) continue;
                Monitor.Enter((object)pc);
                pc.capDown = 0;
                pc.capUp = 0;
                Monitor.Exit((object)pc);
                node.Cells[5].Value = 0;
                node.Cells[6].Value = 0;
                deviceProfiles.Save(pc);
            }
            this.lblStatus.Text = "  All bandwidth caps removed";
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog
            {
                FileName = "network_devices.csv",
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Export Devices to CSV"
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Name,IP,MAC,Download,Upload,DownCap (KB/s),UpCap (KB/s),Block,Spoof");
                for (int i = 0; i < this.treeGridView1.Nodes[0].Nodes.Count; i++)
                {
                    var cells = this.treeGridView1.Nodes[0].Nodes[i].Cells;
                    sb.Append(csvEscape(cells[0].Value)).Append(",");
                    sb.Append(csvEscape(cells[1].Value)).Append(",");
                    sb.Append(csvEscape(cells[2].Value)).Append(",");
                    sb.Append(csvEscape(cells[3].Value)).Append(",");
                    sb.Append(csvEscape(cells[4].Value)).Append(",");
                    sb.Append(csvEscape(cells[5].Value)).Append(",");
                    sb.Append(csvEscape(cells[6].Value)).Append(",");
                    sb.Append(csvEscape(cells[7].Value)).Append(",");
                    sb.AppendLine(csvEscape(cells[8].Value));
                }
                File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                this.lblStatus.Text = string.Format("  Exported {0} device(s) to {1}", this.treeGridView1.Nodes[0].Nodes.Count, Path.GetFileName(dlg.FileName));
            }
        }

        private string csvEscape(object val)
        {
            string s = val?.ToString() ?? "";
            if (s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
                s = "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }

        #endregion

        private static int WM_QUERYENDSESSION = 0x11;
        public static bool systemShutdown = false;
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == WM_QUERYENDSESSION)
            {
                //MessageBox.Show("queryendsession: this is a logoff, shutdown, or reboot");
                systemShutdown = true;
            }

            // If this is WM_QUERYENDSESSION, the closing event should be  
            // raised in the base WndProc.  
            base.WndProc(ref m);

        } //WndProc   

#pragma warning restore  // Falta el comentario XML para el tipo o miembro visible públicamente
    }
}