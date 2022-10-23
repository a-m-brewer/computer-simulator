namespace ComputerSimulator.Core.Parts;

public interface IIoBus
{
    IBus CpuBus { get; }
    
    IWire<bool> InputOutput { get; }

    IWire<bool> DataAddress { get; }
    
    ISetEnableWire<bool> Clk { get; }
}

public class IoBus : IIoBus
{
    public IoBus(IBus cpuBus, IWire<bool> inputOutput, IWire<bool> dataAddress, ISetEnableWire<bool> clk)
    {
        CpuBus = cpuBus;
        InputOutput = inputOutput;
        DataAddress = dataAddress;
        Clk = clk;
    }

    public IBus CpuBus { get; }
    public IWire<bool> InputOutput { get; }
    public IWire<bool> DataAddress { get; }
    public ISetEnableWire<bool> Clk { get; }
}