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
    private readonly INot _irNot;
    private readonly IAnd2 _step4Ir0And;
    private readonly IAnd2 _step5Ir0And;
    private readonly IAnd _step6Ir0IrNotAnd;

    public CentralProcessingUnit(
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

        _aluAnds = 3
            .InitArray<IAnd>()
            .Fill(i => 
                ComponentFactory.CreateAnd(WireFactory.CreateGroup(InstructionRegister[0], StepWire(5), InstructionRegister[i + 1]),
                    WireFactory.CreateWire(false, $"{_aluAnds}-output[{i}]")));

        _irAnd = ComponentFactory.CreateAnd(
            WireFactory.CreateGroup(InstructionRegister[1], InstructionRegister[2], InstructionRegister[3]),
            WireFactory.CreateWire(false, $"{_irAnd}-output"));
        _irNot = ComponentFactory.CreateNot(
            _irAnd.Output,
            WireFactory.CreateWire(false, $"{nameof(_irNot)}-output"));

        _step4Ir0And = ComponentFactory.CreateAnd2(StepWire(4), InstructionRegister[0],
            WireFactory.CreateWire(false, $"{nameof(_step4Ir0And)}-output"));
        _step5Ir0And = ComponentFactory.CreateAnd2(StepWire(5), InstructionRegister[0],
            WireFactory.CreateWire(false, $"{nameof(_step5Ir0And)}-output"));
        _step6Ir0IrNotAnd = ComponentFactory.CreateAnd(
            WireFactory.CreateGroup(StepWire(6), InstructionRegister[0], _irNot.Output),
            WireFactory.CreateWire(false, $"{nameof(_step6Ir0IrNotAnd)}-output"));

        _accEnableOr = ComponentFactory.CreateOr(WireFactory.CreateGroup(StepWire(3), _step6Ir0IrNotAnd.Output),
            WireFactory.CreateWire(false, nameof(_accEnableOr)));
        
        _iarEnableAnd = ComponentFactory.CreateAnd2(_clock.ClkE, StepWire(1), Iar.Enable);
        _ramEnableAnd = ComponentFactory.CreateAnd2(_clock.ClkE, StepWire(2), Ram.Enable);
        _accEnableAnd = ComponentFactory.CreateAnd2(_clock.ClkE, _accEnableOr.Output, Acc.Enable);

        _regAEnableOr = ComponentFactory.CreateOr(
            WireFactory.CreateGroup(new [] {_step5Ir0And.Output}),
            WireFactory.CreateWire(false, nameof(_regAEnableOr)));
        _regBEnableOr = ComponentFactory.CreateOr(
            WireFactory.CreateGroup(new [] {_step4Ir0And.Output}),
            WireFactory.CreateWire(false, nameof(_regBEnableOr)));
        _regBSetOr = ComponentFactory.CreateOr(
            WireFactory.CreateGroup(new [] {_step6Ir0IrNotAnd.Output}),
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

        _marSetOr = ComponentFactory.CreateOr(WireFactory.CreateGroup(StepWire(1), StepWire(4)),
            WireFactory.CreateWire(false, nameof(_marSetOr)));
        _accSetOr = ComponentFactory.CreateOr(WireFactory.CreateGroup(StepWire(1), _step5Ir0And.Output),
            WireFactory.CreateWire(false, nameof(_accSetOr)));

        _irSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, StepWire(2), IrSet);
        _marSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, StepWire(4), MarSet);
        _iarSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, StepWire(3), Iar.Set);
        _accSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, _accSetOr.Output, Acc.Set);
        _ramSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, StepWire(5), Ram.Set);
        _tmpSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, _step4Ir0And.Output, TmpSet);
        
    }
    
    public IWire<bool> Bus1 => StepWire(1);

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
        
        _aluAnds.Update();
        _irAnd.Update();
        _irNot.Update();
        
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
        
        _accEnableOr.Update();
        
        _iarEnableAnd.Update();
        _ramEnableAnd.Update();
        _accEnableAnd.Update();

        _marSetOr.Update();
        _accSetOr.Update();
        
        _irSetAnd.Update();
        _marSetAnd.Update();
        _iarSetAnd.Update();
        _accSetAnd.Update();
        _ramSetAnd.Update();
        _tmpSetAnd.Update();
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