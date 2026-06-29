using ComputerSimulator.Core.Peripherals.Keyboard;
using Terminal.Gui.Drivers;
using Terminal.Gui.Input;

namespace ComputerSimulator.Tui;

public static class TerminalKeyboardInput
{
    public static bool TryMapKey(Key key, out byte keycode)
    {
        if (key.KeyCode == KeyCode.Enter)
        {
            keycode = 13;
            return true;
        }

        if (key.KeyCode == KeyCode.Backspace || key.KeyCode == KeyCode.Delete)
        {
            keycode = 8;
            return true;
        }

        if (key.TryGetPrintableRune(out var rune) && rune.Value is >= 32 and <= 126)
        {
            keycode = (byte)rune.Value;
            return true;
        }

        keycode = 0;
        return false;
    }

    public static void PushMappedKey(IKeyboardInput keyboardInput, Key key)
    {
        if (TryMapKey(key, out var keycode))
        {
            keyboardInput.Push(keycode);
        }
    }
}
