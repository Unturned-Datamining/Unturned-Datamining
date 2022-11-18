namespace SDG.Unturned;

public class CommandLineFloat : CommandLineValue<float>
{
    public CommandLineFloat(string key)
        : base(key)
    {
    }

    protected override bool tryParse(string stringValue)
    {
        if (float.TryParse(stringValue, out var result))
        {
            base.value = result;
            return true;
        }
        return false;
    }
}
