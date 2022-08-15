using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;

namespace ComputerSimulator.Core.Parts;

public interface IRam : ICircuit
{
    IBus MarInputBus { get; }

    IWire2<bool> MarSet { get; }

    IBus Io { get; }

    IWire2<bool> Set { get; }

    IWire2<bool> Enable { get; }
}

public class Ram : PartsBase, IRam
{
    // External Wires

    // Internal Circuits
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IRamSlot[][] _slots;
    private readonly IRegister _mar;
    private readonly IDecoder _decoderX;
    private readonly IDecoder _decoderY;

    public Ram(
        IWire2<bool> marSet,
        IBus marInputBus,
        IWire2<bool> set,
        IWire2<bool> enable,
        IBus io,
        ComputerSettings computerSettings,
        IComponentFactory2 componentFactory,
        IWire2Factory2 wireFactory) : base(componentFactory, wireFactory)
    {
        Set = set;
        Enable = enable;
        Io = io;
        
        // enable always true for MAR
        _mar = ComponentFactory
            .CreateRegister(marSet, WireFactory.CreateWire(true), marInputBus, WireFactory.CreateGroup(false));

        var decoderInputSize = computerSettings.WordSize / 2;

        _decoderX = ComponentFactory.CreateDecoder(WireFactory.CreateGroup(decoderInputSize
            .InitArray<IWire2<bool>>()
            .Fill(i => _mar.Outputs[i])));
        
        _decoderY = ComponentFactory.CreateDecoder(WireFactory.CreateGroup(decoderInputSize
            .InitArray<IWire2<bool>>()
            .Fill(i => _mar.Outputs[i + decoderInputSize])));

        _slots = new IRamSlot[_decoderY.OutputSize][];
        for (var y = 0; y < _decoderY.OutputSize; y++)
        {
            _slots[y] = new IRamSlot[_decoderX.OutputSize];
            for (var x = 0; x < _decoderX.OutputSize; x++)
            {
                _slots[y][x] = ComponentFactory.CreateRamSlot(_decoderX.Outputs[x], _decoderY.Outputs[y], Set, Enable, Io);
            }
        }
    }

    public IBus MarInputBus => _mar.Inputs as IBus ?? throw new Exception("expected that MAR is using a IBus WireGroup<bool>");

    public IWire2<bool> MarSet => _mar.Set;

    public IBus Io { get; }

    public IWire2<bool> Set { get; }

    public IWire2<bool> Enable { get; }

    public void Update()
    {
        _mar.Update();

        _decoderX.Update();
        _decoderY.Update();
        
        var currSlot = _slots[_decoderY.EnabledIndex][_decoderX.EnabledIndex];
        
        currSlot.Update();
    }
}