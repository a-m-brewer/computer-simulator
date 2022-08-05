using Avoid.MessageBroker;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core;

public interface IComputer : IDisposable
{
    public void Run();
}

public class Computer  : IComputer
{
    // private readonly ComputerSettings _computerSettings;
    // private readonly ILogger<Computer> _logger;

    // public Computer(
    //     ComputerSettings computerSettings,
    //     ILogger<Computer> logger)
    // {
    //     _computerSettings = computerSettings;
    //     _logger = logger;
    // }
    
    public void Run()
    {
        var broker = new MessageBroker(new MessageListenerFactory());

        var andInputWire = new MessageBrokerWire(broker)
        {
            Label = "and_input_1"
        };
        
        
        var andOutputWire = new MessageBrokerWire(broker)
        {
            Label = "and_output"
        };
        
        
        var and2 = new And2
        {
            Input = andInputWire,
            Output = andOutputWire
        };

        andInputWire.Label = "and_input_2";

        andInputWire.Value = true;
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}