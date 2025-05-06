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

    /// <summary>
    /// If non-zero, NPC name is shown as ??? until bool flag is true.
    /// </summary>
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

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        npcName = p.localization.format("Character");
        npcName = ItemTool.filterRarityRichText(npcName);
        defaultOutfit = new NPCAssetOutfit(p.data, ENPCHoliday.NONE);
        if (p.data.ParseBool("Has_Halloween_Outfit"))
        {
            halloweenOutfit = new NPCAssetOutfit(p.data, ENPCHoliday.HALLOWEEN);
        }
        if (p.data.ParseBool("Has_Christmas_Outfit"))
        {
            christmasOutfit = new NPCAssetOutfit(p.data, ENPCHoliday.CHRISTMAS);
        }
        face = p.data.ParseUInt8("Face", 0);
        hair = p.data.ParseUInt8("Hair", 0);
        beard = p.data.ParseUInt8("Beard", 0);
        skin = Palette.hex(p.data.GetString("Color_Skin"));
        color = Palette.hex(p.data.GetString("Color_Hair"));
        IsLeftHanded = p.data.ContainsKey("Backward");
        primary = p.data.ParseGuidOrLegacyId("Primary", out primaryWeaponGuid);
        secondary = p.data.ParseGuidOrLegacyId("Secondary", out secondaryWeaponGuid);
        tertiary = p.data.ParseGuidOrLegacyId("Tertiary", out tertiaryWeaponGuid);
        if (p.data.ContainsKey("Equipped"))
        {
            equipped = (ESlotType)Enum.Parse(typeof(ESlotType), p.data.GetString("Equipped"), ignoreCase: true);
        }
        else
        {
            equipped = ESlotType.NONE;
        }
        dialogue = p.data.ParseGuidOrLegacyId("Dialogue", out dialogueGuid);
        if (p.data.ContainsKey("Pose"))
        {
            pose = (ENPCPose)Enum.Parse(typeof(ENPCPose), p.data.GetString("Pose"), ignoreCase: true);
        }
        else
        {
            pose = ENPCPose.STAND;
        }
        if (p.data.ContainsKey("Pose_Lean"))
        {
            poseLean = p.data.ParseFloat("Pose_Lean");
        }
        if (p.data.ContainsKey("Pose_Pitch"))
        {
            posePitch = p.data.ParseFloat("Pose_Pitch");
        }
        else
        {
            posePitch = 90f;
        }
        if (p.data.ContainsKey("Pose_Head_Offset"))
        {
            poseHeadOffset = p.data.ParseFloat("Pose_Head_Offset");
        }
        else if (pose == ENPCPose.CROUCH)
        {
            poseHeadOffset = 0.1f;
        }
        playerKnowsNameFlagId = p.data.ParseUInt16("PlayerKnowsNameFlagID", 0);
    }

    [Obsolete("Server now tracks dialogue tree")]
    public bool doesPlayerHaveAccessToVendor(Player player, VendorAsset vendorAsset)
    {
        return true;
    }
}
