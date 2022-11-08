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
        IComponentFactory componentFactory) : this(decoderX, decoderY, set, enable, bus, bus, componentFactory)
    {
    }

    public LazyRamSlotGroup(
        IDecoder decoderX,
        IDecoder decoderY,
        IWire<bool> set,
        IWire<bool> enable,
        IBus inputBus,
        IBus outputBus,
        IComponentFactory componentFactory)
    {
        _lazySlots = new Lazy<IRamSlot>[decoderY.OutputSize][];
        for (var y = 0; y < decoderY.OutputSize; y++)
        {
            _lazySlots[y] = new Lazy<IRamSlot>[decoderX.OutputSize];
            for (var x = 0; x < decoderX.OutputSize; x++)
            {
                var x1 = x;
                var y1 = y;
                _lazySlots[y][x] = new Lazy<IRamSlot>(() =>
                    componentFactory.CreateRamSlot(decoderX.Outputs[x1], decoderY.Outputs[y1], set, enable, inputBus, outputBus));
            }
        }
    }

    public IRamSlot GetSlot(int x, int y)
    {
        return _lazySlots[y][x].Value;
    }
}