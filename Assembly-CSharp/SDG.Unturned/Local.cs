namespace SDG.Unturned;

public class Local
{
    private Data data;

    private Data fallbackData;

    public string read(string key)
    {
        if (data != null)
        {
            return data.readString(key);
        }
        return null;
    }

    public string format(string key)
    {
        if (TryReadString(key, out var text))
        {
            return text;
        }
        return key;
    }

    public string format(string key, object arg0)
    {
        if (TryReadString(key, out var text))
        {
            try
            {
                return string.Format(text, arg0);
            }
            catch
            {
                UnturnedLog.error($"Caught localization string formatting exception (key: \"{key}\" text: \"{text}\" arg0: \"{arg0}\")");
                return key;
            }
        }
        return key;
    }

    public string format(string key, object arg0, object arg1)
    {
        if (TryReadString(key, out var text))
        {
            try
            {
                return string.Format(text, arg0, arg1);
            }
            catch
            {
                UnturnedLog.error($"Caught localization string formatting exception (key: \"{key}\" text: \"{text}\" arg0: \"{arg0}\" arg1: \"{arg1}\")");
                return key;
            }
        }
        return key;
    }

    public string format(string key, object arg0, object arg1, object arg2)
    {
        if (TryReadString(key, out var text))
        {
            try
            {
                return string.Format(text, arg0, arg1, arg2);
            }
            catch
            {
                UnturnedLog.error($"Caught localization string formatting exception (key: \"{key}\" text: \"{text}\" arg0: \"{arg0}\" arg1: \"{arg1}\" arg2: \"{arg2}\")");
                return key;
            }
        }
        return key;
    }

    public string format(string key, params object[] args)
    {
        if (TryReadString(key, out var text))
        {
            try
            {
                return string.Format(text, args);
            }
            catch
            {
                string text2 = string.Empty;
                for (int i = 0; i < args.Length; i++)
                {
                    if (text2.Length > 0)
                    {
                        text2 += " ";
                    }
                    text2 += $"arg{i}: \"{args[i]}\"";
                }
                UnturnedLog.error("Caught localization string formatting exception (key: \"" + key + "\" text: \"" + text + "\" " + text2 + ")");
                return key;
            }
        }
        return key;
    }

    public bool has(string key)
    {
        if (data != null)
        {
            return data.has(key);
        }
        return false;
    }

    public Local(Data newData)
        : this(newData, null)
    {
    }

    public Local(Data data, Data fallbackData)
    {
        this.data = data;
        this.fallbackData = fallbackData;
    }

    public Local()
    {
        data = null;
    }

    private bool TryReadString(string key, out string text)
    {
        text = null;
        if (data == null || !data.TryReadString(key, out text) || string.IsNullOrEmpty(text))
        {
            if (fallbackData != null && fallbackData.TryReadString(key, out text))
            {
                return !string.IsNullOrEmpty(text);
            }
            return false;
        }
        return true;
    }
}
