using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Enums;
using ComputerSimulator.Core.Exceptions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;

namespace ComputerSimulator.Core.Parts;

public interface IArithmeticLogicUnit : IPart
{
    // Inputs
    IWireGroup<bool> InputsA { get; }

    IWireGroup<bool> InputsB { get; }
    
    IWire<bool> CarryIn { get; }
    
    /// <summary>
    /// 000 | ADD  | Add
    /// 001 | SHR  | Shift Right
    /// 010 | SHL  | Shift Left
    /// 011 | NOT  | Not
    /// 100 | AND  | And
    /// 101 | OR   | Or
    /// 110 | XOR  | Exclusive Or
    /// 111 | CMP  | Compare
    /// </summary>
    IOp Op { get; }
    
    // Outputs
    
    // C in diagram
    IWireGroup<bool> Outputs { get; }
    
    IWire<bool> CarryOut { get; }
    
    IWire<bool> ALarger { get; }
    
    IWire<bool> Equal { get; }
    
    IWire<bool> IsZero { get; }
}

public class ArithmeticLogicUnit : PartsBase, IArithmeticLogicUnit
{
    private readonly IDecoder _decoder3X8;
    
    // Is Zero
    private readonly IIsZeroChecker _isZeroChecker;
    
    // Add
    private readonly IWordAdder _add;
    private readonly IAnd2 _addAnd;
    private readonly IEnabler _addEnabler;
    
    // Shift Right
    private readonly IShifter _shiftRight;
    private readonly IAnd2 _shiftRightAnd;
    private readonly IEnabler _shiftRightEnabler;
    
    // Shift Left
    private readonly IShifter _shiftLeft;
    private readonly IAnd2 _shiftLeftAnd;
    private readonly IEnabler _shiftLeftEnabler;
    
    // Noter
    private readonly INoter _noter;
    private readonly IEnabler _noterEnabler;
    
    // Ander
    private readonly IAnder _ander;
    private readonly IEnabler _anderEnabler;
    
    // Orer
    private readonly IOrer _orer;
    private readonly IEnabler _orerEnabler;
    
    // Xorer
    private readonly IXOrer _xorer;
    private readonly IEnabler _xorerEnabler;
    
    // Word Comparator
    private readonly IWordComparator _wordComparator;
    private readonly IEnabler _wordComparatorEnabler;

    public ArithmeticLogicUnit(
        IWireGroup<bool> inputsA,
        IWireGroup<bool> inputsB,
        IWire<bool> carryIn,
        IOp op,
        IWireGroup<bool> outputs,
        IWire<bool> carryOut,
        IWire<bool> aLarger,
        IWire<bool> equal,
        IWire<bool> isZero,
        IComponentFactory componentFactory,
        IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        InputsA = inputsA;
        InputsB = inputsB;
        CarryIn = carryIn;
        Op = op;

        if (Op.Count != 3)
        {
            throw new ComputerSimulatorException($"{nameof(Op)} needs to be of length 3");
        }
        
        Outputs = outputs;
        CarryOut = carryOut;
        ALarger = aLarger;
        Equal = equal;
        IsZero = isZero;

        _decoder3X8 = ComponentFactory.CreateDecoder(Op);

        // Add
        _add = ComponentFactory.CreateWordAdder(InputsA, InputsB, CarryIn, WireFactory.CreateWire(false), WireFactory.CreateGroup(false));
        _addAnd = ComponentFactory.CreateAnd2(_add.CarryOut, _decoder3X8.Outputs[OpCode.Add], CarryOut);
        _addEnabler = ComponentFactory.CreateEnabler(_decoder3X8.Outputs[OpCode.Add], _add.Sum, Outputs);

        // Shift Right
        _shiftRight = ComponentFactory.CreateRightShifter(CarryIn, WireFactory.CreateWire(false), InputsA, WireFactory.CreateGroup(false));
        _shiftRightAnd = ComponentFactory.CreateAnd2(_shiftRight.ShiftOut, _decoder3X8.Outputs[OpCode.Shr], CarryOut);
        _shiftRightEnabler = ComponentFactory.CreateEnabler(_decoder3X8.Outputs[OpCode.Shr], _shiftRight.Output, Outputs);
        
        // Shift Left
        _shiftLeft = ComponentFactory.CreateLeftShifter(CarryIn, WireFactory.CreateWire(false), InputsA, WireFactory.CreateGroup(false));
        _shiftLeftAnd = ComponentFactory.CreateAnd2(_shiftLeft.ShiftOut, _decoder3X8.Outputs[OpCode.Shl], CarryOut);
        _shiftLeftEnabler = ComponentFactory.CreateEnabler(_decoder3X8.Outputs[OpCode.Shl], _shiftLeft.Output, Outputs);
        
        // Noter
        _noter = ComponentFactory.CreateNoter(InputsA, WireFactory.CreateGroup(false));
        _noterEnabler = ComponentFactory.CreateEnabler(_decoder3X8.Outputs[OpCode.Not], _noter.Outputs, Outputs);
        
        // And
        _ander = ComponentFactory.CreateAnder(InputsA, InputsB, WireFactory.CreateGroup(false));
        _anderEnabler = ComponentFactory.CreateEnabler(_decoder3X8.Outputs[OpCode.And], _ander.Outputs, Outputs);
        
        // Orer
        _orer = ComponentFactory.CreateOrer(InputsA, InputsB, WireFactory.CreateGroup(false));
        _orerEnabler = ComponentFactory.CreateEnabler(_decoder3X8.Outputs[OpCode.Or], _orer.Outputs, Outputs);
        
        // Xorer
        _xorer = ComponentFactory.CreateXOrer(InputsA, InputsB, WireFactory.CreateGroup(false));
        _xorerEnabler = ComponentFactory.CreateEnabler(_decoder3X8.Outputs[OpCode.XOr], _xorer.Outputs, Outputs);
        
        // Word Comparator
        _wordComparator = ComponentFactory.CreateWordComparator(
            InputsA, InputsB, WireFactory.PowerWire, WireFactory.CreateWire(false), WireFactory.CreateGroup(false), 
            Equal, ALarger);
        _wordComparatorEnabler = ComponentFactory.CreateEnabler(_decoder3X8.Outputs[OpCode.Cmp], _wordComparator.UnEqual, Outputs);

        // Is Zero
        _isZeroChecker = ComponentFactory.CreateIsZeroChecker(Outputs, IsZero);
    }

    public IWireGroup<bool> InputsA { get; }
    public IWireGroup<bool> InputsB { get; }
    public IWire<bool> CarryIn { get; }
    public IOp Op { get; }
    public IWireGroup<bool> Outputs { get; }
    public IWire<bool> CarryOut { get; }
    public IWire<bool> ALarger { get; }
    public IWire<bool> Equal { get; }
    public IWire<bool> IsZero { get; }
    
    public void Update()
    {
        _decoder3X8.Update();

        switch ((OpCode)_decoder3X8.EnabledIndex)
        {
            case OpCode.Add:
                _add.Update();
                _addAnd.Update();
                _addEnabler.Update();
                break;
            case OpCode.Shr:
                _shiftRight.Update();
                _shiftRightAnd.Update();
                _shiftRightEnabler.Update();
                break;
            case OpCode.Shl:
                _shiftLeft.Update();
                _shiftLeftAnd.Update();
                _shiftLeftEnabler.Update();
                break;
            case OpCode.Not:
                _noter.Update();
                _noterEnabler.Update();
                break;
            case OpCode.And:
                _ander.Update();
                _anderEnabler.Update();
                break;
            case OpCode.Or:
                _orer.Update();
                _orerEnabler.Update();
                break;
            case OpCode.XOr:
                _xorer.Update();
                _xorerEnabler.Update();
                break;
            case OpCode.Cmp:
                _wordComparator.Update();
                _wordComparatorEnabler.Update();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        _isZeroChecker.Update();
    }
}