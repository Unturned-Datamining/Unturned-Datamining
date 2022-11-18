using System;

namespace SDG.Unturned;

public class LegacyInputOutput : ICommandInputOutput
{
    public event CommandInputHandler inputCommitted;

    public virtual void initialize(CommandWindow commandWindow)
    {
    }

    public virtual void shutdown(CommandWindow commandWindow)
    {
    }

    public virtual void update()
    {
    }

    public virtual void outputInformation(string information)
    {
        outputToConsole(information, ConsoleColor.White);
    }

    public virtual void outputWarning(string warning)
    {
        outputToConsole(warning, ConsoleColor.Yellow);
    }

    public virtual void outputError(string error)
    {
        outputToConsole(error, ConsoleColor.Red);
    }

    protected virtual void outputToConsole(string value, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(value);
    }
}
