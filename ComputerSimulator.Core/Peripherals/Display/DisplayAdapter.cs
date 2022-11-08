using ComputerSimulator.Core.Factories;

namespace ComputerSimulator.Core.Peripherals.Display;

public interface IDisplayAdapter : IAdapter
{
}

public class DisplayAdapter : AdapterBase, IDisplayAdapter
{
    public DisplayAdapter(
        IComponentFactory componentFactory, IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
    }

    public void Update()
    {
        throw new NotImplementedException();
    }
}