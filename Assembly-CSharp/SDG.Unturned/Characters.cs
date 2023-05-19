using System;
using System.Collections.Generic;
using SDG.Provider;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class Characters : MonoBehaviour
{
    public static readonly byte SAVEDATA_VERSION = 21;

    private static bool hasLoaded;

    private static bool initialApply;

    public static bool hasPlayed;

    private static bool hasDropped;

    public static CharacterUpdated onCharacterUpdated;

    private static byte _selected;

    private static Transform character;

    public static HumanClothes clothes;

    private static Transform[] slots;

    private static Transform primaryMeleeSlot;

    private static Transform primaryLargeGunSlot;

    private static Transform primarySmallGunSlot;

    private static Transform secondaryMeleeSlot;

    private static Transform secondaryGunSlot;

    private static List<ulong> _packageSkins;

    private static float characterOffset;

    private static float _characterYaw;

    public static float characterYaw;

    public static byte selected
    {
        get
        {
            return _selected;
        }
        set
        {
            _selected = value;
            onCharacterUpdated?.Invoke(selected, active);
            apply();
        }
    }

    public static Character active => list[selected];

    public static Character[] list { get; private set; }

    public static List<ulong> packageSkins => _packageSkins;

    public static void rename(string name)
    {
        active.name = name;
        onCharacterUpdated?.Invoke(selected, active);
    }

    public static void skillify(EPlayerSkillset skillset)
    {
        active.skillset = skillset;
        onCharacterUpdated?.Invoke(selected, active);
        active.applyHero();
        apply();
    }

    public static void growFace(byte face)
    {
        active.face = face;
        apply(showItems: false, showCosmetics: false);
    }

    public static void growHair(byte hair)
    {
        active.hair = hair;
        apply(showItems: false, showCosmetics: false);
    }

    public static void growBeard(byte beard)
    {
        active.beard = beard;
        apply(showItems: false, showCosmetics: false);
    }

    public static void paintSkin(Color color)
    {
        active.skin = color;
        apply(showItems: false, showCosmetics: false);
    }

    public static void paintColor(Color color)
    {
        active.color = color;
        apply(showItems: false, showCosmetics: false);
    }

    public static void renick(string nick)
    {
        active.nick = nick;
        onCharacterUpdated?.Invoke(selected, active);
    }

    public static void paintMarkerColor(Color color)
    {
        active.markerColor = color;
    }

    public static void group(CSteamID group)
    {
        if (active.group == group)
        {
            active.group = CSteamID.Nil;
        }
        else
        {
            active.group = group;
        }
        onCharacterUpdated?.Invoke(selected, active);
    }

    public static void ungroup()
    {
        active.group = CSteamID.Nil;
        onCharacterUpdated?.Invoke(selected, active);
    }

    public static void hand(bool state)
    {
        active.hand = state;
        apply(showItems: false, showCosmetics: false);
        onCharacterUpdated?.Invoke(selected, active);
    }

    public static bool isSkinEquipped(ulong instance)
    {
        if (instance == 0L)
        {
            return false;
        }
        return packageSkins.IndexOf(instance) != -1;
    }

    public static bool isCosmeticEquipped(ulong instance)
    {
        if (instance == 0L)
        {
            return false;
        }
        if (active.packageBackpack != instance && active.packageGlasses != instance && active.packageHat != instance && active.packageMask != instance && active.packagePants != instance && active.packageShirt != instance)
        {
            return active.packageVest == instance;
        }
        return true;
    }

    public static bool isEquipped(ulong instanceID)
    {
        if (!isSkinEquipped(instanceID))
        {
            return isCosmeticEquipped(instanceID);
        }
        return true;
    }

    public static void ToggleEquipItemByInstanceId(ulong itemInstanceId)
    {
        int inventoryItem = Provider.provider.economyService.getInventoryItem(itemInstanceId);
        if (inventoryItem == 0)
        {
            return;
        }
        Provider.provider.economyService.getInventoryTargetID(inventoryItem, out var item_guid, out var vehicle_guid);
        if (item_guid == default(Guid) && vehicle_guid == default(Guid))
        {
            return;
        }
        ItemAsset itemAsset = Assets.find<ItemAsset>(item_guid);
        if (itemAsset == null || itemAsset.proPath == null || itemAsset.proPath.Length == 0)
        {
            if (Provider.provider.economyService.getInventorySkinID(inventoryItem) == 0)
            {
                return;
            }
            if (!packageSkins.Remove(itemInstanceId))
            {
                for (int i = 0; i < packageSkins.Count; i++)
                {
                    ulong num = packageSkins[i];
                    if (num == 0L)
                    {
                        continue;
                    }
                    int inventoryItem2 = Provider.provider.economyService.getInventoryItem(num);
                    if (inventoryItem2 != 0)
                    {
                        Provider.provider.economyService.getInventoryTargetID(inventoryItem2, out var item_guid2, out var vehicle_guid2);
                        if ((item_guid != default(Guid) && item_guid == item_guid2) || (vehicle_guid != default(Guid) && vehicle_guid == vehicle_guid2))
                        {
                            packageSkins.RemoveAt(i);
                            break;
                        }
                    }
                }
                packageSkins.Add(itemInstanceId);
            }
        }
        if (itemAsset != null)
        {
            if (itemAsset.type == EItemType.SHIRT)
            {
                if (active.packageShirt == itemInstanceId)
                {
                    active.packageShirt = 0uL;
                }
                else
                {
                    active.packageShirt = itemInstanceId;
                }
            }
            else if (itemAsset.type == EItemType.PANTS)
            {
                if (active.packagePants == itemInstanceId)
                {
                    active.packagePants = 0uL;
                }
                else
                {
                    active.packagePants = itemInstanceId;
                }
            }
            else if (itemAsset.type == EItemType.HAT)
            {
                if (active.packageHat == itemInstanceId)
                {
                    active.packageHat = 0uL;
                }
                else
                {
                    active.packageHat = itemInstanceId;
                }
            }
            else if (itemAsset.type == EItemType.BACKPACK)
            {
                if (active.packageBackpack == itemInstanceId)
                {
                    active.packageBackpack = 0uL;
                }
                else
                {
                    active.packageBackpack = itemInstanceId;
                }
            }
            else if (itemAsset.type == EItemType.VEST)
            {
                if (active.packageVest == itemInstanceId)
                {
                    active.packageVest = 0uL;
                }
                else
                {
                    active.packageVest = itemInstanceId;
                }
            }
            else if (itemAsset.type == EItemType.MASK)
            {
                if (active.packageMask == itemInstanceId)
                {
                    active.packageMask = 0uL;
                }
                else
                {
                    active.packageMask = itemInstanceId;
                }
            }
            else if (itemAsset.type == EItemType.GLASSES)
            {
                if (active.packageGlasses == itemInstanceId)
                {
                    active.packageGlasses = 0uL;
                }
                else
                {
                    active.packageGlasses = itemInstanceId;
                }
            }
        }
        apply(showItems: false, showCosmetics: true);
        onCharacterUpdated?.Invoke(selected, active);
    }

    public static bool getPackageForItemID(ushort itemID, out ulong itemInstanceId)
    {
        itemInstanceId = 0uL;
        if (!(Assets.find(EAssetType.ITEM, itemID) is ItemAsset itemAsset))
        {
            return false;
        }
        for (int i = 0; i < packageSkins.Count; i++)
        {
            itemInstanceId = packageSkins[i];
            if (itemInstanceId == 0L)
            {
                continue;
            }
            int inventoryItem = Provider.provider.economyService.getInventoryItem(itemInstanceId);
            if (inventoryItem != 0)
            {
                Guid inventoryItemGuid = Provider.provider.economyService.getInventoryItemGuid(inventoryItem);
                if (!(inventoryItemGuid == default(Guid)) && itemAsset.GUID == inventoryItemGuid)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static bool getSlot0StatTrackerValue(out EStatTrackerType type, out int kills)
    {
        type = EStatTrackerType.NONE;
        kills = -1;
        if (!getPackageForItemID(active.primaryItem, out var itemInstanceId))
        {
            return false;
        }
        return Provider.provider.economyService.getInventoryStatTrackerValue(itemInstanceId, out type, out kills);
    }

    private static bool getSlot1StatTrackerValue(out EStatTrackerType type, out int kills)
    {
        type = EStatTrackerType.NONE;
        kills = -1;
        if (!getPackageForItemID(active.secondaryItem, out var itemInstanceId))
        {
            return false;
        }
        return Provider.provider.economyService.getInventoryStatTrackerValue(itemInstanceId, out type, out kills);
    }

    private static void apply(byte slot, bool showItems)
    {
        if (slots[slot] != null)
        {
            UnityEngine.Object.Destroy(slots[slot].gameObject);
        }
        if (!showItems)
        {
            return;
        }
        ushort num = 0;
        byte[] state = null;
        switch (slot)
        {
        case 0:
            num = active.primaryItem;
            state = active.primaryState;
            break;
        case 1:
            num = active.secondaryItem;
            state = active.secondaryState;
            break;
        }
        if (num == 0 || !(Assets.find(EAssetType.ITEM, num) is ItemAsset itemAsset))
        {
            return;
        }
        ushort skin = 0;
        ushort num2 = 0;
        for (int i = 0; i < packageSkins.Count; i++)
        {
            ulong num3 = packageSkins[i];
            if (num3 == 0L)
            {
                continue;
            }
            int inventoryItem = Provider.provider.economyService.getInventoryItem(num3);
            if (inventoryItem == 0)
            {
                continue;
            }
            Guid inventoryItemGuid = Provider.provider.economyService.getInventoryItemGuid(inventoryItem);
            if (!(inventoryItemGuid == default(Guid)) && itemAsset.GUID == inventoryItemGuid)
            {
                skin = Provider.provider.economyService.getInventorySkinID(inventoryItem);
                num2 = Provider.provider.economyService.getInventoryMythicID(inventoryItem);
                if (num2 == 0)
                {
                    num2 = Provider.provider.economyService.getInventoryParticleEffect(num3);
                }
                break;
            }
        }
        GetStatTrackerValueHandler statTrackerCallback = null;
        switch (slot)
        {
        case 0:
            statTrackerCallback = getSlot0StatTrackerValue;
            break;
        case 1:
            statTrackerCallback = getSlot1StatTrackerValue;
            break;
        }
        Transform item = ItemTool.getItem(num, skin, 100, state, viewmodel: false, itemAsset, statTrackerCallback);
        switch (slot)
        {
        case 0:
            if (itemAsset.type == EItemType.MELEE)
            {
                item.transform.parent = primaryMeleeSlot;
            }
            else if (itemAsset.slot == ESlotType.PRIMARY)
            {
                item.transform.parent = primaryLargeGunSlot;
            }
            else
            {
                item.transform.parent = primarySmallGunSlot;
            }
            break;
        case 1:
            if (itemAsset.type == EItemType.MELEE)
            {
                item.transform.parent = secondaryMeleeSlot;
            }
            else
            {
                item.transform.parent = secondaryGunSlot;
            }
            break;
        }
        item.localPosition = Vector3.zero;
        item.localRotation = Quaternion.Euler(0f, 0f, 90f);
        item.localScale = Vector3.one;
        UnityEngine.Object.Destroy(item.GetComponent<Collider>());
        if (num2 != 0)
        {
            ItemTool.applyEffect(item, num2, EEffectType.THIRD);
        }
        slots[slot] = item;
    }

    public static void apply()
    {
        apply(showItems: true, showCosmetics: true);
    }

    public static void apply(bool showItems, bool showCosmetics)
    {
        if (active == null)
        {
            UnturnedLog.error("Failed to find an active character.");
            return;
        }
        if (clothes == null)
        {
            UnturnedLog.error("Failed to find character clothes.");
            return;
        }
        try
        {
            applyInternal(showItems, showCosmetics);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e);
        }
    }

    private static void applyInternal(bool showItems, bool showCosmetics)
    {
        character.localScale = new Vector3((!active.hand) ? 1 : (-1), 1f, 1f);
        if (showItems)
        {
            clothes.shirt = active.shirt;
            clothes.pants = active.pants;
            clothes.hat = active.hat;
            clothes.backpack = active.backpack;
            clothes.vest = active.vest;
            clothes.mask = active.mask;
            clothes.glasses = active.glasses;
        }
        else
        {
            clothes.shirt = 0;
            clothes.pants = 0;
            clothes.hat = 0;
            clothes.backpack = 0;
            clothes.vest = 0;
            clothes.mask = 0;
            clothes.glasses = 0;
        }
        if (showCosmetics)
        {
            if (active.packageShirt != 0L)
            {
                clothes.visualShirt = Provider.provider.economyService.getInventoryItem(active.packageShirt);
            }
            else
            {
                clothes.visualShirt = 0;
            }
            if (active.packagePants != 0L)
            {
                clothes.visualPants = Provider.provider.economyService.getInventoryItem(active.packagePants);
            }
            else
            {
                clothes.visualPants = 0;
            }
            if (active.packageHat != 0L)
            {
                clothes.visualHat = Provider.provider.economyService.getInventoryItem(active.packageHat);
            }
            else
            {
                clothes.visualHat = 0;
            }
            if (active.packageBackpack != 0L)
            {
                clothes.visualBackpack = Provider.provider.economyService.getInventoryItem(active.packageBackpack);
            }
            else
            {
                clothes.visualBackpack = 0;
            }
            if (active.packageVest != 0L)
            {
                clothes.visualVest = Provider.provider.economyService.getInventoryItem(active.packageVest);
            }
            else
            {
                clothes.visualVest = 0;
            }
            if (active.packageMask != 0L)
            {
                clothes.visualMask = Provider.provider.economyService.getInventoryItem(active.packageMask);
            }
            else
            {
                clothes.visualMask = 0;
            }
            if (active.packageGlasses != 0L)
            {
                clothes.visualGlasses = Provider.provider.economyService.getInventoryItem(active.packageGlasses);
            }
            else
            {
                clothes.visualGlasses = 0;
            }
        }
        else
        {
            clothes.visualShirt = 0;
            clothes.visualPants = 0;
            clothes.visualHat = 0;
            clothes.visualBackpack = 0;
            clothes.visualVest = 0;
            clothes.visualMask = 0;
            clothes.visualGlasses = 0;
        }
        clothes.face = active.face;
        clothes.hair = active.hair;
        clothes.beard = active.beard;
        clothes.skin = active.skin;
        clothes.color = active.color;
        clothes.hand = active.hand;
        clothes.apply();
        for (byte b = 0; b < slots.Length; b = (byte)(b + 1))
        {
            apply(b, showItems);
        }
    }

    private static void onInventoryRefreshed()
    {
        if (clothes != null && list != null && packageSkins != null)
        {
            for (int num = packageSkins.Count - 1; num >= 0; num--)
            {
                ulong num2 = packageSkins[num];
                if (num2 != 0L && Provider.provider.economyService.getInventoryItem(num2) == 0)
                {
                    packageSkins.RemoveAt(num);
                }
            }
            for (int i = 0; i < list.Length; i++)
            {
                Character character = list[i];
                if (character != null)
                {
                    if (character.packageShirt != 0L && Provider.provider.economyService.getInventoryItem(character.packageShirt) == 0)
                    {
                        character.packageShirt = 0uL;
                    }
                    if (character.packagePants != 0L && Provider.provider.economyService.getInventoryItem(character.packagePants) == 0)
                    {
                        character.packagePants = 0uL;
                    }
                    if (character.packageHat != 0L && Provider.provider.economyService.getInventoryItem(character.packageHat) == 0)
                    {
                        character.packageHat = 0uL;
                    }
                    if (character.packageBackpack != 0L && Provider.provider.economyService.getInventoryItem(character.packageBackpack) == 0)
                    {
                        character.packageBackpack = 0uL;
                    }
                    if (character.packageVest != 0L && Provider.provider.economyService.getInventoryItem(character.packageVest) == 0)
                    {
                        character.packageVest = 0uL;
                    }
                    if (character.packageMask != 0L && Provider.provider.economyService.getInventoryItem(character.packageMask) == 0)
                    {
                        character.packageMask = 0uL;
                    }
                    if (character.packageGlasses != 0L && Provider.provider.economyService.getInventoryItem(character.packageGlasses) == 0)
                    {
                        character.packageGlasses = 0uL;
                    }
                }
            }
            if (!initialApply)
            {
                initialApply = true;
                apply();
            }
        }
        if (!hasDropped)
        {
            hasDropped = true;
            if (hasPlayed)
            {
                Provider.provider.economyService.dropInventory();
                LiveConfig.Refresh();
            }
        }
    }

    private void Update()
    {
        if (!Dedicator.IsDedicatedServer && !(character == null))
        {
            _characterYaw = Mathf.Lerp(_characterYaw, characterOffset + characterYaw, 4f * Time.deltaTime);
            character.transform.rotation = Quaternion.Euler(90f, _characterYaw, 0f);
        }
    }

    internal void customStart()
    {
        character = GameObject.Find("Hero").transform;
        clothes = character.GetComponent<HumanClothes>();
        clothes.isView = true;
        slots = new Transform[PlayerInventory.SLOTS];
        primaryMeleeSlot = character.Find("Skeleton").Find("Spine").Find("Primary_Melee");
        primaryLargeGunSlot = character.Find("Skeleton").Find("Spine").Find("Primary_Large_Gun");
        primarySmallGunSlot = character.Find("Skeleton").Find("Spine").Find("Primary_Small_Gun");
        secondaryMeleeSlot = character.Find("Skeleton").Find("Right_Hip").Find("Right_Leg")
            .Find("Secondary_Melee");
        secondaryGunSlot = character.Find("Skeleton").Find("Right_Hip").Find("Right_Leg")
            .Find("Secondary_Gun");
        characterOffset = character.transform.eulerAngles.y;
        _characterYaw = characterOffset;
        characterYaw = 0f;
        hasDropped = false;
        if (!hasLoaded)
        {
            TempSteamworksEconomy economyService = Provider.provider.economyService;
            economyService.onInventoryRefreshed = (TempSteamworksEconomy.InventoryRefreshed)Delegate.Combine(economyService.onInventoryRefreshed, new TempSteamworksEconomy.InventoryRefreshed(onInventoryRefreshed));
        }
        load();
    }

    public static void load()
    {
        initialApply = false;
        Provider.provider.economyService.refreshInventory();
        if (list != null)
        {
            for (byte b = 0; b < list.Length; b = (byte)(b + 1))
            {
                if (list[b] != null)
                {
                    onCharacterUpdated?.Invoke(b, list[b]);
                }
            }
            return;
        }
        list = new Character[Customization.FREE_CHARACTERS + Customization.PRO_CHARACTERS];
        _packageSkins = new List<ulong>();
        if (ReadWrite.fileExists("/Characters.dat", useCloud: true))
        {
            Block block = ReadWrite.readBlock("/Characters.dat", useCloud: true, 0);
            if (block != null)
            {
                byte b2 = block.readByte();
                if (b2 >= 12)
                {
                    if (b2 >= 14)
                    {
                        ushort num = block.readUInt16();
                        for (ushort num2 = 0; num2 < num; num2 = (ushort)(num2 + 1))
                        {
                            ulong num3 = block.readUInt64();
                            if (num3 != 0L)
                            {
                                packageSkins.Add(num3);
                            }
                        }
                    }
                    _selected = block.readByte();
                    if (_selected >= list.Length || (!Provider.isPro && selected >= Customization.FREE_CHARACTERS))
                    {
                        _selected = 0;
                    }
                    for (byte b3 = 0; b3 < list.Length; b3 = (byte)(b3 + 1))
                    {
                        ushort newShirt = block.readUInt16();
                        ushort newPants = block.readUInt16();
                        ushort newHat = block.readUInt16();
                        ushort newBackpack = block.readUInt16();
                        ushort newVest = block.readUInt16();
                        ushort newMask = block.readUInt16();
                        ushort newGlasses = block.readUInt16();
                        ulong newPackageShirt = block.readUInt64();
                        ulong newPackagePants = block.readUInt64();
                        ulong newPackageHat = block.readUInt64();
                        ulong newPackageBackpack = block.readUInt64();
                        ulong newPackageVest = block.readUInt64();
                        ulong newPackageMask = block.readUInt64();
                        ulong newPackageGlasses = block.readUInt64();
                        ushort newPrimaryItem = block.readUInt16();
                        byte[] newPrimaryState = block.readByteArray();
                        ushort newSecondaryItem = block.readUInt16();
                        byte[] newSecondaryState = block.readByteArray();
                        byte b4 = block.readByte();
                        byte b5 = block.readByte();
                        byte b6 = block.readByte();
                        Color color = block.readColor();
                        Color color2 = block.readColor();
                        Color newMarkerColor = ((b2 <= 20) ? Customization.MARKER_COLORS[UnityEngine.Random.Range(0, Customization.MARKER_COLORS.Length)] : block.readColor());
                        bool newHand = block.readBoolean();
                        string newName = block.readString();
                        if (b2 < 19)
                        {
                            newName = Provider.clientName;
                        }
                        string newNick = block.readString();
                        CSteamID cSteamID = block.readSteamID();
                        byte b7 = block.readByte();
                        if (!Provider.provider.communityService.checkGroup(cSteamID))
                        {
                            cSteamID = CSteamID.Nil;
                        }
                        if (b7 >= Customization.SKILLSETS)
                        {
                            b7 = 0;
                        }
                        if (b2 < 16)
                        {
                            b7 = (byte)UnityEngine.Random.Range(1, Customization.SKILLSETS);
                        }
                        if (b2 > 16 && b2 < 20)
                        {
                            block.readBoolean();
                        }
                        if (!Provider.isPro)
                        {
                            if (b3 >= Customization.FREE_CHARACTERS)
                            {
                                newName = Provider.clientName;
                                newNick = Provider.clientName;
                            }
                            if (b4 >= Customization.FACES_FREE)
                            {
                                b4 = (byte)UnityEngine.Random.Range(0, Customization.FACES_FREE);
                            }
                            if (b5 >= Customization.HAIRS_FREE)
                            {
                                b5 = (byte)UnityEngine.Random.Range(0, Customization.HAIRS_FREE);
                            }
                            if (b6 >= Customization.BEARDS_FREE)
                            {
                                b6 = 0;
                            }
                            if (!Customization.checkSkin(color))
                            {
                                color = Customization.SKINS[UnityEngine.Random.Range(0, Customization.SKINS.Length)];
                            }
                            if (!Customization.checkColor(color2))
                            {
                                color2 = Customization.COLORS[UnityEngine.Random.Range(0, Customization.COLORS.Length)];
                            }
                        }
                        list[b3] = new Character(newShirt, newPants, newHat, newBackpack, newVest, newMask, newGlasses, newPackageShirt, newPackagePants, newPackageHat, newPackageBackpack, newPackageVest, newPackageMask, newPackageGlasses, newPrimaryItem, newPrimaryState, newSecondaryItem, newSecondaryState, b4, b5, b6, color, color2, newMarkerColor, newHand, newName, newNick, cSteamID, (EPlayerSkillset)b7);
                        onCharacterUpdated?.Invoke(b3, list[b3]);
                    }
                }
                else
                {
                    for (byte b8 = 0; b8 < list.Length; b8 = (byte)(b8 + 1))
                    {
                        list[b8] = new Character();
                        onCharacterUpdated?.Invoke(b8, list[b8]);
                    }
                }
            }
        }
        else
        {
            _selected = 0;
        }
        for (byte b9 = 0; b9 < list.Length; b9 = (byte)(b9 + 1))
        {
            if (list[b9] == null)
            {
                list[b9] = new Character();
                onCharacterUpdated?.Invoke(b9, list[b9]);
            }
        }
        apply();
        hasLoaded = true;
        UnturnedLog.info("Loaded characters");
    }

    public static void save()
    {
        if (!hasLoaded)
        {
            return;
        }
        Block block = new Block();
        block.writeByte(SAVEDATA_VERSION);
        block.writeUInt16((ushort)packageSkins.Count);
        for (ushort num = 0; num < packageSkins.Count; num = (ushort)(num + 1))
        {
            ulong value = packageSkins[num];
            block.writeUInt64(value);
        }
        block.writeByte(selected);
        for (byte b = 0; b < list.Length; b = (byte)(b + 1))
        {
            Character character = list[b];
            if (character == null)
            {
                character = new Character();
            }
            block.writeUInt16(character.shirt);
            block.writeUInt16(character.pants);
            block.writeUInt16(character.hat);
            block.writeUInt16(character.backpack);
            block.writeUInt16(character.vest);
            block.writeUInt16(character.mask);
            block.writeUInt16(character.glasses);
            block.writeUInt64(character.packageShirt);
            block.writeUInt64(character.packagePants);
            block.writeUInt64(character.packageHat);
            block.writeUInt64(character.packageBackpack);
            block.writeUInt64(character.packageVest);
            block.writeUInt64(character.packageMask);
            block.writeUInt64(character.packageGlasses);
            block.writeUInt16(character.primaryItem);
            block.writeByteArray(character.primaryState);
            block.writeUInt16(character.secondaryItem);
            block.writeByteArray(character.secondaryState);
            block.writeByte(character.face);
            block.writeByte(character.hair);
            block.writeByte(character.beard);
            block.writeColor(character.skin);
            block.writeColor(character.color);
            block.writeColor(character.markerColor);
            block.writeBoolean(character.hand);
            block.writeString(character.name);
            block.writeString(character.nick);
            block.writeSteamID(character.group);
            block.writeByte((byte)character.skillset);
        }
        ReadWrite.writeBlock("/Characters.dat", useCloud: true, block);
    }
}
