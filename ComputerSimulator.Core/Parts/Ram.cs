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

    public Ram(
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
    }

    private void SetMarInputsAndOutputs()
    {
        _mar.SetInputs(_inputBus);
        _outputBus.SetWires(_mar);
    }
}