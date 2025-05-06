using System;
using System.Text;
using UnityEngine;

namespace SDG.Unturned;

public class SleekCraftingTagProviderButton : SleekWrapper
{
    public ICraftingTagProvider tagProvider;

    private ISleekButton button;

    private SleekItemIcon icon;

    private ISleekLabel nameLabel;

    private static StringBuilder tagsSb = new StringBuilder();

    public event Action<ICraftingTagProvider> OnClicked;

    internal void SetTagProvider(NearbyCraftingTagProvider tagProvider)
    {
        this.tagProvider = tagProvider.component;
        string arg;
        if (tagProvider.asset is ItemAsset itemAsset)
        {
            nameLabel.PositionOffset_X = 50f;
            nameLabel.SizeOffset_X = -50f;
            nameLabel.Text = itemAsset.itemName;
            nameLabel.TextColor = ItemTool.getRarityColorUI(itemAsset.rarity);
            icon.Refresh(itemAsset, 40, 40);
            icon.IsVisible = true;
            arg = itemAsset.RarityRichTextName;
        }
        else
        {
            nameLabel.PositionOffset_X = 0f;
            nameLabel.SizeOffset_X = 0f;
            nameLabel.Text = tagProvider.asset.FriendlyName;
            nameLabel.TextColor = ESleekTint.FONT;
            icon.IsVisible = false;
            arg = tagProvider.asset.FriendlyName;
        }
        tagsSb.Clear();
        tagsSb.AppendFormat(PlayerDashboardCraftingUI.localization.format("TagProvider_Tooltip", arg));
        tagsSb.AppendLine();
        bool flag = true;
        foreach (TagAsset tag in tagProvider.tags)
        {
            if (!flag)
            {
                tagsSb.Append(PlayerDashboardCraftingUI.localization.format("Requirements_Separator"));
            }
            tagsSb.Append(tag.RichTextOrPreferredFontColor);
            flag = false;
        }
        button.TooltipText = tagsSb.ToString();
    }

    public SleekCraftingTagProviderButton()
    {
        button = Glazier.Get().CreateButton();
        button.SizeScale_X = 1f;
        button.SizeScale_Y = 1f;
        button.AllowRichText = true;
        button.OnClicked += OnClickedInternalButton;
        AddChild(button);
        icon = new SleekItemIcon();
        icon.PositionOffset_X = 5f;
        icon.PositionOffset_Y = 5f;
        icon.SizeOffset_X = 40f;
        icon.SizeOffset_Y = 40f;
        icon.IsVisible = false;
        AddChild(icon);
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.SizeScale_X = 1f;
        nameLabel.SizeScale_Y = 1f;
        nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        nameLabel.TextAlignment = TextAnchor.MiddleLeft;
        nameLabel.FontSize = ESleekFontSize.Medium;
        AddChild(nameLabel);
    }

    private void OnClickedInternalButton(ISleekElement internalButton)
    {
        this.OnClicked?.Invoke(tagProvider);
    }
}
