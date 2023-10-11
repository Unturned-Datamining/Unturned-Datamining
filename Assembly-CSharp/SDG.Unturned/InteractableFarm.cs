using System;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableFarm : Interactable
{
    public delegate void HarvestRequestHandler(InteractableFarm harvestable, SteamPlayer instigatorPlayer, ref bool shouldAllow);

    private uint _planted;

    private bool isGrown;

    public uint harvestRewardExperience;

    internal static readonly ClientInstanceMethod<uint> SendPlanted = ClientInstanceMethod<uint>.Get(typeof(InteractableFarm), "ReceivePlanted");

    private static readonly ServerInstanceMethod SendHarvestRequest = ServerInstanceMethod.Get(typeof(InteractableFarm), "ReceiveHarvestRequest");

    private ItemFarmAsset farmAsset;

    public uint planted => _planted;

    public uint growth => farmAsset?.growth ?? 1;

    public ushort grow => farmAsset?.grow ?? 0;

    public bool canFertilize => farmAsset?.canFertilize ?? false;

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
        farmAsset = asset as ItemFarmAsset;
        harvestRewardExperience = farmAsset?.harvestRewardExperience ?? 0;
        if (state.Length >= 4)
        {
            _planted = BitConverter.ToUInt32(state, 0);
        }
        else
        {
            _planted = 0u;
        }
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
        if (rain == ELightingRain.POST_DRIZZLE && (farmAsset == null || farmAsset.shouldRainAffectGrowth) && !Physics.Raycast(base.transform.position + Vector3.up, Vector3.up, 32f, RayMasks.BLOCK_WIND))
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
        if (!shouldAllow || !checkFarm())
        {
            return;
        }
        if (farmAsset != null)
        {
            ushort num = farmAsset.grow;
            if (num == 0)
            {
                num = SpawnTableTool.ResolveLegacyId(farmAsset.growSpawnTableGuid, EAssetType.ITEM, OnGetGrowSpawnTableErrorContext);
            }
            player.inventory.forceAddItem(new Item(num, EItemOrigin.NATURE), auto: true);
            if (farmAsset.isAffectedByAgricultureSkill && UnityEngine.Random.value < player.skills.mastery(2, 5))
            {
                player.inventory.forceAddItem(new Item(num, EItemOrigin.NATURE), auto: true);
            }
            farmAsset.harvestRewardsList.Grant(player);
        }
        BarricadeManager.damage(base.transform, 2f, 1f, armor: false, default(CSteamID), EDamageOrigin.Plant_Harvested);
        player.sendStat(EPlayerStat.FOUND_PLANTS);
        player.skills.askPay(harvestRewardExperience);
    }

    private string OnGetGrowSpawnTableErrorContext()
    {
        return "Farmable " + farmAsset?.FriendlyName;
    }
}
