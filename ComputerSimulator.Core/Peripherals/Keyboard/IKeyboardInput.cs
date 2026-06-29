using System.Collections.Concurrent;

namespace ComputerSimulator.Core.Peripherals.Keyboard;

public interface IKeyboardInput
{
    void Push(byte keycode);

    bool TryRead(out byte keycode);
}

public sealed class BufferedKeyboardInput : IKeyboardInput
{
    private readonly ConcurrentQueue<byte> _keycodes = [];

    public void Push(byte keycode)
    {
        _keycodes.Enqueue(keycode);
    }

    public bool TryRead(out byte keycode)
    {
        return _keycodes.TryDequeue(out keycode);
    }
}
