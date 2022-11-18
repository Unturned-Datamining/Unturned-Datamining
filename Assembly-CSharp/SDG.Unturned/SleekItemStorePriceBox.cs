using UnityEngine;

namespace SDG.Unturned;

internal class SleekItemStorePriceBox : SleekWrapper
{
    private ISleekBox backdropBox;

    private ISleekLabel basePriceLabel;

    private ISleekLabel currentPriceLabel;

    private ISleekImage discountStrikethrough;

    private ISleekLabel percentageLabel;

    public void SetPrice(ulong basePrice, ulong currentPrice, int quantity)
    {
        uint num = (uint)Mathf.Max(quantity, 1);
        if (currentPrice == basePrice)
        {
            basePriceLabel.isVisible = false;
            discountStrikethrough.isVisible = false;
            percentageLabel.isVisible = false;
            currentPriceLabel.positionScale_X = 0f;
            currentPriceLabel.positionScale_Y = 0f;
            currentPriceLabel.sizeScale_X = 1f;
            currentPriceLabel.sizeScale_Y = 1f;
            currentPriceLabel.text = ItemStore.Get().FormatPrice(currentPrice * num);
            if (quantity > 1)
            {
                backdropBox.tooltipText = $"{ItemStore.Get().FormatPrice(currentPrice)} x {quantity} = {currentPriceLabel.text}";
            }
            else
            {
                backdropBox.tooltipText = currentPriceLabel.text;
            }
            return;
        }
        basePriceLabel.isVisible = true;
        discountStrikethrough.isVisible = true;
        percentageLabel.isVisible = true;
        currentPriceLabel.positionScale_X = 0.5f;
        currentPriceLabel.positionScale_Y = 0.5f;
        currentPriceLabel.sizeScale_X = 0.5f;
        currentPriceLabel.sizeScale_Y = 0.5f;
        ulong num2 = basePrice * num;
        ulong num3 = currentPrice * num;
        basePriceLabel.text = ItemStore.Get().FormatPrice(num2);
        currentPriceLabel.text = ItemStore.Get().FormatPrice(num3);
        percentageLabel.text = ItemStore.Get().FormatDiscount(num3, num2);
        if (quantity > 1)
        {
            string text = $"{ItemStore.Get().FormatPrice(basePrice)} x {quantity} = {basePriceLabel.text}";
            string text2 = $"{ItemStore.Get().FormatPrice(currentPrice)} x {quantity} = {currentPriceLabel.text}";
            backdropBox.tooltipText = RichTextUtil.wrapWithColor(text, Color.gray) + "\n" + RichTextUtil.wrapWithColor(percentageLabel.text, Color.green) + "\n" + RichTextUtil.wrapWithColor(text2, ItemStore.PremiumColor);
        }
        else
        {
            backdropBox.tooltipText = RichTextUtil.wrapWithColor(basePriceLabel.text, Color.gray) + "\n" + RichTextUtil.wrapWithColor(percentageLabel.text, Color.green) + "\n" + RichTextUtil.wrapWithColor(currentPriceLabel.text, ItemStore.PremiumColor);
        }
    }

    public SleekItemStorePriceBox()
    {
        backdropBox = Glazier.Get().CreateBox();
        backdropBox.sizeScale_X = 1f;
        backdropBox.sizeScale_Y = 1f;
        backdropBox.textColor = ItemStore.PremiumColor;
        AddChild(backdropBox);
        basePriceLabel = Glazier.Get().CreateLabel();
        basePriceLabel.positionScale_X = 0.5f;
        basePriceLabel.sizeScale_X = 0.5f;
        basePriceLabel.sizeScale_Y = 0.5f;
        basePriceLabel.fontSize = ESleekFontSize.Medium;
        basePriceLabel.textColor = Color.gray;
        AddChild(basePriceLabel);
        discountStrikethrough = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
        discountStrikethrough.positionScale_X = 0.5f;
        discountStrikethrough.positionScale_Y = 0.25f;
        discountStrikethrough.positionOffset_Y = -1;
        discountStrikethrough.sizeOffset_Y = 1;
        discountStrikethrough.sizeScale_X = 0.5f;
        discountStrikethrough.isAngled = true;
        discountStrikethrough.angle = -15f;
        discountStrikethrough.color = Palette.COLOR_R;
        AddChild(discountStrikethrough);
        currentPriceLabel = Glazier.Get().CreateLabel();
        currentPriceLabel.sizeScale_X = 1f;
        currentPriceLabel.fontSize = ESleekFontSize.Medium;
        currentPriceLabel.textColor = ItemStore.PremiumColor;
        AddChild(currentPriceLabel);
        percentageLabel = Glazier.Get().CreateLabel();
        percentageLabel.sizeScale_X = 0.5f;
        percentageLabel.sizeScale_Y = 1f;
        percentageLabel.fontSize = ESleekFontSize.Medium;
        percentageLabel.textColor = Color.green;
        AddChild(percentageLabel);
    }
}
