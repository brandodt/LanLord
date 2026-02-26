using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LanLord
{
#pragma warning disable
    /// <summary>
    /// A <see cref="Button"/> that paints itself with rounded corners and a
    /// subtle top-edge highlight to add visual depth on dark backgrounds.
    /// Reads <see cref="FlatAppearance.MouseOverBackColor"/> and
    /// <see cref="FlatAppearance.MouseDownBackColor"/> for hover / press colours,
    /// exactly as the standard WinForms flat button does.
    /// </summary>
    public class RoundedButton : Button
    {
        // ── Public properties ────────────────────────────────────────

        private int _cornerRadius = UITheme.ButtonRadius;
        /// <summary>Corner radius in pixels (default 8).</summary>
        public int CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = value; Invalidate(); }
        }

        // ── State tracking ───────────────────────────────────────────
        private bool _hovered;
        private bool _pressed;

        // ── Constructor ──────────────────────────────────────────────
        public RoundedButton()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
        }

        // ── Background ───────────────────────────────────────────────
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Clear the full rectangle with the parent's color so the four
            // corners outside the rounded path don't bleed the button's BackColor.
            if (Parent != null)
                using (var b = new SolidBrush(Parent.BackColor))
                    e.Graphics.FillRectangle(b, ClientRectangle);
            else
                base.OnPaintBackground(e);
        }

        // ── Mouse state ──────────────────────────────────────────────
        protected override void OnMouseEnter(EventArgs e)
        {
            _hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            _hovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            _pressed = true;
            Invalidate();
            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        // ── Painting ─────────────────────────────────────────────────
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode    = SmoothingMode.AntiAlias;
            g.PixelOffsetMode  = PixelOffsetMode.HighQuality;
            g.CompositingMode  = CompositingMode.SourceOver;

            // 1 px inset so the anti-aliased edge isn't clipped
            var rect = new Rectangle(1, 1, Width - 2, Height - 2);
            int r = Math.Min(_cornerRadius, Math.Min(rect.Width, rect.Height) / 2);

            // ── Fill colour ──────────────────────────────────────────
            Color fill = !Enabled              ? UITheme.Neutral :
                         _pressed             ? ResolveColor(FlatAppearance.MouseDownBackColor, BackColor) :
                         _hovered             ? ResolveColor(FlatAppearance.MouseOverBackColor, BackColor) :
                         BackColor;

            using (var path = RoundedRect(rect, r))
            using (var brush = new SolidBrush(fill))
                g.FillPath(brush, path);

            // ── Subtle top-edge highlight (simulates depth) ──────────
            if (Enabled && !_pressed && r > 0)
            {
                var hi = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, r + 2);
                using (var path = RoundedRect(hi, r - 1))
                using (var lgb  = new LinearGradientBrush(
                                      new Point(hi.X, hi.Y),
                                      new Point(hi.X, hi.Bottom),
                                      Color.FromArgb(50, Color.White),
                                      Color.FromArgb(0,  Color.White)))
                    g.FillPath(lgb, path);
            }

            // ── Text ─────────────────────────────────────────────────
            Color fg = Enabled ? ForeColor : UITheme.TextDisabled;
            var tf = TextFormatFlags.HorizontalCenter |
                     TextFormatFlags.VerticalCenter   |
                     TextFormatFlags.SingleLine;
            TextRenderer.DrawText(g, Text, Font, ClientRectangle, fg, tf);
        }

        // ── Helpers ──────────────────────────────────────────────────

        /// Returns <paramref name="candidate"/> unless it is empty/transparent,
        /// in which case <paramref name="fallback"/> is returned.
        private static Color ResolveColor(Color candidate, Color fallback)
            => (candidate.IsEmpty || candidate == Color.Transparent) ? fallback : candidate;

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d    = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X,            r.Y,             d, d,   180, 90);
            path.AddArc(r.Right - d,    r.Y,             d, d,   270, 90);
            path.AddArc(r.Right - d,    r.Bottom - d,    d, d,     0, 90);
            path.AddArc(r.X,            r.Bottom - d,    d, d,    90, 90);
            path.CloseFigure();
            return path;
        }

        }
#pragma warning restore
}
