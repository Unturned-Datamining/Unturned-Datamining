namespace SDG.NetTransport;

public abstract class TransportBase
{
    public delegate string GetMessageCallback(string key, params object[] args);

    public static GetMessageCallback OnGetMessage;

    public string GetMessageText(string key)
    {
        if (OnGetMessage != null)
        {
            return OnGetMessage(key);
        }
        return key;
    }

    public string GetMessageText(string key, params object[] args)
    {
        if (OnGetMessage != null)
        {
            return OnGetMessage(key, args);
        }
        return key;
    }
}
