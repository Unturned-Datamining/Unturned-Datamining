namespace SDG.Unturned;

public static class GlazierFactory
{
    private static CommandLineString clImpl = new CommandLineString("-Glazier");

    public static void Create()
    {
        if (clImpl.hasValue)
        {
            string value = clImpl.value;
            if (string.Equals(value, "IMGUI"))
            {
                Glazier.instance = Glazier_IMGUI.CreateGlazier();
                return;
            }
            if (string.Equals(value, "uGUI"))
            {
                Glazier.instance = Glazier_uGUI.CreateGlazier();
                return;
            }
            if (string.Equals(value, "UIToolkit"))
            {
                Glazier.instance = Glazier_UIToolkit.CreateGlazier();
                return;
            }
            UnturnedLog.warn("Unknown glazier implementation \"{0}\"", value);
        }
        Glazier.instance = Glazier_uGUI.CreateGlazier();
    }
}
