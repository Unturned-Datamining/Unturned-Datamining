namespace SDG.Provider;

public class UnturnedEconInfo
{
    public enum EQuality
    {
        None,
        Common,
        Uncommon,
        Gold,
        Rare,
        Epic,
        Legendary,
        Mythical,
        Premium,
        Achievement
    }

    public enum ERarity
    {
        Common,
        Uncommon,
        Achievement,
        Unknown,
        Gold,
        Premium,
        Rare,
        Epic,
        Legendary,
        Mythical
    }

    public string name;

    public string type;

    public string description;

    public string name_color;

    public int itemdefid;

    public bool marketable;

    public int scraps;

    public int item_id;

    public int item_skin;

    public int item_effect;

    public int vehicle_id;

    public EQuality quality;

    public UnturnedEconInfo()
    {
        name = "";
        type = "";
        description = "";
        name_color = "";
        itemdefid = 0;
        scraps = 0;
        item_id = 0;
        item_skin = 0;
        item_effect = 0;
        vehicle_id = 0;
        quality = EQuality.None;
    }
}
