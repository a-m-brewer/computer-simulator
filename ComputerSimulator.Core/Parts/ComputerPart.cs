using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Constants;
using ComputerSimulator.Core.Factories;

namespace ComputerSimulator.Core.Parts;

public interface IComputerPart : IPart
{
}

public class ComputerPart : PartsBase, IComputerPart
{
    private readonly ICentralProcessingUnit _cpu;
    private readonly IBus _bus;
    private readonly IRegister[] _registers;
    private readonly IRam _ram;
    private readonly IRegister _instructionRegister;
    private readonly IRegister _instructionAddressRegister;
    private readonly IRegister _tmpRegister;
    private readonly IBus1 _bus1;
    private readonly IArithmeticLogicUnit _alu;
    private readonly IRegister _acc;

    public ComputerPart(
        IComponentFactory componentFactory,
        IWireFactory wireFactory)
        : base(componentFactory, wireFactory)
    {
        _bus = WireFactory.CreateBus("bus");

        var irSet = WireFactory.CreateWire(false, "ir-set");
        _instructionRegister = ComponentFactory.CreateRegister(irSet, WireFactory.PowerWire, _bus, WireFactory.CreateGroup(false, "ir-output"));

        _cpu = ComponentFactory.CreateCentralProcessingUnit(
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
            WireFactory.CreateGroup(false, "instruction-register"),
            WireFactory.CreateCaez(false, "caez")
        );

        _instructionAddressRegister = ComponentFactory.CreateRegister(_cpu.Iar.Set, _cpu.Iar.Enable, _bus, _bus);
        
        _registers = new IRegister[WireConstants.ExpectedNumberOfGeneralPurposeRegisters];
        for (var i = 0; i < WireConstants.ExpectedNumberOfGeneralPurposeRegisters; i++)
        {
            _registers[i] = ComponentFactory.CreateRegister(_cpu.GeneralPurposeRegisters[i].Set, _cpu.GeneralPurposeRegisters[i].Enable, _bus, _bus);
        }

        _ram = ComponentFactory.CreateRam(_cpu.MarSet, _bus, _cpu.Ram.Set, _cpu.Ram.Enable, _bus);

        _tmpRegister = ComponentFactory.CreateRegister(_cpu.TmpSet, WireFactory.PowerWire, _bus, WireFactory.CreateGroup(false, "tmp-output"));

        _bus1 = ComponentFactory.CreateBus1(_cpu.Bus1, _tmpRegister.Outputs, WireFactory.CreateGroup(false, "bus1-output"));

        _alu = ComponentFactory.CreateArithmeticLogicUnit(
            _bus,
            _bus1.Outputs,
            WireFactory.OffWire,
            _cpu.Op,
            WireFactory.CreateGroup(false, "alu-output"),
            WireFactory.OffWire,
            WireFactory.OffWire,
            WireFactory.OffWire,
            WireFactory.OffWire);

        _acc = ComponentFactory.CreateRegister(_cpu.Acc.Set, _cpu.Acc.Enable, _alu.Outputs, _bus);
    }

    public void Update()
    {
        _cpu.Update();
    }
}