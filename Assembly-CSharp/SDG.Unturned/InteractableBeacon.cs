using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableBeacon : MonoBehaviour, IManualOnDestroy
{
    private ItemBeaconAsset asset;

    private byte nav;

    private bool wasInit;

    private float started;

    private int remaining;

    private int alive;

    private bool isRegistered;

    public bool isPlant
    {
        get
        {
            if (base.transform.parent != null)
            {
                return base.transform.parent.CompareTag("Vehicle");
            }
            return false;
        }
    }

    public int initialParticipants { get; private set; }

    public void updateState(ItemBarricadeAsset asset)
    {
        this.asset = (ItemBeaconAsset)asset;
    }

    public void init(int amount)
    {
        if (!wasInit)
        {
            if (amount >= asset.wave)
            {
                remaining = 0;
                alive = asset.wave;
            }
            else
            {
                remaining = asset.wave - amount;
                alive = amount;
            }
            wasInit = true;
        }
    }

    public int getRemaining()
    {
        return remaining;
    }

    public void spawnRemaining()
    {
        if (remaining > 0)
        {
            remaining--;
            alive++;
        }
    }

    public int getAlive()
    {
        return alive;
    }

    public void despawnAlive()
    {
        if (alive > 0)
        {
            alive--;
            if (remaining == 0 && alive == 0)
            {
                BarricadeManager.damage(base.transform, 10000f, 1f, armor: false, default(CSteamID), EDamageOrigin.Horde_Beacon_Self_Destruct);
            }
        }
    }

    private void Update()
    {
        if (!Provider.isServer || Time.realtimeSinceStartup - started < 3f)
        {
            return;
        }
        if (isRegistered)
        {
            for (int i = 0; i < Provider.clients.Count; i++)
            {
                SteamPlayer steamPlayer = Provider.clients[i];
                if (!(steamPlayer.player == null) && !(steamPlayer.player.movement == null) && !(steamPlayer.player.life == null) && !steamPlayer.player.life.isDead && steamPlayer.player.movement.nav == nav)
                {
                    return;
                }
            }
        }
        BarricadeManager.damage(base.transform, 10000f, 1f, armor: false, default(CSteamID), EDamageOrigin.Horde_Beacon_Self_Destruct);
    }

    private void Start()
    {
        started = Time.realtimeSinceStartup;
        Transform transform = base.transform.Find("Engine");
        if (transform != null)
        {
            transform.gameObject.SetActive(value: true);
        }
        if (Provider.isServer && !isRegistered && !isPlant && LevelNavigation.checkNavigation(base.transform.position))
        {
            LevelNavigation.tryGetNavigation(base.transform.position, out nav);
            if (asset.ShouldScaleWithNumberOfParticipants)
            {
                initialParticipants = BeaconManager.getParticipants(nav);
            }
            else
            {
                initialParticipants = 1;
            }
            BeaconManager.registerBeacon(nav, this);
            isRegistered = true;
        }
    }

    public void ManualOnDestroy()
    {
        if (!Provider.isServer || !isRegistered)
        {
            return;
        }
        BeaconManager.deregisterBeacon(nav, this);
        isRegistered = false;
        if (!wasInit || remaining > 0 || alive > 0)
        {
            return;
        }
        for (int i = 0; i < Provider.clients.Count; i++)
        {
            if (Provider.clients[i].player != null && !Provider.clients[i].player.life.isDead && Provider.clients[i].player.movement.nav == nav)
            {
                Provider.clients[i].player.quests.trackHordeKill();
            }
        }
        int rewards = asset.rewards;
        int num = Mathf.Max(1, initialParticipants);
        uint beacon_Max_Participants = Provider.modeConfigData.Zombies.Beacon_Max_Participants;
        if (beacon_Max_Participants != 0)
        {
            num = Mathf.Min(initialParticipants, (int)beacon_Max_Participants);
        }
        float num2 = Mathf.Sqrt(num);
        rewards = Mathf.CeilToInt((float)rewards * num2);
        rewards = Mathf.CeilToInt((float)rewards * Provider.modeConfigData.Zombies.Beacon_Rewards_Multiplier);
        uint beacon_Max_Rewards = Provider.modeConfigData.Zombies.Beacon_Max_Rewards;
        if (beacon_Max_Rewards != 0)
        {
            rewards = Mathf.Min(rewards, (int)beacon_Max_Rewards);
        }
        for (int j = 0; j < rewards; j++)
        {
            ushort num3 = SpawnTableTool.resolve(asset.rewardID);
            if (num3 != 0)
            {
                ItemManager.dropItem(new Item(num3, EItemOrigin.NATURE), base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
            }
        }
    }
}
