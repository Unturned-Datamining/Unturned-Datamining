using UnityEngine;

namespace SDG.Unturned;

public class SleekSelectedBlueprintItem : SleekWrapper
{
    internal BlueprintStatus blueprintStatus;

    private ItemAsset currentAsset;

    private ISleekBox backgroundBox;

    private SleekItemIcon itemImage;

    private ISleekLabel nameLabel;

    private ISleekLabel descriptionLabel;

    internal void SetInputItem(BlueprintSupply config, BlueprintInputItemStatus status, int index)
    {
        ItemAsset itemAsset = config.FindItemAsset();
        PlayerInventorySearchResultV2? playerInventorySearchResultV = null;
        if (status.searchResults.Count > 0)
        {
            playerInventorySearchResultV = status.searchResults[0];
        }
        Item item = playerInventorySearchResultV?.Jar?.item ?? null;
        byte quality;
        byte[] state;
        if (item != null)
        {
            quality = item.quality;
            state = item.state;
        }
        else
        {
            quality = 100;
            state = itemAsset.getState(isFull: false);
        }
        SetItemAsset(itemAsset, quality, state);
        descriptionLabel.IsVisible = true;
        descriptionLabel.TextColor = ESleekTint.FONT;
        if (blueprintStatus.blueprint.Operation == EBlueprintOperation.FillTargetItem && index == 0)
        {
            descriptionLabel.AllowRichText = false;
            descriptionLabel.Text = $"x{status.totalAmount}";
        }
        else
        {
            Local localization = PlayerDashboardCraftingUI.localization;
            if (status.isMissingRequiredAmount)
            {
                int num = config.amount - status.totalAmount;
                string text = localization.format("MissingAmount", num);
                text = RichTextUtil.wrapWithColor(text, OptionsSettings.badColor);
                descriptionLabel.AllowRichText = true;
                descriptionLabel.Text = localization.format("BlueprintAmountLabel_Missing", status.totalAmount, config.amount, text);
            }
            else
            {
                descriptionLabel.AllowRichText = false;
                descriptionLabel.Text = localization.format("BlueprintAmountLabel", status.totalAmount, config.amount);
            }
        }
        nameLabel.SizeOffset_Y = (descriptionLabel.IsVisible ? 30 : 50);
    }

    internal void SetOutputItem(BlueprintStatus blueprintStatus, BlueprintOutput output, int outputIndex)
    {
        ItemAsset itemAsset = output.FindItemAsset();
        byte quality;
        byte[] state;
        if (blueprintStatus.blueprint.transferState)
        {
            blueprintStatus.GetPreviewOutputTransferState(itemAsset, out quality, out state);
        }
        else
        {
            quality = 100;
            state = itemAsset.getState();
        }
        SetItemAsset(itemAsset, quality, state);
        if (output.amount > 1 || quality != 100)
        {
            string text = string.Empty;
            if (output.amount > 1)
            {
                text = $"x{output.amount}";
            }
            if (quality != 100)
            {
                if (text.Length > 0)
                {
                    text += " ";
                }
                Color qualityColor = ItemTool.getQualityColor((float)(int)quality / 100f);
                text += $"<color={Palette.hex(qualityColor)}>{quality}%</color>";
            }
            descriptionLabel.IsVisible = true;
            descriptionLabel.AllowRichText = true;
            descriptionLabel.Text = text;
            descriptionLabel.TextColor = ESleekTint.FONT;
        }
        else
        {
            descriptionLabel.IsVisible = false;
        }
        nameLabel.SizeScale_Y = (descriptionLabel.IsVisible ? 0.5f : 1f);
    }

    private void SetItemAsset(ItemAsset itemAsset, byte quality, byte[] state)
    {
        if (currentAsset != itemAsset)
        {
            currentAsset = itemAsset;
            itemImage.Clear();
        }
        itemImage.Refresh(itemAsset.id, quality, state, itemAsset, Mathf.RoundToInt(itemImage.SizeOffset_X), Mathf.RoundToInt(itemImage.SizeOffset_Y));
        nameLabel.TextColor = ItemTool.getRarityColorUI(itemAsset.rarity);
        nameLabel.Text = itemAsset.itemName;
    }

    public SleekSelectedBlueprintItem()
    {
        base.SizeOffset_Y = 50f;
        backgroundBox = Glazier.Get().CreateBox();
        backgroundBox.SizeScale_X = 1f;
        backgroundBox.SizeScale_Y = 1f;
        AddChild(backgroundBox);
        itemImage = new SleekItemIcon();
        itemImage.PositionOffset_X = 5f;
        itemImage.PositionOffset_Y = 5f;
        itemImage.SizeOffset_X = 40f;
        itemImage.SizeOffset_Y = 40f;
        AddChild(itemImage);
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.PositionOffset_X = 50f;
        nameLabel.SizeScale_X = 1f;
        nameLabel.SizeOffset_X = -50f;
        nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        nameLabel.TextAlignment = TextAnchor.MiddleLeft;
        AddChild(nameLabel);
        descriptionLabel = Glazier.Get().CreateLabel();
        descriptionLabel.PositionOffset_X = 50f;
        descriptionLabel.PositionOffset_Y = 20f;
        descriptionLabel.SizeScale_X = 1f;
        descriptionLabel.SizeOffset_X = -50f;
        descriptionLabel.SizeOffset_Y = 30f;
        descriptionLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        descriptionLabel.TextAlignment = TextAnchor.MiddleLeft;
        AddChild(descriptionLabel);
    }
}
