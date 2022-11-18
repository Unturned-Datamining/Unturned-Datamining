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

    public ItemRefillAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _use = LoadRedirectableAsset<AudioClip>(bundle, "Use", data, "ConsumeAudioClip");
        float defaultValue = data.readSingle("Water");
        cleanHealth = data.readSingle("Clean_Health");
        saltyHealth = data.readSingle("Salty_Health", cleanHealth * 0.25f);
        dirtyHealth = data.readSingle("Dirty_Health", cleanHealth * 0.6f);
        cleanFood = data.readSingle("Clean_Food");
        saltyFood = data.readSingle("Salty_Food", cleanFood * 0.25f);
        dirtyFood = data.readSingle("Dirty_Food", cleanFood * 0.6f);
        cleanWater = data.readSingle("Clean_Water", defaultValue);
        saltyWater = data.readSingle("Salty_Water", cleanWater * 0.25f);
        dirtyWater = data.readSingle("Dirty_Water", cleanWater * 0.6f);
        cleanVirus = data.readSingle("Clean_Virus");
        saltyVirus = data.readSingle("Salty_Virus", cleanWater * -0.75f);
        dirtyVirus = data.readSingle("Dirty_Virus", cleanWater * -0.39999998f);
        cleanStamina = data.readSingle("Clean_Stamina");
        saltyStamina = data.readSingle("Salty_Stamina", cleanStamina * 0.25f);
        dirtyStamina = data.readSingle("Dirty_Stamina", cleanStamina * 0.6f);
        cleanOxygen = data.readSingle("Clean_Oxygen");
        saltyOxygen = data.readSingle("Salty_Oxygen", cleanOxygen * 0.25f);
        dirtyOxygen = data.readSingle("Dirty_Oxygen", cleanOxygen * 0.6f);
    }
}
