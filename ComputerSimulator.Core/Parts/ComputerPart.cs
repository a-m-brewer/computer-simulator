using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Constants;
using ComputerSimulator.Core.Factories;

namespace ComputerSimulator.Core.Parts;

public interface IComputerPart : IPart
{
    IBus Bus { get; }
    
    ICentralProcessingUnit Cpu { get; }
    
    IRegister Iar { get; }
    
    IRam Ram { get; }
}

public class ComputerPart : PartsBase, IComputerPart
{
    public ComputerPart(
        IComponentFactory componentFactory,
        IWireFactory wireFactory)
        : base(componentFactory, wireFactory)
    {
        Bus = WireFactory.CreateBus("bus");

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
            WireFactory.CreateWire(false, "ir-set"),
            WireFactory.CreateWire(false, "flags-set"),
            WireFactory.CreateWire(false, "carry-in-tmp"),
            WireFactory.CreateWire(false, "io-input-output"),
            WireFactory.CreateWire(false, "io-data-address"),
            WireFactory.CreateGroup(false, "instruction-register"),
            WireFactory.CreateCaez(false, "caez")
        );

        Ram = ComponentFactory.CreateRam(
            Cpu.MarSet,
            Bus,
            Cpu.Ram.Set,
            Cpu.Ram.Enable,
            Bus
        );

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

    public IBus Bus { get; }
    public ICentralProcessingUnit Cpu { get; }
    public IRegister Iar { get; }
    public IRam Ram { get; }
}