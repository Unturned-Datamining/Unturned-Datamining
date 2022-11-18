namespace SDG.Unturned;

public static class FloatExtension
{
    public static bool IsFinite(this float value)
    {
        if (!float.IsInfinity(value))
        {
            return !float.IsNaN(value);
        }
        return false;
    }
}
