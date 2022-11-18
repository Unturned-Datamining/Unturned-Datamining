namespace SDG.Unturned;

public class HotkeyInfo
{
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
