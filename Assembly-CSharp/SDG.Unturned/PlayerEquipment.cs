using System;
using System.Collections.Generic;
using SDG.NetTransport;
using SDG.Provider;
using Steamworks;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class PlayerEquipment : PlayerCaller
{
    public static readonly byte SAVEDATA_VERSION = 1;

    private static readonly float DAMAGE_BARRICADE = 2f;

    private static readonly float DAMAGE_STRUCTURE = 2f;

    private static readonly float DAMAGE_VEHICLE = 0f;

    private static readonly float DAMAGE_RESOURCE = 20f;

    private static readonly float DAMAGE_OBJECT = 5f;

    private static readonly PlayerDamageMultiplier DAMAGE_PLAYER_MULTIPLIER = new PlayerDamageMultiplier(15f, 0.6f, 0.6f, 0.8f, 1.1f);

    private static readonly ZombieDamageMultiplier DAMAGE_ZOMBIE_MULTIPLIER = new ZombieDamageMultiplier(15f, 0.3f, 0.3f, 0.6f, 1.1f);

    private static readonly AnimalDamageMultiplier DAMAGE_ANIMAL_MULTIPLIER = new AnimalDamageMultiplier(15f, 0.3f, 0.6f, 1.1f);

    public PlayerEquipRequestHandler onEquipRequested;

    public PlayerDequipRequestHandler onDequipRequested;

    private ERagdollEffect skinRagdollEffect;

    private byte[] _state;

    private byte _quality;

    private Transform[] thirdSlots;

    private bool[] thirdSkinneds;

    private List<Mesh>[] tempThirdMeshes;

    private Material[] tempThirdMaterials;

    private MythicLockee[] thirdMythics;

    private Transform[] characterSlots;

    private bool[] characterSkinneds;

    private List<Mesh>[] tempCharacterMeshes;

    private Material[] tempCharacterMaterials;

    private MythicLockee[] characterMythics;

    private Transform _firstModel;

    private bool firstSkinned;

    private List<Mesh> tempFirstMesh;

    private Material tempFirstMaterial;

    private MythicLockee firstMythic;

    private Transform _thirdModel;

    private bool thirdSkinned;

    private List<Mesh> tempThirdMesh;

    private Material tempThirdMaterial;

    private MythicLockee thirdMythic;

    private Transform _characterModel;

    private bool characterSkinned;

    private List<Mesh> tempCharacterMesh;

    private Material tempCharacterMaterial;

    private MythicLockee characterMythic;

    private ItemAsset _asset;

    private Useable _useable;

    private Transform _thirdPrimaryMeleeSlot;

    private Transform _thirdPrimaryLargeGunSlot;

    private Transform _thirdPrimarySmallGunSlot;

    private Transform _thirdSecondaryMeleeSlot;

    private Transform _thirdSecondaryGunSlot;

    private Transform _characterPrimaryMeleeSlot;

    private Transform _characterPrimaryLargeGunSlot;

    private Transform _characterPrimarySmallGunSlot;

    private Transform _characterSecondaryMeleeSlot;

    private Transform _characterSecondaryGunSlot;

    private Transform _firstLeftHook;

    private Transform _firstRightHook;

    private Transform _thirdLeftHook;

    private Transform _thirdRightHook;

    private Transform _characterLeftHook;

    private Transform _characterRightHook;

    private HotkeyInfo[] _hotkeys;

    public HotkeysUpdated onHotkeysUpdated;

    public bool wasTryingToSelect;

    public bool isBusy;

    public bool canEquip;

    private byte slot = byte.MaxValue;

    private bool warp;

    private byte _equippedPage;

    private byte _equipped_x;

    private byte _equipped_y;

    private bool prim;

    private bool lastPrimary;

    private bool _primary;

    private bool sec;

    private bool flipSecondary;

    private bool lastSecondary;

    private bool _secondary;

    private bool hasVision;

    private float lastEquip;

    private float lastEquipped;

    private float equippedTime;

    private uint lastPunch;

    private static float lastInspect;

    private static float inspectTime;

    private static readonly ClientInstanceMethod<byte, Guid, byte, byte, byte> SendItemHotkeySuggestion = ClientInstanceMethod<byte, Guid, byte, byte, byte>.Get(typeof(PlayerEquipment), "ReceiveItemHotkeySuggeston");

    private static readonly ServerInstanceMethod SendToggleVisionRequest = ServerInstanceMethod.Get(typeof(PlayerEquipment), "ReceiveToggleVisionRequest");

    private static readonly AssetReference<EffectAsset> BeepRef = new AssetReference<EffectAsset>("f515fcbe1b5241e39217b52317e68d72");

    private static readonly ClientInstanceMethod SendToggleVision = ClientInstanceMethod.Get(typeof(PlayerEquipment), "ReceiveToggleVision");

    private static readonly ClientInstanceMethod<byte, ushort, byte[]> SendSlot = ClientInstanceMethod<byte, ushort, byte[]>.Get(typeof(PlayerEquipment), "ReceiveSlot");

    private static readonly ClientInstanceMethod<byte[]> SendUpdateStateTemp = ClientInstanceMethod<byte[]>.Get(typeof(PlayerEquipment), "ReceiveUpdateStateTemp");

    private static readonly ClientInstanceMethod<byte, byte, byte[]> SendUpdateState = ClientInstanceMethod<byte, byte, byte[]>.Get(typeof(PlayerEquipment), "ReceiveUpdateState");

    private static readonly ClientInstanceMethod<byte, byte, byte, Guid, byte, byte[], NetId> SendEquip = ClientInstanceMethod<byte, byte, byte, Guid, byte, byte[], NetId>.Get(typeof(PlayerEquipment), "ReceiveEquip");

    private static readonly ServerInstanceMethod<byte, byte, byte> SendEquipRequest = ServerInstanceMethod<byte, byte, byte>.Get(typeof(PlayerEquipment), "ReceiveEquipRequest");

    protected byte page_A;

    protected byte x_A;

    protected byte y_A;

    protected byte rot_A;

    protected bool ignoreDequip_A;

    public static Action<PlayerEquipment, EPlayerPunch> OnPunch_Global;

    public ushort itemID => asset?.id ?? 0;

    public byte[] state
    {
        get
        {
            return _state;
        }
        set
        {
            _state = value;
        }
    }

    public byte quality
    {
        get
        {
            return _quality;
        }
        set
        {
            if (!isTurret)
            {
                _quality = value;
            }
        }
    }

    public Transform firstModel => _firstModel;

    public Transform thirdModel => _thirdModel;

    public Transform characterModel => _characterModel;

    public ItemAsset asset => _asset;

    public Useable useable => _useable;

    public Transform thirdPrimaryMeleeSlot => _thirdPrimaryMeleeSlot;

    public Transform thirdPrimaryLargeGunSlot => _thirdPrimaryLargeGunSlot;

    public Transform thirdPrimarySmallGunSlot => _thirdPrimarySmallGunSlot;

    public Transform thirdSecondaryMeleeSlot => _thirdSecondaryMeleeSlot;

    public Transform thirdSecondaryGunSlot => _thirdSecondaryGunSlot;

    public Transform characterPrimaryMeleeSlot => _characterPrimaryMeleeSlot;

    public Transform characterPrimaryLargeGunSlot => _characterPrimaryLargeGunSlot;

    public Transform characterPrimarySmallGunSlot => _characterPrimarySmallGunSlot;

    public Transform characterSecondaryMeleeSlot => _characterSecondaryMeleeSlot;

    public Transform characterSecondaryGunSlot => _characterSecondaryGunSlot;

    public Transform firstLeftHook => _firstLeftHook;

    public Transform firstRightHook => _firstRightHook;

    public Transform thirdLeftHook => _thirdLeftHook;

    public Transform thirdRightHook => _thirdRightHook;

    public Transform characterLeftHook => _characterLeftHook;

    public Transform characterRightHook => _characterRightHook;

    public HotkeyInfo[] hotkeys => _hotkeys;

    public bool isSelected
    {
        get
        {
            if (thirdModel != null)
            {
                return useable != null;
            }
            return false;
        }
    }

    public bool isEquipped => Time.realtimeSinceStartup - lastEquipped > equippedTime;

    public bool isTurret { get; private set; }

    public bool isUseableShowingMenu
    {
        get
        {
            if (useable != null)
            {
                return useable.isUseableShowingMenu;
            }
            return false;
        }
    }

    public byte equippedPage => _equippedPage;

    public byte equipped_x => _equipped_x;

    public byte equipped_y => _equipped_y;

    public bool primary => _primary;

    public bool secondary => _secondary;

    public float lastPunching { get; private set; }

    public bool isInspecting => Time.realtimeSinceStartup - lastInspect < inspectTime;

    public bool canInspect
    {
        get
        {
            if (isSelected && isEquipped && !isBusy && base.player.animator.checkExists("Inspect") && !isInspecting)
            {
                return useable.canInspect;
            }
            return false;
        }
    }

    public static event Action<PlayerEquipment> OnUseableChanged_Global;

    public static event Action<PlayerEquipment> OnInspectingUseable_Global;

    public bool isItemHotkeyed(byte page, byte index, ItemJar jar, out byte button)
    {
        if (page < PlayerInventory.SLOTS)
        {
            button = page;
            return true;
        }
        for (byte b = 0; b < hotkeys.Length; b = (byte)(b + 1))
        {
            HotkeyInfo hotkeyInfo = hotkeys[b];
            if (hotkeyInfo.page == page && hotkeyInfo.x == jar.x && hotkeyInfo.y == jar.y && hotkeyInfo.id == jar.item.id)
            {
                button = (byte)(b + 2);
                return true;
            }
        }
        button = 0;
        return false;
    }

    public ERagdollEffect getUseableRagdollEffect()
    {
        if (base.player.clothing.isMythic)
        {
            return skinRagdollEffect;
        }
        return ERagdollEffect.NONE;
    }

    public void ServerBindItemHotkey(byte hotkeyIndex, ItemAsset expectedItem, byte page, byte x, byte y)
    {
        SendItemHotkeySuggestion.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), hotkeyIndex, expectedItem?.GUID ?? Guid.Empty, page, x, y);
    }

    public void ServerClearItemHotkey(byte hotkeyIndex)
    {
        SendItemHotkeySuggestion.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), hotkeyIndex, Guid.Empty, byte.MaxValue, byte.MaxValue, byte.MaxValue);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveItemHotkeySuggeston(in ClientInvocationContext context, byte hotkeyIndex, Guid expectedAssetGuid, byte page, byte x, byte y)
    {
        if (hotkeys == null || hotkeyIndex >= hotkeys.Length)
        {
            return;
        }
        ushort num = 0;
        if (!expectedAssetGuid.IsEmpty())
        {
            ItemAsset itemAsset = Assets.find<ItemAsset>(expectedAssetGuid);
            if (itemAsset != null)
            {
                num = itemAsset.id;
            }
            else
            {
                UnturnedLog.warn($"Unable to use server item hotkey suggestion because asset was not found ({expectedAssetGuid})");
            }
        }
        if (num == 0)
        {
            page = byte.MaxValue;
            x = byte.MaxValue;
            y = byte.MaxValue;
        }
        HotkeyInfo obj = hotkeys[hotkeyIndex];
        obj.id = num;
        obj.page = page;
        obj.x = x;
        obj.y = y;
        ClearDuplicateHotkeys(hotkeyIndex);
        if (onHotkeysUpdated != null)
        {
            onHotkeysUpdated();
        }
    }

    private void ClearDuplicateHotkeys(int newHotkeyIndex)
    {
        HotkeyInfo hotkeyInfo = hotkeys[newHotkeyIndex];
        for (int i = 0; i < hotkeys.Length; i++)
        {
            if (i != newHotkeyIndex)
            {
                HotkeyInfo hotkeyInfo2 = hotkeys[i];
                if (hotkeyInfo2.page == hotkeyInfo.page && hotkeyInfo2.x == hotkeyInfo.x && hotkeyInfo2.y == hotkeyInfo.y)
                {
                    hotkeyInfo2.id = 0;
                    hotkeyInfo2.page = byte.MaxValue;
                    hotkeyInfo2.x = byte.MaxValue;
                    hotkeyInfo2.y = byte.MaxValue;
                }
            }
        }
    }

    public bool getUseableStatTrackerValue(out EStatTrackerType type, out int kills)
    {
        return base.channel.owner.getStatTrackerValue(itemID, out type, out kills);
    }

    protected bool getSlot0StatTrackerValue(out EStatTrackerType type, out int kills)
    {
        ItemJar item = base.player.inventory.getItem(0, 0);
        if (item != null)
        {
            return base.channel.owner.getStatTrackerValue(item.item.id, out type, out kills);
        }
        type = EStatTrackerType.NONE;
        kills = -1;
        return false;
    }

    protected bool getSlot1StatTrackerValue(out EStatTrackerType type, out int kills)
    {
        ItemJar item = base.player.inventory.getItem(1, 0);
        if (item != null)
        {
            return base.channel.owner.getStatTrackerValue(item.item.id, out type, out kills);
        }
        type = EStatTrackerType.NONE;
        kills = -1;
        return false;
    }

    protected void fixStatTrackerHookScale(Transform itemModelTransform)
    {
        if (base.channel.owner.hand)
        {
            Transform transform = itemModelTransform.Find("Stat_Tracker");
            if ((bool)transform)
            {
                transform.localScale = new Vector3(0f - transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
        }
    }

    protected void syncStatTrackTrackerVisibility(Transform itemModelTransform)
    {
        if (!(itemModelTransform == null))
        {
            Transform transform = itemModelTransform.Find("Stat_Tracker");
            if ((bool)transform)
            {
                transform.gameObject.SetActive(base.player.clothing.isSkinned);
            }
        }
    }

    protected void syncAllStatTrackerVisibility()
    {
        syncStatTrackTrackerVisibility(firstModel);
        syncStatTrackTrackerVisibility(thirdModel);
        syncStatTrackTrackerVisibility(characterModel);
        if (thirdSlots != null)
        {
            Transform[] array = thirdSlots;
            foreach (Transform itemModelTransform in array)
            {
                syncStatTrackTrackerVisibility(itemModelTransform);
            }
        }
        if (characterSlots != null)
        {
            Transform[] array = characterSlots;
            foreach (Transform itemModelTransform2 in array)
            {
                syncStatTrackTrackerVisibility(itemModelTransform2);
            }
        }
    }

    public void inspect()
    {
        base.player.animator.setAnimationSpeed("Inspect", 1f);
        lastInspect = Time.realtimeSinceStartup;
        inspectTime = base.player.animator.getAnimationLength("Inspect");
        base.player.animator.play("Inspect", smooth: false);
    }

    internal void InvokeOnInspectingUseable()
    {
        PlayerEquipment.OnInspectingUseable_Global.TryInvoke("OnInspectingUseable_Global", this);
    }

    public void uninspect()
    {
        base.player.animator.setAnimationSpeed("Inspect", float.MaxValue);
    }

    public bool checkSelection(byte page)
    {
        return page == equippedPage;
    }

    public bool checkSelection(byte page, byte x, byte y)
    {
        if (page == equippedPage && x == equipped_x)
        {
            return y == equipped_y;
        }
        return false;
    }

    public void applySkinVisual()
    {
        if (firstModel != null && firstSkinned != base.player.clothing.isSkinned)
        {
            firstSkinned = base.player.clothing.isSkinned;
            if (tempFirstMaterial != null)
            {
                Attachments component = firstModel.GetComponent<Attachments>();
                if (component != null)
                {
                    component.isSkinned = firstSkinned;
                    component.applyVisual();
                }
                if (tempFirstMesh.Count > 0)
                {
                    HighlighterTool.remesh(firstModel, tempFirstMesh, tempFirstMesh);
                }
                HighlighterTool.rematerialize(firstModel, tempFirstMaterial, out tempFirstMaterial);
            }
        }
        if (thirdModel != null && thirdSkinned != base.player.clothing.isSkinned)
        {
            thirdSkinned = base.player.clothing.isSkinned;
            if (tempThirdMaterial != null)
            {
                Attachments component2 = thirdModel.GetComponent<Attachments>();
                if (component2 != null)
                {
                    component2.isSkinned = thirdSkinned;
                    component2.applyVisual();
                }
                if (tempThirdMesh.Count > 0)
                {
                    HighlighterTool.remesh(thirdModel, tempThirdMesh, tempThirdMesh);
                }
                HighlighterTool.rematerialize(thirdModel, tempThirdMaterial, out tempThirdMaterial);
            }
        }
        if (characterModel != null && characterSkinned != base.player.clothing.isSkinned)
        {
            characterSkinned = base.player.clothing.isSkinned;
            if (tempCharacterMaterial != null)
            {
                Attachments component3 = characterModel.GetComponent<Attachments>();
                if (component3 != null)
                {
                    component3.isSkinned = characterSkinned;
                    component3.applyVisual();
                }
                if (tempCharacterMesh.Count > 0)
                {
                    HighlighterTool.remesh(characterModel, tempCharacterMesh, tempCharacterMesh);
                }
                HighlighterTool.rematerialize(characterModel, tempCharacterMaterial, out tempCharacterMaterial);
            }
        }
        if (thirdSlots != null)
        {
            for (byte b = 0; b < thirdSlots.Length; b = (byte)(b + 1))
            {
                if (thirdSlots[b] != null && thirdSkinneds[b] != base.player.clothing.isSkinned)
                {
                    thirdSkinneds[b] = base.player.clothing.isSkinned;
                    if (tempThirdMaterials[b] != null)
                    {
                        Attachments component4 = thirdSlots[b].GetComponent<Attachments>();
                        if (component4 != null)
                        {
                            component4.isSkinned = thirdSkinneds[b];
                            component4.applyVisual();
                        }
                        if (tempThirdMeshes[b].Count > 0)
                        {
                            HighlighterTool.remesh(thirdSlots[b], tempThirdMeshes[b], tempThirdMeshes[b]);
                        }
                        HighlighterTool.rematerialize(thirdSlots[b], tempThirdMaterials[b], out tempThirdMaterials[b]);
                    }
                }
                if (characterSlots != null && characterSlots[b] != null && characterSkinneds[b] != base.player.clothing.isSkinned)
                {
                    characterSkinneds[b] = base.player.clothing.isSkinned;
                    if (tempCharacterMaterials[b] != null)
                    {
                        Attachments component5 = characterSlots[b].GetComponent<Attachments>();
                        if (component5 != null)
                        {
                            component5.isSkinned = characterSkinneds[b];
                            component5.applyVisual();
                        }
                        if (tempCharacterMeshes[b].Count > 0)
                        {
                            HighlighterTool.remesh(characterSlots[b], tempCharacterMeshes[b], tempCharacterMeshes[b]);
                        }
                        HighlighterTool.rematerialize(characterSlots[b], tempCharacterMaterials[b], out tempCharacterMaterials[b]);
                    }
                }
            }
        }
        syncAllStatTrackerVisibility();
    }

    public void applyMythicVisual()
    {
        if (firstMythic != null)
        {
            firstMythic.isMythic = base.player.clothing.isSkinned && base.player.clothing.isMythic;
        }
        if (thirdMythic != null)
        {
            thirdMythic.isMythic = base.player.clothing.isSkinned && base.player.clothing.isMythic;
        }
        if (characterMythic != null)
        {
            characterMythic.isMythic = base.player.clothing.isSkinned && base.player.clothing.isMythic;
        }
        if (thirdSlots == null)
        {
            return;
        }
        for (byte b = 0; b < thirdSlots.Length; b = (byte)(b + 1))
        {
            if (thirdMythics[b] != null)
            {
                thirdMythics[b].isMythic = base.player.clothing.isSkinned && base.player.clothing.isMythic;
            }
            if (characterSlots != null && characterMythics[b] != null)
            {
                characterMythics[b].isMythic = base.player.clothing.isSkinned && base.player.clothing.isMythic;
            }
        }
    }

    private void updateSlot(byte slot, ushort id, byte[] state)
    {
    }

    [Obsolete]
    public void askToggleVision(CSteamID steamID)
    {
        ReceiveToggleVisionRequest();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askToggleVision")]
    public void ReceiveToggleVisionRequest()
    {
        if (!hasVision || base.player.clothing.glassesState.Length != 1)
        {
            return;
        }
        SendToggleVision.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote());
        if (base.player.clothing.glassesAsset == null)
        {
            return;
        }
        if (base.player.clothing.glassesAsset.vision == ELightingVision.HEADLAMP)
        {
            EffectManager.TriggerFiremodeEffect(base.transform.position);
        }
        else if (base.player.clothing.glassesAsset.vision == ELightingVision.CIVILIAN || base.player.clothing.glassesAsset.vision == ELightingVision.MILITARY)
        {
            EffectAsset effectAsset = BeepRef.Find();
            if (effectAsset != null)
            {
                TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
                parameters.relevantDistance = EffectManager.SMALL;
                parameters.position = base.transform.position;
                EffectManager.triggerEffect(parameters);
            }
        }
    }

    [Obsolete]
    public void tellToggleVision(CSteamID steamID)
    {
        ReceiveToggleVision();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellToggleVision")]
    public void ReceiveToggleVision()
    {
        if (hasVision && base.player.clothing.glassesState.Length == 1)
        {
            base.player.clothing.glassesState[0] = (byte)((base.player.clothing.glassesState[0] == 0) ? 1u : 0u);
            updateVision();
        }
    }

    [Obsolete]
    public void tellSlot(CSteamID steamID, byte slot, ushort id, byte[] state)
    {
        ReceiveSlot(slot, id, state);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellSlot")]
    public void ReceiveSlot(byte slot, ushort id, byte[] state)
    {
        updateSlot(slot, id, state);
    }

    [Obsolete]
    public void tellUpdateStateTemp(CSteamID steamID, byte[] newState)
    {
        ReceiveUpdateStateTemp(newState);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUpdateStateTemp")]
    public void ReceiveUpdateStateTemp(byte[] newState)
    {
        _state = newState;
        if (useable != null)
        {
            try
            {
                useable.updateState(state);
            }
            catch (Exception e)
            {
                UnturnedLog.warn("{0} raised an exception during ReceiveUpdateStateTemp.updateState:", asset);
                UnturnedLog.exception(e);
            }
        }
    }

    [Obsolete]
    public void tellUpdateState(CSteamID steamID, byte page, byte index, byte[] newState)
    {
        ReceiveUpdateState(page, index, newState);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellUpdateState")]
    public void ReceiveUpdateState(byte page, byte index, byte[] newState)
    {
        if (thirdSlots == null)
        {
            return;
        }
        _state = newState;
        if (slot != byte.MaxValue && slot < thirdSlots.Length && thirdSlots[slot] != null)
        {
            updateSlot(slot, itemID, newState);
            thirdSlots[slot].gameObject.SetActive(value: false);
            if (characterSlots != null)
            {
                characterSlots[slot].gameObject.SetActive(value: false);
            }
        }
        if (base.channel.isOwner || Provider.isServer)
        {
            base.player.inventory.updateState(page, index, state);
        }
        if (useable != null)
        {
            try
            {
                useable.updateState(state);
            }
            catch (Exception e)
            {
                UnturnedLog.warn("{0} raised an exception during tellUpdateState.updateState:", asset);
                UnturnedLog.exception(e);
            }
        }
        if (!(characterModel != null))
        {
            return;
        }
        UnityEngine.Object.Destroy(characterModel.gameObject);
        if (!(Assets.find(EAssetType.ITEM, itemID) is ItemAsset itemAsset))
        {
            return;
        }
        int value = 0;
        ushort skin = 0;
        ushort num = 0;
        if (base.channel.owner.skinItems != null && base.channel.owner.itemSkins != null && base.channel.owner.itemSkins.TryGetValue(itemAsset.sharedSkinLookupID, out value))
        {
            skin = Provider.provider.economyService.getInventorySkinID(value);
            num = Provider.provider.economyService.getInventoryMythicID(value);
            if (num == 0)
            {
                num = base.channel.owner.getParticleEffectForItemDef(value);
            }
        }
        GetStatTrackerValueHandler statTrackerCallback = null;
        if (slot == 0)
        {
            statTrackerCallback = getSlot0StatTrackerValue;
        }
        else if (slot == 1)
        {
            statTrackerCallback = getSlot1StatTrackerValue;
        }
        _characterModel = ItemTool.getItem(itemID, skin, 100, state, viewmodel: false, itemAsset, tempCharacterMesh, out tempCharacterMaterial, statTrackerCallback);
        fixStatTrackerHookScale(_characterModel);
        syncStatTrackTrackerVisibility(_characterModel);
        if (itemAsset.isBackward)
        {
            characterModel.transform.parent = _characterLeftHook;
        }
        else
        {
            characterModel.transform.parent = _characterRightHook;
        }
        characterModel.localPosition = Vector3.zero;
        characterModel.localRotation = Quaternion.Euler(0f, 0f, 90f);
        characterModel.localScale = Vector3.one;
        characterModel.gameObject.AddComponent<Rigidbody>();
        characterModel.GetComponent<Rigidbody>().useGravity = false;
        characterModel.GetComponent<Rigidbody>().isKinematic = true;
        if (num != 0)
        {
            Transform transform = ItemTool.applyEffect(characterModel, num, EEffectType.THIRD);
            if (transform != null)
            {
                characterMythic = transform.GetComponent<MythicLockee>();
            }
        }
        characterSkinned = true;
        applySkinVisual();
        if (characterMythic != null)
        {
            characterMythic.isMythic = base.player.clothing.isSkinned && base.player.clothing.isMythic;
        }
    }

    [Obsolete]
    public void tellEquip(CSteamID steamID, byte page, byte x, byte y, ushort id, byte newQuality, byte[] newState)
    {
        ReceiveEquip(page, x, y, Assets.find(EAssetType.ITEM, id)?.GUID ?? Guid.Empty, newQuality, newState, default(NetId));
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellEquip")]
    public void ReceiveEquip(byte page, byte x, byte y, Guid newAssetGuid, byte newQuality, byte[] newState, NetId useableNetId)
    {
        if (thirdSlots == null)
        {
            return;
        }
        if (slot != byte.MaxValue && slot < thirdSlots.Length && thirdSlots[slot] != null)
        {
            thirdSlots[slot].gameObject.SetActive(value: true);
            if (characterSlots != null)
            {
                characterSlots[slot].gameObject.SetActive(value: true);
            }
        }
        slot = page;
        if (slot != byte.MaxValue && slot < thirdSlots.Length && thirdSlots[slot] != null)
        {
            thirdSlots[slot].gameObject.SetActive(value: false);
            if (characterSlots != null)
            {
                characterSlots[slot].gameObject.SetActive(value: false);
            }
        }
        if (useable != null)
        {
            try
            {
                useable.dequip();
            }
            catch (Exception e)
            {
                UnturnedLog.warn("{0} raised an exception during tellEquip.dequip:", asset);
                UnturnedLog.exception(e);
            }
            _useable.ReleaseNetId();
            useable.hideFlags |= HideFlags.NotEditable;
            UnityEngine.Object.Destroy(useable);
            _useable = null;
            base.channel.markDirty();
        }
        skinRagdollEffect = ERagdollEffect.NONE;
        if (firstModel != null)
        {
            UnityEngine.Object.Destroy(firstModel.gameObject);
        }
        firstSkinned = false;
        tempFirstMaterial = null;
        firstMythic = null;
        if (thirdModel != null)
        {
            UnityEngine.Object.Destroy(thirdModel.gameObject);
        }
        thirdSkinned = false;
        tempThirdMaterial = null;
        thirdMythic = null;
        if (characterModel != null)
        {
            UnityEngine.Object.Destroy(characterModel.gameObject);
        }
        characterSkinned = false;
        tempCharacterMaterial = null;
        characterMythic = null;
        if (asset != null && asset.animations != null && asset.animations.Length != 0)
        {
            for (int i = 0; i < asset.animations.Length; i++)
            {
                base.player.animator.removeAnimation(asset.animations[i]);
            }
        }
        isBusy = false;
        if (newAssetGuid.IsEmpty())
        {
            _equippedPage = byte.MaxValue;
            _equipped_x = byte.MaxValue;
            _equipped_y = byte.MaxValue;
            _asset = null;
            PlayerEquipment.OnUseableChanged_Global.TryInvoke("OnUseableChanged_Global", this);
            return;
        }
        _equippedPage = page;
        _equipped_x = x;
        _equipped_y = y;
        _asset = Assets.find(newAssetGuid) as ItemAsset;
        if (asset == null || !(asset.useableType != null))
        {
            return;
        }
        quality = newQuality;
        _state = newState;
        int value = 0;
        ushort num = 0;
        ushort num2 = 0;
        if (base.channel.owner.skinItems != null && base.channel.owner.itemSkins != null && base.channel.owner.itemSkins.TryGetValue(asset.sharedSkinLookupID, out value))
        {
            num = Provider.provider.economyService.getInventorySkinID(value);
            num2 = Provider.provider.economyService.getInventoryMythicID(value);
            if (num2 == 0)
            {
                num2 = base.channel.owner.getParticleEffectForItemDef(value);
            }
        }
        SkinAsset skinAsset = Assets.find(EAssetType.SKIN, num) as SkinAsset;
        skinRagdollEffect = ERagdollEffect.NONE;
        if (!base.channel.owner.getRagdollEffect(itemID, out skinRagdollEffect) && skinAsset != null)
        {
            skinRagdollEffect = skinAsset.ragdollEffect;
        }
        GameObject prefabOverride = ((asset.equipablePrefab != null) ? asset.equipablePrefab : asset.item);
        if (base.channel.isOwner)
        {
            ClientAssetIntegrity.QueueRequest(_asset);
            _firstModel = ItemTool.getItem(asset.id, num, quality, state, viewmodel: true, asset, skinAsset, tempFirstMesh, out tempFirstMaterial, getUseableStatTrackerValue, prefabOverride);
            fixStatTrackerHookScale(_firstModel);
            syncStatTrackTrackerVisibility(_firstModel);
            if (asset.isBackward)
            {
                firstModel.transform.parent = firstLeftHook;
            }
            else
            {
                firstModel.transform.parent = firstRightHook;
            }
            firstModel.localPosition = Vector3.zero;
            firstModel.localRotation = Quaternion.Euler(0f, 0f, 90f);
            firstModel.localScale = Vector3.one;
            firstModel.gameObject.SetActive(value: false);
            firstModel.gameObject.SetActive(value: true);
            firstModel.gameObject.AddComponent<Rigidbody>();
            firstModel.GetComponent<Rigidbody>().useGravity = false;
            firstModel.GetComponent<Rigidbody>().isKinematic = true;
            Collider component = firstModel.GetComponent<Collider>();
            if (component != null)
            {
                UnityEngine.Object.Destroy(component);
            }
            Layerer.viewmodel(firstModel);
            if (num2 != 0)
            {
                Transform transform = ItemTool.applyEffect(firstModel, num2, EEffectType.FIRST);
                if (transform != null)
                {
                    firstMythic = transform.GetComponent<MythicLockee>();
                }
            }
            firstSkinned = true;
            applySkinVisual();
            if (firstMythic != null)
            {
                firstMythic.isMythic = base.player.clothing.isSkinned && base.player.clothing.isMythic;
            }
            _characterModel = ItemTool.getItem(asset.id, num, quality, state, viewmodel: false, asset, skinAsset, tempCharacterMesh, out tempCharacterMaterial, getUseableStatTrackerValue, prefabOverride);
            fixStatTrackerHookScale(_characterModel);
            syncStatTrackTrackerVisibility(_characterModel);
            if (asset.isBackward)
            {
                characterModel.transform.parent = characterLeftHook;
            }
            else
            {
                characterModel.transform.parent = characterRightHook;
            }
            characterModel.localPosition = Vector3.zero;
            characterModel.localRotation = Quaternion.Euler(0f, 0f, 90f);
            characterModel.localScale = Vector3.one;
            Rigidbody orAddComponent = characterModel.gameObject.GetOrAddComponent<Rigidbody>();
            orAddComponent.useGravity = false;
            orAddComponent.isKinematic = true;
            if (num2 != 0)
            {
                Transform transform2 = ItemTool.applyEffect(characterModel, num2, EEffectType.THIRD);
                if (transform2 != null)
                {
                    characterMythic = transform2.GetComponent<MythicLockee>();
                }
            }
            characterSkinned = true;
            applySkinVisual();
            if (characterMythic != null)
            {
                characterMythic.isMythic = base.player.clothing.isSkinned && base.player.clothing.isMythic;
            }
        }
        _thirdModel = ItemTool.getItem(asset.id, num, quality, state, viewmodel: false, asset, skinAsset, tempThirdMesh, out tempThirdMaterial, getUseableStatTrackerValue, prefabOverride);
        fixStatTrackerHookScale(_thirdModel);
        syncStatTrackTrackerVisibility(_thirdModel);
        if (asset.isBackward)
        {
            thirdModel.transform.parent = thirdLeftHook;
        }
        else
        {
            thirdModel.transform.parent = thirdRightHook;
        }
        thirdModel.localPosition = Vector3.zero;
        thirdModel.localRotation = Quaternion.Euler(0f, 0f, 90f);
        thirdModel.localScale = Vector3.one;
        thirdModel.gameObject.SetActive(value: false);
        thirdModel.gameObject.SetActive(value: true);
        Rigidbody orAddComponent2 = thirdModel.gameObject.GetOrAddComponent<Rigidbody>();
        orAddComponent2.useGravity = false;
        orAddComponent2.isKinematic = true;
        Collider component2 = thirdModel.GetComponent<Collider>();
        if (component2 != null)
        {
            UnityEngine.Object.Destroy(component2);
        }
        Layerer.enemy(thirdModel);
        if (num2 != 0)
        {
            Transform transform3 = ItemTool.applyEffect(thirdModel, num2, EEffectType.THIRD);
            if (transform3 != null)
            {
                thirdMythic = transform3.GetComponent<MythicLockee>();
            }
        }
        thirdSkinned = true;
        applySkinVisual();
        if (thirdMythic != null)
        {
            thirdMythic.isMythic = base.player.clothing.isSkinned && base.player.clothing.isMythic;
        }
        if (asset.animations != null && asset.animations.Length != 0)
        {
            for (int j = 0; j < asset.animations.Length; j++)
            {
                base.player.animator.addAnimation(asset.animations[j]);
            }
        }
        _useable = base.gameObject.AddComponent(asset.useableType) as Useable;
        _useable.AssignNetId(useableNetId);
        base.channel.markDirty();
        try
        {
            useable.equip();
        }
        catch (Exception e2)
        {
            UnturnedLog.warn("{0} raised an exception during tellEquip.equip:", asset);
            UnturnedLog.exception(e2);
        }
        lastEquipped = Time.realtimeSinceStartup;
        equippedTime = base.player.animator.getAnimationLength("Equip");
        PlayerEquipment.OnUseableChanged_Global.TryInvoke("OnUseableChanged_Global", this);
    }

    [Obsolete("Renamed to ServerEquip")]
    public void tryEquip(byte page, byte x, byte y)
    {
        ServerEquip(page, x, y);
    }

    [Obsolete("No longer necessary after hash check was converted to newer system")]
    public void tryEquip(byte page, byte x, byte y, byte[] hash)
    {
        ServerEquip(page, x, y);
    }

    public void ServerEquip(byte page, byte x, byte y)
    {
        if (isBusy || !canEquip || base.player.life.isDead || base.player.stance.stance == EPlayerStance.CLIMB || base.player.stance.stance == EPlayerStance.DRIVING || (isSelected && !isEquipped) || isTurret)
        {
            return;
        }
        if ((page == equippedPage && x == equipped_x && y == equipped_y) || page == byte.MaxValue)
        {
            bool shouldAllow = true;
            if (onDequipRequested != null)
            {
                onDequipRequested(this, ref shouldAllow);
            }
            if (shouldAllow)
            {
                dequip();
            }
        }
        else
        {
            if (page < 0 || page >= PlayerInventory.PAGES - 2)
            {
                return;
            }
            byte index = base.player.inventory.getIndex(page, x, y);
            if (index == byte.MaxValue)
            {
                return;
            }
            ItemJar item = base.player.inventory.getItem(page, index);
            if (item == null || !ItemTool.checkUseable(page, item.item.id))
            {
                return;
            }
            ItemAsset itemAsset = item.GetAsset();
            if (itemAsset == null || ((base.player.stance.isSubmerged || base.player.stance.stance == EPlayerStance.SWIM) && !itemAsset.canUseUnderwater) || base.player.animator.gesture == EPlayerGesture.ARREST_START)
            {
                return;
            }
            bool shouldAllow2 = true;
            if (onEquipRequested != null)
            {
                onEquipRequested(this, item, itemAsset, ref shouldAllow2);
            }
            if (shouldAllow2)
            {
                NetId arg = NetIdRegistry.Claim();
                if (item.item.state != null)
                {
                    SendEquip.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), page, x, y, itemAsset.GUID, item.item.quality, item.item.state, arg);
                }
                else
                {
                    SendEquip.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), page, x, y, itemAsset.GUID, item.item.quality, new byte[0], arg);
                }
            }
        }
    }

    public void turretEquipClient()
    {
        isTurret = true;
    }

    public void turretEquipServer(ushort id, byte[] state)
    {
        Guid guid = Assets.find(EAssetType.ITEM, id)?.GUID ?? Guid.Empty;
        NetId netId = NetIdRegistry.Claim();
        SendEquip.Invoke(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), 254, 254, 254, guid, 100, state, netId);
        ReceiveEquip(254, 254, 254, guid, 100, state, netId);
    }

    public void turretDequipClient()
    {
        isTurret = false;
    }

    public void turretDequipServer()
    {
        SendEquip.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), byte.MaxValue, byte.MaxValue, byte.MaxValue, Guid.Empty, 0, new byte[0], default(NetId));
    }

    [Obsolete]
    public void askEquip(CSteamID steamID, byte page, byte x, byte y, byte[] hash)
    {
        ServerEquip(page, x, y);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 5, legacyName = "askEquip")]
    public void ReceiveEquipRequest(byte page, byte x, byte y)
    {
        ServerEquip(page, x, y);
    }

    [Obsolete]
    public void askEquipment(CSteamID steamID)
    {
    }

    internal void SendInitialPlayerState(SteamPlayer client)
    {
        for (byte b = 0; b < PlayerInventory.SLOTS; b = (byte)(b + 1))
        {
            ItemJar item = base.player.inventory.getItem(b, 0);
            if (item != null)
            {
                if (item.item.state != null)
                {
                    SendSlot.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, b, item.item.id, item.item.state);
                }
                else
                {
                    SendSlot.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, b, item.item.id, new byte[0]);
                }
            }
            else
            {
                SendSlot.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, b, 0, new byte[0]);
            }
        }
        if (isSelected)
        {
            Guid arg = asset?.GUID ?? Guid.Empty;
            NetId netId = useable.GetNetId();
            if (state != null)
            {
                SendEquip.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, equippedPage, equipped_x, equipped_y, arg, quality, state, netId);
            }
            else
            {
                SendEquip.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, equippedPage, equipped_x, equipped_y, arg, quality, new byte[0], netId);
            }
        }
    }

    internal void SendInitialPlayerState(IEnumerable<ITransportConnection> transportConnections)
    {
        for (byte b = 0; b < PlayerInventory.SLOTS; b = (byte)(b + 1))
        {
            ItemJar item = base.player.inventory.getItem(b, 0);
            if (item != null)
            {
                if (item.item.state != null)
                {
                    SendSlot.Invoke(GetNetId(), ENetReliability.Reliable, transportConnections, b, item.item.id, item.item.state);
                }
                else
                {
                    SendSlot.Invoke(GetNetId(), ENetReliability.Reliable, transportConnections, b, item.item.id, new byte[0]);
                }
            }
            else
            {
                SendSlot.Invoke(GetNetId(), ENetReliability.Reliable, transportConnections, b, 0, new byte[0]);
            }
        }
        if (isSelected)
        {
            Guid arg = asset?.GUID ?? Guid.Empty;
            NetId netId = useable.GetNetId();
            if (state != null)
            {
                SendEquip.Invoke(GetNetId(), ENetReliability.Reliable, transportConnections, equippedPage, equipped_x, equipped_y, arg, quality, state, netId);
            }
            else
            {
                SendEquip.Invoke(GetNetId(), ENetReliability.Reliable, transportConnections, equippedPage, equipped_x, equipped_y, arg, quality, new byte[0], netId);
            }
        }
    }

    public void updateState()
    {
        if (!isTurret)
        {
            byte index = base.player.inventory.getIndex(equippedPage, equipped_x, equipped_y);
            if (index != byte.MaxValue)
            {
                base.player.inventory.updateState(equippedPage, index, state);
            }
        }
    }

    public void updateQuality()
    {
        if (!isTurret)
        {
            byte index = base.player.inventory.getIndex(equippedPage, equipped_x, equipped_y);
            if (index != byte.MaxValue)
            {
                base.player.inventory.updateQuality(equippedPage, index, quality);
            }
        }
    }

    public void sendUpdateState()
    {
        if (isTurret)
        {
            SendUpdateStateTemp.Invoke(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), state);
            ReceiveUpdateStateTemp(state);
            return;
        }
        byte index = base.player.inventory.getIndex(equippedPage, equipped_x, equipped_y);
        if (index != byte.MaxValue)
        {
            SendUpdateState.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), equippedPage, index, state);
        }
    }

    public void sendUpdateQuality()
    {
        if (!isTurret)
        {
            base.player.inventory.sendUpdateQuality(equippedPage, equipped_x, equipped_y, quality);
        }
    }

    public void sendSlot(byte slot)
    {
        ItemJar item = base.player.inventory.getItem(slot, 0);
        if (item != null)
        {
            if (item.item.state != null)
            {
                SendSlot.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), slot, item.item.id, item.item.state);
            }
            else
            {
                SendSlot.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), slot, item.item.id, new byte[0]);
            }
        }
        else
        {
            SendSlot.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), slot, 0, new byte[0]);
        }
    }

    public void equip(byte page, byte x, byte y)
    {
        if (page < 0 || page >= PlayerInventory.PAGES - 2 || isBusy || !canEquip || base.player.life.isDead || base.player.stance.stance == EPlayerStance.CLIMB || base.player.stance.stance == EPlayerStance.DRIVING || (isSelected && !isEquipped))
        {
            return;
        }
        byte index = base.player.inventory.getIndex(page, x, y);
        if (index == byte.MaxValue)
        {
            return;
        }
        ItemJar item = base.player.inventory.getItem(page, index);
        if (item != null)
        {
            ItemAsset itemAsset = item.GetAsset();
            if (itemAsset != null && ((!base.player.stance.isSubmerged && base.player.stance.stance != EPlayerStance.SWIM) || itemAsset.canUseUnderwater) && base.player.animator.gesture != EPlayerGesture.ARREST_START)
            {
                lastEquip = Time.realtimeSinceStartup;
                SendEquipRequest.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
            }
        }
    }

    internal void ClientEquipAfterItemDrag(byte page, byte x, byte y)
    {
        SendEquipRequest.Invoke(GetNetId(), ENetReliability.Unreliable, page, x, y);
    }

    public void dequip()
    {
        if (!isTurret && !ignoreDequip_A)
        {
            if (Provider.isServer)
            {
                SendEquip.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.EnumerateClients_Remote(), byte.MaxValue, byte.MaxValue, byte.MaxValue, Guid.Empty, 0, new byte[0], default(NetId));
            }
            else if (!isBusy)
            {
                SendEquipRequest.Invoke(GetNetId(), ENetReliability.Unreliable, byte.MaxValue, byte.MaxValue, byte.MaxValue);
            }
        }
    }

    public void use()
    {
        if (isSelected)
        {
            ushort id = itemID;
            byte index = base.player.inventory.getIndex(equippedPage, equipped_x, equipped_y);
            ItemJar item = base.player.inventory.getItem(equippedPage, index);
            byte b = equippedPage;
            byte b2 = equipped_x;
            byte b3 = equipped_y;
            byte rot = item.rot;
            base.player.inventory.removeItem(equippedPage, index);
            dequip();
            InventorySearch inventorySearch = base.player.inventory.has(id);
            if (inventorySearch != null)
            {
                base.player.inventory.ReceiveDragItem(inventorySearch.page, inventorySearch.jar.x, inventorySearch.jar.y, b, b2, b3, rot);
                ServerEquip(b, b2, b3);
            }
        }
    }

    public void useStepA()
    {
        if (isSelected)
        {
            byte index = base.player.inventory.getIndex(equippedPage, equipped_x, equipped_y);
            ItemJar item = base.player.inventory.getItem(equippedPage, index);
            page_A = equippedPage;
            x_A = equipped_x;
            y_A = equipped_y;
            rot_A = item.rot;
            ignoreDequip_A = true;
            base.player.inventory.removeItem(equippedPage, index);
            ignoreDequip_A = false;
        }
    }

    public void useStepB()
    {
        if (isSelected)
        {
            ushort id = itemID;
            dequip();
            InventorySearch inventorySearch = base.player.inventory.has(id);
            if (inventorySearch != null)
            {
                base.player.inventory.ReceiveDragItem(inventorySearch.page, inventorySearch.jar.x, inventorySearch.jar.y, page_A, x_A, y_A, rot_A);
                ServerEquip(page_A, x_A, y_A);
            }
        }
    }

    private void punch(EPlayerPunch mode)
    {
        if (base.channel.isOwner)
        {
            AudioClip clip = Assets.coreMasterBundle.assetBundle.LoadAsset<AudioClip>("Assets/CoreMasterBundle/Sounds/MeleeAttack_02.mp3");
            base.player.playSound(clip);
            RaycastInfo raycastInfo = DamageTool.raycast(new Ray(base.player.look.aim.position, base.player.look.aim.forward), 1.75f, RayMasks.DAMAGE_CLIENT, base.player);
            if (raycastInfo.player != null && DAMAGE_PLAYER_MULTIPLIER.damage > 1f && DamageTool.isPlayerAllowedToDamagePlayer(base.player, raycastInfo.player))
            {
                PlayerUI.hitmark(0, raycastInfo.point, worldspace: false, (raycastInfo.limb != ELimb.SKULL) ? EPlayerHit.ENTITIY : EPlayerHit.CRITICAL);
            }
            else if ((raycastInfo.zombie != null && DAMAGE_ZOMBIE_MULTIPLIER.damage > 1f) || (raycastInfo.animal != null && DAMAGE_ANIMAL_MULTIPLIER.damage > 1f))
            {
                PlayerUI.hitmark(0, raycastInfo.point, worldspace: false, (raycastInfo.limb != ELimb.SKULL) ? EPlayerHit.ENTITIY : EPlayerHit.CRITICAL);
            }
            else if (raycastInfo.transform != null && raycastInfo.transform.CompareTag("Barricade") && DAMAGE_BARRICADE > 1f)
            {
                BarricadeDrop barricadeDrop = BarricadeDrop.FindByRootFast(raycastInfo.transform);
                if (barricadeDrop != null)
                {
                    ItemBarricadeAsset itemBarricadeAsset = barricadeDrop.asset;
                    if (itemBarricadeAsset != null && itemBarricadeAsset.canBeDamaged && itemBarricadeAsset.isVulnerable)
                    {
                        PlayerUI.hitmark(0, raycastInfo.point, worldspace: false, EPlayerHit.BUILD);
                    }
                }
            }
            else if (raycastInfo.transform != null && raycastInfo.transform.CompareTag("Structure") && DAMAGE_STRUCTURE > 1f)
            {
                StructureDrop structureDrop = StructureDrop.FindByRootFast(raycastInfo.transform);
                if (structureDrop != null)
                {
                    ItemStructureAsset itemStructureAsset = structureDrop.asset;
                    if (itemStructureAsset != null && itemStructureAsset.canBeDamaged && itemStructureAsset.isVulnerable)
                    {
                        PlayerUI.hitmark(0, raycastInfo.point, worldspace: false, EPlayerHit.BUILD);
                    }
                }
            }
            else if (raycastInfo.vehicle != null && !raycastInfo.vehicle.isDead && DAMAGE_VEHICLE > 1f)
            {
                if (raycastInfo.vehicle.asset != null && raycastInfo.vehicle.canBeDamaged && raycastInfo.vehicle.asset.isVulnerable)
                {
                    PlayerUI.hitmark(0, raycastInfo.point, worldspace: false, EPlayerHit.BUILD);
                }
            }
            else if (raycastInfo.transform != null && raycastInfo.transform.CompareTag("Resource") && DAMAGE_RESOURCE > 1f)
            {
                if (ResourceManager.tryGetRegion(raycastInfo.transform, out var x, out var y, out var index))
                {
                    ResourceSpawnpoint resourceSpawnpoint = ResourceManager.getResourceSpawnpoint(x, y, index);
                    if (resourceSpawnpoint != null && !resourceSpawnpoint.isDead && resourceSpawnpoint.asset.vulnerableToFists)
                    {
                        PlayerUI.hitmark(0, raycastInfo.point, worldspace: false, EPlayerHit.BUILD);
                    }
                }
            }
            else if (raycastInfo.transform != null && DAMAGE_OBJECT > 1f)
            {
                InteractableObjectRubble componentInParent = raycastInfo.transform.GetComponentInParent<InteractableObjectRubble>();
                if (componentInParent != null)
                {
                    raycastInfo.transform = componentInParent.transform;
                    raycastInfo.section = componentInParent.getSection(raycastInfo.collider.transform);
                    if (!componentInParent.isSectionDead(raycastInfo.section) && componentInParent.asset.rubbleBladeID == 0 && componentInParent.asset.rubbleIsVulnerable)
                    {
                        PlayerUI.hitmark(0, raycastInfo.point, worldspace: false, EPlayerHit.BUILD);
                    }
                }
            }
            base.player.input.sendRaycast(raycastInfo, ERaycastInfoUsage.Punch);
        }
        switch (mode)
        {
        case EPlayerPunch.LEFT:
            base.player.animator.play("Punch_Left", smooth: false);
            if (Provider.isServer)
            {
                base.player.animator.sendGesture(EPlayerGesture.PUNCH_LEFT, all: false);
            }
            break;
        case EPlayerPunch.RIGHT:
            base.player.animator.play("Punch_Right", smooth: false);
            if (Provider.isServer)
            {
                base.player.animator.sendGesture(EPlayerGesture.PUNCH_RIGHT, all: false);
            }
            break;
        }
        OnPunch_Global.TryInvoke("OnPunch_Global", this, mode);
        if (!Provider.isServer || !base.player.input.hasInputs())
        {
            return;
        }
        InputInfo input = base.player.input.getInput(doOcclusionCheck: true, ERaycastInfoUsage.Punch);
        if (input == null || (input.point - base.player.look.aim.position).sqrMagnitude > 36f)
        {
            return;
        }
        if (!string.IsNullOrEmpty(input.materialName))
        {
            DamageTool.ServerSpawnLegacyImpact(input.point, input.normal, input.materialName, input.colliderTransform, base.channel.EnumerateClients_WithinSphereOrOwner(input.point, EffectManager.SMALL));
        }
        EPlayerKill kill = EPlayerKill.NONE;
        uint xp = 0u;
        float num = 1f;
        num *= 1f + base.channel.owner.player.skills.mastery(0, 0) * 0.5f;
        if (input.type == ERaycastInfoType.PLAYER)
        {
            lastPunching = Time.realtimeSinceStartup;
            if (input.player != null && DamageTool.isPlayerAllowedToDamagePlayer(base.player, input.player))
            {
                DamagePlayerParameters parameters = DamagePlayerParameters.make(input.player, EDeathCause.PUNCH, input.direction, DAMAGE_PLAYER_MULTIPLIER, input.limb);
                parameters.killer = base.channel.owner.playerID.steamID;
                parameters.times = num;
                parameters.respectArmor = true;
                parameters.trackKill = true;
                if (base.player.input.IsUnderFakeLagPenalty)
                {
                    parameters.times *= Provider.configData.Server.Fake_Lag_Damage_Penalty_Multiplier;
                }
                DamageTool.damagePlayer(parameters, out kill);
            }
        }
        else if (input.type == ERaycastInfoType.ZOMBIE)
        {
            if (input.zombie != null)
            {
                IDamageMultiplier dAMAGE_ZOMBIE_MULTIPLIER = DAMAGE_ZOMBIE_MULTIPLIER;
                DamageZombieParameters parameters2 = DamageZombieParameters.make(input.zombie, input.direction, dAMAGE_ZOMBIE_MULTIPLIER, input.limb);
                parameters2.times = num;
                parameters2.allowBackstab = true;
                parameters2.respectArmor = true;
                parameters2.instigator = base.player;
                DamageTool.damageZombie(parameters2, out kill, out xp);
                if (base.player.movement.nav != byte.MaxValue)
                {
                    input.zombie.alert(base.transform.position, isStartling: true);
                }
            }
        }
        else if (input.type == ERaycastInfoType.ANIMAL)
        {
            lastPunching = Time.realtimeSinceStartup;
            if (input.animal != null)
            {
                IDamageMultiplier dAMAGE_ANIMAL_MULTIPLIER = DAMAGE_ANIMAL_MULTIPLIER;
                DamageAnimalParameters parameters3 = DamageAnimalParameters.make(input.animal, input.direction, dAMAGE_ANIMAL_MULTIPLIER, input.limb);
                parameters3.times = num;
                parameters3.instigator = base.player;
                DamageTool.damageAnimal(parameters3, out kill, out xp);
                input.animal.alertDamagedFromPoint(base.transform.position);
            }
        }
        else if (input.type == ERaycastInfoType.VEHICLE)
        {
            lastPunching = Time.realtimeSinceStartup;
            if (input.vehicle != null && input.vehicle.asset != null && input.vehicle.canBeDamaged && input.vehicle.asset.isVulnerable)
            {
                DamageTool.damage(input.vehicle, damageTires: false, Vector3.zero, isRepairing: false, DAMAGE_VEHICLE, num * Provider.modeConfigData.Vehicles.Melee_Damage_Multiplier, canRepair: true, out kill, base.channel.owner.playerID.steamID, EDamageOrigin.Punch);
            }
        }
        else if (input.type == ERaycastInfoType.BARRICADE)
        {
            lastPunching = Time.realtimeSinceStartup;
            if (input.transform != null && input.transform.CompareTag("Barricade"))
            {
                BarricadeDrop barricadeDrop2 = BarricadeDrop.FindByRootFast(input.transform);
                if (barricadeDrop2 != null)
                {
                    ItemBarricadeAsset itemBarricadeAsset2 = barricadeDrop2.asset;
                    if (itemBarricadeAsset2 != null && itemBarricadeAsset2.canBeDamaged && itemBarricadeAsset2.isVulnerable)
                    {
                        DamageTool.damage(input.transform, isRepairing: false, DAMAGE_BARRICADE, num * Provider.modeConfigData.Barricades.Melee_Damage_Multiplier, out kill, base.channel.owner.playerID.steamID, EDamageOrigin.Punch);
                    }
                }
            }
        }
        else if (input.type == ERaycastInfoType.STRUCTURE)
        {
            lastPunching = Time.realtimeSinceStartup;
            if (input.transform != null && input.transform.CompareTag("Structure"))
            {
                StructureDrop structureDrop2 = StructureDrop.FindByRootFast(input.transform);
                if (structureDrop2 != null)
                {
                    ItemStructureAsset itemStructureAsset2 = structureDrop2.asset;
                    if (itemStructureAsset2 != null && itemStructureAsset2.canBeDamaged && itemStructureAsset2.isVulnerable)
                    {
                        DamageTool.damage(input.transform, isRepairing: false, input.direction, DAMAGE_STRUCTURE, num * Provider.modeConfigData.Structures.Melee_Damage_Multiplier, out kill, base.channel.owner.playerID.steamID, EDamageOrigin.Punch);
                    }
                }
            }
        }
        else if (input.type == ERaycastInfoType.RESOURCE)
        {
            lastPunching = Time.realtimeSinceStartup;
            if (input.transform != null && input.transform.CompareTag("Resource") && ResourceManager.tryGetRegion(input.transform, out var x2, out var y2, out var index2))
            {
                ResourceSpawnpoint resourceSpawnpoint2 = ResourceManager.getResourceSpawnpoint(x2, y2, index2);
                if (resourceSpawnpoint2 != null && !resourceSpawnpoint2.isDead && resourceSpawnpoint2.asset.vulnerableToFists)
                {
                    DamageTool.damage(input.transform, input.direction, DAMAGE_RESOURCE, num, 1f, out kill, out xp, base.channel.owner.playerID.steamID, EDamageOrigin.Punch);
                }
            }
        }
        else if (input.type == ERaycastInfoType.OBJECT && input.transform != null && input.section < byte.MaxValue)
        {
            InteractableObjectRubble componentInParent2 = input.transform.GetComponentInParent<InteractableObjectRubble>();
            if (componentInParent2 != null && !componentInParent2.isSectionDead(input.section) && componentInParent2.asset.rubbleBladeID == 0 && componentInParent2.asset.rubbleIsVulnerable)
            {
                DamageTool.damage(componentInParent2.transform, input.direction, input.section, DAMAGE_OBJECT, num, out kill, out xp, base.channel.owner.playerID.steamID, EDamageOrigin.Punch);
            }
        }
        if (input.type != ERaycastInfoType.PLAYER && input.type != ERaycastInfoType.ZOMBIE && input.type != ERaycastInfoType.ANIMAL && !base.player.life.isAggressor)
        {
            float num2 = 2f + Provider.modeConfigData.Players.Ray_Aggressor_Distance;
            num2 *= num2;
            float ray_Aggressor_Distance = Provider.modeConfigData.Players.Ray_Aggressor_Distance;
            ray_Aggressor_Distance *= ray_Aggressor_Distance;
            Vector3 forward = base.player.look.aim.forward;
            for (int i = 0; i < Provider.clients.Count; i++)
            {
                if (Provider.clients[i] == base.channel.owner)
                {
                    continue;
                }
                Player player = Provider.clients[i].player;
                if (!(player == null))
                {
                    Vector3 vector = player.look.aim.position - base.player.look.aim.position;
                    Vector3 vector2 = Vector3.Project(vector, forward);
                    if (vector2.sqrMagnitude < num2 && (vector2 - vector).sqrMagnitude < ray_Aggressor_Distance)
                    {
                        base.player.life.markAggressive(force: false);
                    }
                }
            }
        }
        if (Level.info.type == ELevelType.HORDE)
        {
            if (input.zombie != null)
            {
                if (input.limb == ELimb.SKULL)
                {
                    base.player.skills.askPay(10u);
                }
                else
                {
                    base.player.skills.askPay(5u);
                }
            }
            if (kill == EPlayerKill.ZOMBIE)
            {
                if (input.limb == ELimb.SKULL)
                {
                    base.player.skills.askPay(50u);
                }
                else
                {
                    base.player.skills.askPay(25u);
                }
            }
        }
        else
        {
            if (kill == EPlayerKill.PLAYER && Level.info.type == ELevelType.ARENA)
            {
                base.player.skills.askPay(100u);
            }
            base.player.sendStat(kill);
            if (xp != 0)
            {
                base.player.skills.askPay(xp);
            }
        }
    }

    private bool simulate_MustDequip()
    {
        if (base.player.stance.stance == EPlayerStance.DRIVING && !isTurret)
        {
            return true;
        }
        if (base.player.stance.stance == EPlayerStance.CLIMB)
        {
            return !isBusy;
        }
        if ((base.player.stance.isSubmerged || base.player.stance.stance == EPlayerStance.SWIM) && asset != null && !asset.canUseUnderwater)
        {
            return !isBusy;
        }
        return false;
    }

    private void simulate_UseableInput(uint simulation, bool inputPrimary, bool inputSecondary, bool inputSteady)
    {
        if (inputPrimary != lastPrimary)
        {
            lastPrimary = inputPrimary;
            if (isEquipped)
            {
                if (inputPrimary)
                {
                    try
                    {
                        useable.startPrimary();
                    }
                    catch (Exception e)
                    {
                        UnturnedLog.warn("{0} raised an exception during simulate.startPrimary:", asset);
                        UnturnedLog.exception(e);
                    }
                }
                else
                {
                    try
                    {
                        useable.stopPrimary();
                    }
                    catch (Exception e2)
                    {
                        UnturnedLog.warn("{0} raised an exception during simulate.stopPrimary:", asset);
                        UnturnedLog.exception(e2);
                    }
                }
            }
        }
        if (inputSecondary != lastSecondary)
        {
            lastSecondary = inputSecondary;
            if (isSelected && isEquipped)
            {
                if (inputSecondary)
                {
                    try
                    {
                        useable.startSecondary();
                    }
                    catch (Exception e3)
                    {
                        UnturnedLog.warn("{0} raised an exception during useable.startSecondary:", asset);
                        UnturnedLog.exception(e3);
                    }
                }
                else
                {
                    try
                    {
                        useable.stopSecondary();
                    }
                    catch (Exception e4)
                    {
                        UnturnedLog.warn("{0} raised an exception during useable.stopSecondary:", asset);
                        UnturnedLog.exception(e4);
                    }
                }
            }
        }
        if (isSelected && isEquipped)
        {
            try
            {
                useable.simulate(simulation, inputSteady);
            }
            catch (Exception e5)
            {
                UnturnedLog.warn("{0} raised an exception during useable.simulate:", asset);
                UnturnedLog.exception(e5);
            }
        }
        if (Provider.isServer && isSelected && isEquipped && asset != null && asset.shouldDeleteAtZeroQuality && quality == 0)
        {
            use();
        }
    }

    private void simulate_PunchInput(uint simulation, bool inputPrimary, bool inputSecondary)
    {
        if (inputPrimary != lastPrimary)
        {
            lastPrimary = inputPrimary;
            if (!isBusy && base.player.stance.stance != EPlayerStance.PRONE && inputPrimary && simulation - lastPunch > 5)
            {
                lastPunch = simulation;
                punch(EPlayerPunch.LEFT);
            }
        }
        if (inputSecondary != lastSecondary)
        {
            lastSecondary = inputSecondary;
            if (!isBusy && base.player.stance.stance != EPlayerStance.PRONE && inputSecondary && simulation - lastPunch > 5)
            {
                lastPunch = simulation;
                punch(EPlayerPunch.RIGHT);
            }
        }
    }

    public void simulate(uint simulation, bool inputPrimary, bool inputSecondary, bool inputSteady)
    {
        if (simulate_MustDequip())
        {
            if (isSelected && Provider.isServer)
            {
                dequip();
            }
        }
        else
        {
            if (Time.realtimeSinceStartup - lastEquip < 0.1f || base.player.life.isDead)
            {
                return;
            }
            if (base.player.movement.isSafe)
            {
                if (asset == null)
                {
                    if (base.player.movement.isSafeInfo == null || base.player.movement.isSafeInfo.noWeapons)
                    {
                        return;
                    }
                }
                else if (base.player.movement.isSafeInfo == null || !asset.canBeUsedInSafezone(base.player.movement.isSafeInfo, base.channel.owner.isAdmin))
                {
                    inputPrimary = false;
                    inputSecondary = false;
                }
            }
            if (Level.info != null && Level.info.type != 0 && asset == null)
            {
                return;
            }
            if ((base.player.stance.isSubmerged || base.player.stance.stance == EPlayerStance.SWIM) && asset == null)
            {
                lastPunch = simulation;
            }
            else if (base.player.animator.gesture != EPlayerGesture.ARREST_START)
            {
                if (isTurret && (base.player.movement.getVehicle() == null || !base.player.movement.getVehicle().canUseTurret))
                {
                    inputPrimary = false;
                    inputSecondary = false;
                }
                if (isSelected)
                {
                    simulate_UseableInput(simulation, inputPrimary, inputSecondary, inputSteady);
                }
                else
                {
                    simulate_PunchInput(simulation, inputPrimary, inputSecondary);
                }
            }
        }
    }

    public void tock(uint clock)
    {
        if (isSelected && isEquipped)
        {
            try
            {
                useable.tock(clock);
            }
            catch (Exception e)
            {
                UnturnedLog.warn("{0} raised an exception during tock.tock:", asset);
                UnturnedLog.exception(e);
            }
        }
    }

    private void updateVision()
    {
        if (hasVision && base.player.clothing.glassesState[0] != 0)
        {
            if (base.player.clothing.glassesAsset.vision == ELightingVision.HEADLAMP)
            {
                base.player.enableHeadlamp(base.player.clothing.glassesAsset.lightConfig);
                if (base.channel.isOwner)
                {
                    LevelLighting.vision = ELightingVision.NONE;
                    LevelLighting.updateLighting();
                    PlayerLifeUI.updateGrayscale();
                }
            }
            else
            {
                base.player.disableHeadlamp();
                if (base.channel.isOwner)
                {
                    LevelLighting.vision = ((base.player.look.perspective == EPlayerPerspective.FIRST) ? base.player.clothing.glassesAsset.vision : ELightingVision.NONE);
                    LevelLighting.nightvisionColor = base.player.clothing.glassesAsset.nightvisionColor;
                    LevelLighting.nightvisionFogIntensity = base.player.clothing.glassesAsset.nightvisionFogIntensity;
                    LevelLighting.updateLighting();
                    PlayerLifeUI.updateGrayscale();
                }
            }
            base.player.updateGlassesLights(on: true);
        }
        else
        {
            base.player.disableHeadlamp();
            if (base.channel.isOwner)
            {
                LevelLighting.vision = ELightingVision.NONE;
                LevelLighting.updateLighting();
                PlayerLifeUI.updateGrayscale();
            }
            base.player.updateGlassesLights(on: false);
        }
    }

    private void onVisionUpdated(bool isViewing)
    {
        if (isViewing)
        {
            warp = UnityEngine.Random.value < 0.25f;
        }
        else
        {
            warp = false;
        }
    }

    private void onPerspectiveUpdated(EPlayerPerspective perspective)
    {
        if (hasVision)
        {
            updateVision();
        }
    }

    private void onGlassesUpdated(ushort id, byte quality, byte[] state)
    {
        hasVision = id != 0 && base.player.clothing.glassesAsset != null && base.player.clothing.glassesAsset.vision != ELightingVision.NONE;
        updateVision();
    }

    private void OnVisualToggleChanged(PlayerClothing sender)
    {
        if (hasVision)
        {
            updateVision();
        }
    }

    private void onLifeUpdated(bool isDead)
    {
        if (!isDead)
        {
            return;
        }
        if (base.player.life.wasPvPDeath ? Provider.modeConfigData.Players.Lose_Weapons_PvP : Provider.modeConfigData.Players.Lose_Weapons_PvE)
        {
            for (byte b = 0; b < PlayerInventory.SLOTS; b = (byte)(b + 1))
            {
                updateSlot(b, 0, new byte[0]);
            }
        }
        if (Provider.isServer)
        {
            dequip();
        }
        isBusy = false;
        canEquip = true;
        _equippedPage = byte.MaxValue;
        _equipped_x = byte.MaxValue;
        _equipped_y = byte.MaxValue;
    }

    private void bindHotkey(byte button)
    {
        if (button < PlayerInventory.SLOTS || !PlayerDashboardInventoryUI.active)
        {
            return;
        }
        byte b = (byte)(button - 2);
        if (PlayerDashboardInventoryUI.selectedPage >= PlayerInventory.SLOTS && PlayerDashboardInventoryUI.selectedPage < PlayerInventory.STORAGE)
        {
            if (ItemTool.checkUseable(PlayerDashboardInventoryUI.selectedPage, PlayerDashboardInventoryUI.selectedJar.item.id))
            {
                HotkeyInfo obj = hotkeys[b];
                obj.id = PlayerDashboardInventoryUI.selectedJar.item.id;
                obj.page = PlayerDashboardInventoryUI.selectedPage;
                obj.x = PlayerDashboardInventoryUI.selected_x;
                obj.y = PlayerDashboardInventoryUI.selected_y;
                PlayerDashboardInventoryUI.closeSelection();
                ClearDuplicateHotkeys(b);
                if (onHotkeysUpdated != null)
                {
                    onHotkeysUpdated();
                }
            }
        }
        else if (PlayerDashboardInventoryUI.selectedPage == byte.MaxValue)
        {
            HotkeyInfo obj2 = hotkeys[b];
            obj2.id = 0;
            obj2.page = byte.MaxValue;
            obj2.x = byte.MaxValue;
            obj2.y = byte.MaxValue;
            if (onHotkeysUpdated != null)
            {
                onHotkeysUpdated();
            }
        }
    }

    private void hotkey(byte button)
    {
        if (PlayerUI.window.showCursor)
        {
            bindHotkey(button);
        }
        else
        {
            if (isBusy)
            {
                return;
            }
            if (button < PlayerInventory.SLOTS)
            {
                ItemJar item = base.player.inventory.getItem(button, 0);
                if (item != null)
                {
                    equip(button, item.x, item.y);
                }
                else if (isSelected && isEquipped)
                {
                    dequip();
                }
                return;
            }
            byte b = (byte)(button - 2);
            HotkeyInfo hotkeyInfo = hotkeys[b];
            if (hotkeyInfo.id != 0)
            {
                equip(hotkeyInfo.page, hotkeyInfo.x, hotkeyInfo.y);
            }
            else if (isSelected && isEquipped)
            {
                dequip();
            }
        }
    }

    private void Update()
    {
        if (base.channel.isOwner)
        {
            if (!PlayerUI.window.showCursor && !PlayerDashboardInventoryUI.WasEventConsumed && !base.player.workzone.isBuilding && (base.player.movement.getVehicle() == null || base.player.look.perspective == EPlayerPerspective.FIRST))
            {
                _primary = prim || InputEx.GetKey(ControlsSettings.primary);
                if (ControlsSettings.aiming == EControlMode.TOGGLE && asset != null && (asset.type == EItemType.GUN || asset.type == EItemType.OPTIC))
                {
                    if ((sec || InputEx.GetKey(ControlsSettings.secondary)) != flipSecondary)
                    {
                        flipSecondary = sec || InputEx.GetKey(ControlsSettings.secondary);
                        if (flipSecondary)
                        {
                            _secondary = !secondary;
                        }
                    }
                }
                else
                {
                    _secondary = sec || InputEx.GetKey(ControlsSettings.secondary);
                    flipSecondary = secondary;
                }
                prim = false;
                sec = false;
                if (warp)
                {
                    bool flag = primary;
                    _primary = secondary;
                    _secondary = flag;
                }
                if (PlayerManager.IsClientUnderFakeLagPenalty)
                {
                    _primary = false;
                    _secondary = false;
                }
            }
            else
            {
                _primary = false;
                _secondary = false;
            }
        }
        wasTryingToSelect = false;
        if (base.channel.isOwner)
        {
            if (!PlayerUI.window.showCursor && !base.player.workzone.isBuilding)
            {
                if (InputEx.GetKeyDown(ControlsSettings.vision) && hasVision && !PlayerLifeUI.scopeOverlay.isVisible)
                {
                    SendToggleVisionRequest.Invoke(GetNetId(), ENetReliability.Unreliable);
                }
                if (InputEx.GetKey(ControlsSettings.primary))
                {
                    prim = true;
                }
                if (InputEx.GetKey(ControlsSettings.secondary))
                {
                    sec = true;
                }
                if (InputEx.GetKeyDown(ControlsSettings.dequip) && isSelected && !isBusy && isEquipped)
                {
                    dequip();
                }
            }
            for (byte b = 0; b < 10; b = (byte)(b + 1))
            {
                if (InputEx.GetKeyDown(ControlsSettings.getEquipmentHotbarKeyCode(b)))
                {
                    hotkey(b);
                }
            }
        }
        if (isSelected)
        {
            try
            {
                useable.tick();
            }
            catch (Exception e)
            {
                UnturnedLog.warn("{0} raised an exception during Update.tick:", asset);
                UnturnedLog.exception(e);
            }
        }
    }

    internal void InitializePlayer()
    {
        hasVision = base.player.clothing.glassesAsset != null && base.player.clothing.glassesAsset.vision != ELightingVision.NONE;
        updateVision();
        thirdSlots = new Transform[PlayerInventory.SLOTS];
        thirdSkinneds = new bool[PlayerInventory.SLOTS];
        tempThirdMeshes = new List<Mesh>[PlayerInventory.SLOTS];
        for (int i = 0; i < tempThirdMeshes.Length; i++)
        {
            tempThirdMeshes[i] = new List<Mesh>(4);
        }
        tempThirdMaterials = new Material[PlayerInventory.SLOTS];
        thirdMythics = new MythicLockee[PlayerInventory.SLOTS];
        tempThirdMesh = new List<Mesh>(4);
        if (base.channel.isOwner && base.player.character != null)
        {
            tempFirstMesh = new List<Mesh>(4);
            tempCharacterMesh = new List<Mesh>(4);
            characterSlots = new Transform[PlayerInventory.SLOTS];
            characterSkinneds = new bool[PlayerInventory.SLOTS];
            tempCharacterMeshes = new List<Mesh>[PlayerInventory.SLOTS];
            for (int j = 0; j < tempCharacterMeshes.Length; j++)
            {
                tempCharacterMeshes[j] = new List<Mesh>(4);
            }
            tempCharacterMaterials = new Material[PlayerInventory.SLOTS];
            characterMythics = new MythicLockee[PlayerInventory.SLOTS];
        }
        warp = false;
        _equippedPage = byte.MaxValue;
        _equipped_x = byte.MaxValue;
        _equipped_y = byte.MaxValue;
        isBusy = false;
        canEquip = true;
        if (base.player.third != null)
        {
            _thirdPrimaryMeleeSlot = base.player.animator.thirdSkeleton.Find("Spine").Find("Primary_Melee");
            _thirdPrimaryLargeGunSlot = base.player.animator.thirdSkeleton.Find("Spine").Find("Primary_Large_Gun");
            _thirdPrimarySmallGunSlot = base.player.animator.thirdSkeleton.Find("Spine").Find("Primary_Small_Gun");
            _thirdSecondaryMeleeSlot = base.player.animator.thirdSkeleton.Find("Right_Hip").Find("Right_Leg").Find("Secondary_Melee");
            _thirdSecondaryGunSlot = base.player.animator.thirdSkeleton.Find("Right_Hip").Find("Right_Leg").Find("Secondary_Gun");
        }
        if (base.channel.isOwner)
        {
            _characterPrimaryMeleeSlot = base.player.character.Find("Skeleton").Find("Spine").Find("Primary_Melee");
            _characterPrimaryLargeGunSlot = base.player.character.Find("Skeleton").Find("Spine").Find("Primary_Large_Gun");
            _characterPrimarySmallGunSlot = base.player.character.Find("Skeleton").Find("Spine").Find("Primary_Small_Gun");
            _characterSecondaryMeleeSlot = base.player.character.Find("Skeleton").Find("Right_Hip").Find("Right_Leg")
                .Find("Secondary_Melee");
            _characterSecondaryGunSlot = base.player.character.Find("Skeleton").Find("Right_Hip").Find("Right_Leg")
                .Find("Secondary_Gun");
        }
        if (base.player.first != null)
        {
            _firstLeftHook = base.player.animator.firstSkeleton.Find("Spine").Find("Left_Shoulder").Find("Left_Arm")
                .Find("Left_Hand")
                .Find("Left_Hook");
            _firstRightHook = base.player.animator.firstSkeleton.Find("Spine").Find("Right_Shoulder").Find("Right_Arm")
                .Find("Right_Hand")
                .Find("Right_Hook");
        }
        if (base.player.third != null)
        {
            _thirdLeftHook = base.player.animator.thirdSkeleton.Find("Spine").Find("Left_Shoulder").Find("Left_Arm")
                .Find("Left_Hand")
                .Find("Left_Hook");
            _thirdRightHook = base.player.animator.thirdSkeleton.Find("Spine").Find("Right_Shoulder").Find("Right_Arm")
                .Find("Right_Hand")
                .Find("Right_Hook");
        }
        if (base.channel.isOwner && base.player.character != null)
        {
            _characterLeftHook = base.player.character.transform.Find("Skeleton").Find("Spine").Find("Left_Shoulder")
                .Find("Left_Arm")
                .Find("Left_Hand")
                .Find("Left_Hook");
            _characterRightHook = base.player.character.transform.Find("Skeleton").Find("Spine").Find("Right_Shoulder")
                .Find("Right_Arm")
                .Find("Right_Hand")
                .Find("Right_Hook");
        }
        if (base.channel.isOwner || Provider.isServer)
        {
            PlayerLife life = base.player.life;
            life.onVisionUpdated = (VisionUpdated)Delegate.Combine(life.onVisionUpdated, new VisionUpdated(onVisionUpdated));
        }
        PlayerClothing clothing = base.player.clothing;
        clothing.onGlassesUpdated = (GlassesUpdated)Delegate.Combine(clothing.onGlassesUpdated, new GlassesUpdated(onGlassesUpdated));
        base.player.clothing.VisualToggleChanged += OnVisualToggleChanged;
        if (base.channel.isOwner)
        {
            _hotkeys = new HotkeyInfo[8];
            for (byte b = 0; b < hotkeys.Length; b = (byte)(b + 1))
            {
                hotkeys[b] = new HotkeyInfo();
            }
            load();
            PlayerLook look = base.player.look;
            look.onPerspectiveUpdated = (PerspectiveUpdated)Delegate.Combine(look.onPerspectiveUpdated, new PerspectiveUpdated(onPerspectiveUpdated));
        }
        PlayerLife life2 = base.player.life;
        life2.onLifeUpdated = (LifeUpdated)Delegate.Combine(life2.onLifeUpdated, new LifeUpdated(onLifeUpdated));
    }

    private void OnDestroy()
    {
        if (useable != null)
        {
            try
            {
                useable.dequip();
            }
            catch (Exception e)
            {
                UnturnedLog.warn("{0} raised an exception during OnDestroy.dequip:", asset);
                UnturnedLog.exception(e);
            }
            _useable.ReleaseNetId();
        }
        if (base.channel.isOwner)
        {
            save();
        }
    }

    private string GetItemHotkeysFilePath()
    {
        return "/Worlds/Hotkeys/Equip_" + Provider.currentServerInfo.ip + "_" + Provider.currentServerInfo.queryPort + "_" + Characters.selected + ".dat";
    }

    private void LogItemHotkeys(string message)
    {
        UnturnedLog.info(message);
    }

    private void load()
    {
        string itemHotkeysFilePath = GetItemHotkeysFilePath();
        if (ReadWrite.fileExists(itemHotkeysFilePath, useCloud: false))
        {
            Block block = ReadWrite.readBlock(itemHotkeysFilePath, useCloud: false, 0);
            block.readByte();
            for (byte b = 0; b < hotkeys.Length; b = (byte)(b + 1))
            {
                HotkeyInfo obj = hotkeys[b];
                obj.id = block.readUInt16();
                obj.page = block.readByte();
                obj.x = block.readByte();
                obj.y = block.readByte();
            }
            LogItemHotkeys("Loaded item hotkeys");
        }
        else
        {
            LogItemHotkeys("No item hotkeys to load");
        }
    }

    private void save()
    {
        if (hotkeys == null)
        {
            LogItemHotkeys("Ignoring request to save item hotkeys because they were not loaded yet");
            return;
        }
        bool flag = false;
        for (byte b = 0; b < hotkeys.Length; b = (byte)(b + 1))
        {
            HotkeyInfo hotkeyInfo = hotkeys[b];
            if (hotkeyInfo.id != 0 || (hotkeyInfo.page != byte.MaxValue && hotkeyInfo.x != byte.MaxValue && hotkeyInfo.y != byte.MaxValue))
            {
                flag = true;
                break;
            }
        }
        string itemHotkeysFilePath = GetItemHotkeysFilePath();
        if (!flag)
        {
            if (ReadWrite.fileExists(itemHotkeysFilePath, useCloud: false))
            {
                LogItemHotkeys("No item hotkeys to save, deleting old item hotkeys file");
                ReadWrite.deleteFile(itemHotkeysFilePath, useCloud: false);
            }
            else
            {
                LogItemHotkeys("No item hotkeys to save");
            }
            return;
        }
        Block block = new Block();
        block.writeByte(SAVEDATA_VERSION);
        for (byte b2 = 0; b2 < hotkeys.Length; b2 = (byte)(b2 + 1))
        {
            HotkeyInfo hotkeyInfo2 = hotkeys[b2];
            block.writeUInt16(hotkeyInfo2.id);
            block.writeByte(hotkeyInfo2.page);
            block.writeByte(hotkeyInfo2.x);
            block.writeByte(hotkeyInfo2.y);
        }
        ReadWrite.writeBlock(itemHotkeysFilePath, useCloud: false, block);
        LogItemHotkeys("Saved item hotkeys");
    }
}
