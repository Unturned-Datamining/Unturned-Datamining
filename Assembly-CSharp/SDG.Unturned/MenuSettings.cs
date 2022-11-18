namespace SDG.Unturned;

public class MenuSettings
{
    private static bool hasLoaded;

    public static void load()
    {
        FilterSettings.load();
        PlaySettings.load();
        GraphicsSettings.load();
        ControlsSettings.load();
        OptionsSettings.load();
        hasLoaded = true;
    }

    public static void save()
    {
        if (hasLoaded)
        {
            FilterSettings.save();
            PlaySettings.save();
            GraphicsSettings.save();
            ControlsSettings.save();
            OptionsSettings.save();
        }
    }

    public static void SaveGraphicsIfLoaded()
    {
        if (hasLoaded)
        {
            GraphicsSettings.save();
        }
    }

    public static void SaveControlsIfLoaded()
    {
        if (hasLoaded)
        {
            ControlsSettings.save();
        }
    }

    public static void SaveOptionsIfLoaded()
    {
        if (hasLoaded)
        {
            OptionsSettings.save();
        }
    }
}
