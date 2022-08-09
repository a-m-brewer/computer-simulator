using Microsoft.Extensions.DependencyInjection;

namespace Avoid.MessageBroker.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterMessageBusServices(this IServiceCollection services)
    {
        services.AddSingleton<IMessageBroker, MessageBroker>();
        services.AddSingleton<IMessageListenerFactory, MessageListenerFactory>();
        
        return services;
    }
}