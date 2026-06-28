using ComputerSimulator.Graphics;
using Terminal.Gui.ViewBase;

namespace ComputerSimulator.Tui;

public class TerminalDisplayView : View
{
    private readonly TerminalDisplayBuffer _displayBuffer;
    private readonly TerminalSettings _settings;

    public TerminalDisplayView(TerminalDisplayBuffer displayBuffer, TerminalSettings settings)
    {
        _displayBuffer = displayBuffer;
        _settings = settings;
        CanFocus = false;
    }

    public void Refresh()
    {
        SetNeedsDraw();
    }

    protected override bool OnDrawingContent(DrawContext? context)
    {
        base.OnDrawingContent(context);

        var lines = TerminalFrameRenderer.Render(
            _displayBuffer.GetSnapshot(),
            _settings.PixelMode,
            Viewport.Width,
            Viewport.Height);

        for (var y = 0; y < lines.Count; y++)
        {
            Move(0, y);
            AddStr(lines[y]);
        }

        return true;
    }
}
