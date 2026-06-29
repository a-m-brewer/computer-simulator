namespace ComputerSimulator.Tui;

public class TerminalDisplayBuffer
{
    private readonly Lock _sync = new();
    private int _width;
    private int _height;
    private long _version;
    private bool[] _pixels = [];

    public long Version
    {
        get
        {
            lock (_sync)
            {
                return _version;
            }
        }
    }

    public void Initialize(int width, int height)
    {
        lock (_sync)
        {
            _width = width;
            _height = height;
            _pixels = new bool[width * height];
            _version++;
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

            var index = (y * _width) + x;
            if (_pixels[index] == on)
            {
                return;
            }

            _pixels[index] = on;
            _version++;
        }
    }

    public void SetPixelByte(int x, int y, int value)
    {
        lock (_sync)
        {
            if (y < 0 || y >= _height || x >= _width || x + 7 < 0)
            {
                return;
            }

            for (var bit = 0; bit < 8; bit++)
            {
                var pixelX = x + bit;
                if (pixelX < 0 || pixelX >= _width)
                {
                    continue;
                }

                var index = (y * _width) + pixelX;
                var on = (value & (1 << bit)) != 0;
                if (_pixels[index] == on)
                {
                    continue;
                }

                _pixels[index] = on;
                _version++;
            }
        }
    }

    public TerminalDisplaySnapshot GetSnapshot()
    {
        lock (_sync)
        {
            return new TerminalDisplaySnapshot(_width, _height, _version, _pixels.ToArray());
        }
    }
}

public readonly record struct TerminalDisplaySnapshot(int Width, int Height, long Version, bool[] Pixels);
