using System.Drawing;

namespace LanLord
{
#pragma warning disable
    /// <summary>
    /// Centralised colour / metric constants used across every form.
    /// </summary>
    internal static class UITheme
    {
        // ── Background surfaces ──────────────────────────────────────
        /// <summary>Deepest background — form body, grid fill.</summary>
        public static readonly Color Surface0       = Color.FromArgb(13,  17,  23);
        /// <summary>Slightly lighter — action-bar, panels, header.</summary>
        public static readonly Color Surface1       = Color.FromArgb(22,  27,  34);
        /// <summary>Grid row colour.</summary>
        public static readonly Color Surface2       = Color.FromArgb(22,  27,  34);
        /// <summary>Alternating grid row.</summary>
        public static readonly Color Surface2Alt    = Color.FromArgb(28,  35,  46);
        /// <summary>Row hover highlight.</summary>
        public static readonly Color Surface3       = Color.FromArgb(38,  48,  66);

        // ── Accent ───────────────────────────────────────────────────
        public static readonly Color Accent         = Color.FromArgb(29,  78, 216);
        public static readonly Color AccentHover    = Color.FromArgb(37,  99, 235);
        public static readonly Color AccentPressed  = Color.FromArgb(22,  63, 181);

        // ── Semantic button tints ─────────────────────────────────────
        public static readonly Color Success        = Color.FromArgb(21, 128,  61);
        public static readonly Color SuccessHover   = Color.FromArgb(22, 163,  74);
        public static readonly Color SuccessPressed = Color.FromArgb(16, 100,  47);
        public static readonly Color Danger         = Color.FromArgb(153, 27,  27);
        public static readonly Color DangerHover    = Color.FromArgb(185, 28,  28);
        public static readonly Color DangerPressed  = Color.FromArgb(127, 22,  22);
        public static readonly Color Neutral        = Color.FromArgb(50,  50,  62);
        public static readonly Color NeutralHover   = Color.FromArgb(68,  68,  85);
        public static readonly Color NeutralPressed = Color.FromArgb(35,  35,  47);

        // ── Text ─────────────────────────────────────────────────────
        public static readonly Color TextPrimary    = Color.FromArgb(230, 237, 243);
        public static readonly Color TextSecondary  = Color.FromArgb(139, 148, 158);
        public static readonly Color TextMuted      = Color.FromArgb(88,  96, 105);
        public static readonly Color TextDisabled   = Color.FromArgb(70,  78,  88);

        // ── Grid ─────────────────────────────────────────────────────
        public static readonly Color GridBackground = Color.FromArgb(13,  17,  23);
        public static readonly Color GridHeader     = Color.FromArgb(18,  22,  30);
        public static readonly Color GridBorder     = Color.FromArgb(38,  44,  54);
        public static readonly Color GridSelection  = Color.FromArgb(29,  78, 216);

        // ── Borders / separators ──────────────────────────────────────
        public static readonly Color Border         = Color.FromArgb(40,  47,  58);
        public static readonly Color AccentSeparator = Color.FromArgb(29,  78, 216);

        // ── Sizing ───────────────────────────────────────────────────
        public const int ButtonRadius    = 8;   // px corner radius on action buttons
        public const int NavButtonRadius = 0;   // sidebar nav buttons stay square
    }
#pragma warning restore
}
