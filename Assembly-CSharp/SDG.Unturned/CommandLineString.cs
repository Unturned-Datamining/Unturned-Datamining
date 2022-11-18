namespace SDG.Unturned;

public class CommandLineString : CommandLineValue<string>
{
    public CommandLineString(string key)
        : base(key)
    {
    }

    protected override bool tryParse(string stringValue)
    {
        base.value = stringValue;
        return true;
    }
}
