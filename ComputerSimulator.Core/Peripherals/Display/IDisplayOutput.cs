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

/// <summary>
/// Optional fast path for render targets that can accept one display-RAM byte at a time.
/// Bit 0 maps to the leftmost pixel, matching the display adapter protocol.
/// </summary>
public interface IDisplayByteOutput : IDisplayOutput
{
    void SetPixelByte(int x, int y, int value);
}
