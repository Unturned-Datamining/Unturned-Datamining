using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class Character
{
    public ushort shirt;

    public ushort pants;

    public ushort hat;

    public ushort backpack;

    public ushort vest;

    public ushort mask;

    public ushort glasses;

    public ulong packageShirt;

    public ulong packagePants;

    public ulong packageHat;

    public ulong packageBackpack;

    public ulong packageVest;

    public ulong packageMask;

    public ulong packageGlasses;

    public ushort primaryItem;

    public byte[] primaryState;

    public ushort secondaryItem;

    public byte[] secondaryState;

    public byte face;

    public byte hair;

    public byte beard;

    public Color skin;

    public Color color;

    public Color markerColor;

    public bool hand;

    public string name;

    public string nick;

    public CSteamID group;

    public EPlayerSkillset skillset;

    public void applyHero()
    {
        shirt = 0;
        pants = 0;
        hat = 0;
        backpack = 0;
        vest = 0;
        mask = 0;
        glasses = 0;
        primaryItem = 0;
        primaryState = new byte[0];
        secondaryItem = 0;
        secondaryState = new byte[0];
        for (int i = 0; i < PlayerInventory.SKILLSETS_HERO[(byte)skillset].Length; i++)
        {
            ushort id = PlayerInventory.SKILLSETS_HERO[(byte)skillset][i];
            if (!(Assets.find(EAssetType.ITEM, id) is ItemAsset itemAsset))
            {
                continue;
            }
            switch (itemAsset.type)
            {
            case EItemType.SHIRT:
                shirt = id;
                break;
            case EItemType.PANTS:
                pants = id;
                break;
            case EItemType.HAT:
                hat = id;
                break;
            case EItemType.BACKPACK:
                backpack = id;
                break;
            case EItemType.VEST:
                vest = id;
                break;
            case EItemType.MASK:
                mask = id;
                break;
            case EItemType.GLASSES:
                glasses = id;
                break;
            case EItemType.GUN:
            case EItemType.MELEE:
                if (itemAsset.slot == ESlotType.PRIMARY)
                {
                    primaryItem = id;
                    primaryState = itemAsset.getState(EItemOrigin.ADMIN);
                }
                else
                {
                    secondaryItem = id;
                    secondaryState = itemAsset.getState(EItemOrigin.ADMIN);
                }
                break;
            }
        }
    }

    public Character()
    {
        face = (byte)Random.Range(0, Customization.FACES_FREE);
        hair = (byte)Random.Range(0, Customization.HAIRS_FREE);
        beard = 0;
        skin = Customization.SKINS[Random.Range(0, Customization.SKINS.Length)];
        color = Customization.COLORS[Random.Range(0, Customization.COLORS.Length)];
        markerColor = Customization.MARKER_COLORS[Random.Range(0, Customization.MARKER_COLORS.Length)];
        hand = false;
        name = Provider.clientName;
        nick = Provider.clientName;
        group = CSteamID.Nil;
        skillset = (EPlayerSkillset)Random.Range(1, Customization.SKILLSETS);
        applyHero();
    }

    public Character(ushort newShirt, ushort newPants, ushort newHat, ushort newBackpack, ushort newVest, ushort newMask, ushort newGlasses, ulong newPackageShirt, ulong newPackagePants, ulong newPackageHat, ulong newPackageBackpack, ulong newPackageVest, ulong newPackageMask, ulong newPackageGlasses, ushort newPrimaryItem, byte[] newPrimaryState, ushort newSecondaryItem, byte[] newSecondaryState, byte newFace, byte newHair, byte newBeard, Color newSkin, Color newColor, Color newMarkerColor, bool newHand, string newName, string newNick, CSteamID newGroup, EPlayerSkillset newSkillset)
    {
        shirt = newShirt;
        pants = newPants;
        hat = newHat;
        backpack = newBackpack;
        vest = newVest;
        mask = newMask;
        glasses = newGlasses;
        packageShirt = newPackageShirt;
        packagePants = newPackagePants;
        packageHat = newPackageHat;
        packageBackpack = newPackageBackpack;
        packageVest = newPackageVest;
        packageMask = newPackageMask;
        packageGlasses = newPackageGlasses;
        primaryItem = newPrimaryItem;
        secondaryItem = newSecondaryItem;
        primaryState = newPrimaryState;
        secondaryState = newSecondaryState;
        face = newFace;
        hair = newHair;
        beard = newBeard;
        skin = newSkin;
        color = newColor;
        markerColor = newMarkerColor;
        hand = newHand;
        name = newName;
        nick = newNick;
        group = newGroup;
        skillset = newSkillset;
    }
}
