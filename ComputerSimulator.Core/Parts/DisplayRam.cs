using ComputerSimulator.Core.Factories;

namespace ComputerSimulator.Core.Parts;

public interface IDisplayRam : IPart
{
}

public class DisplayRam : PartsBase, IDisplayRam
{
    public DisplayRam(
        
        IComponentFactory componentFactory, IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
    }

    public void Update()
    {
        throw new NotImplementedException();
    }
}