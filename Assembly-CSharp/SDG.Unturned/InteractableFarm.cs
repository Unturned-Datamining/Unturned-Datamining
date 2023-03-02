using System;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableFarm : Interactable
{
    public delegate void HarvestRequestHandler(InteractableFarm harvestable, SteamPlayer instigatorPlayer, ref bool shouldAllow);

    private uint _planted;

    private uint _growth;

    private ushort _grow;

    private Guid growSpawnTableGuid;

    private bool isGrown;

    public uint harvestRewardExperience;

    private bool isAffectedByAgricultureSkill;

    internal static readonly ClientInstanceMethod<uint> SendPlanted = ClientInstanceMethod<uint>.Get(typeof(InteractableFarm), "ReceivePlanted");

    private static readonly ServerInstanceMethod SendHarvestRequest = ServerInstanceMethod.Get(typeof(InteractableFarm), "ReceiveHarvestRequest");

    public uint planted => _planted;

    public uint growth => _growth;

    public ushort grow => _grow;

    public bool canFertilize { get; protected set; }

    public bool IsFullyGrown
    {
        get
        {
            if (planted != 0 && Provider.time > planted)
            {
                return Provider.time - planted > growth;
            }
            return false;
        }
    }

    public static event HarvestRequestHandler OnHarvestRequested_Global;

    public void updatePlanted(uint newPlanted)
    {
        _planted = newPlanted;
    }

    public override void updateState(Asset asset, byte[] state)
    {
        ItemFarmAsset itemFarmAsset = asset as ItemFarmAsset;
        _growth = itemFarmAsset.growth;
        _grow = itemFarmAsset.grow;
        growSpawnTableGuid = itemFarmAsset.growSpawnTableGuid;
        isAffectedByAgricultureSkill = itemFarmAsset.isAffectedByAgricultureSkill;
        if (state.Length >= 4)
        {
            _planted = BitConverter.ToUInt32(state, 0);
        }
        else
        {
            _planted = 0u;
        }
        canFertilize = ((ItemFarmAsset)asset).canFertilize;
        harvestRewardExperience = ((ItemFarmAsset)asset).harvestRewardExperience;
        if (isGrown)
        {
            isGrown = false;
            base.transform.Find("Foliage_0")?.gameObject.SetActive(value: true);
            base.transform.Find("Foliage_1")?.gameObject.SetActive(value: false);
        }
    }

    public bool checkFarm()
    {
        return IsFullyGrown;
    }

    public override bool checkUseable()
    {
        return checkFarm();
    }

    public override void use()
    {
        ClientHarvest();
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        if (checkUseable())
        {
            message = EPlayerMessage.FARM;
        }
        else
        {
            message = EPlayerMessage.GROW;
        }
        text = "";
        color = Color.white;
        return true;
    }

    private void onRainUpdated(ELightingRain rain)
    {
        if (rain == ELightingRain.POST_DRIZZLE && !Physics.Raycast(base.transform.position + Vector3.up, Vector3.up, 32f, RayMasks.BLOCK_WIND))
        {
            updatePlanted(1u);
            if (Provider.isServer)
            {
                BarricadeManager.updateFarm(base.transform, planted, shouldSend: false);
            }
        }
    }

    private void Update()
    {
        if (!Dedicator.IsDedicatedServer && !isGrown && checkFarm())
        {
            isGrown = true;
            Transform transform = base.transform.Find("Foliage_0");
            if (transform != null)
            {
                transform.gameObject.SetActive(value: false);
            }
            Transform transform2 = base.transform.Find("Foliage_1");
            if (transform2 != null)
            {
                transform2.gameObject.SetActive(value: true);
            }
        }
    }

    private void OnEnable()
    {
        LightingManager.onRainUpdated = (RainUpdated)Delegate.Combine(LightingManager.onRainUpdated, new RainUpdated(onRainUpdated));
    }

    private void OnDisable()
    {
        LightingManager.onRainUpdated = (RainUpdated)Delegate.Remove(LightingManager.onRainUpdated, new RainUpdated(onRainUpdated));
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceivePlanted(uint newPlanted)
    {
        updatePlanted(newPlanted);
    }

    public void ClientHarvest()
    {
        SendHarvestRequest.Invoke(GetNetId(), ENetReliability.Unreliable);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 10)]
    public void ReceiveHarvestRequest(in ServerInvocationContext context)
    {
        Player player = context.GetPlayer();
        if (player == null || player.life.isDead || (base.transform.position - player.transform.position).sqrMagnitude > 400f || !BarricadeManager.tryGetRegion(base.transform, out var x, out var y, out var plant, out var region))
        {
            return;
        }
        bool shouldAllow = true;
        if (BarricadeManager.onHarvestPlantRequested != null)
        {
            ushort index = (ushort)region.IndexOfBarricadeByRootTransform(base.transform);
            BarricadeManager.onHarvestPlantRequested(player.channel.owner.playerID.steamID, x, y, plant, index, ref shouldAllow);
        }
        InteractableFarm.OnHarvestRequested_Global?.Invoke(this, context.GetCallingPlayer(), ref shouldAllow);
        if (shouldAllow && checkFarm())
        {
            ushort num = _grow;
            if (num == 0)
            {
                num = SpawnTableTool.resolve(growSpawnTableGuid);
            }
            player.inventory.forceAddItem(new Item(num, EItemOrigin.NATURE), auto: true);
            if (isAffectedByAgricultureSkill && UnityEngine.Random.value < player.skills.mastery(2, 5))
            {
                player.inventory.forceAddItem(new Item(num, EItemOrigin.NATURE), auto: true);
            }
            BarricadeManager.damage(base.transform, 2f, 1f, armor: false, default(CSteamID), EDamageOrigin.Plant_Harvested);
            player.sendStat(EPlayerStat.FOUND_PLANTS);
            player.skills.askPay(harvestRewardExperience);
        }
    }
}
