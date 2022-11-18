using System;

namespace SDG.Unturned;

public class CommandLineFlag
{
    public bool value;

    public string flag { get; protected set; }

    public bool defaultValue { get; protected set; }

    public static implicit operator bool(CommandLineFlag flag)
    {
        return flag.value;
    }

    public CommandLineFlag(bool defaultValue, string flag)
    {
        this.defaultValue = defaultValue;
        this.flag = flag;
        bool flag2 = Environment.CommandLine.IndexOf(flag, StringComparison.InvariantCultureIgnoreCase) >= 0;
        value = (flag2 ? (!defaultValue) : defaultValue);
    }
}
