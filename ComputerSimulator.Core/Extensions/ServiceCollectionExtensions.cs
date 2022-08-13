using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Repositories;
using ComputerSimulator.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ComputerSimulator.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterCoreServices(
        this IServiceCollection services,
        ComputerSettings computerSettings)
    {
        services.Scan(s => s.FromAssemblyOf<IComponent2>()
            .AddClasses(f => f.AssignableTo<IComponent2>())
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        services.AddTransient<IComputer, Computer>();
        services.AddTransient<IWireService, WireService>();

        services.AddSingleton<IWire2Factory2, MessageBrokerWireFactory>();
        services.AddSingleton<IComponentFactory2, ComponentFactory2>();
        services.AddScoped<IWireRepository, WireRepository>();

        services.AddSingleton(computerSettings);

        return services;
    }
}