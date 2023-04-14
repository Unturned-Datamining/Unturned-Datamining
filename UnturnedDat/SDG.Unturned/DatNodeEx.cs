namespace SDG.Unturned;

public static class DatNodeEx
{
    public static bool TryParseStruct<T>(this IDatNode node, out T value) where T : struct, IDatParseable
    {
        value = default(T);
        return value.TryParse(node);
    }

    public static T ParseStruct<T>(this IDatNode node, T defaultValue = default(T)) where T : struct, IDatParseable
    {
        if (!node.TryParseStruct<T>(out var value))
        {
            return defaultValue;
        }
        return value;
    }
}
