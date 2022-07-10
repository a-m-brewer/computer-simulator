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
}