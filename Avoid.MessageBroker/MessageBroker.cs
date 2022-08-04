using System.Collections.Concurrent;

namespace Avoid.MessageBroker;

public interface IMessageBroker
{
    void AddQueue<TMessageType>(IMessageQueue<TMessageType> queue);
    void AddHandler<TMessageType>(string topic, IMessageHandler<TMessageType> handler);
    void Publish<T>(string topic, T message);
}

public class MessageBroker : IMessageBroker
{
    private readonly IMessageListenerFactory _messageListenerFactory;
    private readonly ConcurrentDictionary<string, IMessageQueue> _queues = new();
    private readonly ConcurrentDictionary<string, List<IMessageListener>> _listeners = new();
    
    public MessageBroker(IMessageListenerFactory messageListenerFactory)
    {
        _messageListenerFactory = messageListenerFactory;
    }
    
    public void AddQueue<TMessageType>(IMessageQueue<TMessageType> queue)
    {
        if (_queues.ContainsKey(queue.Topic))
        {
            throw new ArgumentException($"Queue with topic: {queue.Topic} already registered", nameof(queue));
        }

        if (string.IsNullOrWhiteSpace(queue.Topic))
        {
            throw new ArgumentException("Queue must have a topic", nameof(queue));
        }

        if (_listeners.TryGetValue(queue.Topic, out var listeners))
        {
            foreach (var listener in listeners)
            {
                if (listener is IMessageListener<TMessageType> messageListener)
                {
                    queue.MessagePublished += messageListener.OnMessagePublished;
                }
            }
        }
        
        _queues[queue.Topic] = queue;
    }

    public void AddHandler<TMessageType>(string topic, IMessageHandler<TMessageType> handler)
    {
        var listeners = _listeners.GetOrAdd(topic, new List<IMessageListener>());

        var listener = _messageListenerFactory.Create(handler);

        listeners.Add(listener);

        if (TryGetQueue<TMessageType>(topic, out var queue))
        {
            queue.MessagePublished += listener.OnMessagePublished;
        }
    }

    public void Publish<T>(string topic, T message)
    {
        var queue = GetQueueOrThrow<T>(topic);
        queue.Publish(message);
    }

    private IMessageQueue<TMessageType> GetQueueOrThrow<TMessageType>(string topic)
    {
        if (!TryGetQueue<TMessageType>(topic, out var queue))
        {
            throw new ArgumentException($"Failed to get queue for topic: {topic}", nameof(topic));
        }

        return queue;
    }

    private bool TryGetQueue<TMessageType>(string topic, out IMessageQueue<TMessageType> queue)
    {
        if (!_queues.TryGetValue(topic, out var preGenericQueue))
        {
            queue = null!;
            return false;
        }

        if (preGenericQueue is not IMessageQueue<TMessageType> genericQueue)
        {
            queue = null!;
            return false;
        }

        queue = genericQueue;
        return true;
    }
}