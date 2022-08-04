using Avoid.MessageBroker;

namespace ComputerSimulator.Core.Parts;

public class Connector<TSource> : IMessageHandler<bool>
{
    private Func<TSource, Action<bool>>? _selector;
    private TSource? _source;

    public void Handle(bool message)
    {
        if (_source == null)
        {
            return;
        }
        
        _selector?.Invoke(_source).Invoke(message);
    }

    public void Connect(TSource source, Func<TSource, Action<bool>> selector)
    {
        _source = source;
        _selector = selector;
    }
}