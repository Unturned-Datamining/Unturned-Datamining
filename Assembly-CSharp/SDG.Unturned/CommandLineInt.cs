namespace SDG.Unturned;

public class CommandLineInt : CommandLineValue<int>
{
    public CommandLineInt(string key)
        : base(key)
    {
    }

    protected override bool tryParse(string stringValue)
    {
        if (int.TryParse(stringValue, out var result))
        {
            base.value = result;
            return true;
        }
        return false;
    }
}
