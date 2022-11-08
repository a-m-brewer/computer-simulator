using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;

namespace ComputerSimulator.Core.Parts;

public interface IDisplayRam : IPart
{
    IRegister SetMar { get; }
    
    IRegister EnableMar { get; }
    
    IWire<bool> Set { get; }

    IWire<bool> Enable { get; }
    
    IWireGroup<bool> InputBus { get; }

    IWireGroup<bool> OutputBus { get; }
    
    IRamSlot GetSlot(int x, int y);
}

public class DisplayRam : PartsBase, IDisplayRam
{
    private readonly IDecoder _setDecoderX;
    private readonly IDecoder _setDecoderY;
    private readonly IDecoder _enableDecoderX;
    private readonly IDecoder _enableDecoderY;
    private readonly LazyRamSlotGroup _slots;

    public DisplayRam(
        IWire<bool> setMarSet,
        IWire<bool> enableMarSet,
        IWireGroup<bool> setMarInputBus,
        IWireGroup<bool> enableMarInputBus,
        IWire<bool> set,
        IWire<bool> enable,
        IWireGroup<bool> inputBus,
        IWireGroup<bool> outputBus,
        IComponentFactory componentFactory, IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        Set = set;
        Enable = enable;
        InputBus = inputBus;
        OutputBus = outputBus;

        SetMar = ComponentFactory
            .CreateRegister(setMarSet, WireFactory.PowerWire, setMarInputBus,
                WireFactory.CreateGroup<bool>(setMarInputBus.Count, $"{nameof(SetMar)}.{nameof(SetMar.Outputs)}"));

        var setMarDecoderSize = SetMar.Outputs.Count / 2;

        _setDecoderX = ComponentFactory.CreateDecoder(WireFactory.CreateGroup(setMarDecoderSize
            .InitArray<IWire<bool>>()
            .Fill(i => SetMar.Outputs[i])));
        _setDecoderY = ComponentFactory.CreateDecoder(WireFactory.CreateGroup(setMarDecoderSize
            .InitArray<IWire<bool>>()
            .Fill(i => SetMar.Outputs[i + setMarDecoderSize])));

        EnableMar = ComponentFactory
            .CreateRegister(enableMarSet, WireFactory.PowerWire, enableMarInputBus,
                WireFactory.CreateGroup<bool>(enableMarInputBus.Count, $"{nameof(EnableMar)}.{nameof(SetMar.Outputs)}"));
        
        var enableMarDecoderSize = EnableMar.Outputs.Count / 2;
        
        _enableDecoderX = ComponentFactory.CreateDecoder(WireFactory.CreateGroup(enableMarDecoderSize
            .InitArray<IWire<bool>>()
            .Fill(i => EnableMar.Outputs[i])));
        _enableDecoderY = ComponentFactory.CreateDecoder(WireFactory.CreateGroup(enableMarDecoderSize
            .InitArray<IWire<bool>>()
            .Fill(i => EnableMar.Outputs[i + enableMarDecoderSize])));

        _slots = new LazyRamSlotGroup(
            _setDecoderX,
            _setDecoderY,
            _enableDecoderX,
            _enableDecoderY,
            Set,
            Enable,
            InputBus,
            OutputBus,
            ComponentFactory);
    }

    public IRegister SetMar { get; }

    public IRegister EnableMar { get; }
    public IWire<bool> Set { get; }

    public IWire<bool> Enable { get; }

    public IWireGroup<bool> InputBus { get; }

    public IWireGroup<bool> OutputBus { get; }

    public IRamSlot GetSlot(int x, int y)
    {
        return _slots.GetSlot(x, y);
    }

    public void Update()
    {
        SetMar.Update();
        EnableMar.Update();
        
        _setDecoderX.Update();
        _setDecoderY.Update();
        
        _enableDecoderX.Update();
        _enableDecoderY.Update();
        
        GetSlot(_setDecoderX.EnabledIndex, _setDecoderY.EnabledIndex)
            .Update();

        GetSlot(_enableDecoderX.EnabledIndex, _enableDecoderY.EnabledIndex)
            .Update();
    }
}