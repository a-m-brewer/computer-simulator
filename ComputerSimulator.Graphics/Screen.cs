namespace ComputerSimulator.Graphics;

public class Screen
{
    private const string On = "█";
    private const string Off = " ";
    
    private readonly IConsole _console;

    public Screen(IConsole console)
    {
        _console = console;
    }

    public void Initialize()
    {
        ClearScreen();
    }
    
    public void SetPixel(bool on, int x, int y)
    {
        _console.SetCursorPosition(x, y);
        _console.Write(on ? On : Off);
    }

    private void ClearScreen()
    {
        _console.Clear();
        _console.SetCursorPosition(0, 0);
        _console.Write(BuildEmptyScreen());
    }

    private string BuildEmptyScreen()
    {
        return Enumerable.Range(0, _console.WindowHeight * _console.WindowWidth)
            .Select(_ => On)
            .Aggregate((a, b) => a + b);
    }
}
