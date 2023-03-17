using System;
using System.Collections.Generic;
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Unturned;

public class ItemAsset : Asset, ISkinableAsset
{
    public static CommandLineFlag shouldAlwaysLoadItemPrefab = new CommandLineFlag(defaultValue: false, "-AlwaysLoadItemPrefab");

    protected bool _shouldVerifyHash;

    protected string _itemName;

    protected string _itemDescription;

    public EItemType type;

    public EItemRarity rarity;

    protected string _proPath;

    public bool econIconUseId;

    public bool isPro;

    public string useable;

    public ESlotType slot;

    public byte size_x;

    public byte size_y;

    public float iconCameraOrthographicSize;

    public float econIconCameraOrthographicSize;

    public bool isEligibleForAutomaticIconMeasurements;

    public byte amount;

    public byte countMin;

    public byte countMax;

    public byte qualityMin;

    public byte qualityMax;

    public bool isBackward;

    public bool shouldProcedurallyAnimateInertia;

    protected GameObject _item;

    public GameObject equipablePrefab;

    public float equipableMovementSpeedMultiplier = 1f;

    protected AudioClip _equip;

    protected AnimationClip[] _animations;

    public AudioReference inspectAudio;

    public AudioReference inventoryAudio;

    protected List<Blueprint> _blueprints;

    protected List<Action> _actions;

    protected Texture2D _albedoBase;

    protected Texture2D _metallicBase;

    protected Texture2D _emissionBase;

    public bool shouldVerifyHash => _shouldVerifyHash;

    internal override bool ShouldVerifyHash => _shouldVerifyHash;

    public override string FriendlyName => _itemName;

    public string itemName => _itemName;

    public string itemDescription => _itemDescription;

    public string proPath => _proPath;

    public Type useableType { get; protected set; }

    public bool canPlayerEquip { get; protected set; }

    [Obsolete("Renamed to canPlayerEquip")]
    public bool isUseable
    {
        get
        {
            return canPlayerEquip;
        }
        set
        {
            canPlayerEquip = value;
        }
    }

    public bool canUseUnderwater { get; protected set; }

    public byte count
    {
        get
        {
            float num;
            float num2;
            if (Provider.modeConfigData != null)
            {
                if (type == EItemType.MAGAZINE)
                {
                    num = Provider.modeConfigData.Items.Magazine_Bullets_Full_Chance;
                    num2 = Provider.modeConfigData.Items.Magazine_Bullets_Multiplier;
                }
                else
                {
                    num = Provider.modeConfigData.Items.Crate_Bullets_Full_Chance;
                    num2 = Provider.modeConfigData.Items.Crate_Bullets_Multiplier;
                }
            }
            else
            {
                num = 0.9f;
                num2 = 1f;
            }
            if (UnityEngine.Random.value < num)
            {
                return amount;
            }
            return (byte)Mathf.CeilToInt((float)UnityEngine.Random.Range(countMin, countMax + 1) * num2);
        }
    }

    public byte quality
    {
        get
        {
            if (UnityEngine.Random.value < ((Provider.modeConfigData != null) ? Provider.modeConfigData.Items.Quality_Full_Chance : 0.9f))
            {
                return 100;
            }
            return (byte)Mathf.CeilToInt((float)UnityEngine.Random.Range(qualityMin, qualityMax + 1) * ((Provider.modeConfigData != null) ? Provider.modeConfigData.Items.Quality_Multiplier : 1f));
        }
    }

    public GameObject item => _item;

    public string instantiatedItemName { get; protected set; }

    public AudioClip equip => _equip;

    public AnimationClip[] animations => _animations;

    public List<Blueprint> blueprints => _blueprints;

    public List<Action> actions => _actions;

    public bool overrideShowQuality { get; protected set; }

    public virtual bool showQuality => overrideShowQuality;

    public bool shouldDropOnDeath { get; protected set; }

    public bool allowManualDrop { get; protected set; }

    public Texture albedoBase => _albedoBase;

    public Texture metallicBase => _metallicBase;

    public Texture emissionBase => _emissionBase;

    public ushort sharedSkinLookupID { get; protected set; }

    public virtual bool shouldFriendlySentryTargetUser => false;

    [Obsolete]
    public virtual bool isDangerous => shouldFriendlySentryTargetUser;

    public bool shouldDeleteAtZeroQuality { get; protected set; }

    public bool shouldDestroyItemColliders { get; protected set; }

    public override EAssetType assetCategory => EAssetType.ITEM;

    protected virtual bool doesItemTypeHaveSkins => false;

    public byte[] getState()
    {
        return getState(isFull: false);
    }

    public byte[] getState(bool isFull)
    {
        return getState(isFull ? EItemOrigin.ADMIN : EItemOrigin.WORLD);
    }

    public virtual byte[] getState(EItemOrigin origin)
    {
        return new byte[0];
    }

    public virtual string getContext(string desc, byte[] state)
    {
        return desc;
    }

    public void applySkinBaseTextures(Material material)
    {
        if (sharedSkinLookupID > 0 && sharedSkinLookupID != id && Assets.find(EAssetType.ITEM, sharedSkinLookupID) is ItemAsset itemAsset)
        {
            itemAsset.applySkinBaseTextures(material);
            return;
        }
        material.SetTexture("_AlbedoBase", albedoBase);
        material.SetTexture("_MetallicBase", metallicBase);
        material.SetTexture("_EmissionBase", emissionBase);
    }

    [Obsolete("canBeUsedInSafezone now has special cases for admins")]
    public virtual bool canBeUsedInSafezone(SafezoneNode safezone)
    {
        return canBeUsedInSafezone(safezone, byAdmin: false);
    }

    public virtual bool canBeUsedInSafezone(SafezoneNode safezone, bool byAdmin)
    {
        if (safezone.noWeapons)
        {
            return !shouldFriendlySentryTargetUser;
        }
        return true;
    }

    private void updateUseableType()
    {
        if (string.IsNullOrEmpty(useable))
        {
            useableType = null;
            return;
        }
        useableType = Assets.useableTypes.getType(useable);
        if (useableType == null)
        {
            Assets.reportError(this, "cannot find useable type \"{0}\"", useable);
        }
        else if (!typeof(Useable).IsAssignableFrom(useableType))
        {
            Assets.reportError(this, "type \"{0}\" is not useable", useableType);
            useableType = null;
        }
    }

    protected override void readAsset(IFormattedFileReader reader)
    {
        base.readAsset(reader);
        isPro = reader.readValue<bool>("Is_Pro");
        type = reader.readValue<EItemType>("Type");
        rarity = reader.readValue<EItemRarity>("Rarity");
        if (isPro)
        {
            if (type == EItemType.SHIRT)
            {
                _proPath = "/Shirts";
            }
            else if (type == EItemType.PANTS)
            {
                _proPath = "/Pants";
            }
            else if (type == EItemType.HAT)
            {
                _proPath = "/Hats";
            }
            else if (type == EItemType.BACKPACK)
            {
                _proPath = "/Backpacks";
            }
            else if (type == EItemType.VEST)
            {
                _proPath = "/Vests";
            }
            else if (type == EItemType.MASK)
            {
                _proPath = "/Masks";
            }
            else if (type == EItemType.GLASSES)
            {
                _proPath = "/Glasses";
            }
            else if (type == EItemType.KEY)
            {
                _proPath = "/Keys";
            }
            else if (type == EItemType.BOX)
            {
                _proPath = "/Boxes";
            }
            _proPath = _proPath + "/" + name;
        }
        size_x = reader.readValue<byte>("Size_X");
        if (size_x < 1)
        {
            size_x = 1;
        }
        size_y = reader.readValue<byte>("Size_Y");
        if (size_y < 1)
        {
            size_y = 1;
        }
        amount = reader.readValue<byte>("Amount");
        if (amount < 1)
        {
            amount = 1;
        }
        countMin = reader.readValue<byte>("Count_Min");
        if (countMin < 1)
        {
            countMin = 1;
        }
        countMax = reader.readValue<byte>("Count_Max");
        if (countMax < 1)
        {
            countMax = 1;
        }
        qualityMin = reader.readValue<byte>("Quality_Min");
        qualityMax = reader.readValue<byte>("Quality_Max");
        isBackward = reader.readValue<bool>("Backward");
        useable = reader.readValue<string>("Useable");
        updateUseableType();
        bool flag2 = (canPlayerEquip = useableType != null);
        slot = reader.readValue<ESlotType>("Slot");
        int num = reader.readArrayLength("Blueprints");
        _blueprints = new List<Blueprint>(num);
        for (int i = 0; i < num; i++)
        {
            IFormattedFileReader formattedFileReader = reader.readObject(i);
            EBlueprintType newType = formattedFileReader.readValue<EBlueprintType>("Type");
            int num2 = formattedFileReader.readArrayLength("Supplies");
            BlueprintSupply[] array = new BlueprintSupply[num2];
            for (int j = 0; j < num2; j++)
            {
                IFormattedFileReader formattedFileReader2 = formattedFileReader.readObject(j);
                ushort newID = formattedFileReader2.readValue<ushort>("ID");
                bool newCritical = formattedFileReader2.readValue<bool>("Critical");
                byte newAmount = formattedFileReader2.readValue<byte>("Amount");
                array[j] = new BlueprintSupply(newID, newCritical, newAmount);
            }
            int num3 = formattedFileReader.readArrayLength("Output");
            BlueprintOutput[] array2 = new BlueprintOutput[num3];
            for (int k = 0; k < num3; k++)
            {
                IFormattedFileReader formattedFileReader3 = formattedFileReader.readObject(k);
                ushort newID2 = formattedFileReader3.readValue<ushort>("ID");
                byte newAmount2 = formattedFileReader3.readValue<byte>("Amount");
                array2[k] = new BlueprintOutput(newID2, newAmount2, EItemOrigin.CRAFT);
            }
            ushort newTool = formattedFileReader.readValue<ushort>("Tool");
            bool newToolCritical = formattedFileReader.readValue<bool>("Tool_Critical");
            ushort newBuild = formattedFileReader.readValue<ushort>("Build");
            byte newLevel = formattedFileReader.readValue<byte>("Level");
            EBlueprintSkill newSkill = formattedFileReader.readValue<EBlueprintSkill>("Skill");
            bool newTransferState = formattedFileReader.readValue<bool>("Transfer_State");
            string newMap = formattedFileReader.readValue("Map");
            blueprints.Add(new Blueprint(this, (byte)i, newType, array, array2, newTool, newToolCritical, newBuild, newLevel, newSkill, newTransferState, newMap, null, null));
        }
        int num4 = reader.readArrayLength("Actions");
        _actions = new List<Action>(num4);
        for (byte b = 0; b < num4; b = (byte)(b + 1))
        {
            IFormattedFileReader formattedFileReader4 = reader.readObject(b);
            EActionType newType2 = formattedFileReader4.readValue<EActionType>("Type");
            ActionBlueprint[] array3 = new ActionBlueprint[formattedFileReader4.readArrayLength("Blueprints")];
            for (byte b2 = 0; b2 < array3.Length; b2 = (byte)(b2 + 1))
            {
                IFormattedFileReader formattedFileReader5 = formattedFileReader4.readObject(b2);
                byte newID3 = formattedFileReader5.readValue<byte>("Index");
                bool newLink = formattedFileReader5.readValue<bool>("Is_Link");
                array3[b2] = new ActionBlueprint(newID3, newLink);
            }
            string newText = formattedFileReader4.readValue<string>("Text");
            string newTooltip = formattedFileReader4.readValue<string>("Tooltip");
            string newKey = formattedFileReader4.readValue<string>("Key");
            ushort num5 = formattedFileReader4.readValue<ushort>("Source");
            if (num5 == 0)
            {
                num5 = id;
            }
            actions.Add(new Action(num5, newType2, array3, newText, newTooltip, newKey));
        }
        if (num4 == 0)
        {
            bool flag3 = false;
            for (byte b3 = 0; b3 < blueprints.Count; b3 = (byte)(b3 + 1))
            {
                Blueprint blueprint = blueprints[b3];
                if (blueprint.type == EBlueprintType.REPAIR)
                {
                    Action action = new Action(id, EActionType.BLUEPRINT, new ActionBlueprint[1]
                    {
                        new ActionBlueprint(b3, newLink: true)
                    }, null, null, "Repair");
                    actions.Insert(0, action);
                }
                else if (blueprint.type == EBlueprintType.AMMO)
                {
                    flag3 = true;
                }
                else if (blueprint.supplies.Length == 1 && blueprint.supplies[0].id == id)
                {
                    Action action2 = new Action(id, EActionType.BLUEPRINT, new ActionBlueprint[1]
                    {
                        new ActionBlueprint(b3, type == EItemType.GUN || type == EItemType.MELEE)
                    }, null, null, "Salvage");
                    actions.Add(action2);
                }
            }
            if (flag3)
            {
                List<ActionBlueprint> list = new List<ActionBlueprint>();
                for (byte b4 = 0; b4 < blueprints.Count; b4 = (byte)(b4 + 1))
                {
                    if (blueprints[b4].type == EBlueprintType.AMMO)
                    {
                        ActionBlueprint actionBlueprint = new ActionBlueprint(b4, newLink: true);
                        list.Add(actionBlueprint);
                    }
                }
                Action action3 = new Action(id, EActionType.BLUEPRINT, list.ToArray(), null, null, "Refill");
                actions.Add(action3);
            }
        }
        _shouldVerifyHash = reader.readValue<bool>("Should_Verify_Hash");
    }

    protected override void writeAsset(IFormattedFileWriter writer)
    {
        base.writeAsset(writer);
        writer.writeValue("Is_Pro", isPro);
        writer.writeValue("Type", type);
        writer.writeValue("Rarity", rarity);
        writer.writeValue("Size_X", size_x);
        writer.writeValue("Size_Y", size_y);
        writer.writeValue("Amount", amount);
        writer.writeValue("Count_Min", countMin);
        writer.writeValue("Count_Max", countMax);
        writer.writeValue("Quality_Min", qualityMin);
        writer.writeValue("Quality_Max", qualityMax);
        writer.writeValue("Backward", isBackward);
        writer.writeValue("Useable", useable);
        writer.writeValue("Slot", slot);
        writer.beginArray("Blueprints");
        for (int i = 0; i < blueprints.Count; i++)
        {
            writer.beginObject();
            Blueprint blueprint = blueprints[i];
            writer.writeValue("Type", blueprint.type);
            writer.beginArray("Supplies");
            for (int j = 0; j < blueprint.supplies.Length; j++)
            {
                writer.beginObject();
                BlueprintSupply blueprintSupply = blueprint.supplies[j];
                writer.writeValue("ID", blueprintSupply.id);
                writer.writeValue("Critical", blueprintSupply.isCritical);
                writer.writeValue("Amount", blueprintSupply.amount);
                writer.endObject();
            }
            writer.endArray();
            writer.beginArray("Output");
            for (int k = 0; k < blueprint.outputs.Length; k++)
            {
                writer.beginObject();
                BlueprintOutput blueprintOutput = blueprint.outputs[k];
                writer.writeValue("ID", blueprintOutput.id);
                writer.writeValue("Amount", blueprintOutput.amount);
                writer.endObject();
            }
            writer.endArray();
            writer.writeValue("Tool", blueprint.tool);
            writer.writeValue("Tool_Critical", blueprint.toolCritical);
            writer.writeValue("Level", blueprint.level);
            writer.writeValue("Skill", blueprint.skill);
            writer.writeValue("Transfer_State", blueprint.transferState);
            writer.endObject();
        }
        writer.endArray();
        writer.beginArray("Actions");
        for (byte b = 0; b < actions.Count; b = (byte)(b + 1))
        {
            writer.beginObject();
            Action action = actions[b];
            writer.writeValue("Type", action.type);
            writer.beginArray("Blueprints");
            for (byte b2 = 0; b2 < action.blueprints.Length; b2 = (byte)(b2 + 1))
            {
                writer.beginObject();
                ActionBlueprint actionBlueprint = action.blueprints[b2];
                writer.writeValue("Index", actionBlueprint.id);
                writer.writeValue("Is_Link", actionBlueprint.isLink);
                writer.endObject();
            }
            writer.endArray();
            writer.writeValue("Text", action.text);
            writer.writeValue("Tooltip", action.tooltip);
            writer.writeValue("Key", action.key);
            writer.writeValue("Source", action.source);
            writer.endObject();
        }
        writer.endArray();
        writer.writeValue("Should_Verify_Hash", _shouldVerifyHash);
    }

    public ItemAsset()
    {
        _animations = new AnimationClip[0];
        _blueprints = new List<Blueprint>();
        _actions = new List<Action>();
    }

    private void initAnimations(GameObject anim)
    {
        Animation component = anim.GetComponent<Animation>();
        if (component == null)
        {
            Assets.reportError(this, "missing Animation component on 'Animations' GameObject");
            return;
        }
        _animations = new AnimationClip[component.GetClipCount()];
        if (animations.Length < 1)
        {
            Assets.reportError(this, "animation clips list is empty");
            return;
        }
        int num = 0;
        bool flag = false;
        bool flag2 = false;
        foreach (AnimationState item in component)
        {
            animations[num] = item.clip;
            num++;
            flag |= item.clip == null;
            flag2 = flag2 || (item.clip != null && item.clip.name == "Equip");
        }
        if (flag)
        {
            Assets.reportError(this, "has invalid animation clip references");
        }
        if (!flag2)
        {
            Assets.reportError(this, "missing 'Equip' animation clip");
        }
    }

    public ItemAsset(Bundle bundle, Local localization, byte[] hash)
        : base(bundle, localization, hash)
    {
        _itemName = localization.format("Name");
        _itemDescription = localization.format("Description");
        _itemDescription = ItemTool.filterRarityRichText(itemDescription);
        RichTextUtil.replaceNewlineMarkup(ref _itemDescription);
        _equip = bundle.load<AudioClip>("Equip");
        GameObject gameObject = bundle.load<GameObject>("Animations");
        if (gameObject != null)
        {
            initAnimations(gameObject);
        }
        else
        {
            _animations = new AnimationClip[0];
        }
        _item = bundle.load<GameObject>("Item");
        if (item == null)
        {
            throw new NotSupportedException("missing 'Item' GameObject");
        }
        if (item.transform.Find("Icon") != null && item.transform.Find("Icon").GetComponent<Camera>() != null)
        {
            throw new NotSupportedException("'Icon' has a camera attached!");
        }
        AssetValidation.searchGameObjectForErrors(this, item);
        if (!Dedicator.IsDedicatedServer)
        {
            _albedoBase = bundle.load<Texture2D>("Albedo_Base");
            _metallicBase = bundle.load<Texture2D>("Metallic_Base");
            _emissionBase = bundle.load<Texture2D>("Emission_Base");
        }
    }

    public ItemAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        isPro = data.has("Pro");
        if (id < 2000 && !bundle.isCoreAsset && !data.has("Bypass_ID_Limit"))
        {
            throw new NotSupportedException("ID < 2000");
        }
        _itemName = localization.format("Name");
        _itemDescription = localization.format("Description");
        _itemDescription = ItemTool.filterRarityRichText(itemDescription);
        RichTextUtil.replaceNewlineMarkup(ref _itemDescription);
        instantiatedItemName = data.readString("Instantiated_Item_Name_Override", id.ToString());
        type = (EItemType)Enum.Parse(typeof(EItemType), data.readString("Type"), ignoreCase: true);
        if (data.has("Rarity"))
        {
            rarity = (EItemRarity)Enum.Parse(typeof(EItemRarity), data.readString("Rarity"), ignoreCase: true);
        }
        else
        {
            rarity = EItemRarity.COMMON;
        }
        if (isPro)
        {
            econIconUseId = data.readBoolean("Econ_Icon_Use_Id");
            if (type == EItemType.SHIRT)
            {
                _proPath = "/Shirts";
            }
            else if (type == EItemType.PANTS)
            {
                _proPath = "/Pants";
            }
            else if (type == EItemType.HAT)
            {
                _proPath = "/Hats";
            }
            else if (type == EItemType.BACKPACK)
            {
                _proPath = "/Backpacks";
            }
            else if (type == EItemType.VEST)
            {
                _proPath = "/Vests";
            }
            else if (type == EItemType.MASK)
            {
                _proPath = "/Masks";
            }
            else if (type == EItemType.GLASSES)
            {
                _proPath = "/Glasses";
            }
            else if (type == EItemType.KEY)
            {
                _proPath = "/Keys";
            }
            else if (type == EItemType.BOX)
            {
                _proPath = "/Boxes";
            }
            _proPath = _proPath + "/" + name;
        }
        size_x = data.readByte("Size_X", 0);
        if (size_x < 1)
        {
            size_x = 1;
        }
        size_y = data.readByte("Size_Y", 0);
        if (size_y < 1)
        {
            size_y = 1;
        }
        iconCameraOrthographicSize = data.readSingle("Size_Z", -1f);
        isEligibleForAutomaticIconMeasurements = data.readBoolean("Use_Auto_Icon_Measurements", defaultValue: true);
        econIconCameraOrthographicSize = data.readSingle("Size2_Z", -1f);
        sharedSkinLookupID = data.readUInt16("Shared_Skin_Lookup_ID", id);
        amount = data.readByte("Amount", 0);
        if (amount < 1)
        {
            amount = 1;
        }
        countMin = data.readByte("Count_Min", 0);
        if (countMin < 1)
        {
            countMin = 1;
        }
        countMax = data.readByte("Count_Max", 0);
        if (countMax < 1)
        {
            countMax = 1;
        }
        if (data.has("Quality_Min"))
        {
            qualityMin = data.readByte("Quality_Min", 0);
        }
        else
        {
            qualityMin = 10;
        }
        if (data.has("Quality_Max"))
        {
            qualityMax = data.readByte("Quality_Max", 0);
        }
        else
        {
            qualityMax = 90;
        }
        isBackward = data.has("Backward");
        shouldProcedurallyAnimateInertia = data.readBoolean("Procedurally_Animate_Inertia", defaultValue: true);
        useable = data.readString("Useable");
        updateUseableType();
        bool defaultValue = useableType != null;
        canPlayerEquip = data.readBoolean("Can_Player_Equip", defaultValue);
        equipableMovementSpeedMultiplier = data.readSingle("Equipable_Movement_Speed_Multiplier", 1f);
        if (canPlayerEquip)
        {
            _equip = LoadRedirectableAsset<AudioClip>(bundle, "Equip", data, "EquipAudioClip");
            inspectAudio = data.ReadAudioReference("InspectAudioDef");
            MasterBundleReference<GameObject> masterBundleReference = data.readMasterBundleReference<GameObject>("EquipablePrefab");
            if (masterBundleReference.isValid)
            {
                equipablePrefab = masterBundleReference.loadAsset();
            }
            if (!isPro)
            {
                GameObject gameObject = bundle.load<GameObject>("Animations");
                if (gameObject != null)
                {
                    initAnimations(gameObject);
                }
                else
                {
                    _animations = new AnimationClip[0];
                }
            }
        }
        if (data.has("InventoryAudio"))
        {
            inventoryAudio = data.ReadAudioReference("InventoryAudio");
        }
        else
        {
            inventoryAudio = GetDefaultInventoryAudio();
        }
        slot = data.readEnum("Slot", ESlotType.NONE);
        bool defaultValue2 = slot != ESlotType.PRIMARY;
        canUseUnderwater = data.readBoolean("Can_Use_Underwater", defaultValue2);
        if (!Dedicator.IsDedicatedServer || type == EItemType.GUN || type == EItemType.MELEE || (bool)shouldAlwaysLoadItemPrefab)
        {
            _item = bundle.load<GameObject>("Item");
            if (item == null)
            {
                throw new NotSupportedException("missing 'Item' GameObject");
            }
            if (item.transform.Find("Icon") != null && item.transform.Find("Icon").GetComponent<Camera>() != null)
            {
                throw new NotSupportedException("'Icon' has a camera attached!");
            }
            if (id < 2000 && (type == EItemType.GUN || type == EItemType.MELEE) && item.transform.Find("Stat_Tracker") == null)
            {
                Assets.reportError(this, "missing 'Stat_Tracker' Transform");
            }
            AssetValidation.searchGameObjectForErrors(this, item);
        }
        byte b = data.readByte("Blueprints", 0);
        byte b2 = data.readByte("Actions", 0);
        _blueprints = new List<Blueprint>(b);
        _actions = new List<Action>(b2);
        for (byte b3 = 0; b3 < b; b3 = (byte)(b3 + 1))
        {
            if (!data.has("Blueprint_" + b3 + "_Type"))
            {
                throw new NotSupportedException("Missing blueprint type");
            }
            EBlueprintType newType = (EBlueprintType)Enum.Parse(typeof(EBlueprintType), data.readString("Blueprint_" + b3 + "_Type"), ignoreCase: true);
            byte b4 = data.readByte("Blueprint_" + b3 + "_Supplies", 0);
            if (b4 < 1)
            {
                b4 = 1;
            }
            BlueprintSupply[] array = new BlueprintSupply[b4];
            for (byte b5 = 0; b5 < array.Length; b5 = (byte)(b5 + 1))
            {
                ushort newID = data.readUInt16("Blueprint_" + b3 + "_Supply_" + b5 + "_ID", 0);
                bool newCritical = data.has("Blueprint_" + b3 + "_Supply_" + b5 + "_Critical");
                byte b6 = data.readByte("Blueprint_" + b3 + "_Supply_" + b5 + "_Amount", 0);
                if (b6 < 1)
                {
                    b6 = 1;
                }
                array[b5] = new BlueprintSupply(newID, newCritical, b6);
            }
            byte b7 = data.readByte("Blueprint_" + b3 + "_Outputs", 0);
            BlueprintOutput[] array2;
            if (b7 > 0)
            {
                array2 = new BlueprintOutput[b7];
                for (byte b8 = 0; b8 < array2.Length; b8 = (byte)(b8 + 1))
                {
                    ushort newID2 = data.readUInt16("Blueprint_" + b3 + "_Output_" + b8 + "_ID", 0);
                    byte b9 = data.readByte("Blueprint_" + b3 + "_Output_" + b8 + "_Amount", 0);
                    if (b9 < 1)
                    {
                        b9 = 1;
                    }
                    EItemOrigin newOrigin = data.readEnum("Blueprint_" + b3 + "_Output_" + b8 + "_Origin", EItemOrigin.CRAFT);
                    array2[b8] = new BlueprintOutput(newID2, b9, newOrigin);
                }
            }
            else
            {
                array2 = new BlueprintOutput[1];
                ushort num = data.readUInt16("Blueprint_" + b3 + "_Product", 0);
                if (num == 0)
                {
                    num = id;
                }
                byte b10 = data.readByte("Blueprint_" + b3 + "_Products", 0);
                if (b10 < 1)
                {
                    b10 = 1;
                }
                EItemOrigin newOrigin2 = data.readEnum("Blueprint_" + b3 + "_Origin", EItemOrigin.CRAFT);
                array2[0] = new BlueprintOutput(num, b10, newOrigin2);
            }
            ushort newTool = data.readUInt16("Blueprint_" + b3 + "_Tool", 0);
            bool newToolCritical = data.has("Blueprint_" + b3 + "_Tool_Critical");
            Guid guid;
            ushort newBuild = data.ReadGuidOrLegacyId("Blueprint_" + b3 + "_Build", out guid);
            byte b11 = data.readByte("Blueprint_" + b3 + "_Level", 0);
            EBlueprintSkill newSkill = EBlueprintSkill.NONE;
            if (b11 > 0)
            {
                newSkill = (EBlueprintSkill)Enum.Parse(typeof(EBlueprintSkill), data.readString("Blueprint_" + b3 + "_Skill"), ignoreCase: true);
            }
            bool newTransferState = data.has("Blueprint_" + b3 + "_State_Transfer");
            string newMap = data.readString("Blueprint_" + b3 + "_Map");
            INPCCondition[] array3 = new INPCCondition[data.readByte("Blueprint_" + b3 + "_Conditions", 0)];
            NPCTool.readConditions(data, localization, "Blueprint_" + b3 + "_Condition_", array3, this);
            INPCReward[] array4 = new INPCReward[data.readByte("Blueprint_" + b3 + "_Rewards", 0)];
            NPCTool.readRewards(data, localization, "Blueprint_" + b3 + "_Reward_", array4, this);
            blueprints.Add(new Blueprint(this, b3, newType, array, array2, newTool, newToolCritical, newBuild, guid, b11, newSkill, newTransferState, newMap, array3, array4));
        }
        for (byte b12 = 0; b12 < b2; b12 = (byte)(b12 + 1))
        {
            if (!data.has("Action_" + b12 + "_Type"))
            {
                throw new NotSupportedException("Missing action type");
            }
            EActionType newType2 = (EActionType)Enum.Parse(typeof(EActionType), data.readString("Action_" + b12 + "_Type"), ignoreCase: true);
            byte b13 = data.readByte("Action_" + b12 + "_Blueprints", 0);
            if (b13 < 1)
            {
                b13 = 1;
            }
            ActionBlueprint[] array5 = new ActionBlueprint[b13];
            for (byte b14 = 0; b14 < array5.Length; b14 = (byte)(b14 + 1))
            {
                byte newID3 = data.readByte("Action_" + b12 + "_Blueprint_" + b14 + "_Index", 0);
                bool newLink = data.has("Action_" + b12 + "_Blueprint_" + b14 + "_Link");
                array5[b14] = new ActionBlueprint(newID3, newLink);
            }
            string text = data.readString("Action_" + b12 + "_Key");
            string newText;
            string newTooltip;
            if (string.IsNullOrEmpty(text))
            {
                string key = "Action_" + b12 + "_Text";
                newText = ((!localization.has(key)) ? data.readString(key) : localization.format(key));
                string key2 = "Action_" + b12 + "_Tooltip";
                newTooltip = ((!localization.has(key2)) ? data.readString(key2) : localization.format(key2));
            }
            else
            {
                newText = string.Empty;
                newTooltip = string.Empty;
            }
            ushort num2 = data.readUInt16("Action_" + b12 + "_Source", 0);
            if (num2 == 0)
            {
                num2 = id;
            }
            actions.Add(new Action(num2, newType2, array5, newText, newTooltip, text));
        }
        if (b2 == 0)
        {
            bool flag = false;
            for (byte b15 = 0; b15 < blueprints.Count; b15 = (byte)(b15 + 1))
            {
                Blueprint blueprint = blueprints[b15];
                if (blueprint.type == EBlueprintType.REPAIR)
                {
                    Action action = new Action(id, EActionType.BLUEPRINT, new ActionBlueprint[1]
                    {
                        new ActionBlueprint(b15, newLink: true)
                    }, null, null, "Repair");
                    actions.Insert(0, action);
                }
                else if (blueprint.type == EBlueprintType.AMMO)
                {
                    flag = true;
                }
                else if (blueprint.supplies.Length == 1 && blueprint.supplies[0].id == id)
                {
                    Action action2 = new Action(id, EActionType.BLUEPRINT, new ActionBlueprint[1]
                    {
                        new ActionBlueprint(b15, type == EItemType.GUN || type == EItemType.MELEE)
                    }, null, null, "Salvage");
                    actions.Add(action2);
                }
            }
            if (flag)
            {
                List<ActionBlueprint> list = new List<ActionBlueprint>();
                for (byte b16 = 0; b16 < blueprints.Count; b16 = (byte)(b16 + 1))
                {
                    if (blueprints[b16].type == EBlueprintType.AMMO)
                    {
                        ActionBlueprint actionBlueprint = new ActionBlueprint(b16, newLink: true);
                        list.Add(actionBlueprint);
                    }
                }
                Action action3 = new Action(id, EActionType.BLUEPRINT, list.ToArray(), null, null, "Refill");
                actions.Add(action3);
            }
        }
        _shouldVerifyHash = !data.has("Bypass_Hash_Verification");
        overrideShowQuality = data.has("Override_Show_Quality");
        shouldDropOnDeath = data.readBoolean("Should_Drop_On_Death", defaultValue: true);
        allowManualDrop = data.readBoolean("Allow_Manual_Drop", defaultValue: true);
        shouldDeleteAtZeroQuality = data.readBoolean("Should_Delete_At_Zero_Quality");
        shouldDestroyItemColliders = data.readBoolean("Destroy_Item_Colliders", defaultValue: true);
        if (!Dedicator.IsDedicatedServer && id < 2000 && doesItemTypeHaveSkins)
        {
            _albedoBase = bundle.load<Texture2D>("Albedo_Base");
            _metallicBase = bundle.load<Texture2D>("Metallic_Base");
            _emissionBase = bundle.load<Texture2D>("Emission_Base");
        }
    }

    protected virtual AudioReference GetDefaultInventoryAudio()
    {
        if (size_x < 2 && size_y < 2)
        {
            return new AudioReference("core.masterbundle", "Sounds/Inventory/LightGrab.asset");
        }
        return new AudioReference("core.masterbundle", "Sounds/Inventory/RoughGrab.asset");
    }
}
