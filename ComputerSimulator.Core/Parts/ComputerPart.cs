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

        var irSet = WireFactory.CreateWire(false, "ir-set");
        var caezSet = WireFactory.CreateWire(false, "flags-set");
        var caez = WireFactory.CreateCaez(false, $"{nameof(Caez)}-outputs");

        Ir = ComponentFactory.CreateRegister(
            irSet,
            WireFactory.PowerWire,
            IoBus.CpuBus,
            IoBus.CpuBus);

        Cpu = ComponentFactory.CreateCentralProcessingUnit(
            WireFactory.CreateWire(false, "bus1"),
            WireFactory.CreateSetEnableWire(false, "iar"),
            WireFactory.CreateSetEnableWire(false, "ram"),
            WireFactory.CreateSetEnableWire(false, "acc"),
            IoBus.Clk,
            WireFactory.CreateSetEnableWireGroup(false, WireConstants.ExpectedNumberOfGeneralPurposeRegisters, "general-purpose-registers"),
            WireFactory.CreateOp("op"),
            WireFactory.CreateWire(false, "mar-set"),
            WireFactory.CreateWire(false, "tmp-set"),
            irSet,
            caezSet,
            WireFactory.CreateWire(false, "carry-in-tmp"),
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
            WireFactory.CreateGroup(false, $"{nameof(Tmp)}-output"));

        Bus1 = ComponentFactory.CreateBus1(
            Cpu.Bus1,
            Tmp.Outputs,
            WireFactory.CreateGroup(false, $"{nameof(Bus1)}-output"));

        Alu = ComponentFactory.CreateArithmeticLogicUnit(
            IoBus.CpuBus,
            Bus1.Outputs,
            Cpu.CarryInTmp,
            Cpu.Op,
            WireFactory.CreateGroup(false, $"{nameof(Alu)}-outputs"),
            WireFactory.CreateCaez(false, $"{nameof(Alu)}.{nameof(Alu.Caez)}")
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
        
        UpdateNonCpuComponents();
        
        Cpu.UpdatePins();
        
        UpdateNonCpuComponents();
    }

    private void UpdateNonCpuComponents()
    {
        Iar.Update();
        
        Ram.Mar.Update();
        
        Ir.Update();
        
        Ram.UpdateMemory();
        
        Tmp.Update();
        
        Caez.Update();
        
        Bus1.Update();
        
        Alu.Update();
        
        Acc.Update();

        GeneralPurposeRegisters.Update();
    }
}