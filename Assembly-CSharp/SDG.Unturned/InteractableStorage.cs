using SDG.NetTransport;
using SDG.Provider;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableStorage : Interactable, IManualOnDestroy
{
    public delegate void RebuiltStateHandler(InteractableStorage storage, byte[] state, int size);

    private CSteamID _owner;

    private CSteamID _group;

    private Items _items;

    private Transform gunLargeTransform;

    private Transform gunSmallTransform;

    private Transform meleeTransform;

    private Transform itemTransform;

    protected Transform displayModel;

    protected ItemAsset displayAsset;

    public Item displayItem;

    public ushort displaySkin;

    public ushort displayMythic;

    public string displayTags = string.Empty;

    public string displayDynamicProps = string.Empty;

    private Quaternion displayRotation;

    public byte rot_comp;

    public byte rot_x;

    public byte rot_y;

    public byte rot_z;

    public bool isOpen;

    public Player opener;

    private bool isLocked;

    private bool _isDisplay;

    public bool despawnWhenDestroyed;

    public bool shouldCloseWhenOutsideRange;

    public RebuiltStateHandler onStateRebuilt;

    private static readonly ServerInstanceMethod<bool> SendInteractRequest = ServerInstanceMethod<bool>.Get(typeof(InteractableStorage), "ReceiveInteractRequest");

    internal static readonly ClientInstanceMethod<ushort, byte, byte[], ushort, ushort, string, string> SendDisplay = ClientInstanceMethod<ushort, byte, byte[], ushort, ushort, string, string>.Get(typeof(InteractableStorage), "ReceiveDisplay");

    private static readonly ClientInstanceMethod<byte> SendRotDisplay = ClientInstanceMethod<byte>.Get(typeof(InteractableStorage), "ReceiveRotDisplay");

    private static readonly ServerInstanceMethod<byte> SendRotDisplayRequest = ServerInstanceMethod<byte>.Get(typeof(InteractableStorage), "ReceiveRotDisplayRequest");

    public CSteamID owner => _owner;

    public CSteamID group => _group;

    public Items items => _items;

    public bool isDisplay => _isDisplay;

    protected bool getDisplayStatTrackerValue(out EStatTrackerType type, out int kills)
    {
        return new DynamicEconDetails(displayTags, displayDynamicProps).getStatTrackerValue(out type, out kills);
    }

    private void onStateUpdated()
    {
        if (isDisplay)
        {
            updateDisplay();
            if (Dedicator.IsDedicatedServer)
            {
                BarricadeManager.sendStorageDisplay(base.transform, displayItem, displaySkin, displayMythic, displayTags, displayDynamicProps);
            }
            refreshDisplay();
        }
        rebuildState();
    }

    public void rebuildState()
    {
        if (items != null)
        {
            Block block = new Block();
            block.write(owner, group, items.getItemCount());
            for (byte b = 0; b < items.getItemCount(); b = (byte)(b + 1))
            {
                ItemJar item = items.getItem(b);
                block.write(item.x, item.y, item.rot, item.item.id, item.item.amount, item.item.quality, item.item.state);
            }
            if (isDisplay)
            {
                block.write(displaySkin);
                block.write(displayMythic);
                block.write(string.IsNullOrEmpty(displayTags) ? string.Empty : displayTags);
                block.write(string.IsNullOrEmpty(displayDynamicProps) ? string.Empty : displayDynamicProps);
                block.write(rot_comp);
            }
            int size;
            byte[] bytes = block.getBytes(out size);
            if (onStateRebuilt == null)
            {
                BarricadeManager.updateState(base.transform, bytes, size);
            }
            else
            {
                onStateRebuilt(this, bytes, size);
            }
        }
    }

    private void updateDisplay()
    {
        if (items != null && items.getItemCount() > 0)
        {
            if (displayItem != null && items.getItem(0).item == displayItem)
            {
                return;
            }
            if (displayItem != null)
            {
                displaySkin = 0;
                displayMythic = 0;
                displayTags = string.Empty;
                displayDynamicProps = string.Empty;
            }
            displayItem = items.getItem(0).item;
            if (!(opener != null))
            {
                return;
            }
            ushort itemID = displayItem.GetAsset()?.sharedSkinLookupID ?? displayItem.id;
            if (opener.channel.owner.getItemSkinItemDefID(itemID, out var itemdefid))
            {
                displaySkin = Provider.provider.economyService.getInventorySkinID(itemdefid);
                displayMythic = Provider.provider.economyService.getInventoryMythicID(itemdefid);
                if (displayMythic == 0)
                {
                    displayMythic = opener.channel.owner.getParticleEffectForItemDef(itemdefid);
                }
                opener.channel.owner.getTagsAndDynamicPropsForItem(itemdefid, out displayTags, out displayDynamicProps);
            }
        }
        else
        {
            displayItem = null;
            displaySkin = 0;
            displayMythic = 0;
            displayTags = string.Empty;
            displayDynamicProps = string.Empty;
        }
    }

    public void setDisplay(ushort id, byte quality, byte[] state, ushort skin, ushort mythic, string tags, string dynamicProps)
    {
        if (id == 0)
        {
            displayItem = null;
        }
        else
        {
            displayItem = new Item(id, 0, quality, state);
        }
        displaySkin = skin;
        displayMythic = mythic;
        displayTags = tags;
        displayDynamicProps = dynamicProps;
        refreshDisplay();
    }

    public byte getRotation(byte rot_x, byte rot_y, byte rot_z)
    {
        return (byte)((rot_x << 4) | (rot_y << 2) | rot_z);
    }

    public void applyRotation(byte rotComp)
    {
        rot_comp = rotComp;
        rot_x = (byte)((uint)(rotComp >> 4) & 3u);
        rot_y = (byte)((uint)(rotComp >> 2) & 3u);
        rot_z = (byte)(rotComp & 3u);
        displayRotation = Quaternion.Euler(rot_x * 90, rot_y * 90, rot_z * 90);
    }

    public void setRotation(byte rotComp)
    {
        applyRotation(rotComp);
        refreshDisplay();
    }

    public virtual void refreshDisplay()
    {
        if (displayModel != null)
        {
            Object.Destroy(displayModel.gameObject);
            displayModel = null;
            displayAsset = null;
        }
        if (displayItem == null || gunLargeTransform == null || gunSmallTransform == null || meleeTransform == null || itemTransform == null)
        {
            return;
        }
        displayAsset = displayItem.GetAsset();
        if (displayAsset == null)
        {
            return;
        }
        if (displaySkin != 0)
        {
            if (!(Assets.find(EAssetType.SKIN, displaySkin) is SkinAsset))
            {
                return;
            }
            displayModel = ItemTool.getItem(displayItem.id, displaySkin, displayItem.quality, displayItem.state, viewmodel: false, displayAsset, shouldDestroyColliders: true, getDisplayStatTrackerValue);
            if (displayMythic != 0)
            {
                ItemTool.applyEffect(displayModel, displayMythic, EEffectType.THIRD);
            }
        }
        else
        {
            displayModel = ItemTool.getItem(displayItem.id, 0, displayItem.quality, displayItem.state, viewmodel: false, displayAsset, shouldDestroyColliders: true, getDisplayStatTrackerValue);
            if (displayMythic != 0)
            {
                ItemTool.applyEffect(displayModel, displayMythic, EEffectType.HOOK);
            }
        }
        if (displayModel == null)
        {
            return;
        }
        if (displayAsset.type == EItemType.GUN)
        {
            if (displayAsset.slot == ESlotType.PRIMARY)
            {
                displayModel.parent = gunLargeTransform;
            }
            else
            {
                displayModel.parent = gunSmallTransform;
            }
        }
        else if (displayAsset.type == EItemType.MELEE)
        {
            displayModel.parent = meleeTransform;
        }
        else
        {
            displayModel.parent = itemTransform;
        }
        displayModel.localPosition = Vector3.zero;
        displayModel.localRotation = displayRotation;
        displayModel.localScale = Vector3.one;
        displayModel.DestroyRigidbody();
    }

    public bool checkRot(CSteamID enemyPlayer, CSteamID enemyGroup)
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

    public bool checkStore(CSteamID enemyPlayer, CSteamID enemyGroup)
    {
        if (Provider.isServer && !Dedicator.IsDedicatedServer)
        {
            return true;
        }
        if (!isLocked || enemyPlayer == owner || (group != CSteamID.Nil && enemyGroup == group))
        {
            return !isOpen;
        }
        return false;
    }

    public override void updateState(Asset asset, byte[] state)
    {
        gunLargeTransform = base.transform.FindChildRecursive("Gun_Large");
        gunSmallTransform = base.transform.FindChildRecursive("Gun_Small");
        meleeTransform = base.transform.FindChildRecursive("Melee");
        itemTransform = base.transform.FindChildRecursive("Item");
        isLocked = ((ItemBarricadeAsset)asset).isLocked;
        _isDisplay = ((ItemStorageAsset)asset).isDisplay;
        shouldCloseWhenOutsideRange = ((ItemStorageAsset)asset).shouldCloseWhenOutsideRange;
        if (Provider.isServer)
        {
            Block block = new Block(state);
            _owner = (CSteamID)block.read(Types.STEAM_ID_TYPE);
            _group = (CSteamID)block.read(Types.STEAM_ID_TYPE);
            _items = new Items(PlayerInventory.STORAGE);
            items.resize(((ItemStorageAsset)asset).storage_x, ((ItemStorageAsset)asset).storage_y);
            byte b = block.readByte();
            for (byte b2 = 0; b2 < b; b2 = (byte)(b2 + 1))
            {
                if (BarricadeManager.version > 7)
                {
                    object[] array = block.read(Types.BYTE_TYPE, Types.BYTE_TYPE, Types.BYTE_TYPE, Types.UINT16_TYPE, Types.BYTE_TYPE, Types.BYTE_TYPE, Types.BYTE_ARRAY_TYPE);
                    if (Assets.find(EAssetType.ITEM, (ushort)array[3]) is ItemAsset)
                    {
                        items.loadItem((byte)array[0], (byte)array[1], (byte)array[2], new Item((ushort)array[3], (byte)array[4], (byte)array[5], (byte[])array[6]));
                    }
                }
                else
                {
                    object[] array2 = block.read(Types.BYTE_TYPE, Types.BYTE_TYPE, Types.UINT16_TYPE, Types.BYTE_TYPE, Types.BYTE_TYPE, Types.BYTE_ARRAY_TYPE);
                    if (Assets.find(EAssetType.ITEM, (ushort)array2[2]) is ItemAsset)
                    {
                        items.loadItem((byte)array2[0], (byte)array2[1], 0, new Item((ushort)array2[2], (byte)array2[3], (byte)array2[4], (byte[])array2[5]));
                    }
                }
            }
            if (isDisplay)
            {
                displaySkin = block.readUInt16();
                displayMythic = block.readUInt16();
                if (BarricadeManager.version > 12)
                {
                    displayTags = block.readString();
                    displayDynamicProps = block.readString();
                }
                else
                {
                    displayTags = string.Empty;
                    displayDynamicProps = string.Empty;
                }
                if (BarricadeManager.version > 8)
                {
                    applyRotation(block.readByte());
                }
                else
                {
                    applyRotation(0);
                }
            }
            items.onStateUpdated = onStateUpdated;
            if (isDisplay)
            {
                updateDisplay();
                refreshDisplay();
            }
        }
        else
        {
            Block block2 = new Block(state);
            _owner = new CSteamID((ulong)block2.read(Types.UINT64_TYPE));
            _group = new CSteamID((ulong)block2.read(Types.UINT64_TYPE));
            if (state.Length > 16)
            {
                object[] array3 = block2.read(Types.UINT16_TYPE, Types.BYTE_TYPE, Types.BYTE_ARRAY_TYPE, Types.UINT16_TYPE, Types.UINT16_TYPE, Types.STRING_TYPE, Types.STRING_TYPE, Types.BYTE_TYPE);
                applyRotation((byte)array3[7]);
                setDisplay((ushort)array3[0], (byte)array3[1], (byte[])array3[2], (ushort)array3[3], (ushort)array3[4], (string)array3[5], (string)array3[6]);
            }
        }
    }

    public override bool checkUseable()
    {
        if (checkStore(Provider.client, Player.player.quests.groupID))
        {
            return !PlayerUI.window.showCursor;
        }
        return false;
    }

    public override void use()
    {
        ClientInteract(InputEx.GetKey(ControlsSettings.other));
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        text = "";
        color = Color.white;
        if (checkUseable())
        {
            message = EPlayerMessage.STORAGE;
        }
        else
        {
            message = EPlayerMessage.LOCKED;
        }
        return true;
    }

    private void Start()
    {
        if (Provider.isServer && BarricadeManager.version < 13)
        {
            onStateUpdated();
        }
    }

    public void ManualOnDestroy()
    {
        if (isDisplay)
        {
            setDisplay(0, 0, null, 0, 0, string.Empty, string.Empty);
        }
        if (!Provider.isServer)
        {
            return;
        }
        items.onStateUpdated = null;
        if (!despawnWhenDestroyed)
        {
            for (byte b = 0; b < items.getItemCount(); b = (byte)(b + 1))
            {
                ItemManager.dropItem(items.getItem(b).item, base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
            }
        }
        items.clear();
        _items = null;
        if (!isOpen)
        {
            return;
        }
        if (opener != null)
        {
            if (opener.inventory.isStoring)
            {
                opener.inventory.closeStorageAndNotifyClient();
            }
            opener = null;
        }
        isOpen = false;
    }

    public void ClientInteract(bool quickGrab)
    {
        SendInteractRequest.Invoke(GetNetId(), ENetReliability.Unreliable, quickGrab);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 4)]
    public void ReceiveInteractRequest(in ServerInvocationContext context, bool quickGrab)
    {
        Player player = context.GetPlayer();
        if (player == null || player.life.isDead || (player.inventory.isStoring && player.inventory.isStorageTrunk) || player.animator.gesture == EPlayerGesture.ARREST_START)
        {
            return;
        }
        Vector3 position = base.transform.position;
        if ((position - player.transform.position).sqrMagnitude > 400f || Physics.Linecast(player.look.getEyesPosition(), position, RayMasks.BLOCK_BARRICADE_INTERACT_LOS, QueryTriggerInteraction.Ignore))
        {
            return;
        }
        if (player.inventory.isStoring)
        {
            player.inventory.closeStorage();
        }
        if (checkStore(player.channel.owner.playerID.steamID, player.quests.groupID))
        {
            bool shouldAllow = true;
            BarricadeManager.onOpenStorageRequested?.Invoke(player.channel.owner.playerID.steamID, this, ref shouldAllow);
            if (!shouldAllow)
            {
                return;
            }
            if (isDisplay && quickGrab)
            {
                if (displayItem != null)
                {
                    player.inventory.forceAddItem(displayItem, auto: true);
                    displayItem = null;
                    displaySkin = 0;
                    displayMythic = 0;
                    displayTags = string.Empty;
                    displayDynamicProps = string.Empty;
                    items.removeItem(0);
                }
            }
            else
            {
                player.inventory.openStorage(this);
            }
        }
        else
        {
            player.sendMessage(EPlayerMessage.BUSY);
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceiveDisplay(ushort id, byte quality, byte[] state, ushort skin, ushort mythic, string tags, string dynamicProps)
    {
        setDisplay(id, quality, state, skin, mythic, tags, dynamicProps);
    }

    public void ClientSetDisplayRotation(byte rotComp)
    {
        SendRotDisplayRequest.Invoke(GetNetId(), ENetReliability.Unreliable, rotComp);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceiveRotDisplay(byte rotComp)
    {
        setRotation(rotComp);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2)]
    public void ReceiveRotDisplayRequest(in ServerInvocationContext context, byte rotComp)
    {
        Player player = context.GetPlayer();
        if (!(player == null) && !player.life.isDead && !((base.transform.position - player.transform.position).sqrMagnitude > 400f) && BarricadeManager.tryGetRegion(base.transform, out var x, out var y, out var plant, out var _) && checkRot(player.channel.owner.playerID.steamID, player.quests.groupID) && isDisplay)
        {
            SendRotDisplay.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, BarricadeManager.GatherRemoteClientConnections(x, y, plant), rotComp);
            rebuildState();
        }
    }
}
