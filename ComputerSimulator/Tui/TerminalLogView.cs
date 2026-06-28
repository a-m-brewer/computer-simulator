using ComputerSimulator.Graphics;
using Terminal.Gui.ViewBase;

namespace ComputerSimulator.Tui;

public class TerminalLogView : View
{
    private readonly ITerminalLogSink _logSink;

    public TerminalLogView(ITerminalLogSink logSink)
    {
        _logSink = logSink;
        CanFocus = false;
    }

    public void Refresh()
    {
        SetNeedsDraw();
    }

    protected override bool OnDrawingContent(DrawContext? context)
    {
        base.OnDrawingContent(context);

        var width = Viewport.Width;
        var height = Viewport.Height;
        if (width <= 0 || height <= 0)
        {
            return true;
        }

        var lines = _logSink.GetLines(height)
            .SelectMany(line => Wrap(line, width))
            .TakeLast(height)
            .ToArray();

        for (var y = 0; y < lines.Length; y++)
        {
            Move(0, y);
            AddStr(Truncate(lines[y], width));
        }

        return true;
    }

    private static IEnumerable<string> Wrap(string value, int width)
    {
        if (width <= 0)
        {
            yield break;
        }

        for (var i = 0; i < value.Length; i += width)
        {
            yield return value.Substring(i, Math.Min(width, value.Length - i));
        }
    }

    private static string Truncate(string value, int width)
    {
        return value.Length <= width ? value : value[..width];
    }
}
