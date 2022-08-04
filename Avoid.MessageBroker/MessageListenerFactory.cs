namespace Avoid.MessageBroker;

public class MessageListenerFactory : IMessageListenerFactory
{
    public IMessageListener<T> Create<T>(IMessageHandler<T> handler)
    {
        return new MessageListener<T>(handler);
    }
}