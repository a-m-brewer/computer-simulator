using ComputerSimulator.Core.Exceptions;
using ComputerSimulator.Core.Extensions;

namespace ComputerSimulator.Core.Peripherals.Display.Text;

public static class GlyphRenderer
{
    public static void DrawCharacter(IDisplayAdapter displayAdapter, char character, int cellX, int cellY)
    {
        ValidateCell(displayAdapter, cellX, cellY);

        SelectDisplay(displayAdapter);

        var bytesPerRow = displayAdapter.Width / AsciiFont8x8.GlyphWidth;
        for (var row = 0; row < AsciiFont8x8.GlyphHeight; row++)
        {
            var displayByteAddress = (((cellY * AsciiFont8x8.GlyphHeight) + row) * bytesPerRow) + cellX;
            WriteAddress(displayAdapter, displayByteAddress);
            WriteData(displayAdapter, AsciiFont8x8.GetGlyphRow(character, row));
        }
    }

    public static void DrawString(IDisplayAdapter displayAdapter, string text, int cellX, int cellY)
    {
        ValidateCell(displayAdapter, cellX, cellY);

        var columns = displayAdapter.Width / AsciiFont8x8.GlyphWidth;
        var currentX = cellX;
        var currentY = cellY;

        foreach (var character in text)
        {
            if (character == '\n')
            {
                currentX = 0;
                currentY++;
                EnsureRowInRange(displayAdapter, currentY);
                continue;
            }

            if (currentX >= columns)
            {
                currentX = 0;
                currentY++;
            }

            DrawCharacter(displayAdapter, character, currentX, currentY);
            currentX++;
        }
    }

    private static void ValidateCell(IDisplayAdapter displayAdapter, int cellX, int cellY)
    {
        if (cellX < 0 || cellX >= displayAdapter.Width / AsciiFont8x8.GlyphWidth)
        {
            throw new ComputerSimulatorException($"Character column {cellX} is outside the display");
        }

        EnsureRowInRange(displayAdapter, cellY);
    }

    private static void EnsureRowInRange(IDisplayAdapter displayAdapter, int cellY)
    {
        if (cellY < 0 || ((cellY + 1) * AsciiFont8x8.GlyphHeight) > displayAdapter.Height)
        {
            throw new ComputerSimulatorException($"Character row {cellY} is outside the display");
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
