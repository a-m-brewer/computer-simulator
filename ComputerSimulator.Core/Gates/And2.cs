namespace ComputerSimulator.Core.Gates;

public class And2
{
    private bool _input;
    private readonly EventHandler<string> _propertyChanged;
    private bool _output;

    public And2()
    {
        _propertyChanged += PropertyChanged;
    }

    public bool Input
    {
        get => _input;
        set
        {
            _input = value;
            RaisePropertyChanged(nameof(Input));
        }
    }
    
    public bool Output
    {
        get => _output;
        set
        {
            _output = value;
            RaisePropertyChanged(nameof(Output));
        }
    }

    private void RaisePropertyChanged(string property)
    {
        _propertyChanged(this, property);
    }
    
    private void PropertyChanged(object? sender, string property)
    {
        switch (property)
        {
            case nameof(Input):
                Output = Input;
                break;
            case nameof(Output):
                break;
        }
    }
}