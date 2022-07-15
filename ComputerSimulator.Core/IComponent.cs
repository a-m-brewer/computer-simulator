using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core;

public interface IInputComponent : IComponent
{
    void SetInputs(IBus bus);

    void SetInputWire(int index, IWire<bool> wire);

    void SetInputWireValue(int index, bool value);
    
    IWire<bool> GetInputWire(int index);
}

public interface IOutputComponent : IComponent
{
    void SetOutputs(IBus bus);

    void SetOutputWire(int index, IWire<bool> wire);

    IWire<bool> GetOutputWire(int index);
    
    bool GetOutputWireValue(int index);
}

public interface IWordComponent : IInputComponent, IOutputComponent
{
}

public interface IComponent : ILabel, IDisposable
{
    void SetInternalLabels(string label);
}