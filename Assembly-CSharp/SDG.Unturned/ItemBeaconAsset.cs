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

    public ItemBeaconAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _wave = data.readUInt16("Wave", 0);
        _rewards = data.readByte("Rewards", 0);
        _rewardID = data.readUInt16("Reward_ID", 0);
        ShouldScaleWithNumberOfParticipants = data.readBoolean("Enable_Participant_Scaling", defaultValue: true);
    }
}
