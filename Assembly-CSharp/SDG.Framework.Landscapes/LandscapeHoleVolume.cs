using SDG.Unturned;

namespace SDG.Framework.Landscapes;

public class LandscapeHoleVolume : LevelVolume<LandscapeHoleVolume, LandscapeHoleVolumeManager>
{
    protected override void Awake()
    {
        supportsSphereShape = false;
        base.Awake();
    }
}
