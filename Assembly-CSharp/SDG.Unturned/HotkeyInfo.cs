namespace SDG.Unturned;

public class HotkeyInfo
{
    /// <summary>
    /// Which item ID we thought was there. If the item ID currently at the coordinates doesn't match we clear this hotkey.
    /// </summary>
    public ushort id;

    public byte page;

    public byte x;

    public byte y;

    public HotkeyInfo()
    {
        id = 0;
        page = byte.MaxValue;
        x = byte.MaxValue;
        y = byte.MaxValue;
    }
}
