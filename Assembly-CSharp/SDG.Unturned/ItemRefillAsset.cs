using System;
using UnityEngine;

namespace SDG.Unturned;

public class ItemRefillAsset : ItemAsset
{
    protected AudioClip _use;

    public AudioClip use => _use;

    [Obsolete("Replaced by separate stats for each water type")]
    public byte water => MathfEx.RoundAndClampToByte(cleanWater);

    public float cleanHealth { get; protected set; }

    public float saltyHealth { get; protected set; }

    public float dirtyHealth { get; protected set; }

    public float cleanFood { get; protected set; }

    public float saltyFood { get; protected set; }

    public float dirtyFood { get; protected set; }

    public float cleanWater { get; protected set; }

    public float saltyWater { get; protected set; }

    public float dirtyWater { get; protected set; }

    public float cleanVirus { get; protected set; }

    public float saltyVirus { get; protected set; }

    public float dirtyVirus { get; protected set; }

    public float cleanStamina { get; protected set; }

    public float saltyStamina { get; protected set; }

    public float dirtyStamina { get; protected set; }

    public float cleanOxygen { get; protected set; }

    public float saltyOxygen { get; protected set; }

    public float dirtyOxygen { get; protected set; }

    public float GetRefillHealth(ERefillWaterType refillWaterType)
    {
        return refillWaterType switch
        {
            ERefillWaterType.CLEAN => cleanHealth, 
            ERefillWaterType.SALTY => saltyHealth, 
            ERefillWaterType.DIRTY => dirtyHealth, 
            _ => 0f, 
        };
    }

    public float GetRefillFood(ERefillWaterType refillWaterType)
    {
        return refillWaterType switch
        {
            ERefillWaterType.CLEAN => cleanFood, 
            ERefillWaterType.SALTY => saltyFood, 
            ERefillWaterType.DIRTY => dirtyFood, 
            _ => 0f, 
        };
    }

    public float GetRefillWater(ERefillWaterType refillWaterType)
    {
        return refillWaterType switch
        {
            ERefillWaterType.CLEAN => cleanWater, 
            ERefillWaterType.SALTY => saltyWater, 
            ERefillWaterType.DIRTY => dirtyWater, 
            _ => 0f, 
        };
    }

    public float GetRefillVirus(ERefillWaterType refillWaterType)
    {
        return refillWaterType switch
        {
            ERefillWaterType.CLEAN => cleanVirus, 
            ERefillWaterType.SALTY => saltyVirus, 
            ERefillWaterType.DIRTY => dirtyVirus, 
            _ => 0f, 
        };
    }

    public float GetRefillStamina(ERefillWaterType refillWaterType)
    {
        return refillWaterType switch
        {
            ERefillWaterType.CLEAN => cleanStamina, 
            ERefillWaterType.SALTY => saltyStamina, 
            ERefillWaterType.DIRTY => dirtyStamina, 
            _ => 0f, 
        };
    }

    public float GetRefillOxygen(ERefillWaterType refillWaterType)
    {
        return refillWaterType switch
        {
            ERefillWaterType.CLEAN => cleanOxygen, 
            ERefillWaterType.SALTY => saltyOxygen, 
            ERefillWaterType.DIRTY => dirtyOxygen, 
            _ => 0f, 
        };
    }

    public override byte[] getState(EItemOrigin origin)
    {
        byte[] array = new byte[1];
        if (origin == EItemOrigin.ADMIN)
        {
            array[0] = 1;
        }
        else
        {
            array[0] = 0;
        }
        return array;
    }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        ERefillWaterType eRefillWaterType = (ERefillWaterType)itemInstance.state[0];
        builder.Append(PlayerDashboardInventoryUI.localization.format("Refill", PlayerDashboardInventoryUI.localization.format(eRefillWaterType switch
        {
            ERefillWaterType.EMPTY => "Empty", 
            ERefillWaterType.CLEAN => "Clean", 
            ERefillWaterType.SALTY => "Salty", 
            ERefillWaterType.DIRTY => "Dirty", 
            _ => "Full", 
        })), 2000);
        if (!builder.shouldRestrictToLegacyContent)
        {
            int num = Mathf.RoundToInt(GetRefillHealth(eRefillWaterType));
            if (num > 0)
            {
                string arg = PlayerDashboardInventoryUI.FormatStatColor(num.ToString(), isBeneficial: true);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_HealthPositive", arg), 10000);
            }
            else if (num < 0)
            {
                string arg2 = PlayerDashboardInventoryUI.FormatStatColor(num.ToString(), isBeneficial: false);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_HealthNegative", arg2), 10000);
            }
            int num2 = Mathf.RoundToInt(GetRefillFood(eRefillWaterType));
            if (num2 > 0)
            {
                string arg3 = PlayerDashboardInventoryUI.FormatStatColor(num2.ToString(), isBeneficial: true);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_FoodPositive", arg3), 10000);
            }
            else if (num2 < 0)
            {
                string arg4 = PlayerDashboardInventoryUI.FormatStatColor(num2.ToString(), isBeneficial: false);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_FoodNegative", arg4), 10000);
            }
            int num3 = Mathf.RoundToInt(GetRefillWater(eRefillWaterType));
            if (num3 > 0)
            {
                string arg5 = PlayerDashboardInventoryUI.FormatStatColor(num3.ToString(), isBeneficial: true);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_WaterPositive", arg5), 10000);
            }
            else if (num3 < 0)
            {
                string arg6 = PlayerDashboardInventoryUI.FormatStatColor(num3.ToString(), isBeneficial: false);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_WaterNegative", arg6), 10000);
            }
            int num4 = Mathf.RoundToInt(GetRefillVirus(eRefillWaterType));
            if (num4 > 0)
            {
                string arg7 = PlayerDashboardInventoryUI.FormatStatColor(num4.ToString(), isBeneficial: true);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_VirusPositive", arg7), 10000);
            }
            else if (num4 < 0)
            {
                string arg8 = PlayerDashboardInventoryUI.FormatStatColor(num4.ToString(), isBeneficial: false);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_VirusNegative", arg8), 10000);
            }
            int num5 = Mathf.RoundToInt(GetRefillStamina(eRefillWaterType));
            if (num5 > 0)
            {
                string arg9 = PlayerDashboardInventoryUI.FormatStatColor(num5.ToString(), isBeneficial: true);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_StaminaPositive", arg9), 10000);
            }
            else if (num5 < 0)
            {
                string arg10 = PlayerDashboardInventoryUI.FormatStatColor(num5.ToString(), isBeneficial: false);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_StaminaNegative", arg10), 10000);
            }
            int num6 = Mathf.RoundToInt(GetRefillOxygen(eRefillWaterType));
            if (num6 > 0)
            {
                string arg11 = PlayerDashboardInventoryUI.FormatStatColor(num6.ToString(), isBeneficial: true);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_OxygenPositive", arg11), 10000);
            }
            else if (num6 < 0)
            {
                string arg12 = PlayerDashboardInventoryUI.FormatStatColor(num6.ToString(), isBeneficial: false);
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Consumeable_OxygenNegative", arg12), 10000);
            }
        }
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _use = LoadRedirectableAsset<AudioClip>(bundle, "Use", data, "ConsumeAudioClip");
        float defaultValue = data.ParseFloat("Water");
        cleanHealth = data.ParseFloat("Clean_Health");
        saltyHealth = data.ParseFloat("Salty_Health", cleanHealth * 0.25f);
        dirtyHealth = data.ParseFloat("Dirty_Health", cleanHealth * 0.6f);
        cleanFood = data.ParseFloat("Clean_Food");
        saltyFood = data.ParseFloat("Salty_Food", cleanFood * 0.25f);
        dirtyFood = data.ParseFloat("Dirty_Food", cleanFood * 0.6f);
        cleanWater = data.ParseFloat("Clean_Water", defaultValue);
        saltyWater = data.ParseFloat("Salty_Water", cleanWater * 0.25f);
        dirtyWater = data.ParseFloat("Dirty_Water", cleanWater * 0.6f);
        cleanVirus = data.ParseFloat("Clean_Virus");
        saltyVirus = data.ParseFloat("Salty_Virus", cleanWater * -0.75f);
        dirtyVirus = data.ParseFloat("Dirty_Virus", cleanWater * -0.39999998f);
        cleanStamina = data.ParseFloat("Clean_Stamina");
        saltyStamina = data.ParseFloat("Salty_Stamina", cleanStamina * 0.25f);
        dirtyStamina = data.ParseFloat("Dirty_Stamina", cleanStamina * 0.6f);
        cleanOxygen = data.ParseFloat("Clean_Oxygen");
        saltyOxygen = data.ParseFloat("Salty_Oxygen", cleanOxygen * 0.25f);
        dirtyOxygen = data.ParseFloat("Dirty_Oxygen", cleanOxygen * 0.6f);
    }
}
