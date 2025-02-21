using System;
using UnityEngine;
using UnityEngine.Events;
using Unturned.UnityEx;

namespace SDG.Unturned;

/// <summary>
/// Can be added to gun item game objects (including children) to receive events.
/// </summary>
[AddComponentMenu("Unturned/Gun Attachment Event Hook")]
public class GunAttachmentEventHook : MonoBehaviour
{
    /// <summary>
    /// Nelson 2025-02-04: Gun attachment slots are currently hard-coded, but if that changes this could be updated
    /// with a "custom" option.
    /// </summary>
    public enum ESlot
    {
        Sight,
        Tactical,
        Grip,
        Barrel,
        Magazine
    }

    public enum EAssetChangeBehavior
    {
        /// <summary>
        /// If emptiness of slot doesn't change (attachment replaced), do nothing.
        /// </summary>
        Ignore,
        /// <summary>
        /// In addition to regular Attached and Detached events, if the item asset in the slot changes invoke
        /// Detached then Attached.
        /// </summary>
        InvokeDetachedThenAttached
    }

    /// <summary>
    /// Which attachment type to monitor.
    /// </summary>
    public ESlot Slot;

    /// <summary>
    /// Optional. If set, only consider item matching this GUID. I.e., slot is considered empty if attached item
    /// has a different asset GUID.
    /// </summary>
    public string AssetGuidFilter;

    /// <summary>
    /// If true, AssetGuidFilter passes when item in slot *doesn't* match GUID.
    /// </summary>
    public bool InvertFilter;

    /// <summary>
    /// Invoked both when:
    /// 1. Gun is first equipped and an item is already present in the slot.
    /// 2. An item is added to the slot.
    /// </summary>
    public UnityEvent OnItemAttached;

    /// <summary>
    /// Invoked both when:
    /// 1. Gun is first equipped and the slot is empty.
    /// 2. An item is removed from the slot.
    /// </summary>
    public UnityEvent OnItemDetached;

    /// <summary>
    /// Controls whether events are invoked when asset in slot changes.
    /// </summary>
    public EAssetChangeBehavior AssetChangeBehavior;

    private Asset assetInSlot;

    private bool hasGuidFilter;

    private Guid parsedGuid;

    internal void InitializeEventHook(Attachments attachments)
    {
        if (string.IsNullOrEmpty(AssetGuidFilter))
        {
            hasGuidFilter = false;
        }
        else if (Guid.TryParse(AssetGuidFilter, out parsedGuid))
        {
            hasGuidFilter = true;
        }
        else
        {
            hasGuidFilter = false;
            UnturnedLog.warn("{0} unable to parse asset guid filter \"{1}\"", base.transform.GetSceneHierarchyPath(), AssetGuidFilter);
        }
        assetInSlot = GetAssetInSlot(attachments);
        if (assetInSlot != null)
        {
            OnItemAttached?.TryInvoke(this);
        }
        else
        {
            OnItemDetached?.TryInvoke(this);
        }
    }

    internal void UpdateEventHook(Attachments attachments)
    {
        Asset asset = GetAssetInSlot(attachments);
        if (assetInSlot == asset)
        {
            return;
        }
        bool num = assetInSlot != null;
        bool flag = asset != null;
        if (num != flag)
        {
            if (flag)
            {
                OnItemAttached?.TryInvoke(this);
            }
            else
            {
                OnItemDetached?.TryInvoke(this);
            }
        }
        else if (flag && AssetChangeBehavior == EAssetChangeBehavior.InvokeDetachedThenAttached)
        {
            OnItemDetached?.TryInvoke(this);
            OnItemAttached?.TryInvoke(this);
        }
        assetInSlot = asset;
    }

    private Asset GetAssetInSlot(Attachments attachments)
    {
        Asset asset = Slot switch
        {
            ESlot.Sight => attachments.sightAsset, 
            ESlot.Tactical => attachments.tacticalAsset, 
            ESlot.Grip => attachments.gripAsset, 
            ESlot.Barrel => attachments.barrelAsset, 
            ESlot.Magazine => attachments.magazineAsset, 
            _ => null, 
        };
        if (asset != null && hasGuidFilter)
        {
            bool flag = asset.GUID == parsedGuid;
            if (InvertFilter)
            {
                flag = !flag;
            }
            if (!flag)
            {
                asset = null;
            }
        }
        return asset;
    }
}
