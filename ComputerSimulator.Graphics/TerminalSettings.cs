namespace ComputerSimulator.Graphics;

public class TerminalSettings
{
    public TerminalPixelMode PixelMode { get; set; } = TerminalPixelMode.Braille;

    public int LogLines { get; set; } = 4;
}

public enum TerminalPixelMode
{
    Block,
    Braille
}
