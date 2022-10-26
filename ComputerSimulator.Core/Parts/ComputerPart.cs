using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Constants;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;

namespace ComputerSimulator.Core.Parts;

public interface IComputerPart : IPart
{
    IRegister Acc { get; }
    
    IArithmeticLogicUnit Alu { get; }
    
    IIoBus IoBus { get; }
    
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
        IoBus = WireFactory.CreateIoBus("io");

        var irSet = WireFactory.CreateWire<bool>("ir-set");
        var caezSet = WireFactory.CreateWire<bool>("flags-set");
        var caez = WireFactory.CreateCaez<bool>($"{nameof(Caez)}-outputs");

        Ir = ComponentFactory.CreateRegister(
            irSet,
            WireFactory.PowerWire,
            IoBus.CpuBus,
            IoBus.CpuBus);

        Cpu = ComponentFactory.CreateCentralProcessingUnit(
            WireFactory.CreateWire<bool>("bus1"),
            WireFactory.CreateSetEnableWire<bool>("iar"),
            WireFactory.CreateSetEnableWire<bool>("ram"),
            WireFactory.CreateSetEnableWire<bool>("acc"),
            IoBus.Clk,
            WireFactory.CreateSetEnableWireGroup<bool>(WireConstants.ExpectedNumberOfGeneralPurposeRegisters, "general-purpose-registers"),
            WireFactory.CreateOp("op"),
            WireFactory.CreateWire<bool>("mar-set"),
            WireFactory.CreateWire<bool>("tmp-set"),
            irSet,
            caezSet,
            WireFactory.CreateWire<bool>("carry-in-tmp"),
            IoBus.InputOutput,
            IoBus.DataAddress,
            Ir.Outputs,
            caez
        );

        GeneralPurposeRegisters = WireConstants.ExpectedNumberOfGeneralPurposeRegisters
            .InitArray<IRegister>()
            .Fill(i =>
                ComponentFactory.CreateRegister(
                    Cpu.GeneralPurposeRegisters[i].Set,
                    Cpu.GeneralPurposeRegisters[i].Enable,
                    IoBus.CpuBus,
                    IoBus.CpuBus));

        Tmp = ComponentFactory.CreateRegister(
            Cpu.TmpSet,
            WireFactory.PowerWire,
            IoBus.CpuBus,
            WireFactory.CreateGroup<bool>($"{nameof(Tmp)}-output"));

        Bus1 = ComponentFactory.CreateBus1(
            Cpu.Bus1,
            Tmp.Outputs,
            WireFactory.CreateGroup<bool>($"{nameof(Bus1)}-output"));

        Alu = ComponentFactory.CreateArithmeticLogicUnit(
            IoBus.CpuBus,
            Bus1.Outputs,
            Cpu.CarryInTmp,
            Cpu.Op,
            WireFactory.CreateGroup<bool>($"{nameof(Alu)}-outputs"),
            WireFactory.CreateCaez<bool>($"{nameof(Alu)}.{nameof(Alu.Caez)}")
        );

        Caez = ComponentFactory.CreateCaezRegister(
            caezSet,
            Alu.Caez,
            caez
        );

        Ram = ComponentFactory.CreateRam(
            Cpu.MarSet,
            IoBus.CpuBus,
            Cpu.Ram.Set,
            Cpu.Ram.Enable,
            IoBus.CpuBus
        );

        Acc = ComponentFactory.CreateRegister(
            Cpu.Acc.Set,
            Cpu.Acc.Enable,
            Alu.Outputs,
            IoBus.CpuBus);
        
        Iar = ComponentFactory.CreateRegister(
            Cpu.Iar.Set,
            Cpu.Iar.Enable,
            IoBus.CpuBus,
            IoBus.CpuBus);
    }

    public IRegister Acc { get; }
    public IArithmeticLogicUnit Alu { get; }
    public IIoBus IoBus { get; }

    public IBus1 Bus1 { get; }
    public ICaezRegister Caez { get; }
    public ICentralProcessingUnit Cpu { get; }
    public IRegister[] GeneralPurposeRegisters { get; }
    public IRegister Iar { get; }
    public IRegister Ir { get; }
    public IRam Ram { get; }
    public IRegister Tmp { get; }
    
    public void Update()
    {
        Cpu.Update();
    }
}