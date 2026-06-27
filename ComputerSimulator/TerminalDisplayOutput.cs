using ComputerSimulator.Core.Peripherals.Display;
using ComputerSimulator.Graphics;

namespace ComputerSimulator;

/// <summary>
/// Bridges the Core display adapter to the terminal renderer in ComputerSimulator.Graphics.
/// </summary>
public class TerminalDisplayOutput : IDisplayOutput
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

    public void Present()
    {
        _screen.Flush();
    }
}
