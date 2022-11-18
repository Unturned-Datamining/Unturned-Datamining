using System;

namespace SDG.Unturned;

public class FlagData
{
    private string _difficultyGUID;

    private ZombieDifficultyAsset cachedDifficulty;

    public byte maxZombies;

    public bool spawnZombies;

    public bool hyperAgro;

    public int maxBossZombies;

    public string difficultyGUID
    {
        get
        {
            return _difficultyGUID;
        }
        set
        {
            _difficultyGUID = value;
            try
            {
                difficulty = new AssetReference<ZombieDifficultyAsset>(new Guid(difficultyGUID));
            }
            catch
            {
                difficulty = AssetReference<ZombieDifficultyAsset>.invalid;
            }
        }
    }

    public AssetReference<ZombieDifficultyAsset> difficulty { get; private set; }

    public ZombieDifficultyAsset resolveDifficulty()
    {
        if (cachedDifficulty == null && difficulty.isValid)
        {
            cachedDifficulty = Assets.find(difficulty);
        }
        return cachedDifficulty;
    }

    public FlagData(string newDifficultyGUID = "", byte newMaxZombies = 64, bool newSpawnZombies = true, bool newHyperAgro = false, int maxBossZombies = -1)
    {
        difficultyGUID = newDifficultyGUID;
        maxZombies = newMaxZombies;
        spawnZombies = newSpawnZombies;
        hyperAgro = newHyperAgro;
        this.maxBossZombies = maxBossZombies;
    }
}
