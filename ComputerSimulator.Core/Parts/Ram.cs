using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;

namespace ComputerSimulator.Core.Parts;

public interface IRam : IComponent
{
}

public class Ram : ComponentBase, IRam
{
    private readonly IRegister _mar;
    private readonly IDecoder _decoderX;
    private readonly IDecoder _decoderY;
    private readonly IBus _inputBus;
    private readonly IBus _outputBus;
    private readonly int _decoderInputSize;
    private IRamSlot[][] _slots;

    public Ram(
        IComponentFactory componentFactory,
        ComputerSettings settings,
        IWireCupboard wireCupboard,
        IRegister mar,
        IDecoder decoderX,
        IDecoder decoderY,
        IBus inputBus,
        IBus outputBus) : base(wireCupboard)
    {
        _mar = mar;
        _decoderX = decoderX;
        _decoderY = decoderY;
        _inputBus = inputBus;
        _outputBus = outputBus;

        SetMarInputsAndOutputs();

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
                _slots[y][x].Register.SetInputs(_outputBus);
                _slots[y][x].Register.SetOutputs(_outputBus);
            }
        }
    }

    private void SetMarInputsAndOutputs()
    {
        _mar.SetInputs(_inputBus);
        _outputBus.SetWires(_mar);
    }
}