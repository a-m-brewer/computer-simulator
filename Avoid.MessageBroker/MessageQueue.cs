namespace Avoid.MessageBroker;

public interface IMessageQueue
{
    string Topic { get; }
}

public interface IMessageQueue<T> : IMessageQueue
{
    void Publish(T message);

    event EventHandler<T>? MessagePublished;
}

public class MessageQueue<T> : IMessageQueue<T>
{
    public MessageQueue(string topic)
    {
        Topic = topic;
    }

    public void Publish(T message)
    {
        MessagePublished?.Invoke(this, message);
    }

    public event EventHandler<T>? MessagePublished;

    public string Topic { get; }
}