using ComputerSimulator.Core.Peripherals.Display;

namespace ComputerSimulator.IntegrationTests.Peripherals.Display;

/// <summary>
/// Captures the pixels pushed by <see cref="DisplayAdapter.RenderFrame"/> for assertion.
/// </summary>
public class FakeDisplayOutput : IDisplayOutput
{
    private bool[,] _pixels = new bool[0, 0];

    public int Presents { get; private set; }

    public void Initialize(int width, int height)
    {
        _pixels = new bool[width, height];
    }

    public void SetPixel(int x, int y, bool on)
    {
        if (_pixels.GetLength(0) == 0)
        {
            return;
        }

        _pixels[x, y] = on;
    }

    public void Present() => Presents++;

    public bool IsLit(int x, int y) => _pixels[x, y];

    public int LitPixelCount
    {
        get
        {
            var count = 0;
            foreach (var pixel in _pixels)
            {
                if (pixel)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
