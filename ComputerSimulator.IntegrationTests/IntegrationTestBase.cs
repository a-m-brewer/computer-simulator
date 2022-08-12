using System;
using System.Linq;
using System.Text.RegularExpressions;
using ComputerSimulator.Core;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;
using ComputerSimulator.Core.Repositories;
using ComputerSimulator.Core.Services;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests;

public class IntegrationTestBase : HostTestBase
{
    private IWireService _wireService = null!;
    private IWireRepository _wireRepository = null!;

    [OneTimeSetUp]
    public void IntegrationOneTimeSetUp()
    {
        ComputerSettings = GetRequiredService<ComputerSettings>();
        _wireService = GetRequiredService<IWireService>();
        _wireRepository = GetRequiredService<IWireRepository>();
        GetRequiredService<IComponentFactory2>();
    }

    protected ComputerSettings ComputerSettings { get; private set; } = null!;
    
    protected IWire2<T> CreateTestWire<T>(string label, T initialValue)
    {
        return _wireService.Create($"{Guid.NewGuid()}-{label}", initialValue);
    }

    protected IWireGroup<T> CreateTestWireGroup<T>(string label, T initialValue)
    {
        return CreateTestWireGroup(label, initialValue, ComputerSettings.WordSize);
    }

    protected IWireGroup<T> CreateTestWireGroup<T>(string label, T initialValue, int size)
    {
        return _wireService.CreateGroup($"{Guid.NewGuid()}-{label}", initialValue, size);
    }

    protected IBus CreateTestBus(string label, bool initialValue)
    {
        return _wireService.CreateBus($"{Guid.NewGuid()}-{label}", initialValue);
    }

    protected IWire2<T> GetWireByLabel<T>(string label)
    {
        return _wireRepository.Wires
            .OfType<IWire2<T>>()
            .First(f => f.Label == label);
    }

    protected IWireGroup<T> GetGroupByLabel<T>(string label)
    {
        return _wireRepository.Groups
            .OfType<IWireGroup<T>>()
            .First(f => f.Label == label);
    }

    protected IWireGroup<T> GetFirstGroupMatchingRegex<T>(string regex)
    {
        return _wireRepository.Groups
            .OfType<IWireGroup<T>>()
            .First(f => Regex.IsMatch(f.Label, regex));
    }

    protected static string GetInternalWireLabel(IComponent2 component, string subLabel)
    {
        return $"{component.GetType().Name}-{component.Id}-{subLabel}";
    }
    
    protected static string GetInternalWireLabel(IBus component, string subLabel)
    {
        return $"{component.GetType().Name}-{component.Id}-{subLabel}";
    }
}