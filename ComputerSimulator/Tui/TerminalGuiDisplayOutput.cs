using ComputerSimulator.Core.Peripherals.Display;

namespace ComputerSimulator.Tui;

public class TerminalGuiDisplayOutput : IDisplayOutput
{
    private readonly TerminalDisplayBuffer _displayBuffer;
    private readonly ITerminalGuiApplication _terminalGui;

    public TerminalGuiDisplayOutput(TerminalDisplayBuffer displayBuffer, ITerminalGuiApplication terminalGui)
    {
        _displayBuffer = displayBuffer;
        _terminalGui = terminalGui;
    }

    public void Initialize(int width, int height)
    {
        _displayBuffer.Initialize(width, height);
        _terminalGui.RefreshDisplay();
    }

    public void SetPixel(int x, int y, bool on)
    {
        _displayBuffer.SetPixel(x, y, on);
    }

    public void Present()
    {
        _terminalGui.RefreshDisplay();
    }
}
