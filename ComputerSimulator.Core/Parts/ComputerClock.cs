using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;

namespace ComputerSimulator.Core.Parts;

public interface IComputerClock : IPart
{
    /// <summary>
    /// Clock Output
    /// </summary>
    IWire<bool> Clk { get; }

    /// <summary>
    /// Clock Enable Output
    /// </summary>
    IWire<bool> ClkE { get; }
    
    /// <summary>
    /// Clock Set Output
    /// </summary>
    IWire<bool> ClkS { get; }
}

public class ComputerClock : PartsBase, IComputerClock
{
    private readonly IOr2 _or;
    private readonly IAnd2 _and;
    
    private bool _clkCycledLast;
    private readonly IClock _clkClock;
    private readonly IClock _clkDClock;

    public ComputerClock(
        IWire<bool> clk,
        IWire<bool> clkE,
        IWire<bool> clkS,
        IComponentFactory componentFactory,
        IWireFactory wireFactory) 
        : base(componentFactory, wireFactory)
    {
        Clk = clk;
        ClkE = clkE;
        ClkS = clkS;

        _clkClock = ComponentFactory.CreateClock(Clk);
        _clkDClock = ComponentFactory.CreateClock(WireFactory.CreateWire<bool>());

        _or = ComponentFactory.CreateOr2(_clkClock.Clk, _clkDClock.Clk, ClkE);
        _and = ComponentFactory.CreateAnd2(_clkClock.Clk, _clkDClock.Clk, ClkS);
    }

    public IWire<bool> Clk { get; }
    public IWire<bool> ClkE { get; }
    public IWire<bool> ClkS { get; }

    public void Update()
    {
        if (_clkCycledLast)
        {
            _clkDClock.Update();
        }
        else
        {
            _clkClock.Update();
        }

        _clkCycledLast = !_clkCycledLast;
        
        _or.Update();
        _and.Update();
    }
}