namespace Avoid.MessageBroker;

public interface IMessageListener
{
}

public interface IMessageListener<in TMessageType> : IMessageListener
{
    void OnMessagePublished(object? sender, TMessageType message);
}

internal class MessageListener<TMessageType> : IMessageListener<TMessageType>
{
    private readonly IMessageHandler<TMessageType> _handler;

    public MessageListener(IMessageHandler<TMessageType> handler)
    {
        _handler = handler;
    }

    public void OnMessagePublished(object? sender, TMessageType message)
    {
        _handler.Handle(message);
    }
}