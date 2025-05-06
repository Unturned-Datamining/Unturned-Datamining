using System.Collections;
using System.Collections.Generic;

namespace SDG.Unturned;

public interface IDatDictionary : IDatNode, IEnumerable<KeyValuePair<string, IDatNode>>, IEnumerable
{
    int Count { get; }

    IDatNode this[string key] { get; }

    bool ContainsKey(string key);

    bool TryGetNode(string key, out IDatNode node);

    bool TryGetKeyLineNumber(string key, out int lineNumber);

    IEditableDatDictionary Edit();
}
