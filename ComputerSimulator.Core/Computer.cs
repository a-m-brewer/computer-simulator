using Avoid.MessageBroker;
using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;
using Microsoft.Extensions.Logging;

namespace ComputerSimulator.Core;

public interface IComputer : IDisposable
{
    public void Run();
}

public class Computer  : IComputer
{
    private readonly ComputerSettings _computerSettings;
    private readonly ILogger<Computer> _logger;

    public Computer(
        ComputerSettings computerSettings,
        ILogger<Computer> logger)
    {
        _computerSettings = computerSettings;
        _logger = logger;
    }
    
    public void Run()
    {
        var broker = new MessageBroker(new MessageListenerFactory());

        var andInput1 = new Wire2("and_input_1", broker);
        // var andInput2 = new Wire2("and_input_2", broker);

        var and2 = new And2();
        
        var andConnector1 = new Connector<And2>();
        andConnector1.Connect(and2, and => newValue => and.Input = newValue);
        
        broker.AddHandler("and_input_1", andConnector1);

        var andOutput = new Wire2("and_output", broker);

        andInput1.SetValue(true);
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}