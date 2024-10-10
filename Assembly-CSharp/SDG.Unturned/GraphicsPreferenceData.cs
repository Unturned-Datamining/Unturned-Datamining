namespace SDG.Unturned;

public class GraphicsPreferenceData
{
    public bool Use_Lens_Dirt;

    public float Chromatic_Aberration_Intensity;

    public float LOD_Bias;

    public int Override_Resolution_Width;

    public int Override_Resolution_Height;

    public float Override_UI_Scale;

    public int Override_Fullscreen_Mode;

    public int Override_Refresh_Rate;

    public float Override_Vertical_Field_Of_View;

    public ETextContrastPreference Default_Text_Contrast;

    public ETextContrastPreference Inconspicuous_Text_Contrast;

    public ETextContrastPreference Colorful_Text_Contrast;

    public GraphicsPreferenceData()
    {
        Use_Lens_Dirt = true;
        Chromatic_Aberration_Intensity = 0.2f;
        LOD_Bias = 0f;
        Override_Resolution_Width = -1;
        Override_Resolution_Height = -1;
        Override_Refresh_Rate = -1;
        Override_UI_Scale = -1f;
        Override_Fullscreen_Mode = -1;
        Override_Vertical_Field_Of_View = -1f;
        Inconspicuous_Text_Contrast = ETextContrastPreference.Default;
        Colorful_Text_Contrast = ETextContrastPreference.Default;
    }
}
