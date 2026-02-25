using System.Drawing;
using System.Windows.Forms;

namespace LanLord
{
#pragma warning disable
    /// <summary>
    /// A <see cref="ToolStripProfessionalRenderer"/> that paints
    /// ContextMenuStrip / ToolStrip items in the application dark theme.
    /// Usage:  myMenu.Renderer = new DarkMenuRenderer();
    /// </summary>
    internal sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkMenuColorTable()) { }

        // Force item text colour instead of relying on ForeColor inheritance
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled ? UITheme.TextPrimary : UITheme.TextMuted;
            base.OnRenderItemText(e);
        }

        // Remove the raised/sunken border around checked items
        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            var r = e.ImageRectangle;
            using (var brush = new SolidBrush(UITheme.Accent))
                e.Graphics.FillRectangle(brush, r);
            base.OnRenderItemCheck(e);
        }
    }

    internal sealed class DarkMenuColorTable : ProfessionalColorTable
    {
        // Drop-down background
        public override Color ToolStripDropDownBackground     => UITheme.Surface1;
        public override Color ImageMarginGradientBegin        => UITheme.Surface1;
        public override Color ImageMarginGradientMiddle       => UITheme.Surface1;
        public override Color ImageMarginGradientEnd          => UITheme.Surface1;

        // Hover / selection
        public override Color MenuItemSelected                => UITheme.Surface3;
        public override Color MenuItemSelectedGradientBegin   => UITheme.Surface3;
        public override Color MenuItemSelectedGradientEnd     => UITheme.Surface3;

        // Pressed
        public override Color MenuItemPressedGradientBegin    => UITheme.Accent;
        public override Color MenuItemPressedGradientEnd      => UITheme.AccentPressed;
        public override Color MenuItemPressedGradientMiddle   => UITheme.Accent;

        // Borders
        public override Color MenuBorder                      => UITheme.Border;
        public override Color MenuItemBorder                  => UITheme.Border;

        // Separators
        public override Color SeparatorDark                   => UITheme.Border;
        public override Color SeparatorLight                  => UITheme.Surface1;

        // Top-level toolbar (not used here but override for completeness)
        public override Color ToolStripBorder                 => UITheme.Border;
        public override Color ToolStripGradientBegin          => UITheme.Surface1;
        public override Color ToolStripGradientMiddle         => UITheme.Surface1;
        public override Color ToolStripGradientEnd            => UITheme.Surface1;
    }
#pragma warning restore
}
