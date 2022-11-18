using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class ZombieTable
{
    private ZombieSlot[] _slots;

    private Color _color;

    public string name;

    public bool isMega;

    public ushort health;

    public byte damage;

    public byte lootIndex;

    public ushort lootID;

    public uint xp;

    public float regen;

    private string _difficultyGUID;

    private ZombieDifficultyAsset cachedDifficulty;

    public ZombieSlot[] slots => _slots;

    public Color color
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;
            for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte)(b + 1))
            {
                for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte)(b2 + 1))
                {
                    for (ushort num = 0; num < LevelZombies.spawns[b, b2].Count; num = (ushort)(num + 1))
                    {
                        ZombieSpawnpoint zombieSpawnpoint = LevelZombies.spawns[b, b2][num];
                        if (zombieSpawnpoint.type == EditorSpawns.selectedZombie)
                        {
                            zombieSpawnpoint.node.GetComponent<Renderer>().material.color = color;
                        }
                    }
                    EditorSpawns.zombieSpawn.GetComponent<Renderer>().material.color = color;
                }
            }
        }
    }

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

    public void addCloth(byte slotIndex, ushort id)
    {
        slots[slotIndex].addCloth(id);
    }

    public void removeCloth(byte slotIndex, byte clothIndex)
    {
        slots[slotIndex].removeCloth(clothIndex);
    }

    public ZombieTable(string newName)
    {
        _slots = new ZombieSlot[4];
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new ZombieSlot(1f, new List<ZombieCloth>());
        }
        _color = Color.white;
        name = newName;
        isMega = false;
        health = 100;
        damage = 15;
        lootIndex = 0;
        lootID = 0;
        xp = 3u;
        regen = 10f;
        difficultyGUID = string.Empty;
    }

    public ZombieTable(ZombieSlot[] newSlots, Color newColor, string newName, bool newMega, ushort newHealth, byte newDamage, byte newLootIndex, ushort newLootID, uint newXP, float newRegen, string newDifficultyGUID)
    {
        _slots = newSlots;
        _color = newColor;
        name = newName;
        isMega = newMega;
        health = newHealth;
        damage = newDamage;
        lootIndex = newLootIndex;
        lootID = newLootID;
        xp = newXP;
        regen = newRegen;
        difficultyGUID = newDifficultyGUID;
    }
}
