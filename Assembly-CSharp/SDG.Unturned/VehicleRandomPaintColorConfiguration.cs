namespace SDG.Unturned;

internal class VehicleRandomPaintColorConfiguration : IDatParseable
{
    public float minSaturation;

    public float maxSaturation;

    public float minValue;

    public float maxValue;

    /// <summary>
    /// [0, 1] color will have zero saturation if random value is less than this. For example, 0.2 means 20% of
    /// vehicles will be grayscale.
    /// </summary>
    public float grayscaleChance;

    public bool TryParse(IDatNode node)
    {
        if (node is IDatDictionary dictionary)
        {
            return dictionary.TryParseFloat("MinSaturation", out minSaturation) & dictionary.TryParseFloat("MaxSaturation", out maxSaturation) & dictionary.TryParseFloat("MinValue", out minValue) & dictionary.TryParseFloat("MaxValue", out maxValue) & dictionary.TryParseFloat("GrayscaleChance", out grayscaleChance);
        }
        return false;
    }
}
