namespace SDG.Unturned;

public class ArenaCompactorVolume : LevelVolume<ArenaCompactorVolume, ArenaCompactorVolumeManager>
{
    protected override void Awake()
    {
        supportsBoxShape = false;
        base.Awake();
    }
}
