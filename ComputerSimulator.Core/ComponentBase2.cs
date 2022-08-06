namespace ComputerSimulator.Core;

public abstract class ComponentBase2 : IComponent2
{
    public Guid Id { get; } = Guid.NewGuid();
}