using System;

namespace SDG.Unturned;

public class CommandLineBool : CommandLineValue<bool>
{
    public CommandLineBool(string key)
        : base(key)
    {
    }

    protected override bool tryParse(string stringValue)
    {
        if (stringValue.Equals("y", StringComparison.InvariantCultureIgnoreCase) || stringValue.Equals("yes", StringComparison.InvariantCultureIgnoreCase) || stringValue == "1" || stringValue.Equals("true", StringComparison.InvariantCultureIgnoreCase))
        {
            base.value = true;
            return true;
        }
        if (stringValue.Equals("n", StringComparison.InvariantCultureIgnoreCase) || stringValue.Equals("no", StringComparison.InvariantCultureIgnoreCase) || stringValue == "0" || stringValue.Equals("false", StringComparison.InvariantCultureIgnoreCase))
        {
            base.value = false;
            return true;
        }
        return false;
    }
}
