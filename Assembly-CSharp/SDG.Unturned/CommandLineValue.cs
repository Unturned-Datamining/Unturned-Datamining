namespace SDG.Unturned;

/// <summary>
/// Parses -X=Y from command-line.
/// Ideally we could do "where T : TryParse" but for the meantime there are specialized subclasses.
/// </summary>
public abstract class CommandLineValue<T>
{
    public string key { get; protected set; }

    public bool hasValue { get; protected set; }

    public T value { get; protected set; }

    protected abstract bool tryParse(string stringValue);

    public CommandLineValue(string key)
    {
        this.key = key;
        hasValue = false;
        value = default(T);
        if (CommandLine.TryParseValue(key, out var text))
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                UnturnedLog.warn("Expected non-empty value for '{0}' on command-line", key);
            }
            else if (tryParse(text))
            {
                hasValue = true;
                UnturnedLog.info("Parsed '{0}' as '{1}' from command-line", key, value);
            }
            else
            {
                UnturnedLog.warn("Unable to parse '{0}' as '{1}' from command-line", key, text);
            }
        }
    }
}
