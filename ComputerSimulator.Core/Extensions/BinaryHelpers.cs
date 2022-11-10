using System.Text;
using ComputerSimulator.Core.Exceptions;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Extensions;

public static class BinaryHelpers
{
    public static bool[] ToBinaryBools(this char quotient)
    {
        var i = (int)quotient;

        if (i > 127)
        {
            throw new ComputerSimulatorException($"NON ASCII: {quotient}");
        }

        return i.ToBinaryBools(8);
    }
    
    public static bool[] ToBinaryBools(this Enum quotient, int padding)
    {
        return Convert.ToInt32(quotient).ToBinaryBools(padding);
    }
    
    public static bool[] ToBinaryBools(this int quotient, int padding = 1)
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
    
    public static int ToInt(this IList<bool> inputs)
    {
        var total = 0;
        for (var i = 0; i < inputs.Count; i++)
        {
            if (!inputs[i])
            {
                continue;
            }

            total += (int) Math.Pow(2, i);
        }

        return total;
    }
}