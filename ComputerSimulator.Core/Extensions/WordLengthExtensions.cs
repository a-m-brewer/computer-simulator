using ComputerSimulator.Core.Models;

namespace ComputerSimulator.Core.Extensions;

public static class WordLengthExtensions
{
    public static T[] InitArray<T>(this ComputerSettings settings)
    {
        return new T[settings.WordSize];
    }
}