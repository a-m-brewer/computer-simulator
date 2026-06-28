namespace ComputerSimulator.Graphics;

public interface ITerminalLogSink
{
    event Action? Changed;

    void Add(string message);

    IReadOnlyList<string> GetLines(int count);
}

public class TerminalLogSink : ITerminalLogSink
{
    private const int Capacity = 200;
    private readonly object _sync = new();
    private readonly Queue<string> _lines = new();

    public event Action? Changed;

    public void Add(string message)
    {
        lock (_sync)
        {
            _lines.Enqueue(message);
            while (_lines.Count > Capacity)
            {
                _lines.Dequeue();
            }
        }

        Changed?.Invoke();
    }

    public IReadOnlyList<string> GetLines(int count)
    {
        if (count <= 0)
        {
            return Array.Empty<string>();
        }

        lock (_sync)
        {
            return _lines.TakeLast(count).ToArray();
        }
    }
}
