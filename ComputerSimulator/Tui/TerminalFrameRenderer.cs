using ComputerSimulator.Graphics;

namespace ComputerSimulator.Tui;

public static class TerminalFrameRenderer
{
    private const char On = '█';
    private const char Off = ' ';
    private const char BrailleBlank = ' ';
    private static readonly int[,] BrailleDots =
    {
        { 0x01, 0x08 },
        { 0x02, 0x10 },
        { 0x04, 0x20 },
        { 0x40, 0x80 }
    };

    public static IReadOnlyList<string> Render(TerminalDisplaySnapshot snapshot, TerminalPixelMode pixelMode, int maxWidth, int maxHeight)
    {
        if (snapshot.Width <= 0 || snapshot.Height <= 0 || maxWidth <= 0 || maxHeight <= 0)
        {
            return Array.Empty<string>();
        }

        return pixelMode == TerminalPixelMode.Block
            ? RenderBlock(snapshot, maxWidth, maxHeight)
            : RenderBraille(snapshot, maxWidth, maxHeight);
    }

    private static IReadOnlyList<string> RenderBlock(TerminalDisplaySnapshot snapshot, int maxWidth, int maxHeight)
    {
        var visibleWidth = Math.Min(maxWidth, snapshot.Width);
        var visibleHeight = Math.Min(maxHeight, snapshot.Height);
        var lines = new string[visibleHeight];

        for (var y = 0; y < visibleHeight; y++)
        {
            var chars = new char[visibleWidth];
            for (var x = 0; x < visibleWidth; x++)
            {
                chars[x] = snapshot.Pixels[(y * snapshot.Width) + x] ? On : Off;
            }

            lines[y] = new string(chars);
        }

        return lines;
    }

    private static IReadOnlyList<string> RenderBraille(TerminalDisplaySnapshot snapshot, int maxWidth, int maxHeight)
    {
        var renderWidth = (snapshot.Width + 1) / 2;
        var renderHeight = (snapshot.Height + 3) / 4;
        var visibleWidth = Math.Min(maxWidth, renderWidth);
        var visibleHeight = Math.Min(maxHeight, renderHeight);
        var lines = new string[visibleHeight];

        for (var cellY = 0; cellY < visibleHeight; cellY++)
        {
            var chars = new char[visibleWidth];
            for (var cellX = 0; cellX < visibleWidth; cellX++)
            {
                chars[cellX] = GetBrailleCell(snapshot, cellX, cellY);
            }

            lines[cellY] = new string(chars);
        }

        return lines;
    }

    private static char GetBrailleCell(TerminalDisplaySnapshot snapshot, int cellX, int cellY)
    {
        var dots = 0;
        var pixelLeft = cellX * 2;
        var pixelTop = cellY * 4;

        for (var y = 0; y < 4; y++)
        {
            for (var x = 0; x < 2; x++)
            {
                if (!IsPixelLit(snapshot, pixelLeft + x, pixelTop + y))
                {
                    continue;
                }

                dots |= BrailleDots[y, x];
            }
        }

        return dots == 0 ? BrailleBlank : (char)(0x2800 + dots);
    }

    private static bool IsPixelLit(TerminalDisplaySnapshot snapshot, int x, int y)
    {
        return x >= 0 && x < snapshot.Width && y >= 0 && y < snapshot.Height && snapshot.Pixels[(y * snapshot.Width) + x];
    }
}
