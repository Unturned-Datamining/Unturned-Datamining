using System.Collections;
using System.Collections.Generic;

namespace SDG.Unturned;

public interface IEditableDatDictionary : IDatDictionary, IDatNode, IEnumerable<KeyValuePair<string, IDatNode>>, IEnumerable, IEditableDatNode
{
    IEditableDatValue AddValue(string key);

    IEditableDatList AddList(string key);

    IEditableDatDictionary AddDictionary(string key);

    IEditableDatValue ReplaceWithValue(string key);

    IEditableDatList ReplaceWithList(string key);

    IEditableDatDictionary ReplaceWithDictionary(string key);

    bool Remove(string key);

    bool Remove(string key, out IDatNode node);
}
