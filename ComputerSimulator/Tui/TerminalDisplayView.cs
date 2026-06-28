using ComputerSimulator.Graphics;
using Terminal.Gui.ViewBase;

namespace ComputerSimulator.Tui;

public class TerminalDisplayView : View
{
    private readonly TerminalDisplayBuffer _displayBuffer;
    private readonly TerminalSettings _settings;
    private IReadOnlyList<string> _lines = Array.Empty<string>();
    private long _renderedVersion = -1;
    private int _renderedWidth = -1;
    private int _renderedHeight = -1;
    private TerminalPixelMode _renderedPixelMode;

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

        var width = Viewport.Width;
        var height = Viewport.Height;
        var version = _displayBuffer.Version;
        if (version != _renderedVersion || width != _renderedWidth || height != _renderedHeight || _settings.PixelMode != _renderedPixelMode)
        {
            _lines = TerminalFrameRenderer.Render(
                _displayBuffer.GetSnapshot(),
                _settings.PixelMode,
                width,
                height);
            _renderedVersion = version;
            _renderedWidth = width;
            _renderedHeight = height;
            _renderedPixelMode = _settings.PixelMode;
        }

        for (var y = 0; y < _lines.Count; y++)
        {
            Move(0, y);
            AddStr(_lines[y]);
        }

        return true;
    }
}
