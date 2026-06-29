using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Peripherals.Keyboard;

public interface IKeyboardAdapter : IAdapter
{
    IIoBus IoBus { get; }
    
    IWireGroup<bool> Input { get; }
}

public class KeyboardAdapter : AdapterBase, IKeyboardAdapter
{
    private readonly IKeyboardInput _keyboardInput;
    private readonly IAnd _and1;
    private readonly INot[] _nots;
    private IAnd _and2;
    private readonly INot _dataAddressNot;
    private readonly INot _inputOutputNot;
    private IAnd _and3;
    private readonly IMemoryBit _memoryBit;
    private readonly IAnd2 _and4;
    private readonly IRegister _keycodeRegister;
    private bool _readActive;

    public KeyboardAdapter(
        IIoBus ioBus,
        IWireGroup<bool> input,
        IKeyboardInput keyboardInput,
        IComponentFactory componentFactory, IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        IoBus = ioBus;
        Input = input;
        _keyboardInput = keyboardInput;

        _nots = 4
            .InitArray<INot>()
            .Fill(i => ComponentFactory.CreateNot(
                IoBus.CpuBus[i + 4], WireFactory.CreateWire<bool>($"{nameof(_nots)}[{i}]-output")));


        _and1 = ComponentFactory.CreateAnd(
            WireFactory.CreateGroup(
                IoBus.CpuBus[0],
                IoBus.CpuBus[1],
                IoBus.CpuBus[2],
                IoBus.CpuBus[3],
                _nots[0].Output,
                _nots[1].Output,
                _nots[2].Output,
                _nots[3].Output),
            WireFactory.CreateWire<bool>($"{nameof(_and1)}-output"));

        _and2 = ComponentFactory.CreateAnd(
            WireFactory.CreateGroup(IoBus.Clk.Set, IoBus.DataAddress, IoBus.InputOutput),
            WireFactory.CreateWire<bool>($"{nameof(_and2)}-output"));

        _dataAddressNot = ComponentFactory
            .CreateNot(IoBus.DataAddress, WireFactory.CreateWire<bool>($"{nameof(_dataAddressNot)}-output"));
        _inputOutputNot = ComponentFactory
            .CreateNot(IoBus.InputOutput, WireFactory.CreateWire<bool>($"{nameof(_inputOutputNot)}-output"));
        _and3 = ComponentFactory
            .CreateAnd(
                WireFactory.CreateGroup(
                    IoBus.Clk.Enable,
                    _dataAddressNot.Output,
                    _inputOutputNot.Output),
                WireFactory.CreateWire<bool>($"{nameof(_and3)}-output"));

        _memoryBit = ComponentFactory
            .CreateMemoryBit(
                _and1.Output,
                WireFactory.CreateWire<bool>(),
                _and2.Output
            );

        _and4 = ComponentFactory.CreateAnd2(
            _memoryBit.Output,
            _and3.Output,
            WireFactory.CreateWire<bool>($"{nameof(_and4)}-output"));

        _keycodeRegister = ComponentFactory
            .CreateRegister(
                _and4.Output,
                _and4.Output,
                Input,
                IoBus.CpuBus);
    }
    
    public IIoBus IoBus { get; }

    public IWireGroup<bool> Input { get; }

    public void Update()
    {
        _nots.Update();
        _and1.Update();
        _and2.Update();
        
        _dataAddressNot.Update();
        _inputOutputNot.Update();
        _and3.Update();
        
        _memoryBit.Update();
        
        _and4.Update();

        // A single `In` read keeps the read-enable line (_and4) high for many
        // update cycles. Dequeue exactly one keycode on the rising edge of the
        // window and hold it for the whole window; only re-arm once the window
        // ends. Re-arming mid-window would dequeue a second key that the CPU
        // never latches, silently dropping it when keys are typed quickly.
        if (_and4.Output.Value)
        {
            if (!_readActive)
            {
                Input.SetValue(_keyboardInput.TryRead(out var keycode) ? keycode : 0);
                _readActive = true;
            }
        }
        else if (_readActive)
        {
            Input.Reset();
            _keycodeRegister.Reset();
            _readActive = false;
        }

        _keycodeRegister.Update();
    }
}
