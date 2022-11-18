using System;

namespace SDG.Unturned;

public class ConsoleInputOutput : ConsoleInputOutputBase
{
    protected string pendingInput = string.Empty;

    public override void update()
    {
        base.update();
        inputFromConsole();
    }

    protected void clearLine()
    {
        Console.CursorLeft = 0;
        Console.Write(new string(' ', Console.BufferWidth));
        Console.CursorTop--;
        Console.CursorLeft = 0;
    }

    protected void redrawInputLine()
    {
        if (Console.CursorLeft > 0)
        {
            clearLine();
        }
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(pendingInput);
    }

    protected override void outputToConsole(string value, ConsoleColor color)
    {
        if (Console.CursorLeft != 0)
        {
            clearLine();
        }
        base.outputToConsole(value, color);
        redrawInputLine();
    }

    protected virtual void inputFromConsole()
    {
        if (Console.KeyAvailable)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            onConsoleInputKey(keyInfo);
        }
    }

    protected virtual void onConsoleInputKey(ConsoleKeyInfo keyInfo)
    {
        switch (keyInfo.Key)
        {
        case ConsoleKey.Enter:
            onConsoleInputEnter();
            return;
        case ConsoleKey.Backspace:
            onConsoleInputBackspace();
            return;
        case ConsoleKey.Escape:
            onConsoleInputEscape();
            return;
        }
        if (keyInfo.KeyChar != 0)
        {
            pendingInput += keyInfo.KeyChar;
            redrawInputLine();
        }
    }

    protected virtual void onConsoleInputEnter()
    {
        string text = pendingInput;
        pendingInput = string.Empty;
        clearLine();
        outputInformation(">" + text);
        notifyInputCommitted(text);
    }

    protected virtual void onConsoleInputBackspace()
    {
        int length = pendingInput.Length;
        switch (length)
        {
        case 0:
            return;
        case 1:
            pendingInput = string.Empty;
            break;
        default:
            pendingInput = pendingInput.Substring(0, length - 1);
            break;
        }
        redrawInputLine();
    }

    protected virtual void onConsoleInputEscape()
    {
        if (pendingInput.Length >= 1)
        {
            pendingInput = string.Empty;
            clearLine();
        }
    }
}
