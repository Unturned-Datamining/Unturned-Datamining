namespace SDG.Unturned;

public static class EItemOriginEx
{
    public static string ToStringPascalCase(this EItemOrigin origin)
    {
        return origin switch
        {
            EItemOrigin.WORLD => "World", 
            EItemOrigin.ADMIN => "Admin", 
            EItemOrigin.CRAFT => "Craft", 
            EItemOrigin.NATURE => "Nature", 
            _ => origin.ToString(), 
        };
    }
}
