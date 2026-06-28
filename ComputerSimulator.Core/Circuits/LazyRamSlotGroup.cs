using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public class LazyRamSlotGroup
{
    private readonly IRamSlot?[][] _slots;
    private readonly IDecoder _setDecoderX;
    private readonly IDecoder _setDecoderY;
    private readonly IDecoder _enableDecoderX;
    private readonly IDecoder _enableDecoderY;
    private readonly IWire<bool> _set;
    private readonly IWire<bool> _enable;
    private readonly IWireGroup<bool> _inputBus;
    private readonly IWireGroup<bool> _outputBus;
    private readonly IComponentFactory _componentFactory;

    public LazyRamSlotGroup(
        IDecoder decoderX,
        IDecoder decoderY,
        IWire<bool> set,
        IWire<bool> enable,
        IBus bus,
        IComponentFactory componentFactory) 
        : this(decoderX, decoderY, decoderX, decoderY, set, enable, bus, bus, componentFactory)
    {
    }

    public LazyRamSlotGroup(
        IDecoder setDecoderX,
        IDecoder setDecoderY,
        IDecoder enableDecoderX,
        IDecoder enableDecoderY,
        IWire<bool> set,
        IWire<bool> enable,
        IWireGroup<bool> inputBus,
        IWireGroup<bool> outputBus,
        IComponentFactory componentFactory)
    {
        _setDecoderX = setDecoderX;
        _setDecoderY = setDecoderY;
        _enableDecoderX = enableDecoderX;
        _enableDecoderY = enableDecoderY;
        _set = set;
        _enable = enable;
        _inputBus = inputBus;
        _outputBus = outputBus;
        _componentFactory = componentFactory;

        _slots = new IRamSlot?[setDecoderY.OutputSize][];
        for (var y = 0; y < setDecoderY.OutputSize; y++)
        {
            _slots[y] = new IRamSlot?[setDecoderX.OutputSize];
        }
    }

    public IRamSlot GetSlot(int x, int y)
    {
        return _slots[y][x] ??= _componentFactory.CreateRamSlot(
            _setDecoderX.Outputs[x],
            _setDecoderY.Outputs[y],
            _enableDecoderX.Outputs[x],
            _enableDecoderY.Outputs[y],
            _set,
            _enable,
            _inputBus,
            _outputBus);
    }

    public bool TryGetSlot(int x, int y, out IRamSlot slot)
    {
        slot = _slots[y][x]!;
        return slot is not null;
    }
}
