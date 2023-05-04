namespace SDG.Unturned;

public static class SleekShadowStyle
{
    private static ETextContrastStyle[] contextToStyleLookupTable;

    public static ETextContrastStyle ContextToStyle(ETextContrastContext context)
    {
        return contextToStyleLookupTable[(int)context];
    }

    private static ETextContrastStyle UncachedContextToStyle(ETextContrastContext context)
    {
        return context switch
        {
            ETextContrastContext.InconspicuousBackdrop => SleekCustomization.inconspicuousTextContrast switch
            {
                ETextContrastPreference.None => ETextContrastStyle.None, 
                ETextContrastPreference.Outline => ETextContrastStyle.Outline, 
                _ => ETextContrastStyle.Shadow, 
            }, 
            ETextContrastContext.ColorfulBackdrop => SleekCustomization.colorfulTextContrast switch
            {
                ETextContrastPreference.None => ETextContrastStyle.None, 
                ETextContrastPreference.Shadow => ETextContrastStyle.Shadow, 
                _ => ETextContrastStyle.Outline, 
            }, 
            ETextContrastContext.Tooltip => ETextContrastStyle.Tooltip, 
            _ => SleekCustomization.defaultTextContrast switch
            {
                ETextContrastPreference.Shadow => ETextContrastStyle.Shadow, 
                ETextContrastPreference.Outline => ETextContrastStyle.Outline, 
                _ => ETextContrastStyle.None, 
            }, 
        };
    }

    static SleekShadowStyle()
    {
        contextToStyleLookupTable = new ETextContrastStyle[4];
        for (int i = 0; i < 4; i++)
        {
            ETextContrastStyle eTextContrastStyle = UncachedContextToStyle((ETextContrastContext)i);
            contextToStyleLookupTable[i] = eTextContrastStyle;
        }
    }
}
