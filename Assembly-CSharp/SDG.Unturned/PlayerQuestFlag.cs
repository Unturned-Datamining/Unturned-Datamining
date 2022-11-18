namespace SDG.Unturned;

public class PlayerQuestFlag
{
    public short value;

    public ushort id { get; private set; }

    public PlayerQuestFlag(ushort newID, short newValue)
    {
        id = newID;
        value = newValue;
    }
}
