using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;
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

        services.AddSingleton<IWire2Factory2, WireFactory>();
        services.AddTransient<IComponentFactory2, ComponentFactory2>();
        
        services.AddSingleton(computerSettings);

        return services;
    }
}