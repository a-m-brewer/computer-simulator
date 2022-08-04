namespace Avoid.MessageBroker;

public interface IMessageListenerFactory
{
    IMessageListener<T> Create<T>(IMessageHandler<T> handler);
}