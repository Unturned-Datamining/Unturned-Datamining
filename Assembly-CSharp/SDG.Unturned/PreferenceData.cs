namespace SDG.Unturned;

public class PreferenceData
{
    public bool Allow_Ctrl_Shift_Alt_Salvage;

    public AudioPreferenceData Audio;

    public GraphicsPreferenceData Graphics;

    public ViewmodelPreferenceData Viewmodel;

    public ChatPreferenceData Chat;

    public PreferenceData()
    {
        Allow_Ctrl_Shift_Alt_Salvage = false;
        Audio = new AudioPreferenceData();
        Graphics = new GraphicsPreferenceData();
        Viewmodel = new ViewmodelPreferenceData();
        Chat = new ChatPreferenceData();
    }
}
