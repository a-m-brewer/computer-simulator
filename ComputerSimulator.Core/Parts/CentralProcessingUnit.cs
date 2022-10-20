using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Constants;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;

namespace ComputerSimulator.Core.Parts;

public interface ICentralProcessingUnit : IPart
{
    IWire<bool> Bus1 { get; }

    ISetEnableWire<bool> Iar { get; }

    ISetEnableWire<bool> Ram { get; }

    ISetEnableWire<bool> Acc { get; }

    ISetEnableWire<bool> IoClk { get; }

    ISetEnableWireGroup<bool> GeneralPurposeRegisters { get; }

    IOp Op { get; }

    #region Sets

    IWire<bool> MarSet { get; }

    IWire<bool> TmpSet { get; }

    IWire<bool> IrSet { get; }

    IWire<bool> FlagsSet { get; }

    #endregion

    IWire<bool> CarryInTmp { get; }

    IWire<bool> IoInputOutput { get; }

    IWire<bool> IoDataAddress { get; }

    #region Inputs

    IWireGroup<bool> InstructionRegister { get; }

    ICaez<bool> Caez { get; }

    #endregion
}

public class CentralProcessingUnit : PartsBase, ICentralProcessingUnit
{
    private readonly IComputerClock _clock;
    private readonly IStepper _stepper;
    private readonly IAnd2 _tmpSetAnd;
    private readonly IAnd2 _accSetAnd;
    private readonly IAnd2 _accEnableAnd;
    private readonly IAnd2 _marSetAnd;
    private readonly IAnd2 _ramSetAnd;
    private readonly IOr _accSetOr;
    private readonly IOr _marSetOr;
    private readonly IAnd2 _iarEnableAnd;
    private readonly IAnd2 _irSetAnd;
    private readonly IAnd2 _ramEnableAnd;
    private readonly IAnd2 _iarSetAnd;
    private readonly IOr _accEnableOr;
    private readonly IDecoder _regAEnable2X4;
    private readonly IDecoder _regBEnable2X4;
    private readonly IDecoder _regBSet2X4;
    private readonly IAnd[] _gprAEnableDecoderAnd;
    private readonly IAnd[] _gprBEnableDecoderAnd;
    private readonly IAnd[] _gprBSetDecoderAnd;
    private readonly IOr2[] _gprEnableOr;
    private readonly IOr _regAEnableOr;
    private readonly IOr _regBEnableOr;
    private readonly IOr _regBSetOr;
    private readonly IAnd[] _aluAnds;
    private readonly IAnd _irAnd;
    private readonly INot _irAndNot;
    private readonly IAnd2 _step4Ir0And;
    private readonly IAnd2 _step5Ir0And;
    private readonly IAnd _step6Ir0IrNotAnd;
    private readonly IDecoder _ir3X8Decoder;
    private readonly INot _irNot;
    private readonly IAnd2[] _ir3X8DecoderAnds;
    private readonly ISingleOutput[] _step4AndIr3X8DecoderAnds;
    private readonly Dictionary<int, ISingleOutput> _step5AndIr3X8DecoderAnds;
    private readonly IOr _ramEnableOr;
    private readonly IOr _bus1Or;
    private readonly IOr _iarEnableOr;
    private readonly Dictionary<int, ISingleOutput> _step6AndIr3X8DecoderAnds;
    private readonly IOr _iarSetOr;
    private readonly IAnd2 _cAnd;
    private readonly IAnd2 _aAnd;
    private readonly IAnd2 _eAnd;
    private readonly IAnd2 _zAnd;
    private readonly IOr _caezOr;
    private readonly IAnd2 _carryInTmpAnd;
    private readonly IOr2 _flagsOr;
    private readonly IAnd2 _flagsAnd;

    public CentralProcessingUnit(
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
        ICaez<bool> caez,
        IComponentFactory componentFactory, IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        Bus1 = bus1;
        Iar = iar;
        Ram = ram;
        Acc = acc;
        IoClk = ioClk;
        GeneralPurposeRegisters = generalPurposeRegisters;
        Op = op;
        MarSet = marSet;
        TmpSet = tmpSet;
        IrSet = irSet;
        FlagsSet = flagsSet;
        CarryInTmp = carryInTmp;
        InstructionRegister = instructionRegister;
        Caez = caez;
        IoInputOutput = ioInputOutput;
        IoDataAddress = ioDataAddress;

        _clock = ComponentFactory.CreateComputerClock(
            WireFactory.CreateWire(false, "clk"),
            WireFactory.CreateWire(false, "clk-enable"),
            WireFactory.CreateWire(false, "clk-set"));

        _stepper = ComponentFactory.CreateStepper(_clock.Clk,
            WireFactory.CreateGroup(false, WireConstants.ExpectedNumberOfSteps, "step"));
        
        // CAEZ
        _cAnd = ComponentFactory.CreateAnd2(Caez.C, InstructionRegister[4], WireFactory.CreateWire(false, $"{nameof(_cAnd)}-output"));
        _aAnd = ComponentFactory.CreateAnd2(Caez.A, InstructionRegister[5], WireFactory.CreateWire(false, $"{nameof(_aAnd)}-output"));
        _eAnd = ComponentFactory.CreateAnd2(Caez.E, InstructionRegister[6], WireFactory.CreateWire(false, $"{nameof(_eAnd)}-output"));
        _zAnd = ComponentFactory.CreateAnd2(Caez.Z, InstructionRegister[7], WireFactory.CreateWire(false, $"{nameof(_zAnd)}-output"));

        _caezOr = ComponentFactory.CreateOr(
            WireFactory.CreateGroup(_cAnd.Output, _aAnd.Output, _eAnd.Output, _zAnd.Output),
            WireFactory.CreateWire(false, $"{nameof(_caezOr)}-output"));

        _ir3X8Decoder = ComponentFactory.CreateDecoder(
            WireFactory.CreateGroup(InstructionRegister[1], InstructionRegister[2], InstructionRegister[3]));
        _irNot = ComponentFactory.CreateNot(InstructionRegister[0],
            WireFactory.CreateWire(false, $"{nameof(_irNot)}-output"));
        _ir3X8DecoderAnds = _ir3X8Decoder.OutputSize
            .InitArray<IAnd2>()
            .Fill(i =>
                ComponentFactory.CreateAnd2(
                    _irNot.Output,
                    _ir3X8Decoder.Outputs[i],
                    WireFactory.CreateWire(false, $"{nameof(_ir3X8DecoderAnds)}[{i}]-output")));

        _step4AndIr3X8DecoderAnds = _ir3X8DecoderAnds.Length
            .InitArray<ISingleOutput>()
            .Fill(i =>
                i != _ir3X8DecoderAnds.Length - 1
                    ? ComponentFactory.CreateAnd2(StepWire(4), _ir3X8DecoderAnds[i].Output,
                        WireFactory.CreateWire(false, $"{nameof(_step4AndIr3X8DecoderAnds)}[{i}]-output"))
                    : ComponentFactory.CreateAnd(
                        WireFactory.CreateGroup(StepWire(4), _ir3X8DecoderAnds[i].Output, InstructionRegister[4]),
                        WireFactory.CreateWire(false, $"{nameof(_step4AndIr3X8DecoderAnds)}[{i}]-output")));

        _step5AndIr3X8DecoderAnds = new Dictionary<int, ISingleOutput>
        {
            {
                0,
                ComponentFactory.CreateAnd2(StepWire(5), _ir3X8DecoderAnds[0].Output,
                    WireFactory.CreateWire(false, $"{nameof(_step5AndIr3X8DecoderAnds)}[0]-output"))
            },
            {
                1,
                ComponentFactory.CreateAnd2(StepWire(5), _ir3X8DecoderAnds[1].Output,
                    WireFactory.CreateWire(false, $"{nameof(_step5AndIr3X8DecoderAnds)}[1]-output"))
            },
            {
                2,
                ComponentFactory.CreateAnd2(StepWire(5), _ir3X8DecoderAnds[2].Output,
                    WireFactory.CreateWire(false, $"{nameof(_step5AndIr3X8DecoderAnds)}[2]-output"))
            },
            {
                4,
                ComponentFactory.CreateAnd2(StepWire(5), _ir3X8DecoderAnds[4].Output,
                    WireFactory.CreateWire(false, $"{nameof(_step5AndIr3X8DecoderAnds)}[4]-output"))
            },
            {
                5,
                ComponentFactory.CreateAnd2(StepWire(5), _ir3X8DecoderAnds[5].Output,
                    WireFactory.CreateWire(false, $"{nameof(_step5AndIr3X8DecoderAnds)}[5]-output"))
            }
        };

        _step6AndIr3X8DecoderAnds = new Dictionary<int, ISingleOutput>
        {
            {
                2,
                ComponentFactory.CreateAnd2(StepWire(6), _ir3X8DecoderAnds[2].Output,
                    WireFactory.CreateWire(false, $"{nameof(_step6AndIr3X8DecoderAnds)}[2]-output"))
            },
            {
                5,
                ComponentFactory.CreateAnd(
                    WireFactory.CreateGroup(StepWire(6), _ir3X8DecoderAnds[5].Output, _caezOr.Output),
                    WireFactory.CreateWire(false, $"{nameof(_step6AndIr3X8DecoderAnds)}[5]-output"))
            }
        };

        _aluAnds = 3
            .InitArray<IAnd>()
            .Fill(i =>
                ComponentFactory.CreateAnd(
                    WireFactory.CreateGroup(InstructionRegister[0], StepWire(5), InstructionRegister[i + 1]),
                    WireFactory.CreateWire(false, $"{nameof(_aluAnds)}-output[{i}]")));

        _irAnd = ComponentFactory.CreateAnd(
            WireFactory.CreateGroup(InstructionRegister[1], InstructionRegister[2], InstructionRegister[3]),
            WireFactory.CreateWire(false, $"{_irAnd}-output"));
        _irAndNot = ComponentFactory.CreateNot(
            _irAnd.Output,
            WireFactory.CreateWire(false, $"{nameof(_irAndNot)}-output"));

        _step4Ir0And = ComponentFactory.CreateAnd2(StepWire(4), InstructionRegister[0],
            WireFactory.CreateWire(false, $"{nameof(_step4Ir0And)}-output"));
        _step5Ir0And = ComponentFactory.CreateAnd2(StepWire(5), InstructionRegister[0],
            WireFactory.CreateWire(false, $"{nameof(_step5Ir0And)}-output"));
        _step6Ir0IrNotAnd = ComponentFactory.CreateAnd(
            WireFactory.CreateGroup(StepWire(6), InstructionRegister[0], _irAndNot.Output),
            WireFactory.CreateWire(false, $"{nameof(_step6Ir0IrNotAnd)}-output"));

        _iarEnableOr =
            ComponentFactory.CreateOr(
                WireFactory.CreateGroup(
                    StepWire(1),
                    _step4AndIr3X8DecoderAnds[2].Output,
                    _step4AndIr3X8DecoderAnds[4].Output,
                    _step4AndIr3X8DecoderAnds[5].Output),
                WireFactory.CreateWire(false, $"{nameof(_iarEnableOr)}-output"));
        _ramEnableOr = ComponentFactory.CreateOr(
            WireFactory.CreateGroup(
                StepWire(2),
                _step5AndIr3X8DecoderAnds[0].Output,
                _step5AndIr3X8DecoderAnds[2].Output,
                _step5AndIr3X8DecoderAnds[4].Output,
                _step6AndIr3X8DecoderAnds[5].Output),
            WireFactory.CreateWire(false, $"{nameof(_ramEnableOr)}-output"));
        _accEnableOr = ComponentFactory.CreateOr(
            WireFactory.CreateGroup(
                StepWire(3),
                _step5AndIr3X8DecoderAnds[5].Output,
                _step6Ir0IrNotAnd.Output,
                _step6AndIr3X8DecoderAnds[2].Output),
            WireFactory.CreateWire(false, nameof(_accEnableOr)));

        _bus1Or = ComponentFactory.CreateOr(
            WireFactory.CreateGroup(
                StepWire(1),
                _step4AndIr3X8DecoderAnds[2].Output,
                _step4AndIr3X8DecoderAnds[5].Output,
                _step4AndIr3X8DecoderAnds[6].Output),
            Bus1);
        _iarEnableAnd = ComponentFactory.CreateAnd2(_clock.ClkE, _iarEnableOr.Output, Iar.Enable);
        _ramEnableAnd = ComponentFactory.CreateAnd2(_clock.ClkE, _ramEnableOr.Output, Ram.Enable);
        _accEnableAnd = ComponentFactory.CreateAnd2(_clock.ClkE, _accEnableOr.Output, Acc.Enable);

        _regAEnableOr = ComponentFactory.CreateOr(
            WireFactory.CreateGroup(_step5Ir0And.Output, _step4AndIr3X8DecoderAnds[0].Output,
                _step4AndIr3X8DecoderAnds[1].Output),
            WireFactory.CreateWire(false, nameof(_regAEnableOr)));
        _regBEnableOr = ComponentFactory.CreateOr(
            WireFactory.CreateGroup(
                _step4Ir0And.Output,
                _step5AndIr3X8DecoderAnds[1].Output,
                _step4AndIr3X8DecoderAnds[3].Output),
            WireFactory.CreateWire(false, nameof(_regBEnableOr)));
        _regBSetOr = ComponentFactory.CreateOr(
            WireFactory.CreateGroup(
                _step6Ir0IrNotAnd.Output,
                _step5AndIr3X8DecoderAnds[0].Output,
                _step5AndIr3X8DecoderAnds[2].Output),
            WireFactory.CreateWire(false, nameof(_regBSetOr)));

        _regAEnable2X4 = ComponentFactory
            .CreateDecoder(WireFactory.CreateGroup(InstructionRegister[4], InstructionRegister[5]));

        var regBWireGroup = WireFactory.CreateGroup(InstructionRegister[6], InstructionRegister[7]);

        _regBEnable2X4 = ComponentFactory
            .CreateDecoder(regBWireGroup);
        _regBSet2X4 = ComponentFactory
            .CreateDecoder(regBWireGroup);

        _gprAEnableDecoderAnd = _regAEnable2X4.OutputSize
            .InitArray<IAnd>()
            .Fill(i => ComponentFactory.CreateAnd(
                WireFactory.CreateGroup(_clock.ClkE, _regAEnableOr.Output, _regAEnable2X4.Outputs[i]),
                WireFactory.CreateWire(false, $"{nameof(_gprAEnableDecoderAnd)}[{i}]")));

        _gprBEnableDecoderAnd = _regBEnable2X4.OutputSize
            .InitArray<IAnd>()
            .Fill(i => ComponentFactory.CreateAnd(
                WireFactory.CreateGroup(_clock.ClkE, _regBEnableOr.Output, _regBEnable2X4.Outputs[i]),
                WireFactory.CreateWire(false, $"{nameof(_gprBEnableDecoderAnd)}[{i}]")));

        _gprBSetDecoderAnd = _regBSet2X4.OutputSize
            .InitArray<IAnd>()
            .Fill(i => ComponentFactory.CreateAnd(
                WireFactory.CreateGroup(_clock.ClkS, _regBSetOr.Output, _regBSet2X4.Outputs[i]),
                WireFactory.CreateWire(false, $"{nameof(_gprBSetDecoderAnd)}[{i}]")));

        _gprEnableOr = 4
            .InitArray<IOr2>()
            .Fill(i =>
                ComponentFactory.CreateOr2(
                    _gprAEnableDecoderAnd[i].Output,
                    _gprBEnableDecoderAnd[i].Output,
                    GeneralPurposeRegisters[i].Enable));

        _marSetOr = ComponentFactory.CreateOr(
            WireFactory.CreateGroup(
                StepWire(1),
                _step4AndIr3X8DecoderAnds[0].Output,
                _step4AndIr3X8DecoderAnds[1].Output,
                _step4AndIr3X8DecoderAnds[2].Output,
                _step4AndIr3X8DecoderAnds[4].Output,
                _step4AndIr3X8DecoderAnds[5].Output),
            WireFactory.CreateWire(false, nameof(_marSetOr)));
        _iarSetOr = ComponentFactory.CreateOr(
            WireFactory.CreateGroup(
                StepWire(3),
                _step4AndIr3X8DecoderAnds[3].Output,
                _step5AndIr3X8DecoderAnds[4].Output,
                _step5AndIr3X8DecoderAnds[5].Output,
                _step6AndIr3X8DecoderAnds[2].Output,
                _step6AndIr3X8DecoderAnds[5].Output),
            WireFactory.CreateWire(false, $"{nameof(_iarSetOr)}-output"));
        _accSetOr = ComponentFactory.CreateOr(
            WireFactory.CreateGroup(
                StepWire(1),
                _step5Ir0And.Output,
                _step4AndIr3X8DecoderAnds[2].Output,
                _step4AndIr3X8DecoderAnds[5].Output),
            WireFactory.CreateWire(false, nameof(_accSetOr)));
        _flagsOr = ComponentFactory.CreateOr2(
            _step4AndIr3X8DecoderAnds[6].Output,
            WireFactory.OffWire,
            WireFactory.CreateWire(false, $"{nameof(_flagsOr)}-output"));

        _irSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, StepWire(2), IrSet);
        _marSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, _marSetOr.Output, MarSet);
        _iarSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, _iarSetOr.Output, Iar.Set);
        _accSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, _accSetOr.Output, Acc.Set);
        _ramSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, _step5AndIr3X8DecoderAnds[1].Output, Ram.Set);
        _tmpSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, _step4Ir0And.Output, TmpSet);
        _carryInTmpAnd = ComponentFactory.CreateAnd2(_clock.ClkS, _step4Ir0And.Output, CarryInTmp);
        _flagsAnd = ComponentFactory.CreateAnd2(_clock.ClkS, _flagsOr.Output, FlagsSet);
    }

    public IWire<bool> Bus1 { get; }

    public ISetEnableWire<bool> Iar { get; }

    public ISetEnableWire<bool> Ram { get; }

    public ISetEnableWire<bool> Acc { get; }
    public ISetEnableWire<bool> IoClk { get; }

    public ISetEnableWireGroup<bool> GeneralPurposeRegisters { get; }
    public IOp Op { get; }
    public IWire<bool> MarSet { get; }
    public IWire<bool> TmpSet { get; }
    public IWire<bool> IrSet { get; }
    public IWire<bool> FlagsSet { get; }
    public IWire<bool> CarryInTmp { get; }

    public IWire<bool> IoInputOutput { get; }

    public IWire<bool> IoDataAddress { get; }

    public IWireGroup<bool> InstructionRegister { get; }

    public ICaez<bool> Caez { get; }

    public void Update()
    {
        _clock.Update();
        _stepper.Update();

        _cAnd.Update();
        _aAnd.Update();
        _eAnd.Update();
        _zAnd.Update();
        _caezOr.Update();
        
        _ir3X8Decoder.Update();
        _irNot.Update();
        _ir3X8DecoderAnds.Update();
        _step4AndIr3X8DecoderAnds.Update();
        _step5AndIr3X8DecoderAnds.Update();
        _step6AndIr3X8DecoderAnds.Update();

        _aluAnds.Update();
        _irAnd.Update();
        _irAndNot.Update();

        _step4Ir0And.Update();
        _step5Ir0And.Update();
        _step6Ir0IrNotAnd.Update();

        _regAEnableOr.Update();
        _regBEnableOr.Update();
        _regBSetOr.Update();

        _regAEnable2X4.Update();
        _regBEnable2X4.Update();
        _regBSet2X4.Update();

        _gprAEnableDecoderAnd.Update();
        _gprBEnableDecoderAnd.Update();
        _gprBSetDecoderAnd.Update();

        _gprEnableOr.Update();

        _iarEnableOr.Update();
        _ramEnableOr.Update();
        _accEnableOr.Update();

        _bus1Or.Update();
        _iarEnableAnd.Update();
        _ramEnableAnd.Update();
        _accEnableAnd.Update();
        
        _marSetOr.Update();
        _iarSetOr.Update();
        _accSetOr.Update();
        _flagsOr.Update();

        _irSetAnd.Update();
        _marSetAnd.Update();
        _iarSetAnd.Update();
        _accSetAnd.Update();
        _ramSetAnd.Update();
        _tmpSetAnd.Update();
        _carryInTmpAnd.Update();
        _flagsAnd.Update();
    }

    /// <summary>
    /// Just an ease of use method so I can write code like the diagram
    /// Diagram is 1 indexed 
    /// </summary>
    private IWire<bool> StepWire(int step)
    {
        return _stepper.Steps[step - 1];
    }
}