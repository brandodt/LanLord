namespace LanLord
{
    partial class about
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panelHeader  = new System.Windows.Forms.Panel();
            this.lblTitle     = new System.Windows.Forms.Label();
            this.lblSubtitle  = new System.Windows.Forms.Label();
            this.panelContent = new System.Windows.Forms.Panel();
            this.lblStack     = new System.Windows.Forms.Label();
            this.lblAuthor    = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.panelFooter  = new System.Windows.Forms.Panel();
            this.btnCerrar    = new System.Windows.Forms.Button();
            this.panelHeader.SuspendLayout();
            this.panelContent.SuspendLayout();
            this.panelFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelHeader  (top bar — dark secondary)
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(22, 27, 34);
            this.panelHeader.Controls.Add(this.lblSubtitle);
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Height = 100;
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.TabIndex = 0;
            // 
            // lblTitle  (added last → docks to top of header)
            // 
            this.lblTitle.AutoSize = false;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(56, 189, 248);
            this.lblTitle.Height = 70;
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(24, 10, 0, 0);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "LanLord v3";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSubtitle  (added first → docks below lblTitle)
            // 
            this.lblSubtitle.AutoSize = false;
            this.lblSubtitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 110, 135);
            this.lblSubtitle.Height = 28;
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Padding = new System.Windows.Forms.Padding(26, 0, 0, 0);
            this.lblSubtitle.TabIndex = 2;
            this.lblSubtitle.Text = "Network Control & ARP Spoofing Tool";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelContent  (fills remaining space)
            // 
            this.panelContent.BackColor = System.Drawing.Color.FromArgb(13, 17, 23);
            this.panelContent.Controls.Add(this.lblStack);
            this.panelContent.Controls.Add(this.lblAuthor);
            this.panelContent.Controls.Add(this.lblDescription);
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Name = "panelContent";
            this.panelContent.Padding = new System.Windows.Forms.Padding(24, 0, 24, 0);
            this.panelContent.TabIndex = 1;
            // 
            // lblDescription  (added last → top of content)
            // 
            this.lblDescription.AutoSize = false;
            this.lblDescription.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDescription.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblDescription.ForeColor = System.Drawing.Color.FromArgb(160, 175, 200);
            this.lblDescription.Height = 72;
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Padding = new System.Windows.Forms.Padding(0, 18, 0, 0);
            this.lblDescription.TabIndex = 3;
            this.lblDescription.Text = "LanLord is an ARP spoofing and bandwidth throttling tool for local networks.\r\n" +
                                       "Monitor, control, and limit the bandwidth of any device on your network.";
            // 
            // lblAuthor
            // 
            this.lblAuthor.AutoSize = false;
            this.lblAuthor.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblAuthor.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblAuthor.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            this.lblAuthor.Height = 38;
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Padding = new System.Windows.Forms.Padding(0, 14, 0, 0);
            this.lblAuthor.TabIndex = 4;
            this.lblAuthor.Text = "Author:  dwaynelifter";
            // 
            // lblStack  (added first → bottom of content)
            // 
            this.lblStack.AutoSize = false;
            this.lblStack.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStack.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStack.ForeColor = System.Drawing.Color.FromArgb(80, 95, 120);
            this.lblStack.Height = 30;
            this.lblStack.Name = "lblStack";
            this.lblStack.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.lblStack.TabIndex = 5;
            this.lblStack.Text = "WinForms  \u00B7  PcapNet  \u00B7  AdvancedDataGridView  \u00B7  .NET 8";
            // 
            // panelFooter  (bottom bar)
            // 
            this.panelFooter.BackColor = System.Drawing.Color.FromArgb(22, 27, 34);
            this.panelFooter.Controls.Add(this.btnCerrar);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelFooter.Height = 62;
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.TabIndex = 2;
            // 
            // btnCerrar
            // 
            this.btnCerrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerrar.BackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            this.btnCerrar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCerrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrar.FlatAppearance.BorderSize = 0;
            this.btnCerrar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnCerrar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(22, 63, 181);
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.ForeColor = System.Drawing.Color.White;
            this.btnCerrar.Location = new System.Drawing.Point(656, 13);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(120, 36);
            this.btnCerrar.TabIndex = 0;
            this.btnCerrar.Text = "Close";
            this.btnCerrar.UseVisualStyleBackColor = false;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            // 
            // about
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(13, 17, 23);
            this.ClientSize = new System.Drawing.Size(800, 480);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.panelFooter);
            this.Controls.Add(this.panelHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "about";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About LanLord";
            this.panelHeader.ResumeLayout(false);
            this.panelContent.ResumeLayout(false);
            this.panelFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel  panelHeader;
        private System.Windows.Forms.Label  lblTitle;
        private System.Windows.Forms.Label  lblSubtitle;
        private System.Windows.Forms.Panel  panelContent;
        private System.Windows.Forms.Label  lblDescription;
        private System.Windows.Forms.Label  lblAuthor;
        private System.Windows.Forms.Label  lblStack;
        private System.Windows.Forms.Panel  panelFooter;
        private System.Windows.Forms.Button btnCerrar;
    }
}