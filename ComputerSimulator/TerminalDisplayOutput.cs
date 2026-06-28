using ComputerSimulator.Core.Peripherals.Display;
using ComputerSimulator.Graphics;

namespace ComputerSimulator;

/// <summary>
/// Bridges the Core display adapter to the terminal renderer in ComputerSimulator.Graphics.
/// </summary>
public class TerminalDisplayOutput : IDisplayByteOutput
{
    private readonly Screen _screen;

    public TerminalDisplayOutput(Screen screen)
    {
        _screen = screen;
    }

    public void Initialize(int width, int height)
    {
        _screen.Initialize(width, height);
    }

    public void SetPixel(int x, int y, bool on)
    {
        _screen.SetPixel(x, y, on);
    }

    public void SetPixelByte(int x, int y, int value)
    {
        _screen.SetPixelByte(x, y, value);
    }

    public void Present()
    {
        _screen.Flush();
    }
}
