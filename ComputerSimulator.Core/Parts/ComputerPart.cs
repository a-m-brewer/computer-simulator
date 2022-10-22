using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Constants;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;

namespace ComputerSimulator.Core.Parts;

public interface IComputerPart : IPart
{
    IRegister Acc { get; }
    
    IArithmeticLogicUnit Alu { get; }
    
    IBus Bus { get; }
    
    IBus1 Bus1 { get; }
    
    ICaezRegister Caez { get; }
    
    ICentralProcessingUnit Cpu { get; }
    
    IRegister[] GeneralPurposeRegisters { get; }
    
    IRegister Iar { get; }
    
    IRegister Ir { get; }
    
    IRam Ram { get; }
    
    IRegister Tmp { get; }
}

public class ComputerPart : PartsBase, IComputerPart
{
    public ComputerPart(
        IComponentFactory componentFactory,
        IWireFactory wireFactory)
        : base(componentFactory, wireFactory)
    {
        Bus = WireFactory.CreateBus("bus");

        var irSet = WireFactory.CreateWire(false, "ir-set");
        var flags = WireFactory.CreateWire(false, "flags-set");

        Ir = ComponentFactory.CreateRegister(
            irSet,
            WireFactory.PowerWire,
            Bus,
            Bus);

        Cpu = ComponentFactory.CreateCentralProcessingUnit(
            WireFactory.CreateWire(false, "bus1"),
            WireFactory.CreateSetEnableWire(false, "iar"),
            WireFactory.CreateSetEnableWire(false, "ram"),
            WireFactory.CreateSetEnableWire(false, "acc"),
            WireFactory.CreateSetEnableWire(false, "ioClk"),
            WireFactory.CreateSetEnableWireGroup(false, WireConstants.ExpectedNumberOfGeneralPurposeRegisters, "general-purpose-registers"),
            WireFactory.CreateOp("op"),
            WireFactory.CreateWire(false, "mar-set"),
            WireFactory.CreateWire(false, "tmp-set"),
            irSet,
            WireFactory.CreateWire(false, "flags-set"),
            WireFactory.CreateWire(false, "carry-in-tmp"),
            WireFactory.CreateWire(false, "io-input-output"),
            WireFactory.CreateWire(false, "io-data-address"),
            Ir.Outputs,
            WireFactory.CreateCaez(false, "caez")
        );

        GeneralPurposeRegisters = WireConstants.ExpectedNumberOfGeneralPurposeRegisters
            .InitArray<IRegister>()
            .Fill(i =>
                ComponentFactory.CreateRegister(
                    Cpu.GeneralPurposeRegisters[i].Set,
                    Cpu.GeneralPurposeRegisters[i].Enable,
                    Bus,
                    Bus));

        Tmp = ComponentFactory.CreateRegister(
            Cpu.TmpSet,
            WireFactory.PowerWire,
            Bus,
            WireFactory.CreateGroup(false, $"{nameof(Tmp)}-output"));

        Bus1 = ComponentFactory.CreateBus1(
            Cpu.Bus1,
            Tmp.Outputs,
            WireFactory.CreateGroup(false, $"{nameof(Bus1)}-output"));

        Alu = ComponentFactory.CreateArithmeticLogicUnit(
            Bus,
            Bus1.Outputs,
            Cpu.CarryInTmp,
            Cpu.Op,
            WireFactory.CreateGroup(false, $"{nameof(Alu)}-outputs"),
            WireFactory.CreateCaez(false, $"{nameof(Alu)}.{nameof(Alu.Caez)}")
        );

        Caez = ComponentFactory.CreateCaezRegister(
            flags,
            Alu.Caez,
            WireFactory.CreateCaez(false, $"{nameof(Caez)}-outputs")
        );

        Ram = ComponentFactory.CreateRam(
            Cpu.MarSet,
            Bus,
            Cpu.Ram.Set,
            Cpu.Ram.Enable,
            Bus
        );

        Acc = ComponentFactory.CreateRegister(
            Cpu.Acc.Set,
            Cpu.Acc.Enable,
            Alu.Outputs,
            Bus);
        
        Iar = ComponentFactory.CreateRegister(
            Cpu.Iar.Set,
            Cpu.Iar.Enable,
            Bus,
            Bus);
    }

    public void Update()
    {
        Cpu.Update();
    }

    public IRegister Acc { get; }
    public IArithmeticLogicUnit Alu { get; }
    public IBus Bus { get; }
    public IBus1 Bus1 { get; }
    public ICaezRegister Caez { get; }
    public ICentralProcessingUnit Cpu { get; }
    public IRegister[] GeneralPurposeRegisters { get; }
    public IRegister Iar { get; }
    public IRegister Ir { get; }
    public IRam Ram { get; }
    public IRegister Tmp { get; }
}