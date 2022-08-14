using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;

namespace ComputerSimulator.Core.Parts;

public interface IRam : IComponent2
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

        var decoderX = ComponentFactory.CreateDecoder(WireFactory.CreateGroup(decoderInputSize
            .InitArray<IWire2<bool>>()
            .Fill(i => _mar.Outputs.GetWire(i))));
        
        var decoderY = ComponentFactory.CreateDecoder(WireFactory.CreateGroup(decoderInputSize
            .InitArray<IWire2<bool>>()
            .Fill(i => _mar.Outputs.GetWire(i + decoderInputSize))));

        _slots = new IRamSlot[decoderY.OutputSize][];
        for (var y = 0; y < decoderY.OutputSize; y++)
        {
            _slots[y] = new IRamSlot[decoderX.OutputSize];
            for (var x = 0; x < decoderX.OutputSize; x++)
            {
                _slots[y][x] = ComponentFactory.CreateRamSlot(decoderX.Outputs.GetWire(x), decoderY.Outputs.GetWire(y), Set, Enable, Io);
            }
        }
    }

    public IBus MarInputBus => _mar.Inputs as IBus ?? throw new Exception("expected that MAR is using a IBus WireGroup<bool>");

    public IWire2<bool> MarSet => _mar.Set;

    public IBus Io { get; }

    public IWire2<bool> Set { get; }

    public IWire2<bool> Enable { get; }
}