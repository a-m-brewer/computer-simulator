using System.Collections.Concurrent;

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
    private readonly ConcurrentQueue<string> _lines = new();

    public event Action? Changed;

    public void Add(string message)
    {
        _lines.Enqueue(message);
        while (_lines.Count > Capacity)
        {
            _lines.TryDequeue(out _);
        }

        Changed?.Invoke();
    }

    public IReadOnlyList<string> GetLines(int count)
    {
        return count <= 0 
            ? []
            : _lines.TakeLast(count).ToArray();
    }
}
