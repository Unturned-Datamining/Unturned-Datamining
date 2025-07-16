namespace SDG.Unturned;

public struct PaintableVehicleSection : IDatParseable
{
    /// <summary>
    /// Scene hierarchy path relative to vehicle root.
    /// </summary>
    public string path;

    /// <summary>
    /// Index in renderer's materials array.
    /// </summary>
    public int materialIndex;

    /// <summary>
    /// If true, apply to every item in renderer's materials array.
    /// </summary>
    public bool allMaterials;

    public bool TryParse(IDatNode node)
    {
        if (node is DatValue datValue)
        {
            path = datValue.value;
            materialIndex = -1;
            allMaterials = true;
            return !string.IsNullOrEmpty(path);
        }
        if (node is IDatDictionary dictionary)
        {
            path = dictionary.GetString("Path");
            materialIndex = dictionary.ParseInt32("MaterialIndex");
            allMaterials = dictionary.ParseBool("AllMaterials");
            return true;
        }
        return false;
    }
}
