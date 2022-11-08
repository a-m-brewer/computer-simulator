using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public class LazyRamSlotGroup
{
    private readonly Lazy<IRamSlot>[][] _lazySlots;

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
        IBus inputBus,
        IBus outputBus,
        IComponentFactory componentFactory)
    {
        _lazySlots = new Lazy<IRamSlot>[setDecoderY.OutputSize][];
        for (var y = 0; y < setDecoderY.OutputSize; y++)
        {
            _lazySlots[y] = new Lazy<IRamSlot>[setDecoderX.OutputSize];
            for (var x = 0; x < setDecoderX.OutputSize; x++)
            {
                var x1 = x;
                var y1 = y;
                _lazySlots[y][x] = new Lazy<IRamSlot>(() =>
                    componentFactory.CreateRamSlot(
                        setDecoderX.Outputs[x1], 
                        setDecoderY.Outputs[y1],
                        enableDecoderX.Outputs[x1], 
                        enableDecoderY.Outputs[y1],
                        set,
                        enable,
                        inputBus,
                        outputBus));
            }
        }
    }

    public IRamSlot GetSlot(int x, int y)
    {
        return _lazySlots[y][x].Value;
    }
}