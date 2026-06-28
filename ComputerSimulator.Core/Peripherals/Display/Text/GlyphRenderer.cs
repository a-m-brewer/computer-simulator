using ComputerSimulator.Core.Exceptions;
using ComputerSimulator.Core.Extensions;

namespace ComputerSimulator.Core.Peripherals.Display.Text;

public static class GlyphRenderer
{
    public static void DrawCharacter(IDisplayAdapter displayAdapter, char character, int cellX, int cellY)
    {
        if (cellX < 0 || cellX >= displayAdapter.Width / AsciiFont8x8.GlyphWidth)
        {
            throw new ComputerSimulatorException($"Character column {cellX} is outside the display");
        }

        if (cellY < 0 || ((cellY + 1) * AsciiFont8x8.GlyphHeight) > displayAdapter.Height)
        {
            throw new ComputerSimulatorException($"Character row {cellY} is outside the display");
        }

        SelectDisplay(displayAdapter);

        var bytesPerRow = displayAdapter.Width / AsciiFont8x8.GlyphWidth;
        for (var row = 0; row < AsciiFont8x8.GlyphHeight; row++)
        {
            var displayByteAddress = (((cellY * AsciiFont8x8.GlyphHeight) + row) * bytesPerRow) + cellX;
            WriteAddress(displayAdapter, displayByteAddress);
            WriteData(displayAdapter, AsciiFont8x8.GetGlyphRow(character, row));
        }
    }

    private static void SelectDisplay(IDisplayAdapter displayAdapter)
    {
        WriteAddress(displayAdapter, (int)IoAddress.Display);
    }

    private static void WriteAddress(IDisplayAdapter displayAdapter, int value)
    {
        displayAdapter.IoBus.CpuBus.SetValue(value);
        displayAdapter.IoBus.DataAddress.Value = true;
        displayAdapter.IoBus.InputOutput.Value = true;
        displayAdapter.IoBus.Clk.Set.Value = true;
        displayAdapter.Update();
        ClearStrobe(displayAdapter);
    }

    private static void WriteData(IDisplayAdapter displayAdapter, int value)
    {
        displayAdapter.IoBus.CpuBus.SetValue(value);
        displayAdapter.IoBus.DataAddress.Value = false;
        displayAdapter.IoBus.InputOutput.Value = true;
        displayAdapter.IoBus.Clk.Set.Value = true;
        displayAdapter.Update();
        ClearStrobe(displayAdapter);
    }

    private static void ClearStrobe(IDisplayAdapter displayAdapter)
    {
        displayAdapter.IoBus.Clk.Set.Value = false;
        displayAdapter.Update();
    }
}
