using System.Text;

namespace SDG.Unturned;

public class NPCVolumeOverlapCondition : NPCLogicCondition
{
    /// <summary>
    /// Check volumes matching this ID.
    /// </summary>
    public string VolumeId { get; set; }

    /// <summary>
    /// Compare number of players in volume to this number.
    /// </summary>
    public int PlayerCount { get; set; }

    public override bool isConditionMet(Player player)
    {
        int a = VolumeManager<NPCOverlapVolume, NPCOverlapVolumeManager>.Get().CountPlayersInVolume(VolumeId);
        return doesLogicPass(a, PlayerCount);
    }

    public override void DebugDumpToStringBuilder(Player player, StringBuilder sb)
    {
        sb.Append("Is volume ID \"");
        sb.Append(VolumeId);
        sb.Append("\" player count ");
        int value = VolumeManager<NPCOverlapVolume, NPCOverlapVolumeManager>.Get().CountPlayersInVolume(VolumeId);
        sb.Append(value);
        sb.Append(' ');
        sb.Append(base.logicType.ToCharAbbr());
        sb.Append(' ');
        sb.Append(PlayerCount);
        sb.Append("? ");
        sb.Append(isConditionMet(player) ? "Yes" : "No");
    }

    internal override void PopulateV2(in PopulateConditionParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryGetString("VolumeID", out var value))
        {
            VolumeId = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("ID");
        }
        if (p.data.TryParseInt32("PlayerCount", out var value2))
        {
            PlayerCount = value2;
        }
        else
        {
            p.ReportRequiredOptionInvalid("PlayerCount");
        }
    }

    internal override void PopulateLegacy(in PopulateConditionParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryGetString(p.legacyPrefix + "_VolumeID", out var value))
        {
            VolumeId = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("VolumeID");
        }
        if (p.data.TryParseInt32(p.legacyPrefix + "_PlayerCount", out var value2))
        {
            PlayerCount = value2;
        }
        else
        {
            p.ReportRequiredOptionInvalid("PlayerCount");
        }
    }
}
