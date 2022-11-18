namespace SDG.Unturned;

public class LocalWorkshopSettings
{
    private static ILocalWorkshopSettings instance;

    public static ILocalWorkshopSettings get()
    {
        if (instance == null)
        {
            instance = new LocalWorkshopSettingsImplementation();
        }
        return instance;
    }
}
