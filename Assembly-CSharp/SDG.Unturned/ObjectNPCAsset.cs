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

    public bool isBackward { get; protected set; }

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

    public bool doesPlayerHaveAccessToVendor(Player player, VendorAsset vendorAsset)
    {
        return FindDialogueAsset()?.doesPlayerHaveAccessToVendor(player, vendorAsset) ?? false;
    }

    public ObjectNPCAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        npcName = localization.format("Character");
        npcName = ItemTool.filterRarityRichText(npcName);
        defaultOutfit = new NPCAssetOutfit(data, ENPCHoliday.NONE);
        if (data.readBoolean("Has_Halloween_Outfit"))
        {
            halloweenOutfit = new NPCAssetOutfit(data, ENPCHoliday.HALLOWEEN);
        }
        if (data.readBoolean("Has_Christmas_Outfit"))
        {
            christmasOutfit = new NPCAssetOutfit(data, ENPCHoliday.CHRISTMAS);
        }
        face = data.readByte("Face", 0);
        hair = data.readByte("Hair", 0);
        beard = data.readByte("Beard", 0);
        skin = Palette.hex(data.readString("Color_Skin"));
        color = Palette.hex(data.readString("Color_Hair"));
        isBackward = data.has("Backward");
        primary = data.ReadGuidOrLegacyId("Primary", out primaryWeaponGuid);
        secondary = data.ReadGuidOrLegacyId("Secondary", out secondaryWeaponGuid);
        tertiary = data.ReadGuidOrLegacyId("Tertiary", out tertiaryWeaponGuid);
        if (data.has("Equipped"))
        {
            equipped = (ESlotType)Enum.Parse(typeof(ESlotType), data.readString("Equipped"), ignoreCase: true);
        }
        else
        {
            equipped = ESlotType.NONE;
        }
        dialogue = data.ReadGuidOrLegacyId("Dialogue", out dialogueGuid);
        if (data.has("Pose"))
        {
            pose = (ENPCPose)Enum.Parse(typeof(ENPCPose), data.readString("Pose"), ignoreCase: true);
        }
        else
        {
            pose = ENPCPose.STAND;
        }
        if (data.has("Pose_Lean"))
        {
            poseLean = data.readSingle("Pose_Lean");
        }
        if (data.has("Pose_Pitch"))
        {
            posePitch = data.readSingle("Pose_Pitch");
        }
        else
        {
            posePitch = 90f;
        }
        if (data.has("Pose_Head_Offset"))
        {
            poseHeadOffset = data.readSingle("Pose_Head_Offset");
        }
        else if (pose == ENPCPose.CROUCH)
        {
            poseHeadOffset = 0.1f;
        }
    }
}
