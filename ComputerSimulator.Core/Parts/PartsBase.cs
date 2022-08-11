using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Services;

namespace ComputerSimulator.Core.Parts;

public abstract class PartsBase : CircuitBase
{
    protected PartsBase(IWireService wireService) : base(wireService)
    {
    }
}