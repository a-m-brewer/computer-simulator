using ComputerSimulator.Core.Factories;

namespace ComputerSimulator.Core;

public abstract class ComponentBase : IComponent
{
    protected ComponentBase(IWireCupboard wireCupboard)
    {
        WireCupboard = wireCupboard;
    }
    
    protected IWireCupboard WireCupboard { get; }
    
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public virtual void SetInternalLabels(string label)
    {
    }

    public virtual string Label { get; set; } = nameof(ComponentBase);
}