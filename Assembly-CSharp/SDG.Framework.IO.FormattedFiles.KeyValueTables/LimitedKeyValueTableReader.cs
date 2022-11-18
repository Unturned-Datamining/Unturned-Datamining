using System.Collections.Generic;
using System.IO;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables;

public class LimitedKeyValueTableReader : KeyValueTableReader
{
    public string limit { get; protected set; }

    protected override bool canContinueReadDictionary(StreamReader input, Dictionary<string, object> scope)
    {
        if (dictionaryKey == limit)
        {
            return false;
        }
        return base.canContinueReadDictionary(input, scope);
    }

    public LimitedKeyValueTableReader()
    {
        limit = null;
    }

    public LimitedKeyValueTableReader(StreamReader input)
        : this(null, input)
    {
    }

    public LimitedKeyValueTableReader(string newLimit, StreamReader input)
        : base(input)
    {
        limit = newLimit;
    }
}
