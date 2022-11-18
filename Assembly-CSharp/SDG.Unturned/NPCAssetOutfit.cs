using System;

namespace SDG.Unturned;

public class NPCAssetOutfit
{
    public Guid shirtGuid;

    public Guid pantsGuid;

    public Guid hatGuid;

    public Guid backpackGuid;

    public Guid vestGuid;

    public Guid maskGuid;

    public Guid glassesGuid;

    [Obsolete]
    public ushort shirt { get; protected set; }

    [Obsolete]
    public ushort pants { get; protected set; }

    [Obsolete]
    public ushort hat { get; protected set; }

    [Obsolete]
    public ushort backpack { get; protected set; }

    [Obsolete]
    public ushort vest { get; protected set; }

    [Obsolete]
    public ushort mask { get; protected set; }

    [Obsolete]
    public ushort glasses { get; protected set; }

    public NPCAssetOutfit(Data data, ENPCHoliday holiday)
    {
        string text = holiday switch
        {
            ENPCHoliday.HALLOWEEN => "Halloween_", 
            ENPCHoliday.CHRISTMAS => "Christmas_", 
            _ => "", 
        };
        shirt = data.ReadGuidOrLegacyId(text + "Shirt", out shirtGuid);
        pants = data.ReadGuidOrLegacyId(text + "Pants", out pantsGuid);
        hat = data.ReadGuidOrLegacyId(text + "Hat", out hatGuid);
        backpack = data.ReadGuidOrLegacyId(text + "Backpack", out backpackGuid);
        vest = data.ReadGuidOrLegacyId(text + "Vest", out vestGuid);
        mask = data.ReadGuidOrLegacyId(text + "Mask", out maskGuid);
        glasses = data.ReadGuidOrLegacyId(text + "Glasses", out glassesGuid);
    }
}
