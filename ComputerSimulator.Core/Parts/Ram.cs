using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;

namespace ComputerSimulator.Core.Parts;

public interface IRam : ICircuit
{
    IRegister Mar { get; }

    IBus Io { get; }

    IWire<bool> Set { get; }

    IWire<bool> Enable { get; }
    
    IRamSlot[][] Slots { get; }

    void UpdateMemory();
}

public class Ram : PartsBase, IRam
{
    // External Wires

    // Internal Circuits
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IDecoder _decoderX;
    private readonly IDecoder _decoderY;

    public Ram(
        IWire<bool> marSet,
        IBus marInputBus,
        IWire<bool> set,
        IWire<bool> enable,
        IBus io,
        ComputerSettings computerSettings,
        IComponentFactory componentFactory,
        IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        Set = set;
        Enable = enable;
        Io = io;
        
        // enable always true for MAR
        Mar = ComponentFactory
            .CreateRegister(marSet, WireFactory.PowerWire, marInputBus, WireFactory.CreateGroup<bool>());

        var decoderInputSize = computerSettings.WordSize / 2;

        _decoderX = ComponentFactory.CreateDecoder(WireFactory.CreateGroup(decoderInputSize
            .InitArray<IWire<bool>>()
            .Fill(i => Mar.Outputs[i])));
        
        _decoderY = ComponentFactory.CreateDecoder(WireFactory.CreateGroup(decoderInputSize
            .InitArray<IWire<bool>>()
            .Fill(i => Mar.Outputs[i + decoderInputSize])));

        Slots = new IRamSlot[_decoderY.OutputSize][];
        for (var y = 0; y < _decoderY.OutputSize; y++)
        {
            Slots[y] = new IRamSlot[_decoderX.OutputSize];
            for (var x = 0; x < _decoderX.OutputSize; x++)
            {
                Slots[y][x] = ComponentFactory.CreateRamSlot(_decoderX.Outputs[x], _decoderY.Outputs[y], Set, Enable, Io);
            }
        }
    }

    public IRegister Mar { get; }

    public IBus Io { get; }

    public IWire<bool> Set { get; }

    public IWire<bool> Enable { get; }
    public IRamSlot[][] Slots { get; }

    public void Update()
    {
        Mar.Update();

        UpdateMemory();
    }
    
    public void UpdateMemory()
    {
        _decoderX.Update();
        _decoderY.Update();
        
        var currSlot = Slots[_decoderY.EnabledIndex][_decoderX.EnabledIndex];
        
        currSlot.Update();
    }
}