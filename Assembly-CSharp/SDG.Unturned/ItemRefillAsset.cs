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

    public override string getContext(string desc, byte[] state)
    {
        string key = state[0] switch
        {
            0 => "Empty", 
            1 => "Clean", 
            2 => "Salty", 
            3 => "Dirty", 
            _ => "Full", 
        };
        desc += PlayerDashboardInventoryUI.localization.format("Refill", PlayerDashboardInventoryUI.localization.format(key));
        desc += "\n\n";
        return desc;
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
