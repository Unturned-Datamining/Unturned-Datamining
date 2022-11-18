namespace SDG.Unturned;

public static class PluginAdvertising
{
    public static IPluginAdvertising Get()
    {
        return SteamPluginAdvertising.Get();
    }
}
