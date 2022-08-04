// See https://aka.ms/new-console-template for more information

using Avoid.MessageBroker;

var broker = new MessageBroker(new MessageListenerFactory());

var exampleTopic = "example-topic";

var handler = new ExampleHandler();

broker.AddHandler(exampleTopic, handler);

var queue = new MessageQueue<ExampleMessage>(exampleTopic);

broker.AddQueue(queue);

broker.Publish(exampleTopic, new ExampleMessage { Message = "test" });

public class ExampleMessage
{
    public string Message { get; set; } = string.Empty;
}

public class ExampleHandler : IMessageHandler<ExampleMessage>
{
    public void Handle(ExampleMessage message)
    {
        Console.WriteLine($"Received Message: {message.Message}");
    }
}