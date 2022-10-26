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
        IBus io,
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
                    componentFactory.CreateRamSlot(decoderX.Outputs[x1], decoderY.Outputs[y1], set, enable, io));
            }
        }
    }

    public IRamSlot GetSlot(int x, int y)
    {
        return _lazySlots[y][x].Value;
    }
}