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

    private readonly IConsole _console;

    private int _width;
    private int _height;
    private char[] _buffer = Array.Empty<char>();

    public Screen(IConsole console)
    {
        _console = console;
    }

    public void Initialize(int width, int height)
    {
        _width = width;
        _height = height;
        _buffer = new char[width * height];
        Array.Fill(_buffer, Off);

        _console.Clear();
        _console.SetCursorPosition(0, 0);
    }

    public void SetPixel(int x, int y, bool on)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
        {
            return;
        }

        _buffer[(y * _width) + x] = on ? On : Off;
    }

    public void Flush()
    {
        var builder = new StringBuilder((_width + 1) * _height);
        for (var y = 0; y < _height; y++)
        {
            builder.Append(_buffer, y * _width, _width);
            if (y < _height - 1)
            {
                builder.Append('\n');
            }
        }

        _console.SetCursorPosition(0, 0);
        _console.Write(builder.ToString());
    }
}
