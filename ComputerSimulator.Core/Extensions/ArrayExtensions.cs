namespace ComputerSimulator.Core.Extensions;

public static class ArrayExtensions
{
    public static T[] Fill<T>(this T[] array, Func<T> factory)
    {
        return Fill(array, _ => factory());
    }
    
    public static T[] Fill<T>(this T[] array, Func<int, T> factory)
    {
        for (var i = 0; i < array.Length; i++)
        {
            array[i] = factory(i);
        }
        return array;
    }

    public static T[] Fill<T>(this T[] array) where T : new()
    {
        return array.Fill(() => new T());
    }
}