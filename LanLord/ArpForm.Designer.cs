using AdvancedDataGridView;

namespace LanLord
{
    partial class ArpForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            AdvancedDataGridView.TreeGridNode treeGridNode2 = new AdvancedDataGridView.TreeGridNode();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArpForm));
            this.panelActionBar = new System.Windows.Forms.Panel();
            this.btnScanNetwork = new System.Windows.Forms.Button();
            this.btnStartSpoof = new System.Windows.Forms.Button();
            this.btnStopSpoof = new System.Windows.Forms.Button();
            this.btnBlockAll = new System.Windows.Forms.Button();
            this.btnUnblockAll = new System.Windows.Forms.Button();
            this.btnExportCsv = new System.Windows.Forms.Button();
            this.treeGridView1 = new AdvancedDataGridView.TreeGridView();
            this.ColPCName = new AdvancedDataGridView.TreeGridColumn();
            this.ColPCIP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColPCMac = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColDownload = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColUpload = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColDownCap = new DataGridViewNumericUpDownElements.DataGridViewNumericUpDownColumn();
            this.ColUploadCap = new DataGridViewNumericUpDownElements.DataGridViewNumericUpDownColumn();
            this.ColBlock = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColSpoof = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ContextMenuViews = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ViewMenuIP = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenuMAC = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenuDownload = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenuUpload = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenuDownloadCap = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenuUploadCap = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenuBlock = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenuSpoof = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.timerSpoof = new System.Windows.Forms.Timer(this.components);
            this.LanLordTrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.LanLordTray = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timerDiscovery = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelActionBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeGridView1)).BeginInit();
            this.ContextMenuViews.SuspendLayout();
            this.LanLordTray.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelActionBar
            // 
            this.panelActionBar.Controls.Add(this.btnStopSpoof);
            this.panelActionBar.Controls.Add(this.btnStartSpoof);
            this.panelActionBar.Controls.Add(this.btnScanNetwork);
            this.panelActionBar.Controls.Add(this.btnBlockAll);
            this.panelActionBar.Controls.Add(this.btnUnblockAll);
            this.panelActionBar.Controls.Add(this.btnExportCsv);
            this.panelActionBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelActionBar.BackColor = System.Drawing.Color.FromArgb(13, 17, 23);
            this.panelActionBar.Height = 68;
            this.panelActionBar.Name = "panelActionBar";
            this.panelActionBar.TabIndex = 0;
            // 
            // btnScanNetwork
            // 
            this.btnScanNetwork.BackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            this.btnScanNetwork.ForeColor = System.Drawing.Color.White;
            this.btnScanNetwork.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnScanNetwork.FlatAppearance.BorderSize = 0;
            this.btnScanNetwork.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnScanNetwork.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(22, 63, 181);
            this.btnScanNetwork.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnScanNetwork.Location = new System.Drawing.Point(14, 12);
            this.btnScanNetwork.Size = new System.Drawing.Size(175, 44);
            this.btnScanNetwork.TabIndex = 0;
            this.btnScanNetwork.Text = "\u27F3  Scan Network";
            this.btnScanNetwork.UseVisualStyleBackColor = false;
            this.btnScanNetwork.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnScanNetwork.Click += new System.EventHandler(this.ToolStripButton1_Click);
            // 
            // btnStartSpoof
            // 
            this.btnStartSpoof.BackColor = System.Drawing.Color.FromArgb(21, 128, 61);
            this.btnStartSpoof.ForeColor = System.Drawing.Color.White;
            this.btnStartSpoof.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartSpoof.FlatAppearance.BorderSize = 0;
            this.btnStartSpoof.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(22, 163, 74);
            this.btnStartSpoof.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(16, 100, 47);
            this.btnStartSpoof.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnStartSpoof.Location = new System.Drawing.Point(201, 12);
            this.btnStartSpoof.Size = new System.Drawing.Size(175, 44);
            this.btnStartSpoof.TabIndex = 1;
            this.btnStartSpoof.Text = "\u25BA  Start Spoofing";
            this.btnStartSpoof.UseVisualStyleBackColor = false;
            this.btnStartSpoof.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStartSpoof.Click += new System.EventHandler(this.ToolStripButton2_Click);
            // 
            // btnStopSpoof
            // 
            this.btnStopSpoof.BackColor = System.Drawing.Color.FromArgb(153, 27, 27);
            this.btnStopSpoof.ForeColor = System.Drawing.Color.White;
            this.btnStopSpoof.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopSpoof.FlatAppearance.BorderSize = 0;
            this.btnStopSpoof.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(185, 28, 28);
            this.btnStopSpoof.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(127, 22, 22);
            this.btnStopSpoof.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnStopSpoof.Location = new System.Drawing.Point(388, 12);
            this.btnStopSpoof.Size = new System.Drawing.Size(175, 44);
            this.btnStopSpoof.TabIndex = 2;
            this.btnStopSpoof.Text = "\u25A0  Stop Spoofing";
            this.btnStopSpoof.UseVisualStyleBackColor = false;
            this.btnStopSpoof.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStopSpoof.Click += new System.EventHandler(this.ToolStripButton3_Click);
            // 
            // btnBlockAll
            //
            this.btnBlockAll.BackColor = System.Drawing.Color.FromArgb(120, 30, 30);
            this.btnBlockAll.ForeColor = System.Drawing.Color.White;
            this.btnBlockAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBlockAll.FlatAppearance.BorderSize = 0;
            this.btnBlockAll.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(160, 40, 40);
            this.btnBlockAll.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(90, 20, 20);
            this.btnBlockAll.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnBlockAll.Location = new System.Drawing.Point(575, 12);
            this.btnBlockAll.Size = new System.Drawing.Size(130, 44);
            this.btnBlockAll.TabIndex = 3;
            this.btnBlockAll.Text = "\u26D9  Block All";
            this.btnBlockAll.UseVisualStyleBackColor = false;
            this.btnBlockAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBlockAll.Click += new System.EventHandler(this.btnBlockAll_Click);
            //
            // btnUnblockAll
            //
            this.btnUnblockAll.BackColor = System.Drawing.Color.FromArgb(30, 80, 30);
            this.btnUnblockAll.ForeColor = System.Drawing.Color.White;
            this.btnUnblockAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUnblockAll.FlatAppearance.BorderSize = 0;
            this.btnUnblockAll.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(40, 110, 40);
            this.btnUnblockAll.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(20, 60, 20);
            this.btnUnblockAll.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnUnblockAll.Location = new System.Drawing.Point(717, 12);
            this.btnUnblockAll.Size = new System.Drawing.Size(130, 44);
            this.btnUnblockAll.TabIndex = 4;
            this.btnUnblockAll.Text = "\u2714  Unblock All";
            this.btnUnblockAll.UseVisualStyleBackColor = false;
            this.btnUnblockAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUnblockAll.Click += new System.EventHandler(this.btnUnblockAll_Click);
            //
            // btnExportCsv
            //
            this.btnExportCsv.BackColor = System.Drawing.Color.FromArgb(50, 50, 60);
            this.btnExportCsv.ForeColor = System.Drawing.Color.White;
            this.btnExportCsv.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportCsv.FlatAppearance.BorderSize = 0;
            this.btnExportCsv.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(70, 70, 85);
            this.btnExportCsv.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(35, 35, 45);
            this.btnExportCsv.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnExportCsv.Location = new System.Drawing.Point(859, 12);
            this.btnExportCsv.Size = new System.Drawing.Size(130, 44);
            this.btnExportCsv.TabIndex = 5;
            this.btnExportCsv.Text = "\u21A7  Export CSV";
            this.btnExportCsv.UseVisualStyleBackColor = false;
            this.btnExportCsv.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExportCsv.Click += new System.EventHandler(this.btnExportCsv_Click);
            //
            // treeGridView1
            // 
            this.treeGridView1.AllowUserToAddRows = false;
            this.treeGridView1.AllowUserToDeleteRows = false;
            this.treeGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.treeGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.treeGridView1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.treeGridView1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.RaisedHorizontal;
            this.treeGridView1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.treeGridView1.ColumnHeadersHeight = 35;
            this.treeGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColPCName,
            this.ColPCIP,
            this.ColPCMac,
            this.ColDownload,
            this.ColUpload,
            this.ColDownCap,
            this.ColUploadCap,
            this.ColBlock,
            this.ColSpoof});
            this.treeGridView1.ContextMenuStrip = this.ContextMenuViews;
            this.treeGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.treeGridView1.ImageList = this.imageList1;
            this.treeGridView1.Name = "treeGridView1";
            treeGridNode2.Height = 27;
            treeGridNode2.ImageIndex = 0;
            this.treeGridView1.Nodes.Add(treeGridNode2);
            this.treeGridView1.RowHeadersVisible = false;
            this.treeGridView1.RowHeadersWidth = 62;
            this.treeGridView1.ShowCellErrors = false;
            this.treeGridView1.ShowCellToolTips = false;
            this.treeGridView1.ShowEditingIcon = false;
            this.treeGridView1.ShowRowErrors = false;
            this.treeGridView1.TabIndex = 1;
            this.treeGridView1.BackgroundColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.treeGridView1.GridColor = System.Drawing.Color.FromArgb(65, 65, 65);
            this.treeGridView1.EnableHeadersVisualStyles = false;
            this.treeGridView1.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            this.treeGridView1.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            this.treeGridView1.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.treeGridView1.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(37, 37, 38);
            this.treeGridView1.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            this.treeGridView1.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.treeGridView1.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            this.treeGridView1.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(50, 50, 52);
            this.treeGridView1.AlternatingRowsDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            this.treeGridView1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.treeGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.TreeGridView1_CellValueChanged);
            this.treeGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.TreeGridView1_CellClick);
            this.treeGridView1.CurrentCellDirtyStateChanged += new System.EventHandler(this.TreeGridView1_CurrentCellDirtyStateChanged);
            this.treeGridView1.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.TreeGridView1_EditingControlShowing);
            this.treeGridView1.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.TreeGridView1_DataError);
            // 
            // ColPCName
            // 
            this.ColPCName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColPCName.DefaultNodeImage = null;
            this.ColPCName.FillWeight = 200F;
            this.ColPCName.HeaderText = "PC Name";
            this.ColPCName.MinimumWidth = 40;
            this.ColPCName.Name = "ColPCName";
            this.ColPCName.ReadOnly = true;
            this.ColPCName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ColPCIP
            // 
            this.ColPCIP.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColPCIP.HeaderText = "IP";
            this.ColPCIP.MinimumWidth = 35;
            this.ColPCIP.Name = "ColPCIP";
            this.ColPCIP.ReadOnly = true;
            this.ColPCIP.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ColPCMac
            // 
            this.ColPCMac.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColPCMac.HeaderText = "Mac";
            this.ColPCMac.MinimumWidth = 35;
            this.ColPCMac.Name = "ColPCMac";
            this.ColPCMac.ReadOnly = true;
            this.ColPCMac.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ColDownload
            // 
            this.ColDownload.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColDownload.HeaderText = "Download";
            this.ColDownload.MinimumWidth = 20;
            this.ColDownload.Name = "ColDownload";
            this.ColDownload.ReadOnly = true;
            this.ColDownload.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ColUpload
            // 
            this.ColUpload.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColUpload.HeaderText = "Upload";
            this.ColUpload.MinimumWidth = 20;
            this.ColUpload.Name = "ColUpload";
            this.ColUpload.ReadOnly = true;
            this.ColUpload.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ColDownCap
            // 
            this.ColDownCap.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColDownCap.HeaderText = "DownCap (KB/s)";
            this.ColDownCap.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
            this.ColDownCap.MinimumWidth = 35;
            this.ColDownCap.Name = "ColDownCap";
            this.ColDownCap.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ColDownCap.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(37, 37, 38);
            this.ColDownCap.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            this.ColDownCap.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.ColDownCap.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            // 
            // ColUploadCap
            // 
            this.ColUploadCap.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColUploadCap.HeaderText = "UploadCap (KB/s)";
            this.ColUploadCap.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
            this.ColUploadCap.MinimumWidth = 35;
            this.ColUploadCap.Name = "ColUploadCap";
            this.ColUploadCap.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ColUploadCap.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(37, 37, 38);
            this.ColUploadCap.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            this.ColUploadCap.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.ColUploadCap.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            // 
            // ColBlock
            // 
            this.ColBlock.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColBlock.HeaderText = "Block";
            this.ColBlock.MinimumWidth = 35;
            this.ColBlock.Name = "ColBlock";
            // 
            // ColSpoof
            // 
            this.ColSpoof.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ColSpoof.HeaderText = "Spoof";
            this.ColSpoof.MinimumWidth = 35;
            this.ColSpoof.Name = "ColSpoof";
            this.ColSpoof.Visible = false;
            // 
            // ContextMenuViews
            // 
            this.ContextMenuViews.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.ContextMenuViews.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ViewMenuIP,
            this.ViewMenuMAC,
            this.ViewMenuDownload,
            this.ViewMenuUpload,
            this.ViewMenuDownloadCap,
            this.ViewMenuUploadCap,
            this.ViewMenuBlock,
            this.ViewMenuSpoof});
            this.ContextMenuViews.Name = "ContextMenuViews";
            this.ContextMenuViews.Size = new System.Drawing.Size(239, 260);
            // 
            // ViewMenuIP
            // 
            this.ViewMenuIP.Checked = true;
            this.ViewMenuIP.CheckOnClick = true;
            this.ViewMenuIP.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ViewMenuIP.Name = "ViewMenuIP";
            this.ViewMenuIP.Size = new System.Drawing.Size(238, 32);
            this.ViewMenuIP.Text = "IP";
            this.ViewMenuIP.CheckStateChanged += new System.EventHandler(this.ViewMenuIP_CheckStateChanged);
            // 
            // ViewMenuMAC
            // 
            this.ViewMenuMAC.Checked = true;
            this.ViewMenuMAC.CheckOnClick = true;
            this.ViewMenuMAC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ViewMenuMAC.Name = "ViewMenuMAC";
            this.ViewMenuMAC.Size = new System.Drawing.Size(238, 32);
            this.ViewMenuMAC.Text = "Mac Address";
            this.ViewMenuMAC.CheckStateChanged += new System.EventHandler(this.ViewMenuMAC_CheckStateChanged);
            // 
            // ViewMenuDownload
            // 
            this.ViewMenuDownload.Checked = true;
            this.ViewMenuDownload.CheckOnClick = true;
            this.ViewMenuDownload.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ViewMenuDownload.Name = "ViewMenuDownload";
            this.ViewMenuDownload.Size = new System.Drawing.Size(238, 32);
            this.ViewMenuDownload.Text = "Download";
            this.ViewMenuDownload.CheckStateChanged += new System.EventHandler(this.ViewMenuDownload_CheckStateChanged);
            // 
            // ViewMenuUpload
            // 
            this.ViewMenuUpload.Checked = true;
            this.ViewMenuUpload.CheckOnClick = true;
            this.ViewMenuUpload.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ViewMenuUpload.Name = "ViewMenuUpload";
            this.ViewMenuUpload.Size = new System.Drawing.Size(238, 32);
            this.ViewMenuUpload.Text = "Upload";
            this.ViewMenuUpload.CheckStateChanged += new System.EventHandler(this.ViewMenuUpload_CheckStateChanged);
            // 
            // ViewMenuDownloadCap
            // 
            this.ViewMenuDownloadCap.Checked = true;
            this.ViewMenuDownloadCap.CheckOnClick = true;
            this.ViewMenuDownloadCap.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ViewMenuDownloadCap.Name = "ViewMenuDownloadCap";
            this.ViewMenuDownloadCap.Size = new System.Drawing.Size(238, 32);
            this.ViewMenuDownloadCap.Text = "Download Capacity";
            this.ViewMenuDownloadCap.CheckStateChanged += new System.EventHandler(this.ViewMenuDownloadCap_CheckStateChanged);
            // 
            // ViewMenuUploadCap
            // 
            this.ViewMenuUploadCap.Checked = true;
            this.ViewMenuUploadCap.CheckOnClick = true;
            this.ViewMenuUploadCap.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ViewMenuUploadCap.Name = "ViewMenuUploadCap";
            this.ViewMenuUploadCap.Size = new System.Drawing.Size(238, 32);
            this.ViewMenuUploadCap.Text = "Upload Capacity";
            this.ViewMenuUploadCap.CheckStateChanged += new System.EventHandler(this.ViewMenuUploadCap_CheckStateChanged);
            // 
            // ViewMenuBlock
            // 
            this.ViewMenuBlock.Checked = true;
            this.ViewMenuBlock.CheckOnClick = true;
            this.ViewMenuBlock.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ViewMenuBlock.Name = "ViewMenuBlock";
            this.ViewMenuBlock.Size = new System.Drawing.Size(238, 32);
            this.ViewMenuBlock.Text = "Block";
            this.ViewMenuBlock.CheckStateChanged += new System.EventHandler(this.ViewMenuBlock_CheckStateChanged);
            // 
            // ViewMenuSpoof
            // 
            this.ViewMenuSpoof.CheckOnClick = true;
            this.ViewMenuSpoof.Name = "ViewMenuSpoof";
            this.ViewMenuSpoof.Size = new System.Drawing.Size(238, 32);
            this.ViewMenuSpoof.Text = "Spoof";
            this.ViewMenuSpoof.CheckStateChanged += new System.EventHandler(this.ViewMenuSpoof_CheckStateChanged);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "toolStripButton1.Image.png");
            this.imageList1.Images.SetKeyName(1, "toolStripButton2.Image.png");
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // timer2
            // 
            this.timer2.Tick += new System.EventHandler(this.Timer2_Tick);
            // 
            // timerSpoof
            // 
            this.timerSpoof.Tick += new System.EventHandler(this.TimerSpoof_Tick);
            // 
            // LanLordTrayIcon
            // 
            this.LanLordTrayIcon.BalloonTipText = "LanLord is minimized";
            this.LanLordTrayIcon.BalloonTipTitle = "\"LanLord\"";
            this.LanLordTrayIcon.ContextMenuStrip = this.LanLordTray;
            this.LanLordTrayIcon.Text = "LanLord v3";
            this.LanLordTrayIcon.Visible = true;
            this.LanLordTrayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LanLordTrayIcon_MouseDoubleClick);
            // 
            // LanLordTray
            // 
            this.LanLordTray.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.LanLordTray.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.LanLordTray.Name = "LanLordTray";
            this.LanLordTray.Size = new System.Drawing.Size(137, 68);
            // 
            // showToolStripMenuItem
            // 
            this.showToolStripMenuItem.Image = global::LanLord.Properties.Resources._167;
            this.showToolStripMenuItem.Name = "showToolStripMenuItem";
            this.showToolStripMenuItem.Size = new System.Drawing.Size(136, 32);
            this.showToolStripMenuItem.Text = "Show";
            this.showToolStripMenuItem.Click += new System.EventHandler(this.ShowToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Image = global::LanLord.Properties.Resources._172;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(136, 32);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // timerDiscovery
            // 
            this.timerDiscovery.Interval = 50;
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.lblStatus });
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.SizingGrip = false;
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Text = "  Ready — select a network adapter to begin";
            this.lblStatus.ForeColor = System.Drawing.Color.White;
            // 
            // ArpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1028, 600);
            this.Controls.Add(this.treeGridView1);
            this.Controls.Add(this.panelActionBar);
            this.Controls.Add(this.statusStrip1);
            this.DoubleBuffered = true;
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "ArpForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LanLord ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ArpForm_FormClosing);
            this.Load += new System.EventHandler(this.ArpForm_Load);
            this.Shown += new System.EventHandler(this.ArpForm_Shown);
            this.Resize += new System.EventHandler(this.ArpForm_Resize);
            this.panelActionBar.ResumeLayout(false);
            this.panelActionBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeGridView1)).EndInit();
            this.ContextMenuViews.ResumeLayout(false);
            this.LanLordTray.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelActionBar;
        private System.Windows.Forms.Button btnScanNetwork;
        private System.Windows.Forms.Button btnStartSpoof;
        private System.Windows.Forms.Button btnStopSpoof;
        private System.Windows.Forms.Button btnBlockAll;
        private System.Windows.Forms.Button btnUnblockAll;
        private System.Windows.Forms.Button btnExportCsv;
        private AdvancedDataGridView.TreeGridView treeGridView1;
        private System.Windows.Forms.ContextMenuStrip ContextMenuViews;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Timer timerSpoof;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.NotifyIcon LanLordTrayIcon;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuIP;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuMAC;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuDownload;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuUpload;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuDownloadCap;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuUploadCap;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuBlock;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuSpoof;
        private System.Windows.Forms.Timer timerDiscovery;
        private System.Windows.Forms.ContextMenuStrip LanLordTray;
        private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private TreeGridColumn ColPCName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColPCIP;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColPCMac;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColDownload;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColUpload;
        private DataGridViewNumericUpDownElements.DataGridViewNumericUpDownColumn ColDownCap;
        private DataGridViewNumericUpDownElements.DataGridViewNumericUpDownColumn ColUploadCap;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColBlock;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColSpoof;
        private System.Windows.Forms.StatusStrip statusStrip1;
        internal System.Windows.Forms.ToolStripStatusLabel lblStatus;
    }
}

