using ImageMagick;
using ImageMagick.Drawing;

// Generates OSListUtilsServerSide icons:
//   - 64×64 PNG for ODC (embedded resource, wired via IconResourceName)
//   - 32×32 ICO for O11 (Integration Studio resource)
//
// Visual family: rounded pale tile + subject glyph + colored badge.
// Subject glyph: stacked horizontal bars (list) with one bar offset (pop).
// Badge (teal): curly braces {} representing JSON/generic structure support.

const int W = 64, H = 64;

var pngPath = Path.GetFullPath(Path.Combine(
    AppContext.BaseDirectory, "..", "..", "..", "..", "..",
    "ListUtils", "resources", "icon.png"));

var icoPath = Path.GetFullPath(Path.Combine(
    AppContext.BaseDirectory, "..", "..", "..", "..", "..",
    "ListUtils.O11", "resources", "icon.ico"));

Directory.CreateDirectory(Path.GetDirectoryName(pngPath)!);
Directory.CreateDirectory(Path.GetDirectoryName(icoPath)!);

// ── Palette ───────────────────────────────────────────────────────────────────
var bgTile    = new MagickColor("#F5F7FB"); // near-white rounded tile
var barColor  = new MagickColor("#1E40AF"); // list bars (deep blue)
var popBar    = new MagickColor("#F59E0B"); // the "popped" bar (amber)
var badgeTeal = new MagickColor("#0D9488"); // badge background (teal)
var badgeGlow = new MagickColor("#CCFBF1"); // pale halo around badge

using var img = new MagickImage(MagickColors.Transparent, W, H);

// ── 1. Rounded background tile ────────────────────────────────────────────────
img.Draw(new Drawables()
    .FillColor(bgTile)
    .StrokeColor(MagickColors.Transparent)
    .RoundRectangle(1, 1, 63, 63, 10, 10));

// ── 2. List bars (4 bars stacked vertically) ──────────────────────────────────
const int barX = 14, barW = 30, barH = 5, gap = 3;
int[] barYs = [14, 14 + barH + gap, 14 + 2 * (barH + gap), 14 + 3 * (barH + gap)];

for (int i = 0; i < 4; i++)
{
    var color = (i == 1) ? popBar : barColor; // second bar is "popped"
    var xOffset = (i == 1) ? 6 : 0;          // offset to show it being removed
    img.Draw(new Drawables()
        .FillColor(color)
        .StrokeColor(MagickColors.Transparent)
        .RoundRectangle(barX + xOffset, barYs[i], barX + barW + xOffset, barYs[i] + barH, 2, 2));
}

// Small arrow showing the pop direction (pointing right from the popped bar)
img.Draw(new Drawables()
    .FillColor(popBar)
    .StrokeColor(MagickColors.Transparent)
    .Polygon(
        new PointD(48, barYs[1] + barH / 2.0 - 3),
        new PointD(53, barYs[1] + barH / 2.0 + 0.5),
        new PointD(48, barYs[1] + barH / 2.0 + 4)));

// ── 3. Badge (bottom-right) — teal circle with {} braces ─────────────────────
const double bx = 50, by = 50, br = 11;

// Glow halo
img.Draw(new Drawables()
    .FillColor(badgeGlow)
    .StrokeColor(MagickColors.Transparent)
    .Circle(bx, by, bx + br + 2, by));

// Badge circle
img.Draw(new Drawables()
    .FillColor(badgeTeal)
    .StrokeColor(MagickColors.Transparent)
    .Circle(bx, by, bx + br, by));

// {} braces in white
img.Draw(new Drawables()
    .Font("Arial")
    .FontPointSize(13)
    .FillColor(MagickColors.White)
    .StrokeColor(MagickColors.Transparent)
    .TextAlignment(TextAlignment.Center)
    .Text(bx, by + 5, "{ }"));

// ── 4. Write PNG (64×64) ──────────────────────────────────────────────────────
img.Write(pngPath);
Console.WriteLine($"PNG: {pngPath}");

// ── 5. Write ICO (32×32) ──────────────────────────────────────────────────────
using var ico = img.Clone();
ico.Resize(32, 32);
ico.Write(icoPath);
Console.WriteLine($"ICO: {icoPath}");

Console.WriteLine("Done.");
