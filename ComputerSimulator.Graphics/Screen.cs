using System.Text;

namespace ComputerSimulator.Graphics;

/// <summary>
/// Terminal renderer. Pixels are written into an off-screen buffer and the whole frame is flushed
/// in a single write to avoid flicker.
/// </summary>
public class Screen
{
    private const char On = '█';
    private const char Off = ' ';
    private const char BrailleBlank = ' ';
    private static readonly int[] LeftBrailleDots = [0x01, 0x02, 0x04, 0x40];
    private static readonly int[] RightBrailleDots = [0x08, 0x10, 0x20, 0x80];

    private readonly IConsole _console;
    private readonly TerminalSettings _settings;
    private readonly ITerminalLogSink _logSink;

    private int _width;
    private int _height;
    private bool[] _pixels = Array.Empty<bool>();

    public Screen(IConsole console)
        : this(console, new TerminalSettings(), new TerminalLogSink())
    {
    }

    public Screen(IConsole console, TerminalSettings settings)
        : this(console, settings, new TerminalLogSink())
    {
    }

    public Screen(IConsole console, TerminalSettings settings, ITerminalLogSink logSink)
    {
        _console = console;
        _settings = settings;
        _logSink = logSink;
    }

    public void Initialize(int width, int height)
    {
        _width = width;
        _height = height;
        _pixels = new bool[width * height];

        _console.Clear();
        _console.SetCursorPosition(0, 0);
    }

    public void SetPixel(int x, int y, bool on)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
        {
            return;
        }

        _pixels[(y * _width) + x] = on;
    }

    public void SetPixelByte(int x, int y, int value)
    {
        if (y < 0 || y >= _height || x >= _width || x + 7 < 0)
        {
            return;
        }

        var pixelIndex = (y * _width) + x;
        for (var bit = 0; bit < 8; bit++)
        {
            var pixelX = x + bit;
            if (pixelX < 0 || pixelX >= _width)
            {
                continue;
            }

            _pixels[pixelIndex + bit] = (value & (1 << bit)) != 0;
        }
    }

    public void Flush()
    {
        if (_settings.PixelMode == TerminalPixelMode.Block)
        {
            FlushBlock();
            return;
        }

        FlushBraille();
    }

    private void FlushBlock()
    {
        var visibleWidth = GetVisibleSize(_console.WindowWidth, _width);
        var visibleHeight = GetVisibleDisplayHeight(_height);

        var builder = new StringBuilder((visibleWidth + 1) * (visibleHeight + _settings.LogLines));
        for (var y = 0; y < visibleHeight; y++)
        {
            for (var x = 0; x < visibleWidth; x++)
            {
                builder.Append(_pixels[(y * _width) + x] ? On : Off);
            }

            builder.Append('\n');
        }

        AppendLogs(builder, GetVisibleLogWidth(visibleWidth), visibleHeight);

        _console.SetCursorPosition(0, 0);
        _console.Write(builder.ToString());
    }

    private void FlushBraille()
    {
        var renderWidth = (_width + 1) / 2;
        var renderHeight = (_height + 3) / 4;
        var visibleWidth = GetVisibleSize(_console.WindowWidth, renderWidth);
        var visibleHeight = GetVisibleDisplayHeight(renderHeight);

        var builder = new StringBuilder((visibleWidth + 1) * (visibleHeight + _settings.LogLines));
        for (var cellY = 0; cellY < visibleHeight; cellY++)
        {
            for (var cellX = 0; cellX < visibleWidth; cellX++)
            {
                builder.Append(GetBrailleCell(cellX, cellY));
            }

            builder.Append('\n');
        }

        AppendLogs(builder, GetVisibleLogWidth(visibleWidth), visibleHeight);

        _console.SetCursorPosition(0, 0);
        _console.Write(builder.ToString());
    }

    private char GetBrailleCell(int cellX, int cellY)
    {
        var dots = 0;
        var pixelLeft = cellX * 2;
        var pixelTop = cellY * 4;

        for (var y = 0; y < 4; y++)
        {
            var pixelY = pixelTop + y;
            if (pixelY >= _height)
            {
                break;
            }

            var rowIndex = pixelY * _width;
            if (pixelLeft < _width && _pixels[rowIndex + pixelLeft])
            {
                dots |= LeftBrailleDots[y];
            }

            var right = pixelLeft + 1;
            if (right < _width && _pixels[rowIndex + right])
            {
                dots |= RightBrailleDots[y];
            }
        }

        return dots == 0 ? BrailleBlank : (char)(0x2800 + dots);
    }

    private static int GetVisibleSize(int consoleSize, int renderSize)
    {
        return consoleSize <= 0 ? renderSize : Math.Min(consoleSize, renderSize);
    }

    private int GetVisibleDisplayHeight(int renderHeight)
    {
        if (_console.WindowHeight <= 0 || _settings.LogLines <= 0)
        {
            return GetVisibleSize(_console.WindowHeight, renderHeight);
        }

        var logRows = Math.Min(_settings.LogLines, Math.Max(0, _console.WindowHeight - 1));
        return Math.Min(renderHeight, Math.Max(0, _console.WindowHeight - logRows));
    }

    private void AppendLogs(StringBuilder builder, int width, int displayRows)
    {
        var logRows = GetVisibleLogRows(displayRows);
        if (logRows <= 0)
        {
            if (builder.Length > 0)
            {
                builder.Length--;
            }

            return;
        }

        var lines = _logSink.GetLines(logRows)
            .SelectMany(line => Wrap(line, width))
            .TakeLast(logRows);

        foreach (var line in lines)
        {
            builder.Append(Truncate(line, width).PadRight(width));
            builder.Append('\n');
        }

        if (builder.Length > 0)
        {
            builder.Length--;
        }
    }

    private int GetVisibleLogRows(int displayRows)
    {
        if (_settings.LogLines <= 0)
        {
            return 0;
        }

        if (_console.WindowHeight <= 0)
        {
            return _settings.LogLines;
        }

        return Math.Min(_settings.LogLines, Math.Max(0, _console.WindowHeight - displayRows));
    }

    private int GetVisibleLogWidth(int displayWidth)
    {
        return _console.WindowWidth <= 0 ? displayWidth : _console.WindowWidth;
    }

    private static string Truncate(string value, int width)
    {
        if (width <= 0)
        {
            return string.Empty;
        }

        return value.Length <= width ? value : value[..width];
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
}
