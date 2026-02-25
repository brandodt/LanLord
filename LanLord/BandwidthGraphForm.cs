using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LanLord
{
#pragma warning disable
    public class BandwidthGraphForm : Form
    {
        private readonly PC pc;
        private readonly Queue<float> downSamples = new Queue<float>();
        private readonly Queue<float> upSamples = new Queue<float>();
        private const int MaxSamples = 60;
        private int lastDown;
        private int lastUp;

        private Panel graphPanel;
        private Label lblTitle;
        private Label lblDown;
        private Label lblUp;
        private System.Windows.Forms.Timer sampleTimer;

        public BandwidthGraphForm(PC pc)
        {
            this.pc = pc;
            this.lastDown = pc.nbPacketReceivedSinceLastReset;
            this.lastUp = pc.nbPacketSentSinceLastReset;
            initComponents();
        }

        private void initComponents()
        {
            this.Text = "Bandwidth \u2014 " + pc.ip;
            this.Size = new Size(620, 380);
            this.MinimumSize = new Size(420, 280);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.FromArgb(220, 220, 220);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterParent;

            lblTitle = new Label
            {
                Text = "Bandwidth \u2014 " + pc.ip + (pc.name != "" ? "  (" + pc.name + ")" : ""),
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 34,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                BackColor = Color.FromArgb(20, 20, 20)
            };

            lblDown = new Label
            {
                Text = "\u2193 Download: 0.0 KB/s",
                ForeColor = Color.FromArgb(56, 189, 248),
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Bottom,
                Height = 24,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                BackColor = Color.FromArgb(20, 20, 20)
            };

            lblUp = new Label
            {
                Text = "\u2191 Upload: 0.0 KB/s",
                ForeColor = Color.FromArgb(251, 146, 60),
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Bottom,
                Height = 24,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                BackColor = Color.FromArgb(20, 20, 20)
            };

            graphPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 20, 20)
            };
            graphPanel.Paint += graphPanel_Paint;

            this.Controls.Add(graphPanel);
            this.Controls.Add(lblDown);
            this.Controls.Add(lblUp);
            this.Controls.Add(lblTitle);

            sampleTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            sampleTimer.Tick += sampleTimer_Tick;
            sampleTimer.Start();
        }

        private void sampleTimer_Tick(object sender, EventArgs e)
        {
            int currentDown = pc.nbPacketReceivedSinceLastReset;
            int currentUp = pc.nbPacketSentSinceLastReset;
            int deltaDown = Math.Max(0, currentDown - lastDown);
            int deltaUp = Math.Max(0, currentUp - lastUp);
            lastDown = currentDown;
            lastUp = currentUp;

            float downKBps = deltaDown / 1024f;
            float upKBps = deltaUp / 1024f;

            if (downSamples.Count >= MaxSamples) downSamples.Dequeue();
            if (upSamples.Count >= MaxSamples) upSamples.Dequeue();
            downSamples.Enqueue(downKBps);
            upSamples.Enqueue(upKBps);

            lblDown.Text = string.Format("\u2193 Download: {0:F1} KB/s", downKBps);
            lblUp.Text = string.Format("\u2191 Upload: {0:F1} KB/s", upKBps);
            graphPanel.Invalidate();
        }

        private void graphPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.FromArgb(20, 20, 20));

            int w = graphPanel.Width;
            int h = graphPanel.Height;
            const int padLeft = 52;
            const int padBottom = 22;
            const int padTop = 10;
            const int padRight = 10;
            int chartW = w - padLeft - padRight;
            int chartH = h - padBottom - padTop;

            if (chartW <= 0 || chartH <= 0) return;

            // horizontal grid lines
            using (Pen gridPen = new Pen(Color.FromArgb(50, 50, 50)))
            {
                for (int i = 0; i <= 4; i++)
                {
                    int y = padTop + chartH * i / 4;
                    g.DrawLine(gridPen, padLeft, y, padLeft + chartW, y);
                }
            }

            float[] downArr = downSamples.ToArray();
            float[] upArr = upSamples.ToArray();

            float maxVal = 1f;
            foreach (float v in downArr) if (v > maxVal) maxVal = v;
            foreach (float v in upArr) if (v > maxVal) maxVal = v;

            // y-axis labels
            using (Font axisFont = new Font("Segoe UI", 7))
            using (Brush axisBrush = new SolidBrush(Color.FromArgb(140, 140, 140)))
            {
                for (int i = 0; i <= 4; i++)
                {
                    float val = maxVal * (4 - i) / 4;
                    int y = padTop + chartH * i / 4;
                    g.DrawString(string.Format("{0:F1}", val), axisFont, axisBrush, 0, y - 7);
                }
                // x-axis labels
                g.DrawString(MaxSamples + "s", axisFont, axisBrush, padLeft, h - padBottom);
                g.DrawString("now", axisFont, axisBrush, w - padRight - 26, h - padBottom);
            }

            drawLine(g, downArr, maxVal, padLeft, padTop, chartW, chartH, Color.FromArgb(56, 189, 248));
            drawLine(g, upArr, maxVal, padLeft, padTop, chartW, chartH, Color.FromArgb(251, 146, 60));
        }

        private void drawLine(Graphics g, float[] data, float maxVal, int padLeft, int padTop, int chartW, int chartH, Color color)
        {
            if (data.Length < 2) return;
            using (Pen pen = new Pen(color, 1.5f))
            {
                for (int i = 1; i < data.Length; i++)
                {
                    float x1 = padLeft + (float)(i - 1) / (MaxSamples - 1) * chartW;
                    float y1 = padTop + chartH - data[i - 1] / maxVal * chartH;
                    float x2 = padLeft + (float)i / (MaxSamples - 1) * chartW;
                    float y2 = padTop + chartH - data[i] / maxVal * chartH;
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            sampleTimer.Stop();
            sampleTimer.Dispose();
            base.OnFormClosed(e);
        }
    }
#pragma warning restore
}
