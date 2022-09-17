using ComputerSimulator.Core.Constants;
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
    private readonly IAnd2[] _generalPurposeRegistersEnableAnd;
    private readonly IAnd2 _marSetAnd;
    private readonly IAnd2 _accSetAnd;
    private readonly IAnd2 _ramSetAnd;
    private readonly IAnd2 _tmpSetAnd;
    private readonly IAnd2[] _generalPurposeRegistersSetAnd;
    private readonly IAnd2 _iarEnableAnd;
    private readonly IAnd2 _irSetAnd;
    private readonly IAnd2 _iarSetAnd;
    private readonly IOr2 _marSetOr;
    private readonly IOr2 _accSetOr;
    private readonly IOr2 _accEnableOr;

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
        
        // Enables
        _iarEnableAnd = ComponentFactory.CreateAnd2(_clock.ClkE, StepWire(1), IarEnable);
        _ramEnableAnd = ComponentFactory.CreateAnd2(_clock.ClkE, StepWire(2), RamEnable);

        _accEnableOr = ComponentFactory.CreateOr2(StepWire(3), StepWire(6), WireFactory.CreateWire(false, "acc-enable-or"));
        _accEnableAnd = ComponentFactory.CreateAnd2(_clock.ClkE, _accEnableOr.Output, AccEnable);
        
        _generalPurposeRegistersEnableAnd = new IAnd2[WireConstants.ExpectedNumberOfGeneralPurposeRegisters];
        for (var i = 0; i < WireConstants.ExpectedNumberOfGeneralPurposeRegisters; i++)
        {
            _generalPurposeRegistersEnableAnd[i] = ComponentFactory.CreateAnd2(
                _clock.ClkE,
                i switch
                {
                    0 => StepWire(5),
                    1 => StepWire(4),
                    2 => StepWire(4),
                    _ => WireFactory.OffWire
                }, 
                GeneralPurposeRegistersEnable[i]);
        }
        
        // Sets
        _irSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, StepWire(2), IrSet);

        _marSetOr = ComponentFactory.CreateOr2(StepWire(1), StepWire(4), WireFactory.CreateWire(false, "mar-set-or"));
        _marSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, _marSetOr.Output, MarSet);
        
        _iarSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, StepWire(3), IarSet);

        _accSetOr = ComponentFactory.CreateOr2(StepWire(1), StepWire(5), WireFactory.CreateWire(false, "acc-set-or"));
        _accSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, _accSetOr.Output, AccSet);

        _ramSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, StepWire(5), RamSet);
        _tmpSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, StepWire(4), TmpSet);
        
        _generalPurposeRegistersSetAnd = new IAnd2[WireConstants.ExpectedNumberOfGeneralPurposeRegisters];
        for (var i = 0; i < WireConstants.ExpectedNumberOfGeneralPurposeRegisters; i++)
        {
            _generalPurposeRegistersSetAnd[i] = ComponentFactory.CreateAnd2(
                _clock.ClkS, 
                i switch
                {
                    0 => StepWire(6),
                    _ => WireFactory.OffWire
                }, 
                GeneralPurposeRegistersSet[i]);
        }
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
        
        _iarEnableAnd.Update();
        _ramEnableAnd.Update();
        
        _accEnableOr.Update();
        _accEnableAnd.Update();

        foreach (var generalPurposeRegisterEnableAnd in _generalPurposeRegistersEnableAnd)
        {
            generalPurposeRegisterEnableAnd.Update();
        }
        
        _irSetAnd.Update();
        
        _marSetOr.Update();
        _marSetAnd.Update();
        
        _iarSetAnd.Update();
        
        _accSetOr.Update();
        _accSetAnd.Update();
        
        _ramSetAnd.Update();
        _tmpSetAnd.Update();
        
        foreach (var generalPurposeRegisterSetAnd in _generalPurposeRegistersSetAnd)
        {
            generalPurposeRegisterSetAnd.Update();
        }
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