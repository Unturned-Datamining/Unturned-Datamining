using UnityEngine;

namespace SDG.Unturned;

public struct ReceivedChatMessage
{
    public SteamPlayer speaker;

    public string iconURL;

    public EChatMode mode;

    public Color color;

    public bool useRichTextFormatting;

    public string contents;

    public float receivedTimestamp;

    public float age => Time.time - receivedTimestamp;

    public ReceivedChatMessage(SteamPlayer speaker, string iconURL, EChatMode newMode, Color newColor, bool newRich, string newContents)
    {
        this.speaker = speaker;
        this.iconURL = iconURL;
        mode = newMode;
        color = newColor;
        useRichTextFormatting = newRich;
        contents = newContents;
        receivedTimestamp = Time.time;
    }
}
