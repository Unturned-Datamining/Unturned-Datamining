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

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _mode = (EUseableTireMode)Enum.Parse(typeof(EUseableTireMode), data.GetString("Mode"), ignoreCase: true);
    }
}
