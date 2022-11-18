using System;

namespace SDG.Unturned;

public class ItemTireAsset : ItemVehicleRepairToolAsset
{
    private EUseableTireMode _mode;

    public EUseableTireMode mode => _mode;

    public override bool shouldFriendlySentryTargetUser => mode == EUseableTireMode.REMOVE;

    public override bool canBeUsedInSafezone(SafezoneNode safezone, bool byAdmin)
    {
        return mode == EUseableTireMode.ADD;
    }

    public ItemTireAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _mode = (EUseableTireMode)Enum.Parse(typeof(EUseableTireMode), data.readString("Mode"), ignoreCase: true);
    }
}
