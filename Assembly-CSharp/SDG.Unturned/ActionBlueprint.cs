using System;

namespace SDG.Unturned;

public class ActionBlueprint
{
    internal int index;

    internal string blueprintName;

    internal bool _isLink;

    /// <summary>
    /// Index into Blueprints list. -1 means blueprint name is used instead.
    /// </summary>
    public int Index => index;

    /// <summary>
    /// Name to look for in Blueprints list.
    /// </summary>
    public string BlueprintName => blueprintName;

    public bool isLink => _isLink;

    [Obsolete("Renamed to Index")]
    public byte id => (byte)index;

    public Blueprint FindBlueprint(IBlueprintOwner blueprintOwner)
    {
        if (index >= 0)
        {
            return blueprintOwner.GetBlueprintByIndex(index);
        }
        if (!string.IsNullOrEmpty(blueprintName))
        {
            return blueprintOwner.FindBlueprintByName(blueprintName);
        }
        return null;
    }

    public override string ToString()
    {
        return $"(Index: {index} Name: {blueprintName} Link: {_isLink})";
    }

    public ActionBlueprint(int newIndex, bool newLink)
    {
        index = newIndex;
        _isLink = newLink;
    }
}
