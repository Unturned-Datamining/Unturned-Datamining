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
        if (node is DatDictionary datDictionary)
        {
            return datDictionary.TryParseFloat("MinSaturation", out minSaturation) & datDictionary.TryParseFloat("MaxSaturation", out maxSaturation) & datDictionary.TryParseFloat("MinValue", out minValue) & datDictionary.TryParseFloat("MaxValue", out maxValue) & datDictionary.TryParseFloat("GrayscaleChance", out grayscaleChance);
        }
        return false;
    }
}
