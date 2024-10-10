namespace SDG.Unturned;

public struct BbCodeToken
{
    public EBbCodeTokenType tokenType;

    public string tokenValue;

    public BbCodeToken(EBbCodeTokenType tokenType)
    {
        this.tokenType = tokenType;
        tokenValue = null;
    }

    public BbCodeToken(EBbCodeTokenType tokenType, string tokenValue)
    {
        this.tokenType = tokenType;
        this.tokenValue = tokenValue;
    }
}
