using System;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class UseableConsumeable : Useable
{
    public delegate void PerformingAidHandler(Player instigator, Player target, ItemConsumeableAsset asset, ref bool shouldAllow);

    public delegate void PerformedAidHandler(Player instigator, Player target);

    public delegate void ConsumeRequestedHandler(Player instigatingPlayer, ItemConsumeableAsset consumeableAsset, ref bool shouldAllow);

    public delegate void ConsumePerformedHandler(Player instigatingPlayer, ItemConsumeableAsset consumeableAsset);

    private float startedUse;

    private float useTime;

    private float aidTime;

    private bool isUsing;

    private EConsumeMode consumeMode;

    private Player enemy;

    private bool hasAid;

    private static readonly ClientInstanceMethod<EConsumeMode> SendPlayConsume = ClientInstanceMethod<EConsumeMode>.Get(typeof(UseableConsumeable), "ReceivePlayConsume");

    private bool isUseable
    {
        get
        {
            if (consumeMode == EConsumeMode.USE)
            {
                return Time.realtimeSinceStartup - startedUse > useTime;
            }
            if (consumeMode == EConsumeMode.AID)
            {
                return Time.realtimeSinceStartup - startedUse > aidTime;
            }
            return false;
        }
    }

    public static event PerformingAidHandler onPerformingAid;

    public static event PerformedAidHandler onPerformedAid;

    public static event ConsumeRequestedHandler onConsumeRequested;

    public static event ConsumePerformedHandler onConsumePerformed;

    private bool invokeConsumeRequested(ItemConsumeableAsset asset)
    {
        if (UseableConsumeable.onConsumeRequested != null)
        {
            bool shouldAllow = true;
            UseableConsumeable.onConsumeRequested(base.player, asset, ref shouldAllow);
            return shouldAllow;
        }
        return true;
    }

    private void invokeConsumePerformed(ItemConsumeableAsset asset)
    {
        UseableConsumeable.onConsumePerformed?.Invoke(base.player, asset);
    }

    private void consume()
    {
        if (consumeMode == EConsumeMode.USE)
        {
            base.player.animator.play("Use", smooth: false);
        }
        else if (consumeMode == EConsumeMode.AID && hasAid)
        {
            base.player.animator.play("Aid", smooth: false);
        }
        if (!Dedicator.IsDedicatedServer)
        {
            base.player.playSound(((ItemConsumeableAsset)base.player.equipment.asset).use, 0.5f);
        }
        if (Provider.isServer)
        {
            AlertTool.alert(base.transform.position, 8f);
        }
    }

    [Obsolete]
    public void askConsume(CSteamID steamID, byte mode)
    {
        ReceivePlayConsume((EConsumeMode)mode);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askConsume")]
    public void ReceivePlayConsume(EConsumeMode mode)
    {
        if (base.player.equipment.isEquipped)
        {
            consumeMode = mode;
            consume();
        }
    }

    public override bool startPrimary()
    {
        if (base.player.equipment.isBusy)
        {
            return false;
        }
        base.player.equipment.isBusy = true;
        startedUse = Time.realtimeSinceStartup;
        isUsing = true;
        consumeMode = EConsumeMode.USE;
        consume();
        if (Provider.isServer)
        {
            SendPlayConsume.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.GatherRemoteClientConnectionsExcludingOwner(), consumeMode);
        }
        return true;
    }

    public override bool startSecondary()
    {
        if (base.player.equipment.isBusy)
        {
            return false;
        }
        if (!hasAid)
        {
            return false;
        }
        if (base.channel.isOwner)
        {
            RaycastInfo raycastInfo = DamageTool.raycast(new Ray(base.player.look.aim.position, base.player.look.aim.forward), 3f, RayMasks.DAMAGE_CLIENT);
            base.player.input.sendRaycast(raycastInfo, ERaycastInfoUsage.ConsumeableAid);
            if (!Provider.isServer && raycastInfo.player != null)
            {
                base.player.equipment.isBusy = true;
                startedUse = Time.realtimeSinceStartup;
                isUsing = true;
                consumeMode = EConsumeMode.AID;
                consume();
            }
        }
        if (Provider.isServer)
        {
            if (!base.player.input.hasInputs())
            {
                return false;
            }
            InputInfo input = base.player.input.getInput(doOcclusionCheck: true, ERaycastInfoUsage.ConsumeableAid);
            if (input == null)
            {
                return false;
            }
            if (input.type == ERaycastInfoType.PLAYER && input.player != null)
            {
                enemy = input.player;
                base.player.equipment.isBusy = true;
                startedUse = Time.realtimeSinceStartup;
                isUsing = true;
                consumeMode = EConsumeMode.AID;
                consume();
                SendPlayConsume.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.GatherRemoteClientConnectionsExcludingOwner(), consumeMode);
            }
        }
        return true;
    }

    public override void equip()
    {
        base.player.animator.play("Equip", smooth: true);
        hasAid = ((ItemConsumeableAsset)base.player.equipment.asset).hasAid;
        useTime = base.player.animator.getAnimationLength("Use");
        if (hasAid)
        {
            aidTime = base.player.animator.getAnimationLength("Aid");
        }
    }

    protected void performHealth(Player target, byte delta)
    {
        if (delta != 0)
        {
            float num = 1f + base.player.skills.mastery(2, 0) * 0.5f;
            int num2 = Mathf.RoundToInt((float)(int)delta * num);
            target.life.askHeal((byte)num2, healBleeding: false, healBroken: false);
        }
    }

    protected void performBleeding(Player target, ItemConsumeableAsset.Bleeding bleedingModifier)
    {
        switch (bleedingModifier)
        {
        case ItemConsumeableAsset.Bleeding.None:
            break;
        case ItemConsumeableAsset.Bleeding.Heal:
            target.life.serverSetBleeding(newBleeding: false);
            break;
        case ItemConsumeableAsset.Bleeding.Cut:
            target.life.serverSetBleeding(newBleeding: true);
            break;
        }
    }

    protected void performBrokenBones(Player target, ItemConsumeableAsset.Bones bonesModifier)
    {
        switch (bonesModifier)
        {
        case ItemConsumeableAsset.Bones.None:
            break;
        case ItemConsumeableAsset.Bones.Heal:
            target.life.serverSetLegsBroken(newLegsBroken: false);
            break;
        case ItemConsumeableAsset.Bones.Break:
            target.life.serverSetLegsBroken(newLegsBroken: true);
            break;
        }
    }

    protected void performAid(ItemConsumeableAsset asset)
    {
        bool shouldAllow = true;
        UseableConsumeable.onPerformingAid?.Invoke(base.player, enemy, asset, ref shouldAllow);
        if (!shouldAllow)
        {
            base.player.equipment.dequip();
            return;
        }
        asset.grantQuestRewards(enemy, shouldSend: true);
        asset.itemRewards.grantItems(enemy, EItemOrigin.CRAFT, shouldAutoEquip: false);
        byte health = enemy.life.health;
        byte virus = enemy.life.virus;
        bool isBleeding = enemy.life.isBleeding;
        bool isBroken = enemy.life.isBroken;
        float num = (float)(int)base.player.equipment.quality / 100f;
        performHealth(enemy, asset.health);
        performBleeding(enemy, asset.bleedingModifier);
        performBrokenBones(enemy, asset.bonesModifier);
        byte food = enemy.life.food;
        enemy.life.askEat((byte)((float)(int)asset.food * num));
        byte food2 = enemy.life.food;
        byte b = (byte)((float)(int)asset.water * num);
        if (asset.foodConstrainsWater)
        {
            b = (byte)Mathf.Min(b, food2 - food);
        }
        enemy.life.askDrink(b);
        float num2 = 1f - enemy.skills.mastery(1, 2) * 0.5f;
        enemy.life.askInfect((byte)((float)(int)asset.virus * num2));
        float num3 = 1f + enemy.skills.mastery(2, 0) * 0.5f;
        enemy.life.askDisinfect((byte)((float)(int)asset.disinfectant * num3));
        if (base.player.equipment.quality < 50)
        {
            enemy.life.askInfect((byte)((float)(asset.food + asset.water) * 0.5f * (1f - (float)(int)base.player.equipment.quality / 50f) * num2));
        }
        byte health2 = enemy.life.health;
        byte virus2 = enemy.life.virus;
        bool isBleeding2 = enemy.life.isBleeding;
        bool isBroken2 = enemy.life.isBroken;
        uint num4 = 0u;
        int num5 = 0;
        if (health2 > health)
        {
            num4 += (uint)Mathf.RoundToInt((float)(health2 - health) / 2f);
            num5++;
        }
        if (virus2 > virus)
        {
            num4 += (uint)Mathf.RoundToInt((float)(virus2 - virus) / 2f);
            num5++;
        }
        if (isBleeding && !isBleeding2)
        {
            num4 += 15;
            num5++;
        }
        if (isBroken && !isBroken2)
        {
            num4 += 15;
            num5++;
        }
        if (num4 != 0)
        {
            base.player.skills.askPay(num4);
        }
        if (num5 > 0)
        {
            base.player.skills.askRep(num5);
        }
        enemy.life.serverModifyHallucination((int)asset.vision);
        enemy.life.serverModifyStamina((int)asset.energy);
        enemy.life.serverModifyWarmth(asset.warmth);
        enemy.skills.ServerModifyExperience(asset.experience);
        UseableConsumeable.onPerformedAid?.Invoke(base.player, enemy);
        if (asset.shouldDeleteAfterUse)
        {
            base.player.equipment.use();
        }
        else
        {
            base.player.equipment.dequip();
        }
    }

    protected void performUseOnSelf(ItemConsumeableAsset asset)
    {
        base.player.life.askRest(asset.energy);
        byte vision = base.player.life.vision;
        byte b = (byte)((float)(int)asset.vision * (1f - base.player.skills.mastery(1, 2)));
        base.player.life.askView((byte)Mathf.Max(vision, b));
        if (base.channel.isOwner && asset.vision > 0 && Provider.provider.achievementsService.getAchievement("Berries", out var has) && !has)
        {
            Provider.provider.achievementsService.setAchievement("Berries");
        }
        base.player.life.simulatedModifyOxygen(asset.oxygen);
        base.player.life.simulatedModifyWarmth((short)asset.warmth);
        if (!Provider.isServer)
        {
            return;
        }
        if (!invokeConsumeRequested(asset))
        {
            base.player.equipment.dequip();
            return;
        }
        asset.grantQuestRewards(base.player, shouldSend: true);
        asset.itemRewards.grantItems(base.player, EItemOrigin.CRAFT, shouldAutoEquip: false);
        Vector3 vector = base.transform.position + Vector3.up;
        performHealth(base.player, asset.health);
        performBleeding(base.player, asset.bleedingModifier);
        performBrokenBones(base.player, asset.bonesModifier);
        base.player.skills.ServerModifyExperience(asset.experience);
        byte food = base.player.life.food;
        base.player.life.askEat((byte)((float)(int)asset.food * ((float)(int)base.player.equipment.quality / 100f)));
        byte food2 = base.player.life.food;
        byte b2 = (byte)((float)(int)asset.water * ((float)(int)base.player.equipment.quality / 100f));
        if (asset.foodConstrainsWater)
        {
            b2 = (byte)Mathf.Min(b2, food2 - food);
        }
        base.player.life.askDrink(b2);
        base.player.life.askInfect((byte)((float)(int)asset.virus * (1f - base.player.skills.mastery(1, 2) * 0.5f)));
        base.player.life.askDisinfect((byte)((float)(int)asset.disinfectant * (1f + base.player.skills.mastery(2, 0) * 0.5f)));
        if (base.player.equipment.quality < 50)
        {
            base.player.life.askInfect((byte)((float)(asset.food + asset.water) * 0.5f * (1f - (float)(int)base.player.equipment.quality / 50f) * (1f - base.player.skills.mastery(1, 2) * 0.5f)));
        }
        invokeConsumePerformed(asset);
        if (asset.shouldDeleteAfterUse)
        {
            base.player.equipment.use();
        }
        else
        {
            base.player.equipment.dequip();
        }
        if (asset.IsExplosive)
        {
            EffectAsset effectAsset = asset.FindExplosionEffectAsset();
            if (effectAsset != null)
            {
                TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
                parameters.relevantDistance = EffectManager.LARGE;
                parameters.position = vector;
                EffectManager.triggerEffect(parameters);
            }
            DamageTool.explode(vector, asset.range, EDeathCause.CHARGE, base.channel.owner.playerID.steamID, asset.playerDamageMultiplier.damage, asset.zombieDamageMultiplier.damage, asset.animalDamageMultiplier.damage, asset.barricadeDamage, asset.structureDamage, asset.vehicleDamage, asset.resourceDamage, asset.objectDamage, out var _, EExplosionDamageType.CONVENTIONAL, 32f, playImpactEffect: true, penetrateBuildables: false, EDamageOrigin.Food_Explosion);
            if (asset.playerDamageMultiplier.damage > 0.5f)
            {
                base.player.life.askDamage(101, Vector3.up, EDeathCause.CHARGE, ELimb.SPINE, base.channel.owner.playerID.steamID, out var _);
            }
        }
    }

    public override void simulate(uint simulation, bool inputSteady)
    {
        if (!isUsing || !isUseable)
        {
            return;
        }
        base.player.equipment.isBusy = false;
        isUsing = false;
        ItemConsumeableAsset itemConsumeableAsset = (ItemConsumeableAsset)base.player.equipment.asset;
        if (itemConsumeableAsset == null)
        {
            base.player.equipment.dequip();
        }
        else if (consumeMode == EConsumeMode.AID)
        {
            if (Provider.isServer && enemy != null)
            {
                performAid(itemConsumeableAsset);
            }
        }
        else
        {
            performUseOnSelf(itemConsumeableAsset);
        }
    }
}
