namespace SDG.Unturned;

public class ChatPreferenceData
{
    public const int DEFAULT_HISTORY_LENGTH = 16;

    public float Fade_Delay;

    public int History_Length;

    public int Preview_Length;

    public ChatPreferenceData()
    {
        Fade_Delay = 10f;
        History_Length = 16;
        Preview_Length = 5;
    }
}
