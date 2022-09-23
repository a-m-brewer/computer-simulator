using ComputerSimulator.Core.Constants;
using ComputerSimulator.Core.Factories;

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