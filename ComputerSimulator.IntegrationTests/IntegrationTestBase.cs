using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests;

public class IntegrationTestBase : HostTestBase
{
    private IWireFactory _wireFactory = null!;

    [SetUp]
    public void IntegrationSetUp()
    {
        ComputerSettings = GetRequiredService<ComputerSettings>();
        _wireFactory = GetRequiredService<IWireFactory>();
        ComponentFactory = GetRequiredService<IComponentFactory>();
    }

    [TearDown]
    public void IntergrationTearDown()
    {
        ComputerSettings = null!;
        _wireFactory = null!;
        ComponentFactory = null!;
    }

    protected ComputerSettings ComputerSettings { get; private set; } = null!;
    protected IComponentFactory ComponentFactory { get; private set; } = null!;
    
    protected IWire<T> CreateTestWire<T>(T initialValue, string? label = null)
    {
        return _wireFactory.CreateWire(initialValue, label);
    }

    protected IWireGroup<T> CreateTestWireGroup<T>(T initialValue, string? label = null)
    {
        return CreateTestWireGroup(initialValue, ComputerSettings.WordSize, label);
    }

    protected IWireGroup<T> CreateTestWireGroup<T>(T initialValue, int size, string? label = null)
    {
        return _wireFactory.CreateGroup(initialValue, size, label);
    }

    protected IBus CreateTestBus(string? label = null)
    {
        return _wireFactory.CreateBus(label);
    }

    protected IOp CreateTestOp(string? label = null)
    {
        return _wireFactory.CreateOp(label);
    }
}