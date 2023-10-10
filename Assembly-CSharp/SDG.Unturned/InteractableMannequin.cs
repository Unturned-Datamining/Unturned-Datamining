using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableMannequin : Interactable, IManualOnDestroy
{
    private CSteamID _owner;

    private CSteamID _group;

    private bool isLocked;

    public byte pose_comp;

    public bool mirror;

    public byte pose;

    private float updated;

    private Animation anim;

    public byte shirtQuality;

    public byte pantsQuality;

    public byte hatQuality;

    public byte backpackQuality;

    public byte vestQuality;

    public byte maskQuality;

    public byte glassesQuality;

    public byte[] shirtState;

    public byte[] pantsState;

    public byte[] hatState;

    public byte[] backpackState;

    public byte[] vestState;

    public byte[] maskState;

    public byte[] glassesState;

    internal static readonly ClientInstanceMethod<byte> SendPose = ClientInstanceMethod<byte>.Get(typeof(InteractableMannequin), "ReceivePose");

    private static readonly ServerInstanceMethod<byte> SendPoseRequest = ServerInstanceMethod<byte>.Get(typeof(InteractableMannequin), "ReceivePoseRequest");

    private static readonly ClientInstanceMethod<byte[]> SendUpdate = ClientInstanceMethod<byte[]>.Get(typeof(InteractableMannequin), "ReceiveUpdate");

    private static readonly ServerInstanceMethod<EMannequinUpdateMode> SendUpdateRequest = ServerInstanceMethod<EMannequinUpdateMode>.Get(typeof(InteractableMannequin), "ReceiveUpdateRequest");

    private static readonly AssetReference<EffectAsset> SleeveRef = new AssetReference<EffectAsset>("704906b407fe4cb9b4a193ab7447d784");

    public CSteamID owner => _owner;

    public CSteamID group => _group;

    public bool isUpdatable => Time.realtimeSinceStartup - updated > 0.5f;

    public HumanClothes clothes { get; private set; }

    public int visualShirt => clothes.visualShirt;

    public int visualPants => clothes.visualPants;

    public int visualHat => clothes.visualHat;

    public int visualBackpack => clothes.visualBackpack;

    public int visualVest => clothes.visualVest;

    public int visualMask => clothes.visualMask;

    public int visualGlasses => clothes.visualGlasses;

    public ushort shirt => clothes.shirt;

    public ushort pants => clothes.pants;

    public ushort hat => clothes.hat;

    public ushort backpack => clothes.backpack;

    public ushort vest => clothes.vest;

    public ushort mask => clothes.mask;

    public ushort glasses => clothes.glasses;

    public bool isObstructedByPlayers()
    {
        Vector3 position = base.transform.position;
        Vector3 point = position + new Vector3(0f, -0.6f, 0f);
        Vector3 point2 = position + new Vector3(0f, 0.6f, 0f);
        return Physics.OverlapCapsuleNonAlloc(point, point2, 0.4f, InteractableDoor.checkColliders, RayMasks.BLOCK_CHAR_HINGE_OVERLAP, QueryTriggerInteraction.Ignore) > 0;
    }

    public bool checkUpdate(CSteamID enemyPlayer, CSteamID enemyGroup)
    {
        if (Provider.isServer && !Dedicator.IsDedicatedServer)
        {
            return true;
        }
        if (isLocked && !(enemyPlayer == owner))
        {
            if (group != CSteamID.Nil)
            {
                return enemyGroup == group;
            }
            return false;
        }
        return true;
    }

    public byte getComp(bool mirror, byte pose)
    {
        return (byte)(((byte)(mirror ? 1 : 0) << 7) | pose);
    }

    public void applyPose(byte poseComp)
    {
        pose_comp = poseComp;
        mirror = ((poseComp >> 7) & 1) == 1;
        pose = (byte)(poseComp & 0x7Fu);
    }

    public void setPose(byte poseComp)
    {
        applyPose(poseComp);
        updatePose();
    }

    public void rebuildState()
    {
        Block block = new Block();
        block.write(owner, group);
        block.writeInt32(visualShirt);
        block.writeInt32(visualPants);
        block.writeInt32(visualHat);
        block.writeInt32(visualBackpack);
        block.writeInt32(visualVest);
        block.writeInt32(visualMask);
        block.writeInt32(visualGlasses);
        block.writeUInt16(clothes.shirt);
        block.writeByte(shirtQuality);
        block.writeUInt16(clothes.pants);
        block.writeByte(pantsQuality);
        block.writeUInt16(clothes.hat);
        block.writeByte(hatQuality);
        block.writeUInt16(clothes.backpack);
        block.writeByte(backpackQuality);
        block.writeUInt16(clothes.vest);
        block.writeByte(vestQuality);
        block.writeUInt16(clothes.mask);
        block.writeByte(maskQuality);
        block.writeUInt16(clothes.glasses);
        block.writeByte(glassesQuality);
        block.writeByteArray(shirtState);
        block.writeByteArray(pantsState);
        block.writeByteArray(hatState);
        block.writeByteArray(backpackState);
        block.writeByteArray(vestState);
        block.writeByteArray(maskState);
        block.writeByteArray(glassesState);
        block.writeByte(pose_comp);
        int size;
        byte[] bytes = block.getBytes(out size);
        BarricadeManager.updateState(base.transform, bytes, size);
        updated = Time.realtimeSinceStartup;
    }

    public void updateVisuals(int newVisualShirt, int newVisualPants, int newVisualHat, int newVisualBackpack, int newVisualVest, int newVisualMask, int newVisualGlasses)
    {
        clothes.visualShirt = newVisualShirt;
        clothes.visualPants = newVisualPants;
        clothes.visualHat = newVisualHat;
        clothes.visualBackpack = newVisualBackpack;
        clothes.visualVest = newVisualVest;
        clothes.visualMask = newVisualMask;
        clothes.visualGlasses = newVisualGlasses;
    }

    public void clearVisuals()
    {
        updateVisuals(0, 0, 0, 0, 0, 0, 0);
    }

    public void updateClothes(ushort newShirt, byte newShirtQuality, byte[] newShirtState, ushort newPants, byte newPantsQuality, byte[] newPantsState, ushort newHat, byte newHatQuality, byte[] newHatState, ushort newBackpack, byte newBackpackQuality, byte[] newBackpackState, ushort newVest, byte newVestQuality, byte[] newVestState, ushort newMask, byte newMaskQuality, byte[] newMaskState, ushort newGlasses, byte newGlassesQuality, byte[] newGlassesState)
    {
        clothes.shirt = newShirt;
        shirtQuality = newShirtQuality;
        shirtState = newShirtState;
        clothes.pants = newPants;
        pantsQuality = newPantsQuality;
        pantsState = newPantsState;
        clothes.hat = newHat;
        hatQuality = newHatQuality;
        hatState = newHatState;
        clothes.backpack = newBackpack;
        backpackQuality = newBackpackQuality;
        backpackState = newBackpackState;
        clothes.vest = newVest;
        vestQuality = newVestQuality;
        vestState = newVestState;
        clothes.mask = newMask;
        maskQuality = newMaskQuality;
        maskState = newMaskState;
        clothes.glasses = newGlasses;
        glassesQuality = newGlassesQuality;
        glassesState = newGlassesState;
    }

    public void dropClothes()
    {
        if (shirt != 0)
        {
            ItemManager.dropItem(new Item(shirt, 1, shirtQuality, shirtState), base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
        }
        if (pants != 0)
        {
            ItemManager.dropItem(new Item(pants, 1, pantsQuality, pantsState), base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
        }
        if (hat != 0)
        {
            ItemManager.dropItem(new Item(hat, 1, hatQuality, hatState), base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
        }
        if (backpack != 0)
        {
            ItemManager.dropItem(new Item(backpack, 1, backpackQuality, backpackState), base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
        }
        if (vest != 0)
        {
            ItemManager.dropItem(new Item(vest, 1, vestQuality, vestState), base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
        }
        if (mask != 0)
        {
            ItemManager.dropItem(new Item(mask, 1, maskQuality, maskState), base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
        }
        if (glasses != 0)
        {
            ItemManager.dropItem(new Item(glasses, 1, glassesQuality, glassesState), base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
        }
        clearClothes();
    }

    public void clearClothes()
    {
        updateClothes(0, 0, new byte[0], 0, 0, new byte[0], 0, 0, new byte[0], 0, 0, new byte[0], 0, 0, new byte[0], 0, 0, new byte[0], 0, 0, new byte[0]);
    }

    public void updatePose()
    {
        string animation;
        switch (pose)
        {
        default:
            return;
        case 0:
            animation = "T";
            break;
        case 1:
            animation = "Classic";
            break;
        case 2:
            animation = "Lie";
            break;
        }
        if (anim != null)
        {
            anim.transform.localScale = new Vector3((!mirror) ? 1 : (-1), 1f, 1f);
            anim.Play(animation);
        }
    }

    public void updateState(byte[] state)
    {
        Block block = new Block(state);
        _owner = new CSteamID((ulong)block.read(Types.UINT64_TYPE));
        _group = new CSteamID((ulong)block.read(Types.UINT64_TYPE));
        clothes.skin = new Color32(210, 210, 210, byte.MaxValue);
        clothes.color = clothes.skin;
        clothes.visualShirt = block.readInt32();
        clothes.visualPants = block.readInt32();
        clothes.visualHat = block.readInt32();
        clothes.visualBackpack = block.readInt32();
        clothes.visualVest = block.readInt32();
        clothes.visualMask = block.readInt32();
        clothes.visualGlasses = block.readInt32();
        clothes.shirt = block.readUInt16();
        shirtQuality = block.readByte();
        clothes.pants = block.readUInt16();
        pantsQuality = block.readByte();
        clothes.hat = block.readUInt16();
        hatQuality = block.readByte();
        clothes.backpack = block.readUInt16();
        backpackQuality = block.readByte();
        clothes.vest = block.readUInt16();
        vestQuality = block.readByte();
        clothes.mask = block.readUInt16();
        maskQuality = block.readByte();
        clothes.glasses = block.readUInt16();
        glassesQuality = block.readByte();
        shirtState = block.readByteArray();
        pantsState = block.readByteArray();
        hatState = block.readByteArray();
        backpackState = block.readByteArray();
        vestState = block.readByteArray();
        maskState = block.readByteArray();
        glassesState = block.readByteArray();
        clothes.apply();
        setPose(block.readByte());
    }

    public override void updateState(Asset asset, byte[] state)
    {
        isLocked = ((ItemBarricadeAsset)asset).isLocked;
        Transform transform = base.transform.Find("Root");
        anim = transform.GetComponent<Animation>();
        clothes = transform.GetOrAddComponent<HumanClothes>();
        updateState(state);
    }

    public override bool checkUseable()
    {
        if (checkUpdate(Provider.client, Player.player.quests.groupID))
        {
            return !PlayerUI.window.showCursor;
        }
        return false;
    }

    public override void use()
    {
        if (InputEx.GetKey(ControlsSettings.other))
        {
            if (Player.player.equipment.useable is UseableClothing)
            {
                ClientRequestUpdate(EMannequinUpdateMode.ADD);
            }
            else
            {
                ClientRequestUpdate(EMannequinUpdateMode.REMOVE);
            }
        }
        else
        {
            PlayerUI.instance.mannequinUI.open(this);
            PlayerLifeUI.close();
        }
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        if (checkUseable())
        {
            message = EPlayerMessage.USE;
        }
        else
        {
            message = EPlayerMessage.LOCKED;
        }
        text = "";
        color = Color.white;
        return !PlayerUI.window.showCursor;
    }

    public void ManualOnDestroy()
    {
        if (Provider.isServer)
        {
            dropClothes();
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceivePose(byte poseComp)
    {
        setPose(poseComp);
    }

    internal void ClientSetPose(byte poseComp)
    {
        SendPoseRequest.Invoke(GetNetId(), ENetReliability.Unreliable, poseComp);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2)]
    public void ReceivePoseRequest(in ServerInvocationContext context, byte poseComp)
    {
        Player player = context.GetPlayer();
        if (!(player == null) && !player.life.isDead && !((base.transform.position - player.transform.position).sqrMagnitude > 400f) && checkUpdate(player.channel.owner.playerID.steamID, player.quests.groupID) && !isObstructedByPlayers() && BarricadeManager.tryGetRegion(base.transform, out var x, out var y, out var plant, out var _))
        {
            BarricadeManager.InternalSetMannequinPose(this, x, y, plant, poseComp);
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceiveUpdate(byte[] state)
    {
        updateState(state);
    }

    internal void ClientRequestUpdate(EMannequinUpdateMode updateMode)
    {
        SendUpdateRequest.Invoke(GetNetId(), ENetReliability.Unreliable, updateMode);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2)]
    public void ReceiveUpdateRequest(in ServerInvocationContext context, EMannequinUpdateMode updateMode)
    {
        Player player = context.GetPlayer();
        if (player == null || player.life.isDead || player.equipment.isBusy || (player.equipment.HasValidUseable && !player.equipment.IsEquipAnimationFinished) || (base.transform.position - player.transform.position).sqrMagnitude > 400f || !BarricadeManager.tryGetRegion(base.transform, out var x, out var y, out var plant, out var region) || !isUpdatable || !checkUpdate(player.channel.owner.playerID.steamID, player.quests.groupID))
        {
            return;
        }
        switch (updateMode)
        {
        default:
            return;
        case EMannequinUpdateMode.COSMETICS:
            updateVisuals(player.clothing.visualShirt, player.clothing.visualPants, player.clothing.visualHat, player.clothing.visualBackpack, player.clothing.visualVest, player.clothing.visualMask, player.clothing.visualGlasses);
            if (shirt != 0)
            {
                player.inventory.forceAddItem(new Item(shirt, 1, shirtQuality, shirtState), auto: false);
            }
            if (pants != 0)
            {
                player.inventory.forceAddItem(new Item(pants, 1, pantsQuality, pantsState), auto: false);
            }
            if (hat != 0)
            {
                player.inventory.forceAddItem(new Item(hat, 1, hatQuality, hatState), auto: false);
            }
            if (backpack != 0)
            {
                player.inventory.forceAddItem(new Item(backpack, 1, backpackQuality, backpackState), auto: false);
            }
            if (vest != 0)
            {
                player.inventory.forceAddItem(new Item(vest, 1, vestQuality, vestState), auto: false);
            }
            if (mask != 0)
            {
                player.inventory.forceAddItem(new Item(mask, 1, maskQuality, maskState), auto: false);
            }
            if (glasses != 0)
            {
                player.inventory.forceAddItem(new Item(glasses, 1, glassesQuality, glassesState), auto: false);
            }
            clearClothes();
            break;
        case EMannequinUpdateMode.ADD:
        {
            if (!player.equipment.HasValidUseable || !player.equipment.IsEquipAnimationFinished || player.equipment.isBusy || player.equipment.asset == null || !(player.equipment.useable is UseableClothing))
            {
                return;
            }
            ItemJar item = player.inventory.getItem(player.equipment.equippedPage, player.inventory.getIndex(player.equipment.equippedPage, player.equipment.equipped_x, player.equipment.equipped_y));
            if (item == null || item.item == null)
            {
                return;
            }
            clearVisuals();
            switch (player.equipment.asset.type)
            {
            default:
                return;
            case EItemType.SHIRT:
                if (shirt != 0)
                {
                    player.inventory.forceAddItem(new Item(shirt, 1, shirtQuality, shirtState), auto: false);
                }
                clothes.shirt = item.item.id;
                shirtQuality = item.item.quality;
                shirtState = item.item.state;
                break;
            case EItemType.PANTS:
                if (pants != 0)
                {
                    player.inventory.forceAddItem(new Item(pants, 1, pantsQuality, pantsState), auto: false);
                }
                clothes.pants = item.item.id;
                pantsQuality = item.item.quality;
                pantsState = item.item.state;
                break;
            case EItemType.HAT:
                if (hat != 0)
                {
                    player.inventory.forceAddItem(new Item(hat, 1, hatQuality, hatState), auto: false);
                }
                clothes.hat = item.item.id;
                hatQuality = item.item.quality;
                hatState = item.item.state;
                break;
            case EItemType.BACKPACK:
                if (backpack != 0)
                {
                    player.inventory.forceAddItem(new Item(backpack, 1, backpackQuality, backpackState), auto: false);
                }
                clothes.backpack = item.item.id;
                backpackQuality = item.item.quality;
                backpackState = item.item.state;
                break;
            case EItemType.VEST:
                if (vest != 0)
                {
                    player.inventory.forceAddItem(new Item(vest, 1, vestQuality, vestState), auto: false);
                }
                clothes.vest = item.item.id;
                vestQuality = item.item.quality;
                vestState = item.item.state;
                break;
            case EItemType.MASK:
                if (mask != 0)
                {
                    player.inventory.forceAddItem(new Item(mask, 1, maskQuality, maskState), auto: false);
                }
                clothes.mask = item.item.id;
                maskQuality = item.item.quality;
                maskState = item.item.state;
                break;
            case EItemType.GLASSES:
                if (glasses != 0)
                {
                    player.inventory.forceAddItem(new Item(glasses, 1, glassesQuality, glassesState), auto: false);
                }
                clothes.glasses = item.item.id;
                glassesQuality = item.item.quality;
                glassesState = item.item.state;
                break;
            }
            player.equipment.use();
            break;
        }
        case EMannequinUpdateMode.REMOVE:
            clearVisuals();
            if (shirt != 0)
            {
                player.inventory.forceAddItem(new Item(shirt, 1, shirtQuality, shirtState), auto: true, playEffect: false);
            }
            if (pants != 0)
            {
                player.inventory.forceAddItem(new Item(pants, 1, pantsQuality, pantsState), auto: true, playEffect: false);
            }
            if (hat != 0)
            {
                player.inventory.forceAddItem(new Item(hat, 1, hatQuality, hatState), auto: true, playEffect: false);
            }
            if (backpack != 0)
            {
                player.inventory.forceAddItem(new Item(backpack, 1, backpackQuality, backpackState), auto: true, playEffect: false);
            }
            if (vest != 0)
            {
                player.inventory.forceAddItem(new Item(vest, 1, vestQuality, vestState), auto: true, playEffect: false);
            }
            if (mask != 0)
            {
                player.inventory.forceAddItem(new Item(mask, 1, maskQuality, maskState), auto: true, playEffect: false);
            }
            if (glasses != 0)
            {
                player.inventory.forceAddItem(new Item(glasses, 1, glassesQuality, glassesState), auto: true, playEffect: false);
            }
            clearClothes();
            break;
        case EMannequinUpdateMode.SWAP:
        {
            clearVisuals();
            ushort newShirt = player.clothing.shirt;
            byte newShirtQuality = player.clothing.shirtQuality;
            byte[] newShirtState = player.clothing.shirtState;
            ushort newPants = player.clothing.pants;
            byte newPantsQuality = player.clothing.pantsQuality;
            byte[] newPantsState = player.clothing.pantsState;
            ushort newHat = player.clothing.hat;
            byte newHatQuality = player.clothing.hatQuality;
            byte[] newHatState = player.clothing.hatState;
            ushort newBackpack = player.clothing.backpack;
            byte newBackpackQuality = player.clothing.backpackQuality;
            byte[] newBackpackState = player.clothing.backpackState;
            ushort newVest = player.clothing.vest;
            byte newVestQuality = player.clothing.vestQuality;
            byte[] newVestState = player.clothing.vestState;
            ushort newMask = player.clothing.mask;
            byte newMaskQuality = player.clothing.maskQuality;
            byte[] newMaskState = player.clothing.maskState;
            ushort newGlasses = player.clothing.glasses;
            byte newGlassesQuality = player.clothing.glassesQuality;
            byte[] newGlassesState = player.clothing.glassesState;
            player.clothing.updateClothes(shirt, shirtQuality, shirtState, pants, pantsQuality, pantsState, hat, hatQuality, hatState, backpack, backpackQuality, backpackState, vest, vestQuality, vestState, mask, maskQuality, maskState, glasses, glassesQuality, glassesState);
            updateClothes(newShirt, newShirtQuality, newShirtState, newPants, newPantsQuality, newPantsState, newHat, newHatQuality, newHatState, newBackpack, newBackpackQuality, newBackpackState, newVest, newVestQuality, newVestState, newMask, newMaskQuality, newMaskState, newGlasses, newGlassesQuality, newGlassesState);
            break;
        }
        }
        rebuildState();
        byte[] state = region.FindBarricadeByRootFast(base.transform).serversideData.barricade.state;
        SendUpdate.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, BarricadeManager.GatherRemoteClientConnections(x, y, plant), state);
        EffectAsset effectAsset = SleeveRef.Find();
        if (effectAsset != null)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
            parameters.position = base.transform.position;
            parameters.relevantDistance = EffectManager.SMALL;
            EffectManager.triggerEffect(parameters);
        }
    }
}
