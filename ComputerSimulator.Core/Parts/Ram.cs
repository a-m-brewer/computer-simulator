using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;

namespace ComputerSimulator.Core.Parts;

public interface IRam : IComponent2
{
    IRegister Mar { get; }
    
    IBus Io { get; }

    IWire2<bool> Set { get; }

    IWire2<bool> Enable { get; }
}

public class Ram : PartsBase, IRam
{
    private readonly IRegister _mar;
    private readonly IDecoder _decoderX;
    private readonly IDecoder _decoderY;
    private readonly IBus _ioBus;
    private readonly int _decoderInputSize;
    private IRamSlot[][] _slots;
    private readonly IWire<bool> _set;
    private readonly IWire<bool> _enable;

    public Ram(
        IComponentFactory componentFactory,
        ComputerSettings settings,
        IWire2Factory wireFactory,
        IRegister mar,
        IDecoder decoderX,
        IDecoder decoderY,
        IBus ioBus) : base(wireFactory)
    {
        _set = WireCupboard.Retrieve(false, this.GenerateLabel(nameof(_set)));
        _enable = WireCupboard.Retrieve(false, this.GenerateLabel(nameof(_enable)));

        _mar = mar;
        _decoderX = decoderX;
        _decoderY = decoderY;
        _ioBus = ioBus;

        _ioBus.SetWires(_mar);

        _decoderInputSize = settings.WordSize / 2;
        _decoderX.Initialize(_decoderInputSize);
        _decoderY.Initialize(_decoderInputSize);

        _mar.Enable.SetInitialValue(true);
        
        _slots = new IRamSlot[_decoderY.OutputSize][];
        for (var y = 0; y < _decoderY.OutputSize; y++)
        {
            _slots[y] = new IRamSlot[_decoderX.OutputSize];
            for (var x = 0; x < _decoderX.OutputSize; x++)
            {
                _slots[y][x] = componentFactory.Create<IRamSlot>();
                _slots[y][x].Register.SetInputs(_ioBus);
                _slots[y][x].Register.SetOutputs(_ioBus);
            }
        }
    }

    public IRegister Mar => _mar;

    public IBus Io => _ioBus;

    public IWire<bool> Set { get; }

    public IWire<bool> Enable { get; }
}