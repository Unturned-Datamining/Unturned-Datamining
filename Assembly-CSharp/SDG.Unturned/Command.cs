using System;
using Steamworks;

namespace SDG.Unturned;

public class Command : IComparable<Command>
{
    protected Local localization;

    protected string _command;

    protected string _info;

    protected string _help;

    public string command => _command;

    public string info => _info;

    public string help => _help;

    protected virtual void execute(CSteamID executorID, string parameter)
    {
    }

    public virtual bool check(CSteamID executorID, string method, string parameter)
    {
        if (method.ToLower() == command.ToLower())
        {
            execute(executorID, parameter);
            return true;
        }
        return false;
    }

    public int CompareTo(Command other)
    {
        return command.CompareTo(other.command);
    }
}
