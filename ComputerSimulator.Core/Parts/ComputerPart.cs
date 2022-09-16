using ComputerSimulator.Core.Constants;
using ComputerSimulator.Core.Factories;

namespace ComputerSimulator.Core.Parts;

public interface IComputerPart : IPart
{
}

public class ComputerPart : PartsBase, IComputerPart
{
    private readonly ICentralProcessingUnit _cpu;

    public ComputerPart(
        IComponentFactory componentFactory,
        IWireFactory wireFactory)
        : base(componentFactory, wireFactory)
    {
        _cpu = ComponentFactory.CreateCentralProcessingUnit(
            WireFactory.OffWire,
            WireFactory.OffWire,
            WireFactory.OffWire,
            WireFactory.CreateGroup(false, WireConstants.ExpectedNumberOfGeneralPurposeRegisters, "general-purpose-register-enable"),
            WireFactory.CreateOp("op"),
            WireFactory.OffWire,
            WireFactory.OffWire,
            WireFactory.OffWire,
            WireFactory.OffWire,
            WireFactory.OffWire,
            WireFactory.OffWire,
            WireFactory.CreateGroup(false, WireConstants.ExpectedNumberOfGeneralPurposeRegisters, "general-purpose-register-set"))
    }

    public void Update()
    {
        _cpu.Update();
    }
}