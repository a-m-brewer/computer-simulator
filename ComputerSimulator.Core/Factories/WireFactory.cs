using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ComputerSimulator.Core.Factories;

public interface IWireCupboard
{
    IWire<T> Retrieve<T>(T initialValue, string label);

    IWire<T>[] RetrieveSet<T>(T initialValue, string label);
}

public class WireCupboard : IWireCupboard
{
    private readonly ComputerSettings _settings;
    private readonly IServiceProvider _serviceProvider;

    public WireCupboard(
        ComputerSettings settings,
        IServiceProvider serviceProvider)
    {
        _settings = settings;
        _serviceProvider = serviceProvider;
    }
    
    public IWire<T> Retrieve<T>(T initialValue, string label)
    {
        return new Wire<T>(initialValue, label, _serviceProvider.GetRequiredService<ILogger<Wire<T>>>());
    }

    public IWire<T>[] RetrieveSet<T>(T initialValue, string label)
    {
        return _settings
            .InitArray<IWire<T>>()
            .Fill(i => Retrieve(initialValue, $"{label}[{i}]"));
    }
}