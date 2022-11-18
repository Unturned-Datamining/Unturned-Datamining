using SDG.Unturned;

namespace SDG.Framework.Devkit;

public class NavClipVolume : LevelVolume<NavClipVolume, NavClipVolumeManager>
{
    protected override void Awake()
    {
        forceShouldAddCollider = true;
        base.Awake();
        volumeCollider.isTrigger = false;
        base.gameObject.layer = 22;
    }
}
