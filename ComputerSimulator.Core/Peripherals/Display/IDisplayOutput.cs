namespace ComputerSimulator.Core.Peripherals.Display;

/// <summary>
/// A render target for the display adapter. Implemented by the host (e.g. a terminal renderer).
/// The adapter sweeps the screen and pushes each pixel via <see cref="SetPixel"/>, then calls
/// <see cref="Present"/> once the frame is complete.
/// </summary>
public interface IDisplayOutput
{
    void Initialize(int width, int height);

    void SetPixel(int x, int y, bool on);

    void Present();
}
