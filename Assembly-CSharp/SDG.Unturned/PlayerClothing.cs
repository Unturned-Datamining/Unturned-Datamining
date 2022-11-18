using System;
using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerClothing : PlayerCaller
{
    public static readonly byte SAVEDATA_VERSION = 7;

    public ShirtUpdated onShirtUpdated;

    public PantsUpdated onPantsUpdated;

    public HatUpdated onHatUpdated;

    public BackpackUpdated onBackpackUpdated;

    public VestUpdated onVestUpdated;

    public MaskUpdated onMaskUpdated;

    public GlassesUpdated onGlassesUpdated;

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

    private static readonly ClientInstanceMethod<byte> SendShirtQuality = ClientInstanceMethod<byte>.Get(typeof(PlayerClothing), "ReceiveShirtQuality");

    private static readonly ClientInstanceMethod<byte> SendPantsQuality = ClientInstanceMethod<byte>.Get(typeof(PlayerClothing), "ReceivePantsQuality");

    private static readonly ClientInstanceMethod<byte> SendHatQuality = ClientInstanceMethod<byte>.Get(typeof(PlayerClothing), "ReceiveHatQuality");

    private static readonly ClientInstanceMethod<byte> SendBackpackQuality = ClientInstanceMethod<byte>.Get(typeof(PlayerClothing), "ReceiveBackpackQuality");

    private static readonly ClientInstanceMethod<byte> SendVestQuality = ClientInstanceMethod<byte>.Get(typeof(PlayerClothing), "ReceiveVestQuality");

    private static readonly ClientInstanceMethod<byte> SendMaskQuality = ClientInstanceMethod<byte>.Get(typeof(PlayerClothing), "ReceiveMaskQuality");

    private static readonly ClientInstanceMethod<byte> SendGlassesQuality = ClientInstanceMethod<byte>.Get(typeof(PlayerClothing), "ReceiveGlassesQuality");

    private static readonly ClientInstanceMethod<Guid, byte, byte[], bool> SendWearShirt = ClientInstanceMethod<Guid, byte, byte[], bool>.Get(typeof(PlayerClothing), "ReceiveWearShirt");

    private static readonly ServerInstanceMethod<byte, byte, byte> SendSwapShirtRequest = ServerInstanceMethod<byte, byte, byte>.Get(typeof(PlayerClothing), "ReceiveSwapShirtRequest");

    private static readonly ClientInstanceMethod<Guid, byte, byte[], bool> SendWearPants = ClientInstanceMethod<Guid, byte, byte[], bool>.Get(typeof(PlayerClothing), "ReceiveWearPants");

    private static readonly ServerInstanceMethod<byte, byte, byte> SendSwapPantsRequest = ServerInstanceMethod<byte, byte, byte>.Get(typeof(PlayerClothing), "ReceiveSwapPantsRequest");

    private static readonly ClientInstanceMethod<Guid, byte, byte[], bool> SendWearHat = ClientInstanceMethod<Guid, byte, byte[], bool>.Get(typeof(PlayerClothing), "ReceiveWearHat");

    private static readonly ServerInstanceMethod<byte, byte, byte> SendSwapHatRequest = ServerInstanceMethod<byte, byte, byte>.Get(typeof(PlayerClothing), "ReceiveSwapHatRequest");

    private static readonly ClientInstanceMethod<Guid, byte, byte[], bool> SendWearBackpack = ClientInstanceMethod<Guid, byte, byte[], bool>.Get(typeof(PlayerClothing), "ReceiveWearBackpack");

    private static readonly ServerInstanceMethod<byte, byte, byte> SendSwapBackpackRequest = ServerInstanceMethod<byte, byte, byte>.Get(typeof(PlayerClothing), "ReceiveSwapBackpackRequest");

    private static readonly ClientInstanceMethod<EVisualToggleType, bool> SendVisualToggleState = ClientInstanceMethod<EVisualToggleType, bool>.Get(typeof(PlayerClothing), "ReceiveVisualToggleState");

    private static readonly ServerInstanceMethod<EVisualToggleType> SendVisualToggleRequest = ServerInstanceMethod<EVisualToggleType>.Get(typeof(PlayerClothing), "ReceiveVisualToggleRequest");

    private static readonly ClientInstanceMethod<Guid, byte, byte[], bool> SendWearVest = ClientInstanceMethod<Guid, byte, byte[], bool>.Get(typeof(PlayerClothing), "ReceiveWearVest");

    private static readonly ServerInstanceMethod<byte, byte, byte> SendSwapVestRequest = ServerInstanceMethod<byte, byte, byte>.Get(typeof(PlayerClothing), "ReceiveSwapVestRequest");

    private static readonly ClientInstanceMethod<Guid, byte, byte[], bool> SendWearMask = ClientInstanceMethod<Guid, byte, byte[], bool>.Get(typeof(PlayerClothing), "ReceiveWearMask");

    private static readonly ServerInstanceMethod<byte, byte, byte> SendSwapMaskRequest = ServerInstanceMethod<byte, byte, byte>.Get(typeof(PlayerClothing), "ReceiveSwapMaskRequest");

    private static readonly ClientInstanceMethod<Guid, byte, byte[], bool> SendWearGlasses = ClientInstanceMethod<Guid, byte, byte[], bool>.Get(typeof(PlayerClothing), "ReceiveWearGlasses");

    private static readonly ServerInstanceMethod<byte, byte, byte> SendSwapGlassesRequest = ServerInstanceMethod<byte, byte, byte>.Get(typeof(PlayerClothing), "ReceiveSwapGlassesRequest");

    private static readonly ClientInstanceMethod SendClothingState = ClientInstanceMethod.Get(typeof(PlayerClothing), "ReceiveClothingState");

    private static readonly ClientInstanceMethod<byte> SendFaceState = ClientInstanceMethod<byte>.Get(typeof(PlayerClothing), "ReceiveFaceState");

    private static readonly ServerInstanceMethod<byte> SendSwapFaceRequest = ServerInstanceMethod<byte>.Get(typeof(PlayerClothing), "ReceiveSwapFaceRequest");

    private bool wasLoadCalled;

    internal float speedMultiplier = 1f;

    public HumanClothes firstClothes { get; private set; }

    public HumanClothes thirdClothes { get; private set; }

    public HumanClothes characterClothes { get; private set; }

    public bool isVisual => thirdClothes.isVisual;

    public bool isSkinned { get; private set; }

    public bool isMythic => thirdClothes.isMythic;

    public ItemShirtAsset shirtAsset => thirdClothes.shirtAsset;

    public ItemPantsAsset pantsAsset => thirdClothes.pantsAsset;

    public ItemHatAsset hatAsset => thirdClothes.hatAsset;

    public ItemBackpackAsset backpackAsset => thirdClothes.backpackAsset;

    public ItemVestAsset vestAsset => thirdClothes.vestAsset;

    public ItemMaskAsset maskAsset => thirdClothes.maskAsset;

    public ItemGlassesAsset glassesAsset => thirdClothes.glassesAsset;

    public int visualShirt => thirdClothes.visualShirt;

    public int visualPants => thirdClothes.visualPants;

    public int visualHat => thirdClothes.visualHat;

    public int visualBackpack => thirdClothes.visualBackpack;

    public int visualVest => thirdClothes.visualVest;

    public int visualMask => thirdClothes.visualMask;

    public int visualGlasses => thirdClothes.visualGlasses;

    public ushort shirt => thirdClothes.shirt;

    public ushort pants => thirdClothes.pants;

    public ushort hat => thirdClothes.hat;

    public ushort backpack => thirdClothes.backpack;

    public ushort vest => thirdClothes.vest;

    public ushort mask => thirdClothes.mask;

    public ushort glasses => thirdClothes.glasses;

    public byte face => thirdClothes.face;

    public byte hair => thirdClothes.hair;

    public byte beard => thirdClothes.beard;

    public Color skin => thirdClothes.skin;

    public Color color => thirdClothes.color;

    public event VisualToggleChanged VisualToggleChanged;

    public static event Action<PlayerClothing> OnShirtChanged_Global;

    public static event Action<PlayerClothing> OnPantsChanged_Global;

    public static event Action<PlayerClothing> OnHatChanged_Global;

    public static event Action<PlayerClothing> OnBackpackChanged_Global;

    public static event Action<PlayerClothing> OnVestChanged_Global;

    public static event Action<PlayerClothing> OnMaskChanged_Global;

    public static event Action<PlayerClothing> OnGlassesChanged_Global;

    [Obsolete]
    public void tellUpdateShirtQuality(CSteamID steamID, byte quality)
    {
        ReceiveShirtQuality(quality);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUpdateShirtQuality")]
    public void ReceiveShirtQuality(byte quality)
    {
        shirtQuality = quality;
        if (onShirtUpdated != null)
        {
            onShirtUpdated(shirt, shirtQuality, shirtState);
        }
    }

    public void sendUpdateShirtQuality()
    {
        SendShirtQuality.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), shirtQuality);
    }

    [Obsolete]
    public void tellUpdatePantsQuality(CSteamID steamID, byte quality)
    {
        ReceivePantsQuality(quality);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUpdatePantsQuality")]
    public void ReceivePantsQuality(byte quality)
    {
        pantsQuality = quality;
        if (onPantsUpdated != null)
        {
            onPantsUpdated(pants, pantsQuality, pantsState);
        }
    }

    public void sendUpdatePantsQuality()
    {
        SendPantsQuality.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), pantsQuality);
    }

    [Obsolete]
    public void tellUpdateHatQuality(CSteamID steamID, byte quality)
    {
        ReceiveHatQuality(quality);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUpdateHatQuality")]
    public void ReceiveHatQuality(byte quality)
    {
        hatQuality = quality;
        if (onHatUpdated != null)
        {
            onHatUpdated(hat, hatQuality, hatState);
        }
    }

    public void sendUpdateHatQuality()
    {
        SendHatQuality.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), hatQuality);
    }

    [Obsolete]
    public void tellUpdateBackpackQuality(CSteamID steamID, byte quality)
    {
        ReceiveBackpackQuality(quality);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUpdateBackpackQuality")]
    public void ReceiveBackpackQuality(byte quality)
    {
        backpackQuality = quality;
        if (onBackpackUpdated != null)
        {
            onBackpackUpdated(backpack, backpackQuality, backpackState);
        }
    }

    public void sendUpdateBackpackQuality()
    {
        SendBackpackQuality.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), backpackQuality);
    }

    [Obsolete]
    public void tellUpdateVestQuality(CSteamID steamID, byte quality)
    {
        ReceiveVestQuality(quality);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUpdateVestQuality")]
    public void ReceiveVestQuality(byte quality)
    {
        vestQuality = quality;
        if (onVestUpdated != null)
        {
            onVestUpdated(vest, vestQuality, vestState);
        }
    }

    public void sendUpdateVestQuality()
    {
        SendVestQuality.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), vestQuality);
    }

    [Obsolete]
    public void tellUpdateMaskQuality(CSteamID steamID, byte quality)
    {
        ReceiveMaskQuality(quality);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUpdateMaskQuality")]
    public void ReceiveMaskQuality(byte quality)
    {
        maskQuality = quality;
        if (onMaskUpdated != null)
        {
            onMaskUpdated(mask, maskQuality, maskState);
        }
    }

    public void sendUpdateMaskQuality()
    {
        SendMaskQuality.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), maskQuality);
    }

    public void updateMaskQuality()
    {
        if (onMaskUpdated != null)
        {
            onMaskUpdated(mask, maskQuality, maskState);
        }
    }

    [Obsolete]
    public void tellUpdateGlassesQuality(CSteamID steamID, byte quality)
    {
        ReceiveGlassesQuality(quality);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUpdateGlassesQuality")]
    public void ReceiveGlassesQuality(byte quality)
    {
        glassesQuality = quality;
        if (onGlassesUpdated != null)
        {
            onGlassesUpdated(glasses, glassesQuality, glassesState);
        }
    }

    public void sendUpdateGlassesQuality()
    {
        SendGlassesQuality.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), glassesQuality);
    }

    [Obsolete]
    public void tellWearShirt(CSteamID steamID, ushort id, byte quality, byte[] state)
    {
        ReceiveWearShirt(Assets.find(EAssetType.ITEM, id)?.GUID ?? Guid.Empty, quality, state, playEffect: false);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellWearShirt")]
    public void ReceiveWearShirt(Guid id, byte quality, byte[] state, bool playEffect)
    {
        if (!(thirdClothes == null))
        {
            thirdClothes.shirtGuid = id;
            shirtQuality = quality;
            shirtState = state;
            thirdClothes.apply();
            if (firstClothes != null)
            {
                firstClothes.shirtGuid = id;
                firstClothes.apply();
            }
            if (characterClothes != null)
            {
                characterClothes.shirtGuid = id;
                characterClothes.apply();
                Characters.active.shirt = shirt;
            }
            UpdateSpeedMultiplier();
            if (onShirtUpdated != null)
            {
                onShirtUpdated(shirt, quality, state);
            }
            PlayerClothing.OnShirtChanged_Global?.Invoke(this);
            if (base.channel.isOwner && !Provider.isServer)
            {
                ClientAssetIntegrity.QueueRequest(thirdClothes.shirtAsset);
            }
        }
    }

    [Obsolete]
    public void askSwapShirt(CSteamID steamID, byte page, byte x, byte y)
    {
        ReceiveSwapShirtRequest(page, x, y);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askSwapShirt")]
    public void ReceiveSwapShirtRequest(byte page, byte x, byte y)
    {
        if (base.player.equipment.checkSelection(PlayerInventory.SHIRT))
        {
            if (base.player.equipment.isBusy)
            {
                return;
            }
            base.player.equipment.dequip();
        }
        if (page == byte.MaxValue)
        {
            if (shirtAsset != null)
            {
                askWearShirt(0, 0, new byte[0], playEffect: true);
            }
            return;
        }
        byte index = base.player.inventory.getIndex(page, x, y);
        if (index != byte.MaxValue)
        {
            ItemJar item = base.player.inventory.getItem(page, index);
            ItemAsset asset = item.GetAsset();
            if (asset != null && asset.type == EItemType.SHIRT)
            {
                base.player.inventory.removeItem(page, index);
                askWearShirt(item.item.id, item.item.quality, item.item.state, playEffect: true);
            }
        }
    }

    public void askWearShirt(ushort id, byte quality, byte[] state, bool playEffect)
    {
        ItemShirtAsset asset = Assets.find(EAssetType.ITEM, id) as ItemShirtAsset;
        askWearShirt(asset, quality, state, playEffect);
    }

    public void askWearShirt(ItemShirtAsset asset, byte quality, byte[] state, bool playEffect)
    {
        ushort num = shirt;
        byte newQuality = shirtQuality;
        byte[] newState = shirtState;
        SendWearShirt.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), asset?.GUID ?? Guid.Empty, quality, state, playEffect);
        if (num != 0)
        {
            base.player.inventory.forceAddItem(new Item(num, 1, newQuality, newState), auto: false);
        }
    }

    public void sendSwapShirt(byte page, byte x, byte y)
    {
        if (page != byte.MaxValue || shirtAsset != null)
        {
            SendSwapShirtRequest.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
        }
    }

    [Obsolete]
    public void tellWearPants(CSteamID steamID, ushort id, byte quality, byte[] state)
    {
        ReceiveWearPants(Assets.find(EAssetType.ITEM, id)?.GUID ?? Guid.Empty, quality, state, playEffect: false);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellWearPants")]
    public void ReceiveWearPants(Guid id, byte quality, byte[] state, bool playEffect)
    {
        if (!(thirdClothes == null))
        {
            thirdClothes.pantsGuid = id;
            pantsQuality = quality;
            pantsState = state;
            thirdClothes.apply();
            if (characterClothes != null)
            {
                characterClothes.pantsGuid = id;
                characterClothes.apply();
                Characters.active.pants = pants;
            }
            UpdateSpeedMultiplier();
            if (onPantsUpdated != null)
            {
                onPantsUpdated(pants, quality, state);
            }
            PlayerClothing.OnPantsChanged_Global?.Invoke(this);
            if (base.channel.isOwner && !Provider.isServer)
            {
                ClientAssetIntegrity.QueueRequest(thirdClothes.pantsAsset);
            }
        }
    }

    [Obsolete]
    public void askSwapPants(CSteamID steamID, byte page, byte x, byte y)
    {
        ReceiveSwapPantsRequest(page, x, y);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askSwapPants")]
    public void ReceiveSwapPantsRequest(byte page, byte x, byte y)
    {
        if (base.player.equipment.checkSelection(PlayerInventory.PANTS))
        {
            if (base.player.equipment.isBusy)
            {
                return;
            }
            base.player.equipment.dequip();
        }
        if (page == byte.MaxValue)
        {
            if (pantsAsset != null)
            {
                askWearPants(0, 0, new byte[0], playEffect: true);
            }
            return;
        }
        byte index = base.player.inventory.getIndex(page, x, y);
        if (index != byte.MaxValue)
        {
            ItemJar item = base.player.inventory.getItem(page, index);
            ItemAsset asset = item.GetAsset();
            if (asset != null && asset.type == EItemType.PANTS)
            {
                base.player.inventory.removeItem(page, index);
                askWearPants(item.item.id, item.item.quality, item.item.state, playEffect: true);
            }
        }
    }

    public void askWearPants(ushort id, byte quality, byte[] state, bool playEffect)
    {
        ItemPantsAsset asset = Assets.find(EAssetType.ITEM, id) as ItemPantsAsset;
        askWearPants(asset, quality, state, playEffect);
    }

    public void askWearPants(ItemPantsAsset asset, byte quality, byte[] state, bool playEffect)
    {
        ushort num = pants;
        byte newQuality = pantsQuality;
        byte[] newState = pantsState;
        SendWearPants.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), asset?.GUID ?? Guid.Empty, quality, state, playEffect);
        if (num != 0)
        {
            base.player.inventory.forceAddItem(new Item(num, 1, newQuality, newState), auto: false);
        }
    }

    public void sendSwapPants(byte page, byte x, byte y)
    {
        if (page != byte.MaxValue || pantsAsset != null)
        {
            SendSwapPantsRequest.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
        }
    }

    [Obsolete]
    public void tellWearHat(CSteamID steamID, ushort id, byte quality, byte[] state)
    {
        ReceiveWearHat(Assets.find(EAssetType.ITEM, id)?.GUID ?? Guid.Empty, quality, state, playEffect: false);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellWearHat")]
    public void ReceiveWearHat(Guid id, byte quality, byte[] state, bool playEffect)
    {
        if (!(thirdClothes == null))
        {
            thirdClothes.hatGuid = id;
            hatQuality = quality;
            hatState = state;
            thirdClothes.apply();
            if (characterClothes != null)
            {
                characterClothes.hatGuid = id;
                characterClothes.apply();
                Characters.active.hat = hat;
            }
            UpdateSpeedMultiplier();
            if (onHatUpdated != null)
            {
                onHatUpdated(hat, quality, state);
            }
            PlayerClothing.OnHatChanged_Global?.Invoke(this);
            if (base.channel.isOwner && !Provider.isServer)
            {
                ClientAssetIntegrity.QueueRequest(thirdClothes.hatAsset);
            }
        }
    }

    [Obsolete]
    public void askSwapHat(CSteamID steamID, byte page, byte x, byte y)
    {
        ReceiveSwapHatRequest(page, x, y);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askSwapHat")]
    public void ReceiveSwapHatRequest(byte page, byte x, byte y)
    {
        if (page == byte.MaxValue)
        {
            if (hatAsset != null)
            {
                askWearHat(0, 0, new byte[0], playEffect: true);
            }
            return;
        }
        byte index = base.player.inventory.getIndex(page, x, y);
        if (index != byte.MaxValue)
        {
            ItemJar item = base.player.inventory.getItem(page, index);
            ItemAsset asset = item.GetAsset();
            if (asset != null && asset.type == EItemType.HAT)
            {
                base.player.inventory.removeItem(page, index);
                askWearHat(item.item.id, item.item.quality, item.item.state, playEffect: true);
            }
        }
    }

    public void askWearHat(ushort id, byte quality, byte[] state, bool playEffect)
    {
        ItemHatAsset asset = Assets.find(EAssetType.ITEM, id) as ItemHatAsset;
        askWearHat(asset, quality, state, playEffect);
    }

    public void askWearHat(ItemHatAsset asset, byte quality, byte[] state, bool playEffect)
    {
        ushort num = hat;
        byte newQuality = hatQuality;
        byte[] newState = hatState;
        SendWearHat.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), asset?.GUID ?? Guid.Empty, quality, state, playEffect);
        if (num != 0)
        {
            base.player.inventory.forceAddItem(new Item(num, 1, newQuality, newState), auto: false);
        }
    }

    public void sendSwapHat(byte page, byte x, byte y)
    {
        if (page != byte.MaxValue || hatAsset != null)
        {
            if (Provider.isServer)
            {
                ReceiveSwapHatRequest(page, x, y);
            }
            else
            {
                SendSwapHatRequest.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
            }
        }
    }

    [Obsolete]
    public void tellWearBackpack(CSteamID steamID, ushort id, byte quality, byte[] state)
    {
        ReceiveWearBackpack(Assets.find(EAssetType.ITEM, id)?.GUID ?? Guid.Empty, quality, state, playEffect: false);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellWearBackpack")]
    public void ReceiveWearBackpack(Guid id, byte quality, byte[] state, bool playEffect)
    {
        if (!(thirdClothes == null))
        {
            thirdClothes.backpackGuid = id;
            backpackQuality = quality;
            backpackState = state;
            thirdClothes.apply();
            if (characterClothes != null)
            {
                characterClothes.backpackGuid = id;
                characterClothes.apply();
                Characters.active.backpack = backpack;
            }
            UpdateSpeedMultiplier();
            if (onBackpackUpdated != null)
            {
                onBackpackUpdated(backpack, quality, state);
            }
            PlayerClothing.OnBackpackChanged_Global?.Invoke(this);
            if (base.channel.isOwner && !Provider.isServer)
            {
                ClientAssetIntegrity.QueueRequest(thirdClothes.backpackAsset);
            }
        }
    }

    [Obsolete]
    public void askSwapBackpack(CSteamID steamID, byte page, byte x, byte y)
    {
        ReceiveSwapBackpackRequest(page, x, y);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askSwapBackpack")]
    public void ReceiveSwapBackpackRequest(byte page, byte x, byte y)
    {
        if (base.player.equipment.checkSelection(PlayerInventory.BACKPACK))
        {
            if (base.player.equipment.isBusy)
            {
                return;
            }
            base.player.equipment.dequip();
        }
        if (page == byte.MaxValue)
        {
            if (backpackAsset != null)
            {
                askWearBackpack(0, 0, new byte[0], playEffect: true);
            }
            return;
        }
        byte index = base.player.inventory.getIndex(page, x, y);
        if (index != byte.MaxValue)
        {
            ItemJar item = base.player.inventory.getItem(page, index);
            ItemAsset asset = item.GetAsset();
            if (asset != null && asset.type == EItemType.BACKPACK)
            {
                base.player.inventory.removeItem(page, index);
                askWearBackpack(item.item.id, item.item.quality, item.item.state, playEffect: true);
            }
        }
    }

    public void askWearBackpack(ushort id, byte quality, byte[] state, bool playEffect)
    {
        ItemBackpackAsset asset = Assets.find(EAssetType.ITEM, id) as ItemBackpackAsset;
        askWearBackpack(asset, quality, state, playEffect);
    }

    public void askWearBackpack(ItemBackpackAsset asset, byte quality, byte[] state, bool playEffect)
    {
        ushort num = backpack;
        byte newQuality = backpackQuality;
        byte[] newState = backpackState;
        SendWearBackpack.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), asset?.GUID ?? Guid.Empty, quality, state, playEffect);
        if (num != 0)
        {
            base.player.inventory.forceAddItem(new Item(num, 1, newQuality, newState), auto: false);
        }
    }

    public void sendSwapBackpack(byte page, byte x, byte y)
    {
        if (page != byte.MaxValue || backpackAsset != null)
        {
            SendSwapBackpackRequest.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
        }
    }

    [Obsolete]
    public void tellVisualToggle(CSteamID steamID, byte index, bool toggle)
    {
        ReceiveVisualToggleState((EVisualToggleType)index, toggle);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellVisualToggle")]
    public void ReceiveVisualToggleState(EVisualToggleType type, bool toggle)
    {
        switch (type)
        {
        case EVisualToggleType.COSMETIC:
            if (thirdClothes != null)
            {
                thirdClothes.isVisual = toggle;
                thirdClothes.apply();
            }
            if (firstClothes != null)
            {
                firstClothes.isVisual = toggle;
                firstClothes.apply();
            }
            if (characterClothes != null)
            {
                characterClothes.isVisual = toggle;
                characterClothes.apply();
            }
            break;
        case EVisualToggleType.SKIN:
            isSkinned = toggle;
            if (base.player.equipment != null)
            {
                base.player.equipment.applySkinVisual();
                base.player.equipment.applyMythicVisual();
            }
            break;
        case EVisualToggleType.MYTHIC:
            if (thirdClothes != null)
            {
                thirdClothes.isMythic = toggle;
                thirdClothes.apply();
            }
            if (firstClothes != null)
            {
                firstClothes.isMythic = toggle;
                firstClothes.apply();
            }
            if (characterClothes != null)
            {
                characterClothes.isMythic = toggle;
                characterClothes.apply();
            }
            if (base.player.equipment != null)
            {
                base.player.equipment.applyMythicVisual();
            }
            break;
        }
        if (this.VisualToggleChanged != null)
        {
            this.VisualToggleChanged(this);
        }
    }

    public void ServerSetVisualToggleState(EVisualToggleType type, bool isVisible)
    {
        SendVisualToggleState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), type, isVisible);
    }

    [Obsolete]
    public void askVisualToggle(CSteamID steamID, byte index)
    {
        if (index <= 2)
        {
            ReceiveVisualToggleRequest((EVisualToggleType)index);
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askVisualToggle")]
    public void ReceiveVisualToggleRequest(EVisualToggleType type)
    {
        switch (type)
        {
        case EVisualToggleType.COSMETIC:
            SendVisualToggleState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), type, !isVisual);
            break;
        case EVisualToggleType.SKIN:
            SendVisualToggleState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), type, !isSkinned);
            break;
        case EVisualToggleType.MYTHIC:
            SendVisualToggleState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), type, !isMythic);
            break;
        }
    }

    public void sendVisualToggle(EVisualToggleType type)
    {
        SendVisualToggleRequest.Invoke(GetNetId(), ENetReliability.Unreliable, type);
    }

    [Obsolete]
    public void tellWearVest(CSteamID steamID, ushort id, byte quality, byte[] state)
    {
        ReceiveWearVest(Assets.find(EAssetType.ITEM, id)?.GUID ?? Guid.Empty, quality, state, playEffect: false);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellWearVest")]
    public void ReceiveWearVest(Guid id, byte quality, byte[] state, bool playEffect)
    {
        if (!(thirdClothes == null))
        {
            thirdClothes.vestGuid = id;
            vestQuality = quality;
            vestState = state;
            thirdClothes.apply();
            if (characterClothes != null)
            {
                characterClothes.vestGuid = id;
                characterClothes.apply();
                Characters.active.vest = vest;
            }
            UpdateSpeedMultiplier();
            if (onVestUpdated != null)
            {
                onVestUpdated(vest, quality, state);
            }
            PlayerClothing.OnVestChanged_Global?.Invoke(this);
            if (base.channel.isOwner && !Provider.isServer)
            {
                ClientAssetIntegrity.QueueRequest(thirdClothes.vestAsset);
            }
        }
    }

    [Obsolete]
    public void askSwapVest(CSteamID steamID, byte page, byte x, byte y)
    {
        ReceiveSwapVestRequest(page, x, y);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askSwapVest")]
    public void ReceiveSwapVestRequest(byte page, byte x, byte y)
    {
        if (base.player.equipment.checkSelection(PlayerInventory.VEST))
        {
            if (base.player.equipment.isBusy)
            {
                return;
            }
            base.player.equipment.dequip();
        }
        if (page == byte.MaxValue)
        {
            if (vestAsset != null)
            {
                askWearVest(0, 0, new byte[0], playEffect: true);
            }
            return;
        }
        byte index = base.player.inventory.getIndex(page, x, y);
        if (index != byte.MaxValue)
        {
            ItemJar item = base.player.inventory.getItem(page, index);
            ItemAsset asset = item.GetAsset();
            if (asset != null && asset.type == EItemType.VEST)
            {
                base.player.inventory.removeItem(page, index);
                askWearVest(item.item.id, item.item.quality, item.item.state, playEffect: true);
            }
        }
    }

    public void askWearVest(ushort id, byte quality, byte[] state, bool playEffect)
    {
        ItemVestAsset asset = Assets.find(EAssetType.ITEM, id) as ItemVestAsset;
        askWearVest(asset, quality, state, playEffect);
    }

    public void askWearVest(ItemVestAsset asset, byte quality, byte[] state, bool playEffect)
    {
        ushort num = vest;
        byte newQuality = vestQuality;
        byte[] newState = vestState;
        SendWearVest.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), asset?.GUID ?? Guid.Empty, quality, state, playEffect);
        if (num != 0)
        {
            base.player.inventory.forceAddItem(new Item(num, 1, newQuality, newState), auto: false);
        }
    }

    public void sendSwapVest(byte page, byte x, byte y)
    {
        if (page != byte.MaxValue || vestAsset != null)
        {
            SendSwapVestRequest.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
        }
    }

    [Obsolete]
    public void tellWearMask(CSteamID steamID, ushort id, byte quality, byte[] state)
    {
        ReceiveWearMask(Assets.find(EAssetType.ITEM, id)?.GUID ?? Guid.Empty, quality, state, playEffect: false);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellWearMask")]
    public void ReceiveWearMask(Guid id, byte quality, byte[] state, bool playEffect)
    {
        if (!(thirdClothes == null))
        {
            thirdClothes.maskGuid = id;
            maskQuality = quality;
            maskState = state;
            thirdClothes.apply();
            if (characterClothes != null)
            {
                characterClothes.maskGuid = id;
                characterClothes.apply();
                Characters.active.mask = mask;
            }
            UpdateSpeedMultiplier();
            if (onMaskUpdated != null)
            {
                onMaskUpdated(mask, quality, state);
            }
            PlayerClothing.OnMaskChanged_Global?.Invoke(this);
            if (base.channel.isOwner && !Provider.isServer)
            {
                ClientAssetIntegrity.QueueRequest(thirdClothes.maskAsset);
            }
        }
    }

    [Obsolete]
    public void askSwapMask(CSteamID steamID, byte page, byte x, byte y)
    {
        ReceiveSwapMaskRequest(page, x, y);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askSwapMask")]
    public void ReceiveSwapMaskRequest(byte page, byte x, byte y)
    {
        if (page == byte.MaxValue)
        {
            if (maskAsset != null)
            {
                askWearMask(0, 0, new byte[0], playEffect: true);
            }
            return;
        }
        byte index = base.player.inventory.getIndex(page, x, y);
        if (index != byte.MaxValue)
        {
            ItemJar item = base.player.inventory.getItem(page, index);
            ItemAsset asset = item.GetAsset();
            if (asset != null && asset.type == EItemType.MASK)
            {
                base.player.inventory.removeItem(page, index);
                askWearMask(item.item.id, item.item.quality, item.item.state, playEffect: true);
            }
        }
    }

    public void askWearMask(ushort id, byte quality, byte[] state, bool playEffect)
    {
        ItemMaskAsset asset = Assets.find(EAssetType.ITEM, id) as ItemMaskAsset;
        askWearMask(asset, quality, state, playEffect);
    }

    public void askWearMask(ItemMaskAsset asset, byte quality, byte[] state, bool playEffect)
    {
        ushort num = mask;
        byte newQuality = maskQuality;
        byte[] newState = maskState;
        SendWearMask.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), asset?.GUID ?? Guid.Empty, quality, state, playEffect);
        if (num != 0)
        {
            base.player.inventory.forceAddItem(new Item(num, 1, newQuality, newState), auto: false);
        }
    }

    public void sendSwapMask(byte page, byte x, byte y)
    {
        if (page != byte.MaxValue || maskAsset != null)
        {
            SendSwapMaskRequest.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
        }
    }

    [Obsolete]
    public void tellWearGlasses(CSteamID steamID, ushort id, byte quality, byte[] state)
    {
        ReceiveWearGlasses(Assets.find(EAssetType.ITEM, id)?.GUID ?? Guid.Empty, quality, state, playEffect: false);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellWearGlasses")]
    public void ReceiveWearGlasses(Guid id, byte quality, byte[] state, bool playEffect)
    {
        if (!(thirdClothes == null))
        {
            thirdClothes.glassesGuid = id;
            glassesQuality = quality;
            glassesState = state;
            thirdClothes.apply();
            if (characterClothes != null)
            {
                characterClothes.glassesGuid = id;
                characterClothes.apply();
                Characters.active.glasses = glasses;
            }
            if (onGlassesUpdated != null)
            {
                onGlassesUpdated(glasses, quality, state);
            }
            UpdateSpeedMultiplier();
            PlayerClothing.OnGlassesChanged_Global?.Invoke(this);
            if (base.channel.isOwner && !Provider.isServer)
            {
                ClientAssetIntegrity.QueueRequest(thirdClothes.glassesAsset);
            }
        }
    }

    [Obsolete]
    public void askSwapGlasses(CSteamID steamID, byte page, byte x, byte y)
    {
        ReceiveSwapGlassesRequest(page, x, y);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askSwapGlasses")]
    public void ReceiveSwapGlassesRequest(byte page, byte x, byte y)
    {
        if (page == byte.MaxValue)
        {
            if (glassesAsset != null)
            {
                askWearGlasses(0, 0, new byte[0], playEffect: true);
            }
            return;
        }
        byte index = base.player.inventory.getIndex(page, x, y);
        if (index != byte.MaxValue)
        {
            ItemJar item = base.player.inventory.getItem(page, index);
            ItemAsset asset = item.GetAsset();
            if (asset != null && asset.type == EItemType.GLASSES)
            {
                base.player.inventory.removeItem(page, index);
                askWearGlasses(item.item.id, item.item.quality, item.item.state, playEffect: true);
            }
        }
    }

    public void askWearGlasses(ushort id, byte quality, byte[] state, bool playEffect)
    {
        ItemGlassesAsset asset = Assets.find(EAssetType.ITEM, id) as ItemGlassesAsset;
        askWearGlasses(asset, quality, state, playEffect);
    }

    public void askWearGlasses(ItemGlassesAsset asset, byte quality, byte[] state, bool playEffect)
    {
        ushort num = glasses;
        byte newQuality = glassesQuality;
        byte[] newState = glassesState;
        SendWearGlasses.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), asset?.GUID ?? Guid.Empty, quality, state, playEffect);
        if (num != 0)
        {
            base.player.inventory.forceAddItem(new Item(num, 1, newQuality, newState), auto: false);
        }
    }

    public void sendSwapGlasses(byte page, byte x, byte y)
    {
        if (page != byte.MaxValue || glassesAsset != null)
        {
            SendSwapGlassesRequest.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
        }
    }

    [Obsolete]
    public void tellClothing(CSteamID steamID, ushort newShirt, byte newShirtQuality, byte[] newShirtState, ushort newPants, byte newPantsQuality, byte[] newPantsState, ushort newHat, byte newHatQuality, byte[] newHatState, ushort newBackpack, byte newBackpackQuality, byte[] newBackpackState, ushort newVest, byte newVestQuality, byte[] newVestState, ushort newMask, byte newMaskQuality, byte[] newMaskState, ushort newGlasses, byte newGlassesQuality, byte[] newGlassesState, bool newVisual, bool newSkinned, bool newMythic)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveClothingState(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadGuid(out var value);
        reader.ReadUInt8(out var value2);
        reader.ReadStateArray(out var value3);
        reader.ReadGuid(out var value4);
        reader.ReadUInt8(out var value5);
        reader.ReadStateArray(out var value6);
        reader.ReadGuid(out var value7);
        reader.ReadUInt8(out var value8);
        reader.ReadStateArray(out var value9);
        reader.ReadGuid(out var value10);
        reader.ReadUInt8(out var value11);
        reader.ReadStateArray(out var value12);
        reader.ReadGuid(out var value13);
        reader.ReadUInt8(out var value14);
        reader.ReadStateArray(out var value15);
        reader.ReadGuid(out var value16);
        reader.ReadUInt8(out var value17);
        reader.ReadStateArray(out var value18);
        reader.ReadGuid(out var value19);
        reader.ReadUInt8(out var value20);
        reader.ReadStateArray(out var value21);
        reader.ReadBit(out var value22);
        reader.ReadBit(out var value23);
        reader.ReadBit(out var value24);
        base.player.animator.NotifyClothingIsVisible();
        if (base.channel.isOwner)
        {
            Player.isLoadingClothing = false;
        }
        if (thirdClothes != null)
        {
            thirdClothes.face = base.channel.owner.face;
            thirdClothes.hair = base.channel.owner.hair;
            thirdClothes.beard = base.channel.owner.beard;
            thirdClothes.skin = base.channel.owner.skin;
            thirdClothes.color = base.channel.owner.color;
            thirdClothes.shirtGuid = value;
            shirtQuality = value2;
            shirtState = value3;
            thirdClothes.pantsGuid = value4;
            pantsQuality = value5;
            pantsState = value6;
            thirdClothes.hatGuid = value7;
            hatQuality = value8;
            hatState = value9;
            thirdClothes.backpackGuid = value10;
            backpackQuality = value11;
            backpackState = value12;
            thirdClothes.vestGuid = value13;
            vestQuality = value14;
            vestState = value15;
            thirdClothes.maskGuid = value16;
            maskQuality = value17;
            maskState = value18;
            thirdClothes.glassesGuid = value19;
            glassesQuality = value20;
            glassesState = value21;
            thirdClothes.isVisual = value22;
            thirdClothes.isMythic = value24;
            thirdClothes.apply();
        }
        if (firstClothes != null)
        {
            firstClothes.skin = base.channel.owner.skin;
            firstClothes.shirtGuid = value;
            firstClothes.isVisual = value22;
            firstClothes.isMythic = value24;
            firstClothes.apply();
        }
        if (characterClothes != null)
        {
            characterClothes.face = base.channel.owner.face;
            characterClothes.hair = base.channel.owner.hair;
            characterClothes.beard = base.channel.owner.beard;
            characterClothes.skin = base.channel.owner.skin;
            characterClothes.color = base.channel.owner.color;
            characterClothes.shirtGuid = value;
            characterClothes.pantsGuid = value4;
            characterClothes.hatGuid = value7;
            characterClothes.backpackGuid = value10;
            characterClothes.vestGuid = value13;
            characterClothes.maskGuid = value16;
            characterClothes.glassesGuid = value19;
            characterClothes.isVisual = value22;
            characterClothes.isMythic = value24;
            characterClothes.apply();
            Characters.active.shirt = shirt;
            Characters.active.pants = pants;
            Characters.active.hat = hat;
            Characters.active.backpack = backpack;
            Characters.active.vest = vest;
            Characters.active.mask = mask;
            Characters.active.glasses = glasses;
            Characters.hasPlayed = true;
        }
        isSkinned = value23;
        base.player.equipment.applySkinVisual();
        base.player.equipment.applyMythicVisual();
        UpdateSpeedMultiplier();
        if (onShirtUpdated != null)
        {
            onShirtUpdated(shirt, value2, value3);
        }
        PlayerClothing.OnShirtChanged_Global?.Invoke(this);
        if (onPantsUpdated != null)
        {
            onPantsUpdated(pants, value5, value6);
        }
        PlayerClothing.OnPantsChanged_Global?.Invoke(this);
        if (onHatUpdated != null)
        {
            onHatUpdated(hat, value8, value9);
        }
        PlayerClothing.OnHatChanged_Global?.Invoke(this);
        if (onBackpackUpdated != null)
        {
            onBackpackUpdated(backpack, value11, value12);
        }
        PlayerClothing.OnBackpackChanged_Global?.Invoke(this);
        if (onVestUpdated != null)
        {
            onVestUpdated(vest, value14, value15);
        }
        PlayerClothing.OnVestChanged_Global?.Invoke(this);
        if (onMaskUpdated != null)
        {
            onMaskUpdated(mask, value17, value18);
        }
        PlayerClothing.OnMaskChanged_Global?.Invoke(this);
        if (onGlassesUpdated != null)
        {
            onGlassesUpdated(glasses, value20, value21);
        }
        PlayerClothing.OnGlassesChanged_Global?.Invoke(this);
        if (base.channel.isOwner && thirdClothes != null && !Provider.isServer)
        {
            ClientAssetIntegrity.QueueRequest(thirdClothes.shirtAsset);
            ClientAssetIntegrity.QueueRequest(thirdClothes.pantsAsset);
            ClientAssetIntegrity.QueueRequest(thirdClothes.hatAsset);
            ClientAssetIntegrity.QueueRequest(thirdClothes.backpackAsset);
            ClientAssetIntegrity.QueueRequest(thirdClothes.vestAsset);
            ClientAssetIntegrity.QueueRequest(thirdClothes.maskAsset);
            ClientAssetIntegrity.QueueRequest(thirdClothes.glassesAsset);
        }
    }

    public void updateClothes(ushort newShirt, byte newShirtQuality, byte[] newShirtState, ushort newPants, byte newPantsQuality, byte[] newPantsState, ushort newHat, byte newHatQuality, byte[] newHatState, ushort newBackpack, byte newBackpackQuality, byte[] newBackpackState, ushort newVest, byte newVestQuality, byte[] newVestState, ushort newMask, byte newMaskQuality, byte[] newMaskState, ushort newGlasses, byte newGlassesQuality, byte[] newGlassesState)
    {
        Asset newShirtAsset = Assets.find(EAssetType.ITEM, newShirt);
        Asset newPantsAsset = Assets.find(EAssetType.ITEM, newPants);
        Asset newHatAsset = Assets.find(EAssetType.ITEM, newHat);
        Asset newBackpackAsset = Assets.find(EAssetType.ITEM, newBackpack);
        Asset newVestAsset = Assets.find(EAssetType.ITEM, newVest);
        Asset newMaskAsset = Assets.find(EAssetType.ITEM, newMask);
        Asset newGlassesAsset = Assets.find(EAssetType.ITEM, newGlasses);
        SendClothingState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), delegate(NetPakWriter writer)
        {
            writer.WriteGuid(newShirtAsset?.GUID ?? Guid.Empty);
            writer.WriteUInt8(newShirtQuality);
            writer.WriteStateArray(newShirtState);
            writer.WriteGuid(newPantsAsset?.GUID ?? Guid.Empty);
            writer.WriteUInt8(newPantsQuality);
            writer.WriteStateArray(newPantsState);
            writer.WriteGuid(newHatAsset?.GUID ?? Guid.Empty);
            writer.WriteUInt8(newHatQuality);
            writer.WriteStateArray(newHatState);
            writer.WriteGuid(newBackpackAsset?.GUID ?? Guid.Empty);
            writer.WriteUInt8(newBackpackQuality);
            writer.WriteStateArray(newBackpackState);
            writer.WriteGuid(newVestAsset?.GUID ?? Guid.Empty);
            writer.WriteUInt8(newVestQuality);
            writer.WriteStateArray(newVestState);
            writer.WriteGuid(newMaskAsset?.GUID ?? Guid.Empty);
            writer.WriteUInt8(newMaskQuality);
            writer.WriteStateArray(newMaskState);
            writer.WriteGuid(newGlassesAsset?.GUID ?? Guid.Empty);
            writer.WriteUInt8(newGlassesQuality);
            writer.WriteStateArray(newGlassesState);
            writer.WriteBit(isVisual);
            writer.WriteBit(isSkinned);
            writer.WriteBit(isMythic);
        });
    }

    [Obsolete]
    public void askClothing(CSteamID steamID)
    {
    }

    private void WriteClothingState(NetPakWriter writer)
    {
        writer.WriteGuid(shirtAsset?.GUID ?? Guid.Empty);
        writer.WriteUInt8(shirtQuality);
        writer.WriteStateArray(shirtState);
        writer.WriteGuid(pantsAsset?.GUID ?? Guid.Empty);
        writer.WriteUInt8(pantsQuality);
        writer.WriteStateArray(pantsState);
        writer.WriteGuid(hatAsset?.GUID ?? Guid.Empty);
        writer.WriteUInt8(hatQuality);
        writer.WriteStateArray(hatState);
        writer.WriteGuid(backpackAsset?.GUID ?? Guid.Empty);
        writer.WriteUInt8(backpackQuality);
        writer.WriteStateArray(backpackState);
        writer.WriteGuid(vestAsset?.GUID ?? Guid.Empty);
        writer.WriteUInt8(vestQuality);
        writer.WriteStateArray(vestState);
        writer.WriteGuid(maskAsset?.GUID ?? Guid.Empty);
        writer.WriteUInt8(maskQuality);
        writer.WriteStateArray(maskState);
        writer.WriteGuid(glassesAsset?.GUID ?? Guid.Empty);
        writer.WriteUInt8(glassesQuality);
        writer.WriteStateArray(glassesState);
        writer.WriteBit(isVisual);
        writer.WriteBit(isSkinned);
        writer.WriteBit(isMythic);
    }

    internal void SendInitialPlayerState(SteamPlayer client)
    {
        SendClothingState.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, WriteClothingState);
    }

    internal void SendInitialPlayerState(IEnumerable<ITransportConnection> transportConnections)
    {
        SendClothingState.Invoke(GetNetId(), ENetReliability.Reliable, transportConnections, WriteClothingState);
    }

    [Obsolete]
    public void tellSwapFace(CSteamID steamID, byte index)
    {
        ReceiveFaceState(index);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellSwapFace")]
    public void ReceiveFaceState(byte index)
    {
        base.channel.owner.face = index;
        if (thirdClothes != null)
        {
            thirdClothes.face = base.channel.owner.face;
            thirdClothes.apply();
        }
        if (characterClothes != null)
        {
            characterClothes.face = base.channel.owner.face;
            characterClothes.apply();
        }
    }

    public bool ServerSetFace(byte index)
    {
        if (index >= Customization.FACES_FREE + Customization.FACES_PRO)
        {
            return false;
        }
        if (!base.channel.owner.isPro && index >= Customization.FACES_FREE)
        {
            return false;
        }
        SendFaceState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), index);
        return true;
    }

    [Obsolete]
    public void askSwapFace(CSteamID steamID, byte index)
    {
        ReceiveSwapFaceRequest(index);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askSwapFace")]
    public void ReceiveSwapFaceRequest(byte index)
    {
        ServerSetFace(index);
    }

    public void sendSwapFace(byte index)
    {
        SendSwapFaceRequest.Invoke(GetNetId(), ENetReliability.Unreliable, index);
    }

    private void onStanceUpdated()
    {
        if (!(thirdClothes == null))
        {
            if (base.player.movement.getVehicle() != null)
            {
                thirdClothes.hasBackpack = base.player.movement.getVehicle().passengers[base.player.movement.getSeat()].obj == null;
            }
            else
            {
                thirdClothes.hasBackpack = true;
            }
        }
    }

    private void onLifeUpdated(bool isDead)
    {
        if (isDead && Provider.isServer && (base.player.life.wasPvPDeath ? Provider.modeConfigData.Players.Lose_Clothes_PvP : Provider.modeConfigData.Players.Lose_Clothes_PvE))
        {
            if (shirtAsset != null && shirtAsset.shouldDropOnDeath)
            {
                ItemManager.dropItem(new Item(shirt, 1, shirtQuality, shirtState), base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
            }
            if (pantsAsset != null && pantsAsset.shouldDropOnDeath)
            {
                ItemManager.dropItem(new Item(pants, 1, pantsQuality, pantsState), base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
            }
            if (hatAsset != null && hatAsset.shouldDropOnDeath)
            {
                ItemManager.dropItem(new Item(hat, 1, hatQuality, hatState), base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
            }
            if (backpackAsset != null && backpackAsset.shouldDropOnDeath)
            {
                ItemManager.dropItem(new Item(backpack, 1, backpackQuality, backpackState), base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
            }
            if (vestAsset != null && vestAsset.shouldDropOnDeath)
            {
                ItemManager.dropItem(new Item(vest, 1, vestQuality, vestState), base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
            }
            if (maskAsset != null && maskAsset.shouldDropOnDeath)
            {
                ItemManager.dropItem(new Item(mask, 1, maskQuality, maskState), base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
            }
            if (glassesAsset != null && glassesAsset.shouldDropOnDeath)
            {
                ItemManager.dropItem(new Item(glasses, 1, glassesQuality, glassesState), base.transform.position, playEffect: false, isDropped: true, wideSpread: true);
            }
            thirdClothes.shirtAsset = null;
            shirtQuality = 0;
            thirdClothes.pantsAsset = null;
            pantsQuality = 0;
            thirdClothes.hatAsset = null;
            hatQuality = 0;
            thirdClothes.backpackAsset = null;
            backpackQuality = 0;
            thirdClothes.vestAsset = null;
            vestQuality = 0;
            thirdClothes.maskAsset = null;
            maskQuality = 0;
            thirdClothes.glassesAsset = null;
            glassesQuality = 0;
            shirtState = new byte[0];
            pantsState = new byte[0];
            hatState = new byte[0];
            backpackState = new byte[0];
            vestState = new byte[0];
            maskState = new byte[0];
            glassesState = new byte[0];
            SendClothingState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), WriteClothingState);
        }
    }

    internal void InitializePlayer()
    {
        if (base.channel.isOwner)
        {
            if (base.player.first != null)
            {
                firstClothes = base.player.first.Find("Camera").Find("Viewmodel").GetComponent<HumanClothes>();
                firstClothes.isMine = true;
            }
            if (base.player.third != null)
            {
                thirdClothes = base.player.third.GetComponent<HumanClothes>();
            }
            if (base.player.character != null)
            {
                characterClothes = base.player.character.GetComponent<HumanClothes>();
            }
        }
        else if (base.player.third != null)
        {
            thirdClothes = base.player.third.GetComponent<HumanClothes>();
        }
        if (firstClothes != null)
        {
            firstClothes.visualShirt = base.channel.owner.shirtItem;
            firstClothes.hand = base.channel.owner.hand;
        }
        if (thirdClothes != null)
        {
            thirdClothes.visualShirt = base.channel.owner.shirtItem;
            thirdClothes.visualPants = base.channel.owner.pantsItem;
            thirdClothes.visualHat = base.channel.owner.hatItem;
            thirdClothes.visualBackpack = base.channel.owner.backpackItem;
            thirdClothes.visualVest = base.channel.owner.vestItem;
            thirdClothes.visualMask = base.channel.owner.maskItem;
            thirdClothes.visualGlasses = base.channel.owner.glassesItem;
            thirdClothes.hand = base.channel.owner.hand;
        }
        if (characterClothes != null)
        {
            characterClothes.visualShirt = base.channel.owner.shirtItem;
            characterClothes.visualPants = base.channel.owner.pantsItem;
            characterClothes.visualHat = base.channel.owner.hatItem;
            characterClothes.visualBackpack = base.channel.owner.backpackItem;
            characterClothes.visualVest = base.channel.owner.vestItem;
            characterClothes.visualMask = base.channel.owner.maskItem;
            characterClothes.visualGlasses = base.channel.owner.glassesItem;
            characterClothes.hand = base.channel.owner.hand;
        }
        isSkinned = true;
        if (Provider.isServer)
        {
            load();
            PlayerLife life = base.player.life;
            life.onLifeUpdated = (LifeUpdated)Delegate.Combine(life.onLifeUpdated, new LifeUpdated(onLifeUpdated));
        }
    }

    public void load()
    {
        wasLoadCalled = true;
        thirdClothes.visualShirt = base.channel.owner.shirtItem;
        thirdClothes.visualPants = base.channel.owner.pantsItem;
        thirdClothes.visualHat = base.channel.owner.hatItem;
        thirdClothes.visualBackpack = base.channel.owner.backpackItem;
        thirdClothes.visualVest = base.channel.owner.vestItem;
        thirdClothes.visualMask = base.channel.owner.maskItem;
        thirdClothes.visualGlasses = base.channel.owner.glassesItem;
        if (PlayerSavedata.fileExists(base.channel.owner.playerID, "/Player/Clothing.dat") && Level.info.type == ELevelType.SURVIVAL)
        {
            Block block = PlayerSavedata.readBlock(base.channel.owner.playerID, "/Player/Clothing.dat", 0);
            byte b = block.readByte();
            if (b > 1)
            {
                if (b > 6)
                {
                    thirdClothes.shirtGuid = block.readGUID();
                }
                else
                {
                    thirdClothes.shirt = block.readUInt16();
                }
                shirtQuality = block.readByte();
                if (b > 6)
                {
                    thirdClothes.pantsGuid = block.readGUID();
                }
                else
                {
                    thirdClothes.pants = block.readUInt16();
                }
                pantsQuality = block.readByte();
                if (b > 6)
                {
                    thirdClothes.hatGuid = block.readGUID();
                }
                else
                {
                    thirdClothes.hat = block.readUInt16();
                }
                hatQuality = block.readByte();
                if (b > 6)
                {
                    thirdClothes.backpackGuid = block.readGUID();
                }
                else
                {
                    thirdClothes.backpack = block.readUInt16();
                }
                backpackQuality = block.readByte();
                if (b > 6)
                {
                    thirdClothes.vestGuid = block.readGUID();
                }
                else
                {
                    thirdClothes.vest = block.readUInt16();
                }
                vestQuality = block.readByte();
                if (b > 6)
                {
                    thirdClothes.maskGuid = block.readGUID();
                }
                else
                {
                    thirdClothes.mask = block.readUInt16();
                }
                maskQuality = block.readByte();
                if (b > 6)
                {
                    thirdClothes.glassesGuid = block.readGUID();
                }
                else
                {
                    thirdClothes.glasses = block.readUInt16();
                }
                glassesQuality = block.readByte();
                if (b > 2)
                {
                    thirdClothes.isVisual = block.readBoolean();
                }
                if (b > 5)
                {
                    isSkinned = block.readBoolean();
                    thirdClothes.isMythic = block.readBoolean();
                }
                else
                {
                    isSkinned = true;
                    thirdClothes.isMythic = true;
                }
                if (b > 4)
                {
                    shirtState = block.readByteArray();
                    pantsState = block.readByteArray();
                    hatState = block.readByteArray();
                    backpackState = block.readByteArray();
                    vestState = block.readByteArray();
                    maskState = block.readByteArray();
                    glassesState = block.readByteArray();
                }
                else
                {
                    shirtState = new byte[0];
                    pantsState = new byte[0];
                    hatState = new byte[0];
                    backpackState = new byte[0];
                    vestState = new byte[0];
                    maskState = new byte[0];
                    glassesState = new byte[0];
                    if (glasses == 334)
                    {
                        glassesState = new byte[1];
                    }
                }
                thirdClothes.apply();
                UpdateSpeedMultiplier();
                return;
            }
        }
        thirdClothes.shirtAsset = null;
        shirtQuality = 0;
        thirdClothes.pantsAsset = null;
        pantsQuality = 0;
        thirdClothes.hatAsset = null;
        hatQuality = 0;
        thirdClothes.backpackAsset = null;
        backpackQuality = 0;
        thirdClothes.vestAsset = null;
        vestQuality = 0;
        thirdClothes.maskAsset = null;
        maskQuality = 0;
        thirdClothes.glassesAsset = null;
        glassesQuality = 0;
        shirtState = new byte[0];
        pantsState = new byte[0];
        hatState = new byte[0];
        backpackState = new byte[0];
        vestState = new byte[0];
        maskState = new byte[0];
        glassesState = new byte[0];
        thirdClothes.apply();
        UpdateSpeedMultiplier();
    }

    public void save()
    {
        if (!wasLoadCalled)
        {
            return;
        }
        bool flag = (base.player.life.wasPvPDeath ? Provider.modeConfigData.Players.Lose_Clothes_PvP : Provider.modeConfigData.Players.Lose_Clothes_PvE);
        if ((base.player.life.isDead && flag) || thirdClothes == null)
        {
            if (PlayerSavedata.fileExists(base.channel.owner.playerID, "/Player/Clothing.dat"))
            {
                PlayerSavedata.deleteFile(base.channel.owner.playerID, "/Player/Clothing.dat");
            }
            return;
        }
        Block block = new Block();
        block.writeByte(SAVEDATA_VERSION);
        block.writeGUID(thirdClothes.shirtGuid);
        block.writeByte(shirtQuality);
        block.writeGUID(thirdClothes.pantsGuid);
        block.writeByte(pantsQuality);
        block.writeGUID(thirdClothes.hatGuid);
        block.writeByte(hatQuality);
        block.writeGUID(thirdClothes.backpackGuid);
        block.writeByte(backpackQuality);
        block.writeGUID(thirdClothes.vestGuid);
        block.writeByte(vestQuality);
        block.writeGUID(thirdClothes.maskGuid);
        block.writeByte(maskQuality);
        block.writeGUID(thirdClothes.glassesGuid);
        block.writeByte(glassesQuality);
        block.writeBoolean(isVisual);
        block.writeBoolean(isSkinned);
        block.writeBoolean(isMythic);
        block.writeByteArray(shirtState);
        block.writeByteArray(pantsState);
        block.writeByteArray(hatState);
        block.writeByteArray(backpackState);
        block.writeByteArray(vestState);
        block.writeByteArray(maskState);
        block.writeByteArray(glassesState);
        PlayerSavedata.writeBlock(base.channel.owner.playerID, "/Player/Clothing.dat", block);
    }

    private void UpdateSpeedMultiplier()
    {
        speedMultiplier = 1f;
        if (thirdClothes != null)
        {
            speedMultiplier *= thirdClothes.shirtAsset?.movementSpeedMultiplier ?? 1f;
            speedMultiplier *= thirdClothes.pantsAsset?.movementSpeedMultiplier ?? 1f;
            speedMultiplier *= thirdClothes.hatAsset?.movementSpeedMultiplier ?? 1f;
            speedMultiplier *= thirdClothes.backpackAsset?.movementSpeedMultiplier ?? 1f;
            speedMultiplier *= thirdClothes.vestAsset?.movementSpeedMultiplier ?? 1f;
            speedMultiplier *= thirdClothes.maskAsset?.movementSpeedMultiplier ?? 1f;
            speedMultiplier *= thirdClothes.glassesAsset?.movementSpeedMultiplier ?? 1f;
        }
    }
}
