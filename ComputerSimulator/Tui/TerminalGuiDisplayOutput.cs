using ComputerSimulator.Core.Peripherals.Display;

namespace ComputerSimulator.Tui;

public class TerminalGuiDisplayOutput : IDisplayByteOutput
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

    public void SetPixelByte(int x, int y, int value)
    {
        _displayBuffer.SetPixelByte(x, y, value);
    }

    public void Present()
    {
        _terminalGui.RefreshDisplay();
    }
}
