using System;

namespace SDG.Unturned;

public class NPCShortFlagReward : INPCReward
{
    public ushort id { get; protected set; }

    public short value { get; protected set; }

    public ENPCModificationType modificationType { get; protected set; }

    public override void GrantReward(Player player)
    {
        if (modificationType == ENPCModificationType.ASSIGN)
        {
            player.quests.sendSetFlag(id, value);
            return;
        }
        player.quests.getFlag(id, out var num);
        if (modificationType == ENPCModificationType.INCREMENT)
        {
            num += value;
        }
        else if (modificationType == ENPCModificationType.DECREMENT)
        {
            num -= value;
        }
        player.quests.sendSetFlag(id, num);
    }

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseUInt16("ID", out var num))
        {
            id = num;
        }
        else
        {
            p.ReportRequiredOptionInvalid("ID");
        }
        if (p.data.TryParseInt16("Value", out var num2))
        {
            value = num2;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
        if (p.data.TryParseEnum<ENPCModificationType>("Modification", out var eNPCModificationType))
        {
            modificationType = eNPCModificationType;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Modification");
        }
    }

    internal override void PopulateLegacy(in PopulateRewardParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryParseUInt16(p.legacyPrefix + "_ID", out var num))
        {
            id = num;
        }
        else
        {
            p.ReportRequiredOptionInvalid("ID");
        }
        if (p.data.TryParseInt16(p.legacyPrefix + "_Value", out var num2))
        {
            value = num2;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
        if (p.data.TryParseEnum<ENPCModificationType>(p.legacyPrefix + "_Modification", out var eNPCModificationType))
        {
            modificationType = eNPCModificationType;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Modification");
        }
    }

    public NPCShortFlagReward()
    {
    }

    [Obsolete]
    public NPCShortFlagReward(ushort newID, short newValue, ENPCModificationType newModificationType, string newText)
        : base(newText)
    {
        id = newID;
        value = newValue;
        modificationType = newModificationType;
    }
}
