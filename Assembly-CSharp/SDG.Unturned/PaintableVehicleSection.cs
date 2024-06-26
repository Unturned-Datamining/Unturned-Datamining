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

    public bool TryParse(IDatNode node)
    {
        if (node is DatDictionary datDictionary)
        {
            path = datDictionary.GetString("Path");
            materialIndex = datDictionary.ParseInt32("MaterialIndex");
            return true;
        }
        return false;
    }
}
