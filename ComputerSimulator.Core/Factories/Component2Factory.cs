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

    INot CreateNot(IWire2<bool> input, IWire2<bool> output);

    INAnd CreateNAnd(IWireGroup<bool> inputs, IWire2<bool> output);
    INAnd2 CreateNAnd2(IWire2<bool> inputA, IWire2<bool> inputB, IWire2<bool> output);

    IOr2 CreateOr2(IWire2<bool> inputA, IWire2<bool> inputB, IWire2<bool> output);
    IXOr2 CreateXOr2(IWire2<bool> inputA, IWire2<bool> inputB, IWire2<bool> output);

    IEnabler CreateEnabler(IWire2<bool> enable, IWireGroup<bool> inputs, IWireGroup<bool> outputs);

    IWord CreateWord(IWireGroup<bool> inputs, IWireGroup<bool> outputs, IWire2<bool> set);

    IRegister CreateRegister(IWire2<bool> set, IWire2<bool> enable, IWireGroup<bool> inputs, IWireGroup<bool> outputs);

    IDecoder CreateDecoder(IWireGroup<bool> inputs);

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
    private readonly ComputerSettings _computerSettings;

    public ComponentFactory2(IWire2Factory2 wireFactory, ComputerSettings computerSettings)
    {
        _wireFactory = wireFactory;
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

    public INot CreateNot(IWire2<bool> input, IWire2<bool> output)
    {
        return new Not(input, output);
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