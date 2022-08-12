using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Services;

namespace ComputerSimulator.Core.Parts;

public interface IRam : IComponent2
{
    IBus MarInputBus { get; set; }

    IWire2<bool> MarSet { get; set; }

    IBus Io { get; set; }

    IWire2<bool> Set { get; set; }

    IWire2<bool> Enable { get; set; }
}

public class Ram : PartsBase, IRam
{
    // External Wires
    private IWire2<bool> _set = DisconnectedWire<bool>.Instance;
    private IWire2<bool> _enable = DisconnectedWire<bool>.Instance;
    private IBus _io = DisconnectedBus.Instance;
    
    // Internal Circuits
    private readonly IRamSlot[][] _slots;
    private readonly IRegister _mar;

    public Ram(
        IComponentFactory2 componentFactory2,
        ComputerSettings computerSettings,
        IDecoder decoderX,
        IDecoder decoderY,
        IRegister mar,
        IWireService wireService) : base(wireService)
    {
        _mar = mar;

        // enable always true for MAR
        var marOutputWireGroup = CreateInternalWireGroup("mar_output", false);
        
        _mar.Enable = CreateInternalWire("mar_enable", true);
        _mar.Outputs = marOutputWireGroup;
        _mar.Outputs.WireValuesChanged += MarOutputChanged;

        var decoderInputSize = computerSettings.WordSize / 2;
        decoderX.Initialize(decoderInputSize);
        decoderY.Initialize(decoderInputSize);

        for (var i = 0; i < decoderInputSize; i++)
        {
            decoderX.Inputs.SetWire(i, marOutputWireGroup[i]);
        }

        for (var i = decoderInputSize; i < computerSettings.WordSize; i++)
        {
            decoderY.Inputs.SetWire(i, marOutputWireGroup[i]);
        }

        var decoderXOutputWires = CreateInternalWireGroup("decoder_x_output", false, decoderX.OutputSize);
        var decoderYOutputWires = CreateInternalWireGroup("decoder_y_output", false, decoderY.OutputSize);

        decoderX.Outputs = decoderXOutputWires;
        decoderY.Outputs = decoderYOutputWires;
        
        _slots = new IRamSlot[decoderY.OutputSize][];
        for (var y = 0; y < decoderY.OutputSize; y++)
        {
            _slots[y] = new IRamSlot[decoderX.OutputSize];
            for (var x = 0; x < decoderX.OutputSize; x++)
            {
                var slot = componentFactory2.Create<IRamSlot>();

                slot.X = decoderX.Outputs[x];
                slot.Y = decoderY.Outputs[y];
                
                _slots[y][x] = slot;
            }
        }
    }

    public IBus MarInputBus
    {
        get => _mar.Inputs as IBus ?? throw new Exception("expected that MAR is using a IBus WireGroup<bool>"); 
        set => _mar.Inputs = value;
    }

    public IWire2<bool> MarSet
    {
        get => _mar.Set; 
        set => _mar.Set = value;
    }

    public IBus Io
    {
        get => _io;
        set
        {
            _io = value;
            foreach (var row in _slots)
            {
                foreach (var slot in row)
                {
                    slot.Io = _io;
                }
            }
        }
    }

    public IWire2<bool> Set
    {
        get => _set;
        set
        {
            WireHelper.SetWire(ref _set, value, WireChanged);
            foreach (var row in _slots)
            {
                foreach (var slot in row)
                {
                    slot.Set = _set;
                }
            }
        }
    }

    public IWire2<bool> Enable
    {
        get => _enable;
        set
        {
            WireHelper.SetWire(ref _enable, value, WireChanged);
            foreach (var row in _slots)
            {
                foreach (var slot in row)
                {
                    slot.Enable = _enable;
                }
            }
        }
    }

    private void WireChanged(object? sender, EventArgs e)
    {
    }
    
    private void MarOutputChanged(object? sender, int e)
    {
        throw new NotImplementedException();
    }
}