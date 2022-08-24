using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public interface IComponentFactory2
{
    IAnd CreateAnd(IWireGroup<bool> inputs, IWire2<bool> output);
    IAnd2 CreateAnd2(IWire2<bool> inputA, IWire2<bool> inputB, IWire2<bool> output);
    IAnder CreateAnder(IWireGroup<bool> inputsA, IWireGroup<bool> inputsB, IWireGroup<bool> outputs);

    INot CreateNot(IWire2<bool> input, IWire2<bool> output);
    
    INoter CreateNoter(IWireGroup<bool> inputs, IWireGroup<bool> outputs);

    INAnd CreateNAnd(IWireGroup<bool> inputs, IWire2<bool> output);
    INAnd2 CreateNAnd2(IWire2<bool> inputA, IWire2<bool> inputB, IWire2<bool> output);

    IOr2 CreateOr2(IWire2<bool> inputA, IWire2<bool> inputB, IWire2<bool> output);
    IXOr2 CreateXOr2(IWire2<bool> inputA, IWire2<bool> inputB, IWire2<bool> output);

    IEnabler CreateEnabler(IWire2<bool> enable, IWireGroup<bool> inputs, IWireGroup<bool> outputs);

    IWord CreateWord(IWireGroup<bool> inputs, IWireGroup<bool> outputs, IWire2<bool> set);

    IRegister CreateRegister(IWire2<bool> set, IWire2<bool> enable, IWireGroup<bool> inputs, IWireGroup<bool> outputs);

    IDecoder CreateDecoder(IWireGroup<bool> inputs);

    IShifter CreateRightShifter(IWire2<bool> shiftIn, IWire2<bool> shiftOut, IWireGroup<bool> input, IWireGroup<bool> output);
    IShifter CreateLeftShifter(IWire2<bool> shiftIn, IWire2<bool> shiftOut, IWireGroup<bool> input, IWireGroup<bool> output);

    IRamSlot CreateRamSlot(IWire2<bool> x, IWire2<bool> y, IWire2<bool> set, IWire2<bool> enable, IBus io);

    IMemoryBit CreateMemoryBit(
        IWire2<bool> input,
        IWire2<bool> output,
        IWire2<bool> set);

    IMemoryBit[] CreateMemoryBitSet(IWireGroup<bool> inputs, IWireGroup<bool> outputs, IWire2<bool> set);

    IRam CreateRam(IWire2<bool> marSet, IBus marInputBus, IWire2<bool> set, IWire2<bool> enable, IBus io);
}

public class ComponentFactory2 : IComponentFactory2
{
    private readonly IWire2Factory2 _wireFactory;
    private readonly LeftShifterWireFactory _leftShifterWireFactory;
    private readonly RightShifterWireFactory _rightShifterWireFactory;
    private readonly ComputerSettings _computerSettings;

    public ComponentFactory2(
        IWire2Factory2 wireFactory,
        LeftShifterWireFactory leftShifterWireFactory,
        RightShifterWireFactory rightShifterWireFactory,
        ComputerSettings computerSettings)
    {
        _wireFactory = wireFactory;
        _leftShifterWireFactory = leftShifterWireFactory;
        _rightShifterWireFactory = rightShifterWireFactory;
        _computerSettings = computerSettings;
    }
    
    public IAnd CreateAnd(IWireGroup<bool> inputs, IWire2<bool> output)
    {
        return new And(inputs, output);
    }

    public IAnd2 CreateAnd2(IWire2<bool> inputA, IWire2<bool> inputB, IWire2<bool> output)
    {
        return new And2(inputA, inputB, output);
    }

    public IAnder CreateAnder(IWireGroup<bool> inputsA, IWireGroup<bool> inputsB, IWireGroup<bool> outputs)
    {
        return new Ander(inputsA, inputsB, outputs);
    }

    public INot CreateNot(IWire2<bool> input, IWire2<bool> output)
    {
        return new Not(input, output);
    }

    public INoter CreateNoter(IWireGroup<bool> inputs, IWireGroup<bool> outputs)
    {
        return new Noter(inputs, outputs);
    }

    public INAnd CreateNAnd(IWireGroup<bool> inputs, IWire2<bool> output)
    {
        return new NAnd(inputs, output, this, _wireFactory);
    }

    public INAnd2 CreateNAnd2(IWire2<bool> inputA, IWire2<bool> inputB, IWire2<bool> output)
    {
        return new NAnd2(inputA, inputB, output, this, _wireFactory);
    }

    public IOr2 CreateOr2(IWire2<bool> inputA, IWire2<bool> inputB, IWire2<bool> output)
    {
        return new Or2(inputA, inputB, output);
    }

    public IXOr2 CreateXOr2(IWire2<bool> inputA, IWire2<bool> inputB, IWire2<bool> output)
    {
        return new XOr2(inputA, inputB, output);
    }

    public IEnabler CreateEnabler(IWire2<bool> enable, IWireGroup<bool> inputs, IWireGroup<bool> outputs)
    {
        return new Enabler(enable, inputs, outputs, this, _wireFactory);
    }

    public IWord CreateWord(IWireGroup<bool> inputs, IWireGroup<bool> outputs, IWire2<bool> set)
    {
        return new Word(inputs, outputs, set, this, _wireFactory);
    }

    public IRegister CreateRegister(IWire2<bool> set, IWire2<bool> enable, IWireGroup<bool> inputs, IWireGroup<bool> outputs)
    {
        return new Register(set, enable, inputs, outputs, this, _wireFactory);
    }

    public IDecoder CreateDecoder(IWireGroup<bool> inputs)
    {
        var outputSize = Decoder.CalculateOutputSize(inputs.Count);

        var outputs = _wireFactory.CreateGroup(false, outputSize);

        return new Decoder(inputs, outputs, this, _wireFactory);
    }

    public IShifter CreateRightShifter(IWire2<bool> shiftIn, IWire2<bool> shiftOut, IWireGroup<bool> input, IWireGroup<bool> output)
    {
        return new Shifter(shiftIn, shiftOut, input, output, _rightShifterWireFactory,this, _wireFactory);
    }

    public IShifter CreateLeftShifter(IWire2<bool> shiftIn, IWire2<bool> shiftOut, IWireGroup<bool> input, IWireGroup<bool> output)
    {
        return new Shifter(shiftIn, shiftOut, input, output, _leftShifterWireFactory,this, _wireFactory);
    }

    public IRamSlot CreateRamSlot(IWire2<bool> x, IWire2<bool> y, IWire2<bool> set, IWire2<bool> enable, IBus io)
    {
        return new RamSlot(x, y, set, enable, io, this, _wireFactory);
    }

    public IMemoryBit[] CreateMemoryBitSet(IWireGroup<bool> inputs, IWireGroup<bool> outputs, IWire2<bool> set)
    {
        var memory = _computerSettings
            .InitArray<IMemoryBit>();

        for (var i = 0; i < _computerSettings.WordSize; i++)
        {
            memory[i] = CreateMemoryBit(inputs[i], outputs[i], set);
        }

        return memory;
    }

    public IRam CreateRam(IWire2<bool> marSet, IBus marInputBus, IWire2<bool> set, IWire2<bool> enable, IBus io)
    {
        return new Ram(marSet, marInputBus, set, enable, io, _computerSettings, this, _wireFactory);
    }

    public IMemoryBit CreateMemoryBit(
        IWire2<bool> input,
        IWire2<bool> output,
        IWire2<bool> set)
    {
        return new MemoryBit(input, set, output, this, _wireFactory);
    }
}