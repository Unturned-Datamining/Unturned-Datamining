using System;
using UnityEngine;

namespace SDG.Unturned;

public class ItemFuelAsset : ItemAsset
{
    protected AudioClip _use;

    protected ushort _fuel;

    private bool shouldAlwaysSpawnFull;

    private byte[] fuelState;

    public AudioClip use => _use;

    public ushort fuel => _fuel;

    public bool shouldDeleteAfterFillingTarget { get; protected set; }

    public override byte[] getState(EItemOrigin origin)
    {
        byte[] array = new byte[2];
        if (origin == EItemOrigin.ADMIN || shouldAlwaysSpawnFull)
        {
            array[0] = fuelState[0];
            array[1] = fuelState[1];
        }
        return array;
    }

    public override string getContext(string desc, byte[] state)
    {
        ushort num = BitConverter.ToUInt16(state, 0);
        desc += PlayerDashboardInventoryUI.localization.format("Fuel", ((int)((float)(int)num / (float)(int)fuel * 100f)).ToString());
        desc += "\n\n";
        return desc;
    }

    public ItemFuelAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _use = bundle.load<AudioClip>("Use");
        _fuel = data.readUInt16("Fuel", 0);
        fuelState = BitConverter.GetBytes(fuel);
        shouldDeleteAfterFillingTarget = data.readBoolean("Delete_After_Filling_Target");
        shouldAlwaysSpawnFull = data.readBoolean("Always_Spawn_Full");
    }
}
