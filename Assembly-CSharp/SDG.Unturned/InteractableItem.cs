using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableItem : Interactable
{
    public Item item;

    public ItemJar jar;

    public ItemAsset asset;

    private bool wasReset;

    public override void use()
    {
        ItemManager.takeItem(base.transform.parent, byte.MaxValue, byte.MaxValue, 0, byte.MaxValue);
    }

    public override bool checkHighlight(out Color color)
    {
        color = ItemTool.getRarityColorHighlight(asset.rarity);
        return true;
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        message = EPlayerMessage.ITEM;
        text = asset.itemName;
        color = ItemTool.getRarityColorUI(asset.rarity);
        return true;
    }

    public void clampRange()
    {
        if (!wasReset && (base.transform.position - base.transform.parent.position).sqrMagnitude > 400f)
        {
            base.transform.position = base.transform.parent.position;
            wasReset = true;
            ItemManager.clampedItems.RemoveFast(this);
            Object.Destroy(GetComponent<Rigidbody>());
        }
    }

    private void OnEnable()
    {
        ItemManager.clampedItems.Add(this);
    }

    private void OnDisable()
    {
        if (!wasReset)
        {
            ItemManager.clampedItems.RemoveFast(this);
        }
    }
}
