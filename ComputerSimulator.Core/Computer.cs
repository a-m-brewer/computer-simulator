namespace ComputerSimulator.Core;

public interface IComputer : IDisposable
{
    public void Run();
}

public class Computer  : IComputer
{
    public void Run()
    {
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}