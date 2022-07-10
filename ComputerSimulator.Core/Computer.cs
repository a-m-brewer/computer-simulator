using ComputerSimulator.Core.Circuits;
using Microsoft.Extensions.Logging;

namespace ComputerSimulator.Core;

public interface IComputer : IDisposable
{
    public void Run();
}

public class Computer  : IComputer
{
    private readonly IDecoder _decoder;
    private readonly ILogger<Computer> _logger;

    public Computer(
        IDecoder decoder,
        ILogger<Computer> logger)
    {
        _decoder = decoder;
        _logger = logger;
    }
    
    public void Run()
    {
        _decoder.Initialize(2);
        
        // 0 0
        _decoder.SetInputWireValue(0, false);
        _decoder.SetInputWireValue(0, false);
        _logger.LogInformation("[After 0 0] => {Result}", _decoder.ToString());
        
        // 0 1
        _decoder.SetInputWireValue(0, true);
        _decoder.SetInputWireValue(1, false);
        _logger.LogInformation("[After 0 1] => {Result}", _decoder.ToString());

        // 1 0
        _decoder.SetInputWireValue(0, false);
        _decoder.SetInputWireValue(1, true);
        _logger.LogInformation("[After 1 0] => {Result}", _decoder.ToString());
        
        _decoder.SetInputWireValue(0, true);
        _decoder.SetInputWireValue(1, true);
        _logger.LogInformation("[After 1 1] => {Result}", _decoder.ToString());
    }
    
    public void Dispose()
    {
        _decoder.Dispose();
        GC.SuppressFinalize(this);
    }
}