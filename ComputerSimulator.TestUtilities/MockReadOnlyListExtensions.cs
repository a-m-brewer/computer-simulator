using System.Collections.Generic;
using Moq;

namespace ComputerSimulator.TestUtilities;

public static class MockReadOnlyListExtensions
{
    public static void SetupListMock<TClass, TListItem>(this Mock<TClass> list, IReadOnlyList<TListItem> actual) where TClass : class, IReadOnlyList<TListItem>
    {
        list.Setup(s => s.GetEnumerator())
            .Returns(actual.GetEnumerator);

        list.Setup(s => s[It.IsAny<int>()])
            .Returns<int>(i => actual[i]);

        list.Setup(s => s.Count)
            .Returns(actual.Count);
    }
}