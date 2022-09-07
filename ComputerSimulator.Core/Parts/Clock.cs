namespace ComputerSimulator.Core.Parts;

public interface IClock : IPart
{
    IWire<bool> Clk { get; }
}

public class Clock : IClock
{
    public Clock(IWire<bool> clk)
    {
        Clk = clk;
    }
    
    public IWire<bool> Clk { get; }
    
    public void Update()
    {
        Clk.Value = !Clk.Value;
    }
}