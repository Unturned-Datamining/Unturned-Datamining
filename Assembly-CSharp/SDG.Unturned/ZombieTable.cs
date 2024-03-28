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
            for (byte b = 0; b < Regions.WORLD_SIZE; b++)
            {
                for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
                {
                    for (ushort num = 0; num < LevelZombies.spawns[b, b2].Count; num++)
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

    /// <summary>
    /// ID unique to this zombie table in the level. If this table is deleted the ID will not be recycled. Used to
    /// refer to zombie table from external files, e.g., NPC zombie kills condition.
    /// </summary>
    public int tableUniqueId { get; private set; }

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

    internal void GetSpawnClothingParameters(out byte shirt, out byte pants, out byte hat, out byte gear)
    {
        shirt = byte.MaxValue;
        if (slots[0].table.Count > 0 && UnityEngine.Random.value < slots[0].chance)
        {
            shirt = (byte)UnityEngine.Random.Range(0, slots[0].table.Count);
        }
        pants = byte.MaxValue;
        if (slots[1].table.Count > 0 && UnityEngine.Random.value < slots[1].chance)
        {
            pants = (byte)UnityEngine.Random.Range(0, slots[1].table.Count);
        }
        hat = byte.MaxValue;
        if (slots[2].table.Count > 0 && UnityEngine.Random.value < slots[2].chance)
        {
            hat = (byte)UnityEngine.Random.Range(0, slots[2].table.Count);
        }
        gear = byte.MaxValue;
        if (slots[3].table.Count > 0 && UnityEngine.Random.value < slots[3].chance)
        {
            gear = (byte)UnityEngine.Random.Range(0, slots[3].table.Count);
        }
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
        tableUniqueId = LevelZombies.GenerateTableUniqueId();
    }

    public ZombieTable(ZombieSlot[] newSlots, Color newColor, string newName, bool newMega, ushort newHealth, byte newDamage, byte newLootIndex, ushort newLootID, uint newXP, float newRegen, string newDifficultyGUID, int newTableUniqueId)
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
        tableUniqueId = newTableUniqueId;
    }
}
