using ComputerSimulator.Core.Peripherals.Display;

namespace ComputerSimulator.Core.Models;

public class ComputerSettings
{
    public int WordSize { get; set; } = 16;

    public int CpuUpdatesPerFrame { get; set; } = 400;

    public int DisplayFrameDelayMs { get; set; } = 16;

    public DisplayScanMode DisplayScanMode { get; set; } = DisplayScanMode.GateLevel;

    /// <summary>
    /// Width of the virtual display in pixels. Must be a multiple of 8 (8 pixels per display-RAM byte).
    /// </summary>
    public int ScreenWidth { get; set; } = 96;

    /// <summary>
    /// Height of the virtual display in pixels.
    /// </summary>
    public int ScreenHeight { get; set; } = 48;

    public void Validate()
    {
        if (WordSize <= 0 || WordSize % 2 != 0)
        {
            throw new ArgumentException("WordSize must be a positive even number.", nameof(WordSize));
        }

        if (ScreenWidth <= 0)
        {
            throw new ArgumentException("ScreenWidth must be positive.", nameof(ScreenWidth));
        }

        if (ScreenHeight <= 0)
        {
            throw new ArgumentException("ScreenHeight must be positive.", nameof(ScreenHeight));
        }

        if (ScreenWidth % 8 != 0)
        {
            throw new ArgumentException("ScreenWidth must be a multiple of 8.", nameof(ScreenWidth));
        }

        if (CpuUpdatesPerFrame <= 0)
        {
            throw new ArgumentException("CpuUpdatesPerFrame must be positive.", nameof(CpuUpdatesPerFrame));
        }

        if (DisplayFrameDelayMs < 0)
        {
            throw new ArgumentException("DisplayFrameDelayMs cannot be negative.", nameof(DisplayFrameDelayMs));
        }

        var addressBitsPerAxis = WordSize / 2;
        var addressableBytes = 1L << WordSize;
        var addressableRows = 1L << addressBitsPerAxis;
        var requiredBytes = (ScreenWidth / 8) * ScreenHeight;

        if (ScreenHeight > addressableRows || requiredBytes > addressableBytes)
        {
            throw new ArgumentException("Screen dimensions exceed display RAM address capacity.");
        }
    }

    public override string ToString()
    {
        return $"WordSize: {WordSize}, CpuUpdatesPerFrame: {CpuUpdatesPerFrame}, DisplayFrameDelayMs: {DisplayFrameDelayMs}, DisplayScanMode: {DisplayScanMode}, ScreenWidth: {ScreenWidth}, ScreenHeight: {ScreenHeight}";
    }
}
