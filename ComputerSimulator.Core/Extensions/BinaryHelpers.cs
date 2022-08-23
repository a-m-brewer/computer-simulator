using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Extensions;

public static class BinaryHelpers
{
    public static bool[] ToBinaryBools(this int quotient, int padding)
    {
        var result = new bool[padding];
        var i = 0;

        while (quotient != 0)
        {
            quotient = Math.DivRem(quotient, 2, out var remainder);
            var state = remainder == 1;

            result[i] = state;
            i++;
        }

        return result;
    }

    public static int ToInt(this IWireGroup<bool> wireGroup)
    {
        var total = 0;
        for (var i = 0; i < wireGroup.Count; i++)
        {
            if (!wireGroup[i].Value)
            {
                continue;
            }

            total += (int) Math.Pow(2, i);
        }

        return total;
    }
}