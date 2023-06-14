using System;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class ObjectNPCAsset : ObjectAsset
{
    public Guid primaryWeaponGuid;

    public Guid secondaryWeaponGuid;

    public Guid tertiaryWeaponGuid;

    public Guid dialogueGuid;

    public string npcName { get; protected set; }

    public NPCAssetOutfit defaultOutfit { get; protected set; }

    public NPCAssetOutfit halloweenOutfit { get; protected set; }

    public NPCAssetOutfit christmasOutfit { get; protected set; }

    public NPCAssetOutfit currentOutfit
    {
        get
        {
            switch (HolidayUtil.getActiveHoliday())
            {
            case ENPCHoliday.HALLOWEEN:
                if (halloweenOutfit == null)
                {
                    return defaultOutfit;
                }
                return halloweenOutfit;
            case ENPCHoliday.CHRISTMAS:
                if (christmasOutfit == null)
                {
                    return defaultOutfit;
                }
                return christmasOutfit;
            default:
                return defaultOutfit;
            }
        }
    }

    public byte face { get; protected set; }

    public byte hair { get; protected set; }

    public byte beard { get; protected set; }

    public Color skin { get; protected set; }

    public Color color { get; protected set; }

    public bool IsLeftHanded { get; protected set; }

    [Obsolete]
    public bool isBackward
    {
        get
        {
            return IsLeftHanded;
        }
        protected set
        {
            IsLeftHanded = value;
        }
    }

    [Obsolete]
    public ushort primary { get; protected set; }

    [Obsolete]
    public ushort secondary { get; protected set; }

    [Obsolete]
    public ushort tertiary { get; protected set; }

    public ESlotType equipped { get; protected set; }

    public ushort dialogue
    {
        [Obsolete]
        get;
        protected set; }

    public ENPCPose pose { get; protected set; }

    public float poseLean { get; protected set; }

    public float posePitch { get; protected set; }

    public float poseHeadOffset { get; protected set; }

    public ushort playerKnowsNameFlagId { get; protected set; }

    public bool IsDialogueRefNull()
    {
        if (dialogue == 0)
        {
            return dialogueGuid.IsEmpty();
        }
        return false;
    }

    public DialogueAsset FindDialogueAsset()
    {
        return Assets.FindNpcAssetByGuidOrLegacyId<DialogueAsset>(dialogueGuid, dialogue);
    }

    public string GetNameShownToPlayer(Player player)
    {
        if (player == null || playerKnowsNameFlagId == 0)
        {
            return npcName;
        }
        if (player.quests.getFlag(playerKnowsNameFlagId, out var value) && value == 1)
        {
            return npcName;
        }
        return "???";
    }

    public bool doesPlayerHaveAccessToVendor(Player player, VendorAsset vendorAsset)
    {
        return FindDialogueAsset()?.doesPlayerHaveAccessToVendor(player, vendorAsset) ?? false;
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        npcName = localization.format("Character");
        npcName = ItemTool.filterRarityRichText(npcName);
        defaultOutfit = new NPCAssetOutfit(data, ENPCHoliday.NONE);
        if (data.ParseBool("Has_Halloween_Outfit"))
        {
            halloweenOutfit = new NPCAssetOutfit(data, ENPCHoliday.HALLOWEEN);
        }
        if (data.ParseBool("Has_Christmas_Outfit"))
        {
            christmasOutfit = new NPCAssetOutfit(data, ENPCHoliday.CHRISTMAS);
        }
        face = data.ParseUInt8("Face", 0);
        hair = data.ParseUInt8("Hair", 0);
        beard = data.ParseUInt8("Beard", 0);
        skin = Palette.hex(data.GetString("Color_Skin"));
        color = Palette.hex(data.GetString("Color_Hair"));
        IsLeftHanded = data.ContainsKey("Backward");
        primary = data.ParseGuidOrLegacyId("Primary", out primaryWeaponGuid);
        secondary = data.ParseGuidOrLegacyId("Secondary", out secondaryWeaponGuid);
        tertiary = data.ParseGuidOrLegacyId("Tertiary", out tertiaryWeaponGuid);
        if (data.ContainsKey("Equipped"))
        {
            equipped = (ESlotType)Enum.Parse(typeof(ESlotType), data.GetString("Equipped"), ignoreCase: true);
        }
        else
        {
            equipped = ESlotType.NONE;
        }
        dialogue = data.ParseGuidOrLegacyId("Dialogue", out dialogueGuid);
        if (data.ContainsKey("Pose"))
        {
            pose = (ENPCPose)Enum.Parse(typeof(ENPCPose), data.GetString("Pose"), ignoreCase: true);
        }
        else
        {
            pose = ENPCPose.STAND;
        }
        if (data.ContainsKey("Pose_Lean"))
        {
            poseLean = data.ParseFloat("Pose_Lean");
        }
        if (data.ContainsKey("Pose_Pitch"))
        {
            posePitch = data.ParseFloat("Pose_Pitch");
        }
        else
        {
            posePitch = 90f;
        }
        if (data.ContainsKey("Pose_Head_Offset"))
        {
            poseHeadOffset = data.ParseFloat("Pose_Head_Offset");
        }
        else if (pose == ENPCPose.CROUCH)
        {
            poseHeadOffset = 0.1f;
        }
        playerKnowsNameFlagId = data.ParseUInt16("PlayerKnowsNameFlagID", 0);
    }
}
