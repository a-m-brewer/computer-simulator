namespace ComputerSimulator.Graphics;

public interface IConsole
{
    int WindowHeight { get; }
    
    int WindowWidth { get; }

    public void SetCursorPosition(int left, int top);

    void Clear();
    
    void Write(string value);
}

public class ConsoleAbstraction : IConsole
{
    public int WindowHeight => Console.WindowHeight;

    public int WindowWidth => Console.WindowWidth;

    public void SetCursorPosition(int left, int top)
    {
        Console.SetCursorPosition(left, top);
    }
    
    public void Clear()
    {
        Console.Clear();
    }

    public void Write(string value)
    {
        Console.Write(value);
    }
}