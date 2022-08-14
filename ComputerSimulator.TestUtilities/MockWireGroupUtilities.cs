using System.Collections.Generic;
using ComputerSimulator.Core.Parts;
using Moq;

namespace ComputerSimulator.TestUtilities;

public static class MockWireGroupUtilities
{
    public static void SetupWireGroupMock<T>(this Mock<IWireGroup<T>> wireGroup, IList<IWire2<T>> wires)
    {
        wireGroup.Setup(s => s.Count)
            .Returns(wires.Count);

        wireGroup.Setup(s => s.GetValue(It.IsAny<int>()))
            .Returns<int>(index => wires[index].Value);

        wireGroup.Setup(s => s.GetWire(It.IsAny<int>()))
            .Returns<int>(index => wires[index]);

        wireGroup.Setup(s => s.SetValue(It.IsAny<int>(), It.IsAny<T>()))
            .Callback<int, T>((index, value) => wires[index].Value = value);
    }
}