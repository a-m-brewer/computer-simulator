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

    IRamSlot GetSlot(int x, int y);
    
    void UpdateMemory();
}

public class Ram : PartsBase, IRam
{
    // External Wires

    // Internal Circuits
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IDecoder _decoderX;
    private readonly IDecoder _decoderY;
    private readonly LazyRamSlotGroup _slots;

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

        _slots = new LazyRamSlotGroup(_decoderX, _decoderY, Set, Enable, Io, ComponentFactory);
    }

    public IRegister Mar { get; }

    public IBus Io { get; }

    public IWire<bool> Set { get; }

    public IWire<bool> Enable { get; }

    public void Update()
    {
        Mar.Update();

        UpdateMemory();
    }

    public IRamSlot GetSlot(int x, int y)
    {
        return _slots.GetSlot(y, x);
    }

    public void UpdateMemory()
    {
        _decoderX.Update();
        _decoderY.Update();
        
        var currSlot = _slots.GetSlot(_decoderY.EnabledIndex, _decoderX.EnabledIndex);
        
        currSlot.Update();
    }
}