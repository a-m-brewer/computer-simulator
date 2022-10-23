using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public interface IComponentFactory
{
    IAnd CreateAnd(IWireGroup<bool> inputs, IWire<bool> output);
    IAnd2 CreateAnd2(IWire<bool> inputA, IWire<bool> inputB, IWire<bool> output);
    IAnder CreateAnder(IWireGroup<bool> inputsA, IWireGroup<bool> inputsB, IWireGroup<bool> outputs);

    INot CreateNot(IWire<bool> input, IWire<bool> output);
    
    INoter CreateNoter(IWireGroup<bool> inputs, IWireGroup<bool> outputs);

    INAnd CreateNAnd(IWireGroup<bool> inputs, IWire<bool> output);
    INAnd2 CreateNAnd2(IWire<bool> inputA, IWire<bool> inputB, IWire<bool> output);

    IOr CreateOr(IWireGroup<bool> inputs, IWire<bool> output);
    IOr2 CreateOr2(IWire<bool> inputA, IWire<bool> inputB, IWire<bool> output);
    IOrer CreateOrer(IWireGroup<bool> inputsA, IWireGroup<bool> inputsB, IWireGroup<bool> outputs);
    IXOrer CreateXOrer(IWireGroup<bool> inputsA, IWireGroup<bool> inputsB, IWireGroup<bool> outputs);
    IXOr2 CreateXOr2(IWire<bool> inputA, IWire<bool> inputB, IWire<bool> output);

    IEnabler CreateEnabler(IWire<bool> enable, IWireGroup<bool> inputs, IWireGroup<bool> outputs);

    IWord CreateWord(IWireGroup<bool> inputs, IWireGroup<bool> outputs, IWire<bool> set);

    IRegister CreateRegister(IWire<bool> set, IWire<bool> enable, IWireGroup<bool> inputs, IWireGroup<bool> outputs);

    ICaezRegister CreateCaezRegister(IWire<bool> set, ICaez<bool> inputs, ICaez<bool> outputs);

    IDecoder CreateDecoder(IWireGroup<bool> inputs);

    IShifter CreateRightShifter(IWire<bool> shiftIn, IWire<bool> shiftOut, IWireGroup<bool> input, IWireGroup<bool> output);
    IShifter CreateLeftShifter(IWire<bool> shiftIn, IWire<bool> shiftOut, IWireGroup<bool> input, IWireGroup<bool> output);

    IRamSlot CreateRamSlot(IWire<bool> x, IWire<bool> y, IWire<bool> set, IWire<bool> enable, IBus io);

    IMemoryBit CreateMemoryBit(
        IWire<bool> input,
        IWire<bool> output,
        IWire<bool> set);

    IMemoryBit[] CreateMemoryBitSet(IWireGroup<bool> inputs, IWireGroup<bool> outputs, IWire<bool> set);

    IRam CreateRam(IWire<bool> marSet, IBus marInputBus, IWire<bool> set, IWire<bool> enable, IBus io);

    IBitAdder CreateBitAdder(IWire<bool> inputA, IWire<bool> inputB, IWire<bool> carryIn, IWire<bool> carryOut, IWire<bool> sum);

    IWordAdder CreateWordAdder(IWireGroup<bool> inputsA, IWireGroup<bool> inputsB, IWire<bool> carryIn, IWire<bool> carryOut, IWireGroup<bool> sum);

    IBitComparator CreateBitComparator(IWire<bool> inputA, IWire<bool> inputB, IWire<bool> allBitsAboveEqual, IWire<bool> aAboveLarger, IWire<bool> unEqual, IWire<bool> equal, IWire<bool> aLarger);

    IWordComparator CreateWordComparator(IWireGroup<bool> inputsA,
        IWireGroup<bool> inputsB,
        IWire<bool> allBitsAboveEqual,
        IWire<bool> aAboveLarger,
        IWireGroup<bool> unEqual,
        IWire<bool> equal,
        IWire<bool> aLarger);

    IIsZeroChecker CreateIsZeroChecker(IWireGroup<bool> inputs, IWire<bool> output);

    IArithmeticLogicUnit CreateArithmeticLogicUnit(IWireGroup<bool> inputsA, IWireGroup<bool> inputsB,
        IWire<bool> carryIn, IOp op, IWireGroup<bool> outputs, ICaez<bool> caez);

    IBus1 CreateBus1(IWire<bool> bit, IWireGroup<bool> inputs, IWireGroup<bool> outputs);

    IClock CreateClock(IWire<bool> clk);

    IComputerClock CreateComputerClock(IWire<bool> clk, IWire<bool> clkE, IWire<bool> clkS);

    IStepper CreateStepper(IWire<bool> clk, IWireGroup<bool> steps);

    IStepper CreateStepper(IWire<bool> clk, IWire<bool> reset, IWireGroup<bool> steps);

    ICentralProcessingUnit CreateCentralProcessingUnit(
        IWire<bool> bus1,
        ISetEnableWire<bool> iar,
        ISetEnableWire<bool> ram,
        ISetEnableWire<bool> acc,
        ISetEnableWire<bool> ioClk,
        ISetEnableWireGroup<bool> generalPurposeRegisters,
        IOp op,
        IWire<bool> marSet,
        IWire<bool> tmpSet,
        IWire<bool> irSet,
        IWire<bool> flagsSet,
        IWire<bool> carryInTmp,
        IWire<bool> ioInputOutput, 
        IWire<bool> ioDataAddress,
        IWireGroup<bool> instructionRegister,
        ICaez<bool> caez);

    IComputerPart CreateComputerPart();
}

public class ComponentFactory : IComponentFactory
{
    private readonly IWireFactory _wireFactory;
    private readonly LeftShifterWireFactory _leftShifterWireFactory;
    private readonly RightShifterWireFactory _rightShifterWireFactory;
    private readonly ComputerSettings _computerSettings;

    public ComponentFactory(
        IWireFactory wireFactory,
        LeftShifterWireFactory leftShifterWireFactory,
        RightShifterWireFactory rightShifterWireFactory,
        ComputerSettings computerSettings)
    {
        _wireFactory = wireFactory;
        _leftShifterWireFactory = leftShifterWireFactory;
        _rightShifterWireFactory = rightShifterWireFactory;
        _computerSettings = computerSettings;
    }
    
    public IAnd CreateAnd(IWireGroup<bool> inputs, IWire<bool> output)
    {
        return new And(inputs, output);
    }

    public IAnd2 CreateAnd2(IWire<bool> inputA, IWire<bool> inputB, IWire<bool> output)
    {
        return new And2(inputA, inputB, output);
    }

    public IAnder CreateAnder(IWireGroup<bool> inputsA, IWireGroup<bool> inputsB, IWireGroup<bool> outputs)
    {
        return new Ander(inputsA, inputsB, outputs);
    }

    public INot CreateNot(IWire<bool> input, IWire<bool> output)
    {
        return new Not(input, output);
    }

    public INoter CreateNoter(IWireGroup<bool> inputs, IWireGroup<bool> outputs)
    {
        return new Noter(inputs, outputs);
    }

    public INAnd CreateNAnd(IWireGroup<bool> inputs, IWire<bool> output)
    {
        return new NAnd(inputs, output, this, _wireFactory);
    }

    public INAnd2 CreateNAnd2(IWire<bool> inputA, IWire<bool> inputB, IWire<bool> output)
    {
        return new NAnd2(inputA, inputB, output, this, _wireFactory);
    }

    public IOr CreateOr(IWireGroup<bool> inputs, IWire<bool> output)
    {
        return new Or(inputs, output);
    }

    public IOr2 CreateOr2(IWire<bool> inputA, IWire<bool> inputB, IWire<bool> output)
    {
        return new Or2(inputA, inputB, output);
    }

    public IOrer CreateOrer(IWireGroup<bool> inputsA, IWireGroup<bool> inputsB, IWireGroup<bool> outputs)
    {
        return new Orer(inputsA, inputsB, outputs);
    }

    public IXOrer CreateXOrer(IWireGroup<bool> inputsA, IWireGroup<bool> inputsB, IWireGroup<bool> outputs)
    {
        return new XOrer(inputsA, inputsB, outputs);
    }

    public IXOr2 CreateXOr2(IWire<bool> inputA, IWire<bool> inputB, IWire<bool> output)
    {
        return new XOr2(inputA, inputB, output);
    }

    public IEnabler CreateEnabler(IWire<bool> enable, IWireGroup<bool> inputs, IWireGroup<bool> outputs)
    {
        return new Enabler(enable, inputs, outputs, this, _wireFactory);
    }

    public IWord CreateWord(IWireGroup<bool> inputs, IWireGroup<bool> outputs, IWire<bool> set)
    {
        return new Word(inputs, outputs, set, this, _wireFactory);
    }

    public IRegister CreateRegister(IWire<bool> set, IWire<bool> enable, IWireGroup<bool> inputs, IWireGroup<bool> outputs)
    {
        return new Register(set, enable, inputs, outputs, this, _wireFactory);
    }

    public ICaezRegister CreateCaezRegister(IWire<bool> set, ICaez<bool> inputs, ICaez<bool> outputs)
    {
        return new CaezRegister(set, inputs, outputs, this, _wireFactory);
    }

    public IDecoder CreateDecoder(IWireGroup<bool> inputs)
    {
        var outputSize = Decoder.CalculateOutputSize(inputs.Count);

        var outputs = _wireFactory.CreateGroup(false, outputSize);

        return new Decoder(inputs, outputs, this, _wireFactory);
    }

    public IShifter CreateRightShifter(IWire<bool> shiftIn, IWire<bool> shiftOut, IWireGroup<bool> input, IWireGroup<bool> output)
    {
        return new Shifter(shiftIn, shiftOut, input, output, _rightShifterWireFactory,this, _wireFactory);
    }

    public IShifter CreateLeftShifter(IWire<bool> shiftIn, IWire<bool> shiftOut, IWireGroup<bool> input, IWireGroup<bool> output)
    {
        return new Shifter(shiftIn, shiftOut, input, output, _leftShifterWireFactory,this, _wireFactory);
    }

    public IRamSlot CreateRamSlot(IWire<bool> x, IWire<bool> y, IWire<bool> set, IWire<bool> enable, IBus io)
    {
        return new RamSlot(x, y, set, enable, io, this, _wireFactory);
    }

    public IMemoryBit[] CreateMemoryBitSet(IWireGroup<bool> inputs, IWireGroup<bool> outputs, IWire<bool> set)
    {
        var memory = inputs.Count
            .InitArray<IMemoryBit>();

        for (var i = 0; i < inputs.Count; i++)
        {
            memory[i] = CreateMemoryBit(inputs[i], outputs[i], set);
        }

        return memory;
    }

    public IRam CreateRam(IWire<bool> marSet, IBus marInputBus, IWire<bool> set, IWire<bool> enable, IBus io)
    {
        return new Ram(marSet, marInputBus, set, enable, io, _computerSettings, this, _wireFactory);
    }

    public IBitAdder CreateBitAdder(IWire<bool> inputA, IWire<bool> inputB, IWire<bool> carryIn, IWire<bool> carryOut, IWire<bool> sum)
    {
        return new BitAdder(inputA, inputB, carryIn, carryOut, sum, this, _wireFactory);
    }

    public IWordAdder CreateWordAdder(IWireGroup<bool> inputsA, IWireGroup<bool> inputsB, IWire<bool> carryIn, IWire<bool> carryOut, IWireGroup<bool> sum)
    {
        return new WordAdder(inputsA, inputsB, carryIn, carryOut, sum, this, _wireFactory);
    }

    public IBitComparator CreateBitComparator(IWire<bool> inputA, IWire<bool> inputB, IWire<bool> allBitsAboveEqual, IWire<bool> aAboveLarger,
        IWire<bool> unEqual, IWire<bool> equal, IWire<bool> aLarger)
    {
        return new BitComparator(inputA, inputB, allBitsAboveEqual, aAboveLarger, unEqual, equal, aLarger, this, _wireFactory);
    }

    public IWordComparator CreateWordComparator(IWireGroup<bool> inputsA, IWireGroup<bool> inputsB, IWire<bool> allBitsAboveEqual,
        IWire<bool> aAboveLarger, IWireGroup<bool> unEqual, IWire<bool> equal, IWire<bool> aLarger)
    {
        return new WordComparator(inputsA, inputsB, allBitsAboveEqual, aAboveLarger, unEqual, equal, aLarger, this, _wireFactory);
    }

    public IIsZeroChecker CreateIsZeroChecker(IWireGroup<bool> inputs, IWire<bool> output)
    {
        return new IsZeroChecker(inputs, output, this, _wireFactory);
    }

    public IArithmeticLogicUnit CreateArithmeticLogicUnit(IWireGroup<bool> inputsA, IWireGroup<bool> inputsB, IWire<bool> carryIn, IOp op,
        IWireGroup<bool> outputs, ICaez<bool> caez)
    {
        return new ArithmeticLogicUnit(inputsA, inputsB, carryIn, op, outputs, caez, this, _wireFactory);
    }

    public IBus1 CreateBus1(IWire<bool> bit, IWireGroup<bool> inputs, IWireGroup<bool> outputs)
    {
        return new Bus1(bit, inputs, outputs, this, _wireFactory);
    }

    public IClock CreateClock(IWire<bool> clk)
    {
        return new Clock(clk);
    }

    public IComputerClock CreateComputerClock(IWire<bool> clk, IWire<bool> clkE, IWire<bool> clkS)
    {
        return new ComputerClock(clk, clkE, clkS, this, _wireFactory);
    }

    public IStepper CreateStepper(IWire<bool> clk, IWireGroup<bool> steps)
    {
        return new Stepper(clk, steps, this, _wireFactory);
    }

    public IStepper CreateStepper(IWire<bool> clk, IWire<bool> reset, IWireGroup<bool> steps)
    {
        return new Stepper(clk, reset, steps, this, _wireFactory);
    }

    public ICentralProcessingUnit CreateCentralProcessingUnit(
        IWire<bool> bus1,
        ISetEnableWire<bool> iar,
        ISetEnableWire<bool> ram,
        ISetEnableWire<bool> acc,
        ISetEnableWire<bool> ioClk,
        ISetEnableWireGroup<bool> generalPurposeRegisters,
        IOp op,
        IWire<bool> marSet,
        IWire<bool> tmpSet,
        IWire<bool> irSet,
        IWire<bool> flagsSet,
        IWire<bool> carryInTmp,
        IWire<bool> ioInputOutput, 
        IWire<bool> ioDataAddress,
        IWireGroup<bool> instructionRegister,
        ICaez<bool> caez)
    {
        return new CentralProcessingUnit(
            bus1,
            iar,
            ram,
            acc,
            ioClk,
            generalPurposeRegisters,
            op,
            marSet,
            tmpSet,
            irSet,
            flagsSet,
            carryInTmp,
            ioInputOutput,
            ioDataAddress,
            instructionRegister,
            caez,
            this,
            _wireFactory);
    }

    public IComputerPart CreateComputerPart()
    {
        return new ComputerPart(this, _wireFactory);
    }

    public IMemoryBit CreateMemoryBit(
        IWire<bool> input,
        IWire<bool> output,
        IWire<bool> set)
    {
        return new MemoryBit(input, set, output, this, _wireFactory);
    }
}