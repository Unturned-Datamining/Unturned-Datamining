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

    public bool TryParseValue(string key, out string value)
    {
        if (string.IsNullOrEmpty(tokenValue))
        {
            value = null;
            return false;
        }
        return CommandLine.TryParseValue(tokenValue, key, out value);
    }

    /// <summary>
    /// Steam's new visual editor quotes value in [url=x] tag. If value is not quoted, this method returns as-is.
    /// If it IS quoted, this methods returns without quotation marks.
    /// </summary>
    public string GetUnquotedValue()
    {
        if (string.IsNullOrEmpty(tokenValue))
        {
            return tokenValue;
        }
        if (tokenValue.Length >= 2 && tokenValue.StartsWith('"') && tokenValue.EndsWith('"'))
        {
            return tokenValue.Substring(1, tokenValue.Length - 2);
        }
        return tokenValue;
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(tokenValue))
        {
            return tokenType.ToString();
        }
        return $"{tokenType}: {tokenValue}";
    }
}
