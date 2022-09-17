using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Constants;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;

namespace ComputerSimulator.Core.Parts;

public interface ICentralProcessingUnit : IPart
{
    IWire<bool> Bus1 { get; }

    #region Enables

    IWire<bool> IarEnable { get; }

    IWire<bool> RamEnable { get; }

    IWire<bool> AccEnable { get; }
    
    IWireGroup<bool> GeneralPurposeRegistersEnable { get; }

    #endregion
    
    IOp Op { get; }

    #region Sets

    IWire<bool> MarSet { get; }

    IWire<bool> AccSet { get; }
    
    IWire<bool> RamSet { get; }

    IWire<bool> TmpSet { get; }
    
    IWire<bool> IarSet { get; }
    
    IWire<bool> IrSet { get; }

    IWireGroup<bool> GeneralPurposeRegistersSet { get; }

    #endregion

    #region Inputs

    IWireGroup<bool> InstructionRegister { get; }

    #endregion
}

public class CentralProcessingUnit : PartsBase, ICentralProcessingUnit
{
    private readonly IComputerClock _clock;
    private readonly IStepper _stepper;
    private readonly IAnd2 _ramEnableAnd;
    private readonly IAnd2 _accEnableAnd;
    private readonly IOr2[] _generalPurposeRegistersEnableOr;
    private readonly IAnd2 _marSetAnd;
    private readonly IAnd2 _accSetAnd;
    private readonly IAnd2 _ramSetAnd;
    private readonly IAnd2 _tmpSetAnd;
    private readonly IAnd2 _iarEnableAnd;
    private readonly IAnd2 _irSetAnd;
    private readonly IAnd2 _iarSetAnd;
    private readonly IOr2 _marSetOr;
    private readonly IDecoder _registerBEnable2X4Decoder;
    private readonly IDecoder _registerAEnable2X4Decoder;
    private readonly IDecoder _registerBSet2X4Decoder;
    private readonly IAnd[] _registerAEnableDecoderAnd;
    private readonly IAnd[] _registerBEnableDecoderAnd;
    private readonly IAnd[] _registerBSetDecoderAnd;
    private readonly IAnd[] _aluOutputAnds;
    private readonly IAnd _irAnd;
    private readonly INot _irNot;
    private readonly IAnd2 _irStep4And;
    private readonly IAnd2 _irStep5And;
    private readonly IAnd _irStep6And;

    public CentralProcessingUnit(
        IWire<bool> iarEnable,
        IWire<bool> ramEnable,
        IWire<bool> accEnable,
        IWireGroup<bool> generalPurposeRegistersEnable,
        IOp op,
        IWire<bool> marSet,
        IWire<bool> accSet,
        IWire<bool> ramSet,
        IWire<bool> tmpSet,
        IWire<bool> iarSet, 
        IWire<bool> irSet,
        IWireGroup<bool> generalPurposeRegistersSet,
        IWireGroup<bool> instructionRegister,
        IComponentFactory componentFactory,
        IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        RamEnable = ramEnable;
        AccEnable = accEnable;
        GeneralPurposeRegistersEnable = generalPurposeRegistersEnable;
        Op = op;
        MarSet = marSet;
        AccSet = accSet;
        RamSet = ramSet;
        TmpSet = tmpSet;
        GeneralPurposeRegistersSet = generalPurposeRegistersSet;
        InstructionRegister = instructionRegister;
        IarEnable = iarEnable;
        IarSet = iarSet;
        IrSet = irSet;

        _clock = ComponentFactory.CreateComputerClock(
            WireFactory.CreateWire(false, "clk"),
            WireFactory.CreateWire(false, "clk-enable"),
            WireFactory.CreateWire(false, "clk-set"));

        _stepper = ComponentFactory.CreateStepper(_clock.Clk,
            WireFactory.CreateGroup(false, WireConstants.ExpectedNumberOfSteps, "step"));

        _aluOutputAnds = 3
            .InitArray<IAnd>()
            .Fill(i => ComponentFactory.CreateAnd(
                WireFactory.CreateGroup(
                    StepWire(5),
                    InstructionRegister[0],
                    InstructionRegister[i + 1]),
                Op[i]));

        // Might become a NAND but the diagram did them as separate
        _irAnd = ComponentFactory.CreateAnd(
            WireFactory.CreateGroup(
                InstructionRegister[1], InstructionRegister[2], InstructionRegister[3]),
            WireFactory.CreateWire(false, "ir-and-output"));
        _irNot = ComponentFactory.CreateNot(_irAnd.Output, WireFactory.CreateWire(false, "ir-not-output"));

        _irStep4And = ComponentFactory.CreateAnd2(
            StepWire(4),
            InstructionRegister[0],
            WireFactory.CreateWire(false, "ir-step-4-and-output"));
        
        _irStep5And = ComponentFactory.CreateAnd2(
            StepWire(5),
            InstructionRegister[0],
            WireFactory.CreateWire(false, "ir-step-5-and-output"));

        _irStep6And = ComponentFactory.CreateAnd(
            WireFactory.CreateGroup(
                StepWire(6),
                InstructionRegister[0],
                _irNot.Output),
            WireFactory.CreateWire(false, "ir-step-6-and-output"));
        
        // IR Decoders
        _registerAEnable2X4Decoder = ComponentFactory.CreateDecoder(WireFactory.CreateGroup(InstructionRegister[4], InstructionRegister[5]));
        var registerBAddress = WireFactory.CreateGroup(InstructionRegister[6], InstructionRegister[7]);
        _registerBEnable2X4Decoder = ComponentFactory.CreateDecoder(registerBAddress);
        _registerBSet2X4Decoder = ComponentFactory.CreateDecoder(registerBAddress);

        _registerAEnableDecoderAnd = WireConstants.ExpectedNumberOfGeneralPurposeRegisters
            .InitArray<IAnd>()
            .Fill(i => ComponentFactory.CreateAnd(
                WireFactory.CreateGroup(
                    _clock.ClkE,
                    _irStep5And.Output, // RegA
                    _registerAEnable2X4Decoder.Outputs[i]),
                WireFactory.CreateWire(false, "register-a-enable-decoder-and-output")));
        
        _registerBEnableDecoderAnd = WireConstants.ExpectedNumberOfGeneralPurposeRegisters
            .InitArray<IAnd>()
            .Fill(i => ComponentFactory.CreateAnd(
                WireFactory.CreateGroup(
                    _clock.ClkE,
                    _irStep4And.Output, // RegB
                    _registerBEnable2X4Decoder.Outputs[i]),
                WireFactory.CreateWire(false, "register-b-enable-decoder-and-output")));
        
        _registerBSetDecoderAnd = WireConstants.ExpectedNumberOfGeneralPurposeRegisters
            .InitArray<IAnd>()
            .Fill(i => ComponentFactory.CreateAnd(
                WireFactory.CreateGroup(
                    _clock.ClkS,
                    _irStep6And.Output, // RegB
                    _registerBSet2X4Decoder.Outputs[i]),
                GeneralPurposeRegistersSet[i]));

        // Enables
        _iarEnableAnd = ComponentFactory.CreateAnd2(_clock.ClkE, StepWire(1), IarEnable);
        _ramEnableAnd = ComponentFactory.CreateAnd2(_clock.ClkE, StepWire(2), RamEnable);

        // StepWire(3) might be needed?
        _accEnableAnd = ComponentFactory.CreateAnd2(_clock.ClkE, _irStep6And.Output, AccEnable);

        _generalPurposeRegistersEnableOr = WireConstants.ExpectedNumberOfGeneralPurposeRegisters
            .InitArray<IOr2>()
            .Fill(i =>
                ComponentFactory.CreateOr2(
                    _registerAEnableDecoderAnd[i].Output,
                    _registerBEnableDecoderAnd[i].Output,
                    GeneralPurposeRegistersEnable[i]));

        // Sets
        _irSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, StepWire(2), IrSet);

        _marSetOr = ComponentFactory.CreateOr2(StepWire(1), StepWire(4), WireFactory.CreateWire(false, "mar-set-or"));
        _marSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, _marSetOr.Output, MarSet);
        
        _iarSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, StepWire(3), IarSet);

        // StepWire(1) might be needed?
        _accSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, _irStep5And.Output, AccSet);

        _ramSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, StepWire(5), RamSet);
        _tmpSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, _irStep4And.Output, TmpSet);
    }

    public IWire<bool> Bus1 => StepWire(1);

    public IWire<bool> IarEnable { get; }

    public IWire<bool> RamEnable { get; }

    public IWire<bool> AccEnable { get; }

    public IWireGroup<bool> GeneralPurposeRegistersEnable { get; }

    public IOp Op { get; }

    public IWire<bool> MarSet { get; }

    public IWire<bool> AccSet { get; }

    public IWire<bool> RamSet { get; }

    public IWire<bool> TmpSet { get; }

    public IWire<bool> IarSet { get; }

    public IWire<bool> IrSet { get; }

    public IWireGroup<bool> GeneralPurposeRegistersSet { get; }

    public IWireGroup<bool> InstructionRegister { get; }

    public void Update()
    {
        _clock.Update();
        _stepper.Update();
        
        _aluOutputAnds.Update();
        _irAnd.Update();
        _irNot.Update();
        
        _irStep4And.Update();
        _irStep5And.Update();
        _irStep6And.Update();
        
        _registerAEnable2X4Decoder.Update();
        _registerAEnableDecoderAnd.Update();
        
        _registerBEnable2X4Decoder.Update();
        _registerBEnableDecoderAnd.Update();
        
        _registerBSet2X4Decoder.Update();
        _registerBSetDecoderAnd.Update();

        _iarEnableAnd.Update();
        _ramEnableAnd.Update();

        _accEnableAnd.Update();

        _generalPurposeRegistersEnableOr.Update();
        
        _irSetAnd.Update();
        
        _marSetOr.Update();
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