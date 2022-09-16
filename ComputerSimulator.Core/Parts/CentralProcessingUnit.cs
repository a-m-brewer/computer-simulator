using ComputerSimulator.Core.Constants;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;

namespace ComputerSimulator.Core.Parts;

public interface ICentralProcessingUnit : IPart
{
    IWire<bool> Bus1 { get; }

    #region Enables

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
    
    IWireGroup<bool> GeneralPurposeRegistersSet { get; }

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

    public CentralProcessingUnit(
        IWire<bool> bus1,
        IWire<bool> ramEnable,
        IWire<bool> accEnable,
        IWireGroup<bool> generalPurposeRegistersEnable,
        IOp op,
        IWire<bool> marSet,
        IWire<bool> accSet,
        IWire<bool> ramSet,
        IWire<bool> tmpSet,
        IWireGroup<bool> generalPurposeRegistersSet,
        IComponentFactory componentFactory,
        IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        Bus1 = bus1;
        RamEnable = ramEnable;
        AccEnable = accEnable;
        GeneralPurposeRegistersEnable = generalPurposeRegistersEnable;
        Op = op;
        MarSet = marSet;
        AccSet = accSet;
        RamSet = ramSet;
        TmpSet = tmpSet;
        GeneralPurposeRegistersSet = generalPurposeRegistersSet;

        _clock = ComponentFactory.CreateComputerClock(
            WireFactory.CreateWire(false, "clk"),
            WireFactory.CreateWire(false, "clk-enable"),
            WireFactory.CreateWire(false, "clk-set"));

        _stepper = ComponentFactory.CreateStepper(_clock.Clk,
            WireFactory.CreateGroup(false, WireConstants.ExpectedNumberOfSteps, "step"));
        
        // Enables
        _ramEnableAnd = ComponentFactory.CreateAnd2(_clock.ClkE, WireFactory.OffWire, RamEnable);
        _accEnableAnd = ComponentFactory.CreateAnd2(_clock.ClkE, StepWire(6), AccEnable);
        
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
        _marSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, StepWire(4), MarSet);
        _accSetAnd = ComponentFactory.CreateAnd2(_clock.ClkS, StepWire(5), AccSet);
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

    public IWire<bool> Bus1 { get; }

    public IWire<bool> RamEnable { get; }

    public IWire<bool> AccEnable { get; }

    public IWireGroup<bool> GeneralPurposeRegistersEnable { get; }

    public IOp Op { get; }

    public IWire<bool> MarSet { get; }

    public IWire<bool> AccSet { get; }

    public IWire<bool> RamSet { get; }

    public IWire<bool> TmpSet { get; }

    public IWireGroup<bool> GeneralPurposeRegistersSet { get; }

    public void Update()
    {
        _clock.Update();
        _stepper.Update();
        
        _ramEnableAnd.Update();
        _accEnableAnd.Update();

        foreach (var generalPurposeRegisterEnableAnd in _generalPurposeRegistersEnableAnd)
        {
            generalPurposeRegisterEnableAnd.Update();
        }
        
        _marSetAnd.Update();
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