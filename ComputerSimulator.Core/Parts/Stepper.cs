using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Exceptions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;

namespace ComputerSimulator.Core.Parts;

public interface IStepper : IPart
{
    IWire<bool> Clk { get; }
    
    IWire<bool> Reset { get; }

    IWireGroup<bool> Steps { get; }
}

public class Stepper : PartsBase, IStepper
{
    private readonly IMemoryBit[] _memoryBits;
    private readonly INot _resetNot;
    private readonly INot _clkNot;
    private readonly IOr2 _resetOrNotClk;
    private readonly IOr2 _clkOrReset;
    private readonly INot[] _nots;
    private readonly IGate2[] _stepGates;

    public Stepper(
        IWire<bool> clk,
        IWireGroup<bool> steps,
        IComponentFactory componentFactory,
        IWireFactory wireFactory) 
        : this(clk, steps[^1], steps, componentFactory, wireFactory)
    {
    }
    
    public Stepper(
        IWire<bool> clk,
        IWire<bool> reset,
        IWireGroup<bool> steps,
        IComponentFactory componentFactory,
        IWireFactory wireFactory)
        : base(componentFactory, wireFactory)
    {
        if (steps.Count != 7)
        {
            throw new ComputerSimulatorException($"{nameof(Steps)} incorrect length. expected 7 got {steps.Count}");
        }
        
        Clk = clk;
        Reset = reset;
        Steps = steps;

        _resetNot = ComponentFactory.CreateNot(Reset, WireFactory.CreateWire(false, "reset-not-output"));
        _clkNot = ComponentFactory.CreateNot(Clk, WireFactory.CreateWire(false, "clk-not-output"));

        _resetOrNotClk = ComponentFactory.CreateOr2(Reset, _clkNot.Output, WireFactory.CreateWire(false, "reset-or-not-clk-output"));
        _clkOrReset = ComponentFactory.CreateOr2(Reset, Clk, WireFactory.CreateWire(false, "clk-or-reset"));
        
        _memoryBits = new IMemoryBit[12];
        for (var i = 0; i < _memoryBits.Length; i++)
        {
            _memoryBits[i] = ComponentFactory.CreateMemoryBit(
                i == 0
                    ? _resetNot.Output
                    : _memoryBits[i - 1].Output,
                i == _memoryBits.Length - 1
                    ? Steps[^1]
                    : WireFactory.CreateWire(false, $"memory-bits-{i}-output"),
                i % 2 == 0
                    ? _resetOrNotClk.Output 
                    : _clkOrReset.Output);
        }

        _nots = new INot[6];
        var mbIndex = 1;
        for (var i = 0; i < _nots.Length; i++)
        {
            _nots[i] = ComponentFactory.CreateNot(_memoryBits[mbIndex].Output, WireFactory.CreateWire(false, $"not-{i}-output"));
            mbIndex += 2;
        }

        // 6 long because step 7 only has a wire not a gate
        _stepGates = new IGate2[6];
        mbIndex = 1;
        for (var i = 0; i < _stepGates.Length; i++)
        {
            _stepGates[i] = i == 0
                ? ComponentFactory.CreateOr2(Reset, _nots[i].Output, Steps[i])
                : ComponentFactory.CreateAnd2(_memoryBits[mbIndex].Output, _nots[i].Output, Steps[i]);

            if (i != 0)
            {
                mbIndex += 2;
            }
        }
    }

    public IWire<bool> Clk { get; }

    public IWire<bool> Reset { get; }
    public IWireGroup<bool> Steps { get; }
    
    public void Update()
    {
        _resetNot.Update();
        _clkNot.Update();
        
        _resetOrNotClk.Update();
        _clkOrReset.Update();

        foreach (var memoryBit in _memoryBits)
        {
            memoryBit.Update();
        }

        foreach (var not in _nots)
        {
            not.Update();
        }

        foreach (var stepGate in _stepGates)
        {
            stepGate.Update();
        }
    }
}