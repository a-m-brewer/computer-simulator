namespace ComputerSimulator.Tui;

public class TerminalDisplayBuffer
{
    private readonly object _sync = new();
    private int _width;
    private int _height;
    private bool[] _pixels = Array.Empty<bool>();

    public void Initialize(int width, int height)
    {
        lock (_sync)
        {
            _width = width;
            _height = height;
            _pixels = new bool[width * height];
        }
    }

    public void SetPixel(int x, int y, bool on)
    {
        lock (_sync)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
            {
                return;
            }

            _pixels[(y * _width) + x] = on;
        }
    }

    public TerminalDisplaySnapshot GetSnapshot()
    {
        lock (_sync)
        {
            return new TerminalDisplaySnapshot(_width, _height, _pixels.ToArray());
        }
    }
}

public readonly record struct TerminalDisplaySnapshot(int Width, int Height, bool[] Pixels);
