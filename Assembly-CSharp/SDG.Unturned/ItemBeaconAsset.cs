namespace SDG.Unturned;

public class ItemBeaconAsset : ItemBarricadeAsset
{
    private ushort _wave;

    private byte _rewards;

    private ushort _rewardID;

    public ushort wave => _wave;

    public byte rewards => _rewards;

    public ushort rewardID => _rewardID;

    public bool ShouldScaleWithNumberOfParticipants { get; private set; }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _wave = data.ParseUInt16("Wave", 0);
        _rewards = data.ParseUInt8("Rewards", 0);
        _rewardID = data.ParseUInt16("Reward_ID", 0);
        ShouldScaleWithNumberOfParticipants = data.ParseBool("Enable_Participant_Scaling", defaultValue: true);
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Beacon");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Wave", wave);
        orAddDeclaration.Append("Rewards", rewards);
        orAddDeclaration.Append("Reward_ID", rewardID);
        orAddDeclaration.Append("Enable_Participant_Scaling", ShouldScaleWithNumberOfParticipants);
    }
}
