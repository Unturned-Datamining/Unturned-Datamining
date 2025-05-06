namespace SDG.Unturned;

internal struct DatToken
{
    public EDatTokenType type;

    public string value;

    public DatToken(EDatTokenType type)
    {
        this.type = type;
        value = null;
    }

    public DatToken(EDatTokenType type, string value)
    {
        this.type = type;
        this.value = value;
    }

    public override string ToString()
    {
        if (value == null)
        {
            return $"(Type: {type}, Value: null)";
        }
        return $"(Type: {type}, Value: \"{value}\")";
    }
}
