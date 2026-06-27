namespace ComputerSimulator.Core.Models;

public class ComputerSettings
{
    public int WordSize { get; set; } = 16;

    /// <summary>
    /// Width of the virtual display in pixels. Must be a multiple of 8 (8 pixels per display-RAM byte).
    /// </summary>
    public int ScreenWidth { get; set; } = 96;

    /// <summary>
    /// Height of the virtual display in pixels.
    /// </summary>
    public int ScreenHeight { get; set; } = 48;
}