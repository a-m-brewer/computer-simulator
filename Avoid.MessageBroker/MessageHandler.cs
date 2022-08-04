namespace Avoid.MessageBroker;

public interface IMessageHandler
{
}

public interface IMessageHandler<in TMessageType> : IMessageHandler
{
    void Handle(TMessageType message);
}